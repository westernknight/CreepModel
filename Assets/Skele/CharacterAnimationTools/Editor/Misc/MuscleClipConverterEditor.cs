using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MH;

using CurveDict = System.Collections.Generic.Dictionary<UnityEngine.Transform, MuscleClipConverterEditor._Curves>;


/// <summary>
/// used to convert a muscle-clip to the generic/legacy animation clip which can be edited in UAW
/// 
/// steps:
/// 1. get your target model, duplicate one into humanoid rig,
/// 2. set the target animation into the target model's animator, only one state is allowed;
/// 3. run this editor, finish the conversion
/// 
/// </summary>
public class MuscleClipConverterEditor : EditorWindow
{
	#region "configurable data"
    // configurable data

    #endregion "configurable data"

	#region "data"
    // data

    private static MuscleClipConverterEditor ms_Instance;

    private Animator m_Animator;
    private SkinnedMeshRenderer m_SMR;
    private ModelImporterAnimationType m_AnimType = ModelImporterAnimationType.Generic;

    #endregion "data"

	#region "unity event handlers"
    // unity event handlers

    [MenuItem("Window/Skele/MuscleClipConverter")]
    public static void OpenWindow()
    {
        if( ms_Instance == null )
        {
            var inst = ms_Instance = (MuscleClipConverterEditor)GetWindow(typeof(MuscleClipConverterEditor));
            EditorApplication.playmodeStateChanged += inst.OnPlayModeChanged;
        }        
    }

    void OnGUI()
    {
        m_Animator = (Animator)EditorGUILayout.ObjectField("Animator", m_Animator, typeof(Animator), true);
        m_AnimType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("AnimType", m_AnimType);
        if( m_AnimType != ModelImporterAnimationType.Legacy )
        {
            m_AnimType = ModelImporterAnimationType.Generic;
        }
        
        bool bSet = (m_Animator != null);
        EUtil.PushGUIEnable(bSet);
        if( EUtil.Button("Convert Animation!", bSet ? Color.green : Color.red) )
        {
            _ConvertAnim();
        }
        EUtil.PopGUIEnable();
    }

    void OnDestroy()
    {
        ms_Instance = null;
        EditorApplication.playmodeStateChanged -= this.OnPlayModeChanged;
    }

    void OnPlayModeChanged()
    {
        if(ms_Instance != null)
        {
            ms_Instance.Close();
        }
	}

    #endregion "unity event handlers"

	#region "public method"
    // public method

    #endregion "public method"

	#region "private method"
    // private method

    private void _ConvertAnim()
    {
        // 0. prepare
        if( !m_Animator.isHuman )
        {
            Dbg.LogWarn("MuscleClipConverterEditor._ConvertAnim: Need to change to Humanoid rig first!");
            return;
        }

        m_SMR = m_Animator.GetComponentInChildren<SkinnedMeshRenderer>();
        if( m_SMR == null )
        {
            Dbg.LogWarn("MuscleClipConverterEditor._ConvertAnim: failed to find SMR under {0}", m_Animator.name);
            return;
        }
        
        m_Animator.Update(0);
        UnityEngine.AnimationInfo[] ainfos = m_Animator.GetCurrentAnimationClipState(0); //only effect after Update is called
        AnimationClip clip = ainfos[0].clip;
        AnimationClipSettings oldSettings = AnimationUtility.GetAnimationClipSettings(clip);

        //{//debug
        //    var bindings = AnimationUtility.GetCurveBindings(clip);
        //    foreach( var b in bindings)
        //    {
        //        Dbg.Log("path: {0}, prop: {1}", b.path, b.propertyName);
        //    }
        //}

        Transform animatorTr = m_Animator.transform;
        CurveDict curveDict = new CurveDict();

        float SAMPLE_RATE = clip.frameRate;
        float clipLen = clip.length;

        string oldClipAssetPath = AssetDatabase.GetAssetPath(clip);
        string newClipAssetPath = PathUtil.StripExtension(oldClipAssetPath) + NEW_CLIP_POSTFIX + ".anim";

        List<Transform> boneLst = new List<Transform>();
        for (HumanBodyBones boneIdx = 0; boneIdx < HumanBodyBones.LastBone; ++boneIdx)
        {
            Transform tr = m_Animator.GetBoneTransform(boneIdx);
            //Dbg.Log("Map: {0}->{1}", boneIdx, tr);
            if( tr != null )
            {
                boneLst.Add(tr);
            }
        }
        Transform[] bones = boneLst.ToArray();

        // init curves for each bone
        for (int idx = 0; idx < bones.Length; ++idx)
        {
            Transform oneBone = bones[idx];
            string trPath = AnimationUtility.CalculateTransformPath(oneBone, animatorTr);

            var curves = new _Curves();
            curves.relPath = trPath;           

            curveDict.Add(oneBone, curves);
        }

        // init rootmotion curve
        {
            var curves = new _Curves();
            curveDict.Add(animatorTr, curves);
        }

        Vector3 InitAnimatorPos = animatorTr.localPosition;
        Quaternion InitAnimatorRot = animatorTr.localRotation;
        Matrix4x4 animatorInitMat = animatorTr.worldToLocalMatrix;        

        AnimatorStateInfo curStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        float nt = curStateInfo.normalizedTime; 
        m_Animator.Update(-nt * clipLen); //revert to 0 time

        { // 1. bake animation info into curve on all bones transform
            float time = 0f;
            float deltaTime = 1f / (SAMPLE_RATE);
            for (; 
                time <= clipLen || Mathf.Approximately(time, clipLen); 
                )
            {
                //Dbg.Log("nt = {0}", m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

                // bone
                for (int idx = 0; idx < bones.Length; ++idx)
                {
                    Transform oneBone = bones[idx];
                    _Curves curves = curveDict[oneBone];

                    Vector3 pos = oneBone.localPosition;
                    Quaternion rot = oneBone.localRotation;

                    curves.X.AddKey(time, rot.x);
                    curves.Y.AddKey(time, rot.y);
                    curves.Z.AddKey(time, rot.z);
                    curves.W.AddKey(time, rot.w);

                    curves.PX.AddKey(time, pos.x);
                    curves.PY.AddKey(time, pos.y);
                    curves.PZ.AddKey(time, pos.z);
                }

                // root motion
                {
                    _Curves curves = curveDict[animatorTr];
                    Vector3 pos = animatorTr.localPosition;
                    Vector3 fwd = animatorTr.forward;
                    Vector3 up = animatorTr.up;

                    pos = animatorInitMat.MultiplyPoint(pos);
                    fwd = animatorInitMat.MultiplyVector(fwd);
                    up = animatorInitMat.MultiplyVector(up);

                    Quaternion rot = Quaternion.LookRotation(fwd, up);

                    curves.X.AddKey(time, rot.x);
                    curves.Y.AddKey(time, rot.y);
                    curves.Z.AddKey(time, rot.z);
                    curves.W.AddKey(time, rot.w);

                    curves.PX.AddKey(time, pos.x);
                    curves.PY.AddKey(time, pos.y);
                    curves.PZ.AddKey(time, pos.z);
                }

                if (!Mathf.Approximately(time + deltaTime, clipLen))
                {
                    m_Animator.Update(deltaTime); 
                    time += deltaTime;
                }
                else
                {
                    m_Animator.Update(deltaTime - 0.005f); //keep it in the range, if go beyond, something bad could happen
                    time += deltaTime - 0.005f;
                }
            }


        } //end of 1.


        { // 2. set animation clip and store in AssetDatabase
            AnimationClip newClip = new AnimationClip();
            newClip.frameRate = SAMPLE_RATE;
            newClip.localBounds = clip.localBounds;

            // set bone curves
            for( var ie = curveDict.GetEnumerator(); ie.MoveNext(); )
            {
                var curves = ie.Current.Value;
                if( ie.Current.Key == animatorTr )
                { //root motion
                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionT.x", curves.PX);
                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionT.y", curves.PY);
                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionT.z", curves.PZ);

                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionQ.x", curves.X);
                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionQ.y", curves.Y);
                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionQ.z", curves.Z);
                    newClip.SetCurve(curves.relPath, typeof(Animator), "MotionQ.w", curves.W);
                }
                else
                {
                    newClip.SetCurve(curves.relPath, typeof(Transform), "localRotation.x", curves.X);
                    newClip.SetCurve(curves.relPath, typeof(Transform), "localRotation.y", curves.Y);
                    newClip.SetCurve(curves.relPath, typeof(Transform), "localRotation.z", curves.Z);
                    newClip.SetCurve(curves.relPath, typeof(Transform), "localRotation.w", curves.W);

                    newClip.SetCurve(curves.relPath, typeof(Transform), "localPosition.x", curves.PX);
                    newClip.SetCurve(curves.relPath, typeof(Transform), "localPosition.y", curves.PY);
                    newClip.SetCurve(curves.relPath, typeof(Transform), "localPosition.z", curves.PZ);
                }                
            }

            newClip.EnsureQuaternionContinuity();
            AnimationUtility.SetAnimationType(newClip, m_AnimType);
            RCall.CallMtd("UnityEditor.AnimationUtility", "SetAnimationClipSettings", null, newClip, oldSettings);

            AnimationClip existClip = AssetDatabase.LoadAssetAtPath(newClipAssetPath, typeof(AnimationClip)) as AnimationClip;
            if(  existClip != null )
            {
                EditorUtility.CopySerialized(newClip, existClip);
            }
            else
            {
                AssetDatabase.CreateAsset(newClip, newClipAssetPath);
            }
        } //end of 2.

        // 3. clean job
        animatorTr.localPosition = InitAnimatorPos; //resume pos & rot
        animatorTr.localRotation = InitAnimatorRot;
        curveDict = null;
        AssetDatabase.SaveAssets();

        Dbg.Log("Converted: {0}", newClipAssetPath); 
    }

    private static void _CopyCurve(AnimationClip clip, AnimationClip newClip, string oldName, string name)
    {
        var binding = EditorCurveBinding.FloatCurve("", typeof(Animator), oldName);
        var curve = AnimationUtility.GetEditorCurve(clip, binding);
        if (curve != null)
        {
            var newBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), name);
            AnimationUtility.SetEditorCurve(newClip, newBinding, curve);
        }
    }

    #endregion "private method"

	#region "constant data"
    // constant data

    public const string NEW_CLIP_POSTFIX = "_Converted";

    #endregion "constant data"

	#region "Inner Struct"
	// "Inner Struct" 

    public class _Curves
    {
        public string relPath = string.Empty;
        public AnimationCurve X;
        public AnimationCurve Y;
        public AnimationCurve Z;
        public AnimationCurve W;

        public AnimationCurve PX;
        public AnimationCurve PY;
        public AnimationCurve PZ;

        public _Curves()
        {
            X = new AnimationCurve();
            Y = new AnimationCurve();
            Z = new AnimationCurve();
            W = new AnimationCurve();

            PX = new AnimationCurve();
            PY = new AnimationCurve();
            PZ = new AnimationCurve();
        }
    }
	
	#endregion "Inner Struct"
}

