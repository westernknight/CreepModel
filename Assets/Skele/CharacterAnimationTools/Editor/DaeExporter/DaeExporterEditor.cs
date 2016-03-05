using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MH;

public class DaeExporterEditor : EditorWindow
{
	#region "data"
    // common
    private Vector3 m_ScrollPos = Vector2.zero;

    // SMR
    private AnimationClip m_Clip;
    private Transform m_RootBone;
    private List<SkinnedMeshRenderer> m_SMRs = new List<SkinnedMeshRenderer>();

    // MF
    private List<MeshFilter> m_MFs = new List<MeshFilter>();

    #endregion "data"

	#region "public method"
    // public method

    [MenuItem("Window/Skele/DAE Exporter")]
    public static void Init()
    {
       var wnd = GetWindow(typeof(DaeExporterEditor));
       wnd.title = "DAE Exporter";
    }

    void OnGUI()
    {
        m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
        _OnGUI_SMR();
        GUILayout.EndScrollView();
    }

    #endregion "public method"

	#region "private method"
    
    private void _OnGUI_SMR()
    {
        m_RootBone = EditorGUILayout.ObjectField("RootBone", m_RootBone, typeof(Transform), true) as Transform;
        GUIUtil.PushGUIEnable(m_RootBone != null);
        {
            if (EUtil.Button("AutoFind", Color.green))
            {
                _AutoFindRenderers();
            }
        }
        GUIUtil.PopGUIEnable();

        EditorGUILayout.Separator();

        //SMR
        for (int idx = 0; idx < m_SMRs.Count; ++idx)
        {
            GUILayout.BeginHorizontal();
            
            if (EUtil.Button("X", "delete", Color.red, GUILayout.Width(30f)))
            {
                m_SMRs.RemoveAt(idx);
                --idx;
                continue;
            }

            Color oc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            m_SMRs[idx] = EditorGUILayout.ObjectField(m_SMRs[idx], typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            GUI.backgroundColor = oc;

            GUILayout.EndHorizontal();
        }
        if (EUtil.Button("Add SMR Entry", Color.green))
        {
            m_SMRs.Add(null);
        }

        //MF
        for (int idx = 0; idx < m_MFs.Count; ++idx)
        {
            GUILayout.BeginHorizontal();
            if (EUtil.Button("X", "delete", Color.red, GUILayout.Width(30f)))
            {
                m_MFs.RemoveAt(idx);
                --idx;
                continue;
            }

            Color oc = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            m_MFs[idx] = EditorGUILayout.ObjectField(m_MFs[idx], typeof(MeshFilter), true) as MeshFilter;
            GUI.backgroundColor = oc;

            GUILayout.EndHorizontal();
        }
        if (EUtil.Button("Add MF Entry", Color.green))
        {
            m_MFs.Add(null);
        }
        EditorGUILayout.Separator();

        m_Clip = EditorGUILayout.ObjectField("AnimClip", m_Clip, typeof(AnimationClip), true) as AnimationClip;

        bool bHasValidEntry = _HasValidEntry(); 
        Color c = (bHasValidEntry) ? Color.green : Color.red;
        GUIUtil.PushGUIColor(c);
        GUIUtil.PushGUIEnable(bHasValidEntry);
        if (GUILayout.Button("Export!"))
        {
            string filePath = EditorUtility.SaveFilePanel("Select export file path", Application.dataPath, "anim", "dae");
            if (filePath.Length > 0)
            {
                SkinnedMeshRenderer[] smrArr = m_SMRs.TakeWhile(x => x != null).ToArray();
                MeshFilter[] mfArr = m_MFs.TakeWhile(x => x != null).ToArray();

                DaeExporter exp = new DaeExporter(smrArr, mfArr, m_RootBone);
                exp.Export(m_Clip, filePath);

                AssetDatabase.Refresh();
            }
            else
            {
                EUtil.ShowNotification("Export Cancelled...");
            }
        }
        GUIUtil.PopGUIEnable();
        GUIUtil.PopGUIColor();
    }

    private bool _HasValidEntry()
    {
        bool bReady4SMR = m_RootBone != null && m_SMRs.Count(x => x != null) > 0;
        bool bReady4MF = m_RootBone == null && m_SMRs.Count(x => x != null) == 0 && m_MFs.Count(x => x != null) > 0;
        return bReady4MF || bReady4SMR;
    }

    private void _AutoFindRenderers()
    {
        // first get to AnimRoot
        Transform tr = m_RootBone;
        while( tr != null && tr.animation == null && tr.GetComponent<Animator>() == null )
        {
            tr = tr.parent;
        }
        if( tr == null )
        {
            Dbg.LogWarn("DaeExporterEditor._AutoFindRenderers: cannot find GO with Animation/Animator in ancestors of {0}", m_RootBone.name);
            return;
        }

        // then recursively find out all SMRs
        SkinnedMeshRenderer[] smrs = tr.GetComponentsInChildren<SkinnedMeshRenderer>();
        m_SMRs.Clear();
        m_SMRs.AddRange(smrs);

        // and recursively find out all MFs
        MeshFilter[] mfs = tr.GetComponentsInChildren<MeshFilter>();
        m_MFs.Clear();
        m_MFs.AddRange(mfs);
    }

    // private method

    #endregion "private method"

	#region "constant data"
    // constant data

    //public enum OpType
    //{
    //    SMR,
    //    MF,
    //}

    #endregion "constant data"
}
