using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshWithSameMat
{
    public Material m_Material;
    public List<GameObject> m_Objets = new List<GameObject>();

    public MeshWithSameMat(Material mat)
    {
        m_Material = mat;
    }
}

public class MeshCombiner : EditorWindow
{
 //   public GameObject m_SrcPrefab;
    private Dictionary<Shader, List<MeshWithSameMat>> m_ShaderGroups = new Dictionary<Shader, List<MeshWithSameMat>>();
    private Vector2 scrollposition;
    private Vector2 scrollpositionRight;
    

     [MenuItem("TMTool/MeshCombiner")]
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
/*
        if (GUILayout.Button("刷新列表"))
        {
            UpdateList();
        }*/

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
                foreach (GameObject obj in matGroup.m_Objets)
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
                foreach(MeshWithSameMat matGroup in matGroups)
                {
                    if(matGroup.m_Material == mat)
                    {
                        matGroup.m_Objets.Add(obj);
                        bInserted = true;
                        break;
                    }
                }

                if(!bInserted)
                {
                    MeshWithSameMat matGroup = new MeshWithSameMat(mat);
                    matGroup.m_Objets.Add(obj);

                    matGroups.Add(matGroup);
                }
            }
            else
            {
                matGroups = new List<MeshWithSameMat>();
                MeshWithSameMat matGroup = new MeshWithSameMat(mat);
                matGroup.m_Objets.Add(obj);

                matGroups.Add(matGroup);
                m_ShaderGroups.Add(shader, matGroups);
            }
        }
    }

    // 保存Prefab
    void SavePrefab()
    {
        //PrefabUtility.
    }

}