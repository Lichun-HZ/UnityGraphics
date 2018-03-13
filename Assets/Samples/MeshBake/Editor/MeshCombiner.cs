using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class MeshWithSameMat
{
    public Material m_Material;
    public List<GameObject> m_Objects = new List<GameObject>();
    public int VertexNum = 0;
    public int IndexNum = 0;

    public MeshWithSameMat(Material mat)
    {
        m_Material = mat;
    }

    // 按照Z轴排序，Z值大的在前面
    public void AddObject(GameObject obj)
    {
        bool bInsert = false;
        int iSize = m_Objects.Count;
        for (int i = 0; i < iSize; ++i)
        {
            if (obj.transform.position.z > m_Objects[i].transform.position.z)
            {
                m_Objects.Insert(i > 0 ? i - 1 : 0, obj);

                bInsert = true;
                break;
            }
        }

        if (!bInsert)
            m_Objects.Add(obj);

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

        VertexNum += meshFilter.sharedMesh.vertexCount;
        IndexNum += (int)meshFilter.sharedMesh.GetIndexCount(0);
    }
}

public class MeshCombiner : EditorWindow
{
    //   public GameObject m_SrcPrefab;
    private Dictionary<Shader, List<MeshWithSameMat>> m_ShaderGroups = new Dictionary<Shader, List<MeshWithSameMat>>();
    private Vector2 scrollposition;
    private Vector2 scrollpositionRight;


    [MenuItem("Tools/MeshCombiner")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshCombiner window = (MeshCombiner)EditorWindow.GetWindow(typeof(MeshCombiner));
        window.Show();
        window.UpdateList();
    }

    private void OnFocus()
    {
        //  SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        //  SceneView.onSceneGUIDelegate += this.OnSceneGUI;

    }

    private void OnDestroy()
    {
        //  SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    void OnHierarchyChange()
    {
        //Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
    }

    void OnProjectChange()
    {
        // Debug.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnSelectionChange()
    {
        //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        UpdateList();
    }


    // Implement your own editor GUI here.
    void OnGUI()
    {
        if (GUILayout.Button("合并模型"))
        {
            CombineMeshes();
        }

        GUILayout.BeginHorizontal();
        DrawSelectObjectList();
        DrawRightList();
        GUILayout.EndHorizontal();
    }

    void DrawSelectObjectList()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        //绘制标签
        GUILayout.Label("SelectedObjects");
        GUILayout.Space(10);

        //开始滑块区域
        scrollposition = GUILayout.BeginScrollView(scrollposition);

        GameObject[] selectedObjs = Selection.gameObjects;
        foreach (GameObject obj in selectedObjs)
        {
            GUILayout.Label(obj.name);
        }


        //结束滑块区域
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void DrawRightList()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        //绘制标签
        GUILayout.Label("CombineInfos");
        GUILayout.Space(10);

        //开始滑块区域
        scrollpositionRight = GUILayout.BeginScrollView(scrollpositionRight);

        Shader shader;
        List<MeshWithSameMat> matGroups;

        foreach (KeyValuePair<Shader, List<MeshWithSameMat>> kvp in m_ShaderGroups)
        {
            shader = kvp.Key;
            matGroups = kvp.Value;
            GUILayout.Label(shader.name);

            foreach (MeshWithSameMat matGroup in matGroups)
            {
                GUILayout.Label("   " + matGroup.m_Material.name);
                foreach (GameObject obj in matGroup.m_Objects)
                {
                    GUILayout.Label("       " + obj.name);
                }
            }
        }


        //结束滑块区域
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    // 用相同Shader的统计到一组（未考虑Keywords），Shader组里面再按照材质分组
    private void UpdateList()
    {
        m_ShaderGroups.Clear();

        List<MeshWithSameMat> matGroups;
        GameObject[] selectedObjs = Selection.gameObjects;
        foreach (GameObject obj in selectedObjs)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            MeshFilter mf = obj.GetComponent<MeshFilter>();

            if (mr == null || mf == null)
                continue;

            Material mat = mr.sharedMaterial;
            Shader shader = mat.shader; // 暂时没考虑keyword，如果用到了一定要考虑

            if (m_ShaderGroups.TryGetValue(shader, out matGroups))
            {
                bool bInserted = false;
                foreach (MeshWithSameMat matGroup in matGroups)
                {
                    if (matGroup.m_Material == mat)
                    {
                        matGroup.AddObject(obj);

                        bInserted = true;
                        break;
                    }
                }

                if (!bInserted)
                {
                    MeshWithSameMat matGroup = new MeshWithSameMat(mat);
                    matGroup.AddObject(obj);

                    matGroups.Add(matGroup);
                }
            }
            else
            {
                matGroups = new List<MeshWithSameMat>();
                MeshWithSameMat matGroup = new MeshWithSameMat(mat);
                matGroup.AddObject(obj);

                matGroups.Add(matGroup);
                m_ShaderGroups.Add(shader, matGroups);
            }
        }
    }

    // 当Scale有奇数个负数时，会导致坐标系翻转，unity会改变shader的CullMode
    bool IsCullModeChanged(Vector3 Scale)
    {
        int i = 0;
        if (Scale.x < 0)
            ++i;
        if (Scale.y < 0)
            ++i;
        if (Scale.z < 0)
            ++i;

        return (i == 1) || (i == 3);
    }

    void CombineMeshes()
    {
        Shader shader;
        List<MeshWithSameMat> matGroups;

        foreach (KeyValuePair<Shader, List<MeshWithSameMat>> kvp in m_ShaderGroups)
        {
            shader = kvp.Key;
            matGroups = kvp.Value;

            // 每个材质球合并为一个模型
            foreach (MeshWithSameMat matGroup in matGroups)
            {
                Transform rootObject = matGroup.m_Objects[0].transform.parent;

                GameObject combineObject = new GameObject();
                combineObject.name = new StringBuilder("CombineMesh_").Append(matGroup.m_Material.name).ToString();
                combineObject.transform.SetParent(rootObject);
                combineObject.transform.localPosition = Vector3.zero;
                combineObject.transform.localRotation = Quaternion.identity;
                combineObject.transform.localScale = Vector3.one;

                MeshRenderer mr = combineObject.AddComponent<MeshRenderer>();
                MeshFilter mf = combineObject.AddComponent<MeshFilter>();
                mf.sharedMesh = new Mesh();

                List<Vector3> pos = new List<Vector3>(matGroup.VertexNum);
                List<int> triangles = new List<int>(matGroup.IndexNum);
                List<Vector2> uv = new List<Vector2>(matGroup.VertexNum);
                int indexOffset = 0;

                foreach (GameObject obj in matGroup.m_Objects)
                {
                    Matrix4x4 transMatrix = rootObject.worldToLocalMatrix * obj.transform.localToWorldMatrix;
                    Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

                    bool bCullChanged = IsCullModeChanged(obj.transform.localScale);

                    Vector3[] vPos = mesh.vertices;
                    int VertexCount = mesh.vertexCount;
                    int IndexCount = (int)mesh.GetIndexCount(0);

                    for (int j = 0; j < VertexCount; j++)
                    {
                        vPos[j] = transMatrix.MultiplyPoint(vPos[j]);
                    }

                    int[] index = mesh.GetIndices(0);

                    // 如果有CullMode改变，通过改变索引顺序来达到合批的效果
                    if (bCullChanged)
                    {
                        if (mesh.GetTopology(0) == MeshTopology.Triangles)
                        {
                            int iChanged = 0;
                            for (int j = 0; j < IndexCount; j++)
                            {
                                index[j] += indexOffset;

                                if ((iChanged % 3) == 2)
                                {
                                    int iSwap = index[j];
                                    index[j] = index[j - 1];
                                    index[j - 1] = iSwap;
                                }

                                ++iChanged;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < IndexCount; j++)
                            {
                                index[j] += indexOffset;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < IndexCount; j++)
                        {
                            index[j] += indexOffset;
                        }
                    }

                    pos.AddRange(vPos);
                    uv.AddRange(mesh.uv);
                    triangles.AddRange(index);

                    obj.transform.SetParent(combineObject.transform);
                    obj.SetActive(false);

                    indexOffset += VertexCount;
                }

                mf.sharedMesh.SetVertices(pos);
                mf.sharedMesh.SetUVs(0, uv);
                mf.sharedMesh.SetTriangles(triangles, 0);

                mr.sharedMaterials = new Material[] { matGroup.m_Material };
                combineObject.SetActive(true);

                // 保存生成的Mesh数据
                AssetDatabase.CreateAsset(mf.sharedMesh, "Assets/Resources/CombineMeshes/" + matGroup.m_Material.name +
                    combineObject.GetHashCode() + matGroup.m_Objects[0].GetHashCode() + ".asset");

                Selection.activeObject = combineObject;
            }
        }
    }

    // 保存Prefab
    void SavePrefab()
    {
        //PrefabUtility.

    }

}