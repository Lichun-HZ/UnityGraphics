using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class MeshCombineInfo : EditorWindow
{
    public GameObject m_Prefab;
    
    private Vector2 scrollposition;


    // Implement your own editor GUI here.
    void OnGUI()
    {
        DrawList();
    }

    void DrawList()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width));
        //绘制标签
        GUILayout.Label("Preview");

        //开始滑块区域
        scrollposition = GUILayout.BeginScrollView(scrollposition);


        //结束滑块区域
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    public void SetPrefab(GameObject rootObj)
    {
        m_Prefab = rootObj;
       
        UpdateList();
    }

    private void UpdateList()
    {
        if (m_Prefab == null)
            return;


    }
}
