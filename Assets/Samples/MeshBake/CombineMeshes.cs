using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombineMeshes : MonoBehaviour
{
    public bool MergeMaterial = false;

    void Start()
    {
        //获取MeshRender;  
        MeshRenderer[] meshRenders = GetComponentsInChildren<MeshRenderer>();

        //材质;  
        Material[] mats;

        if (MergeMaterial)
        {
            mats = new Material[1];
            for (int i = 0; i < meshRenders.Length; i++)
            {
                mats[i] = meshRenders[i].sharedMaterial;
                break;
            }
        }
        else
        {
            mats = new Material[meshRenders.Length];
            for (int i = 0; i < meshRenders.Length; i++)
            {
                mats[i] = meshRenders[i].sharedMaterial;
            }
        }

        //合并Mesh;  
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();


        {
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);
            }

            mf.mesh.CombineMeshes(combine, MergeMaterial, true);
        }

        {
            int iVertexCount = 0;
            uint iIndexCount = 0;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                iVertexCount += meshFilters[i].sharedMesh.vertexCount;
                iIndexCount += meshFilters[i].sharedMesh.GetIndexCount(0);
            }

            List<Vector3> pos = new List<Vector3>(iVertexCount);

            mf.mesh.SetVertices
        }

        gameObject.SetActive(true);
        mr.sharedMaterials = mats;
    }

}