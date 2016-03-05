using UnityEngine;
using System.Collections;
using UnityEditor;
using Mixamo.UnityStore.Utility;
using System.Collections.Generic;
using MH;
using System.Linq;
using System.IO;
public class CopyAnimEditor : EditorWindow
{


    GameObject firstObject;
    GameObject lastFirstObject;
    string daeName;
    /// <summary>
    /// //////////////////////////////////////////////////////
    /// </summary>
    private Vector3 m_ScrollPos = Vector2.zero;

    // SMR
    private AnimationClip m_Clip;
    private Transform m_RootBone;
    private List<SkinnedMeshRenderer> m_SMRs = new List<SkinnedMeshRenderer>();

    // MF
    private List<MeshFilter> m_MFs = new List<MeshFilter>();

    [MenuItem("Debug/CopyAnimEditor ")]
    static void Init()
    {
        CopyAnimEditor window = (CopyAnimEditor)EditorWindow.GetWindow(typeof(CopyAnimEditor));
        window.Show();

    }

    GameObject previewGameObject;
    private void _AutoFindRenderers()
    {
        // first get to AnimRoot
        Transform tr = m_RootBone;
        while (tr != null && tr.animation == null && tr.GetComponent<Animator>() == null)
        {
            tr = tr.parent;
        }
        if (tr == null)
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
    void OnGUI()
    {
        GameObject go = GameObject.Find("MixamoPreviewGO");
        if (go!=null)
        {
            firstObject = go;
        }

        firstObject = EditorGUILayout.ObjectField("first", firstObject, typeof(GameObject), true) as GameObject;

        if (lastFirstObject != firstObject)
        {
            lastFirstObject = firstObject;
            
        }
        daeName = EditorGUILayout.TextField("dea file name: ",daeName);
        if (GUILayout.Button("save anim"))
        {
            previewGameObject = GameObject.Instantiate(firstObject) as GameObject;
           

            AnimationClip animationClip = new AnimationClip();
            animationClip.name = ("h_preview");
            AnimationClipCurveData[] allCurves = AnimationUtility.GetAllCurves(firstObject.animation.clip);
            for (int i = 0; i < allCurves.Length; i++)
            {
                AnimationClipCurveData animationClipCurveData = allCurves[i];
                animationClip.SetCurve(animationClipCurveData.path, animationClipCurveData.type, animationClipCurveData.propertyName, animationClipCurveData.curve);
            }
            AssetDatabase.CreateAsset(animationClip, "Assets/newAnim.anim");
            previewGameObject.animation.clip = animationClip;

            SkinnedMeshRenderer skin = previewGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            Debug.Log("skin" + skin + " .");
            string modelTemplate = "Assets/meshToSave.prefab";
            Mesh meshToSave = Object.Instantiate(skin.sharedMesh) as Mesh;
            AssetDatabase.CreateAsset(meshToSave, modelTemplate);
            AssetDatabase.SaveAssets();
            skin.sharedMesh = meshToSave;

            previewGameObject.SampleAnimation(animationClip, 0);

            m_RootBone = previewGameObject.transform;
            _AutoFindRenderers();
            
           
            string filePath = Path.Combine(Application.dataPath, "__Test/Actions/" + daeName+".dae");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(filePath);
            }
          
            m_Clip = animationClip;

            SkinnedMeshRenderer[] smrArr = m_SMRs.TakeWhile(x => x != null).ToArray();
            MeshFilter[] mfArr = m_MFs.TakeWhile(x => x != null).ToArray();

            DaeExporter exp = new DaeExporter(smrArr, mfArr, m_RootBone);
            exp.Export(m_Clip, filePath);

            AssetDatabase.Refresh();

            GameObject.DestroyImmediate(previewGameObject);

            
        }
    }

    void Update()
    {
      
    }


}
