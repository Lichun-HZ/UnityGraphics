using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombineMeshes : MonoBehaviour
{
    readonly private string combineShaderName = "Hidden/HeroGo/General/UnLit/HG_Unlit_Dye_TransparentCombine2";

    void CopyMaterialParam(Material dest, Material src, int index)
    {
       // int texID = Shader.PropertyToID("_MainTex");
        Texture tex = src.GetTexture("_MainTex");
        Vector4 texST = src.GetVector("_MainTex_ST");
        Color dyeColor = src.GetColor("_DyeColor");
        float alphaFactor = src.GetFloat("_AlphaFactor");
        float colorStrength = src.GetFloat("_ColorStrength");

        if(index == 0)
        {
            dest.SetTexture("_MainTex1", tex);
            dest.SetVector("_MainTex1_ST", texST);
            dest.SetColor("_DyeColor1", dyeColor);
            dest.SetFloat("_AlphaFactor1", alphaFactor);
            dest.SetFloat("_ColorStrength1", colorStrength);
        }
        else
        {
            dest.SetTexture("_MainTex2", tex);
            dest.SetVector("_MainTex2_ST", texST);
            dest.SetColor("_DyeColor2", dyeColor);
            dest.SetFloat("_AlphaFactor2", alphaFactor);
            dest.SetFloat("_ColorStrength2", colorStrength);
        }
    }

    bool CombineMesh()
    {
        Shader combineShader = Shader.Find(combineShaderName);
        if (combineShader == null)
            return false;

        //获取MeshRender;  
        MeshRenderer[] meshRenders = GetComponentsInChildren<MeshRenderer>();

        Dictionary<Material, List<MeshFilter>> meshGroups = new Dictionary<Material, List<MeshFilter>>();
        Material mat;
        List<MeshFilter> meshes;
        int iVertexCount = 0;
        uint iIndexCount = 0;

        for (int i = 0; i < meshRenders.Length; i++)
        {
            mat = meshRenders[i].sharedMaterial;
            MeshFilter meshFilter = meshRenders[i].GetComponent<MeshFilter>();
            if (meshFilter == null)
                continue;

            if (meshGroups.TryGetValue(mat, out meshes))
            {
                meshes.Add(meshFilter);
            }
            else
            {
                meshes = new List<MeshFilter>();
                meshes.Add(meshFilter);

                meshGroups.Add(mat, meshes);
            }

            // 统计顶点和三角面数量
            iVertexCount += meshFilter.sharedMesh.vertexCount;
            iIndexCount += meshFilter.sharedMesh.GetIndexCount(0);
        }

        // 创建合并材质和Mesh
        Material combineMat = new Material(combineShader);
        List<Vector3> pos = new List<Vector3>(iVertexCount);
        List<int> triangles = new List<int>((int)iIndexCount);
        List<Vector3> uv = new List<Vector3>(iVertexCount);
        int indexOffset = 0;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        int matIndex = 0;

        foreach (KeyValuePair<Material, List<MeshFilter>> kvp in meshGroups)
        {
            mat = kvp.Key;
            meshes = kvp.Value;

            foreach (MeshFilter meshFilter in meshes)
            {
                Mesh mesh = meshFilter.sharedMesh;

                Matrix4x4 transMatrix = transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
                Vector3[] vPos = mesh.vertices;
                Vector2[] vUV = mesh.uv;
                int VertexCount = mesh.vertexCount;
                int IndexCount = (int)mesh.GetIndexCount(0);

                Vector3[] uvMatID = new Vector3[VertexCount];
                float fMatID = (float)matIndex;

                for (int j = 0; j < VertexCount; j++)
                {
                    vPos[j] = transMatrix.MultiplyPoint(vPos[j]);
                    uvMatID[j].Set(vUV[j].x, vUV[j].y, fMatID);
                }

                int[] index = mesh.GetIndices(0);

                for (int j = 0; j < IndexCount; j++)
                {
                    index[j] += indexOffset;
                }

                pos.AddRange(vPos);
                uv.AddRange(uvMatID);
                triangles.AddRange(index);

                meshFilter.gameObject.SetActive(false);
                indexOffset += VertexCount;
            }

            CopyMaterialParam(combineMat, mat, matIndex++);
        }
        mf.mesh.SetVertices(pos);
        mf.mesh.SetUVs(0, uv);
        mf.mesh.SetTriangles(triangles, 0);
        mr.sharedMaterials = new Material[] { combineMat };
        gameObject.SetActive(true);

        return true;
    }

    void Start()
    {
        CombineMesh();
    }

}