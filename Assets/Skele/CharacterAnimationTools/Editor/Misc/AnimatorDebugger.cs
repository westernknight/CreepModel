using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MH
{
	public class AnimatorDebugger : EditorWindow
	{
	    #region "configurable data"
        // configurable data

        #endregion "configurable data"

	    #region "data"
        // data

        private Animator m_CurAnimator;

        #endregion "data"

	    #region "unity event handlers"
        // unity event handlers

        [MenuItem("Window/Skele/AnimatorDebugger")]
        public static void OpenWindow()
        {
            var wnd = GetWindow(typeof(AnimatorDebugger)) as AnimatorDebugger;
            wnd.m_CurAnimator = Selection.activeGameObject.GetComponent<Animator>();
        }

        void OnGUI()
        {
            if( m_CurAnimator == null )
            {
                GUILayout.Label("Select animator gameobject first !");
                return;
            }

            GUILayout.Label(string.Format("Cur: {0}", m_CurAnimator.name));
            var stateInfo = m_CurAnimator.GetCurrentAnimatorStateInfo(0);
            float nt = stateInfo.normalizedTime;
            float len = stateInfo.length;
            float t = nt * len;
            GUILayout.Label(string.Format("Cur time: nt:{0}, t:{1}", nt, t));

            if( EUtil.Button("OneFrame", Color.green) )
            {
                m_CurAnimator.Update(ONE_FRAME);
            }

            float newT = EditorGUILayout.Slider(t, 0, len);
            if( newT != t )
            {
                m_CurAnimator.Update(newT - t);
            }
        }

        void OnSelectionChange()
        {
            if( Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Animator>() != null )
            {
                m_CurAnimator = Selection.activeGameObject.GetComponent<Animator>();
            }
            Repaint();
        }
	    
        #endregion "unity event handlers"

	    #region "public method"
        // public method

        #endregion "public method"

	    #region "private method"
        // private method

        #endregion "private method"

	    #region "constant data"
        // constant data

        public const float ONE_FRAME = 0.0333f;

        #endregion "constant data"

	}
}
