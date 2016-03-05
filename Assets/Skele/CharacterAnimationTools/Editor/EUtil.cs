using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;

using Object = UnityEngine.Object;

/// <summary>
/// Editor Utility
/// </summary>
namespace MH
{

    public class EUtil
    {
        public static Stack<Color> ms_clrStack = new Stack<Color>();
        public static Stack<Color> ms_contentClrStack = new Stack<Color>();
        public static Stack<bool> ms_enableStack = new Stack<bool>();
        public static Stack<Color> ms_BackgroundClrStack = new Stack<Color>();

        private static double ms_notificationHideTime = double.MinValue;

        public static void PushGUIColor(Color newClr)
        {
            ms_clrStack.Push(GUI.color);
            GUI.color = newClr;
        }

        public static Color PopGUIColor()
        {
            Color r = GUI.color;
            GUI.color = ms_clrStack.Pop();
            return r;
        }

        public static void PushBackgroundColor(Color newClr)
        {
            ms_BackgroundClrStack.Push(GUI.backgroundColor);
            GUI.backgroundColor = newClr;
        }

        public static Color PopBackgroundColor()
        {
            Color r = GUI.backgroundColor;
            GUI.backgroundColor = ms_BackgroundClrStack.Pop();
            return r;
        }

        public static void PushContentColor(Color clr)
        {
            ms_contentClrStack.Push(GUI.contentColor);
            GUI.contentColor = clr;
        }

        public static Color PopContentColor()
        {
            Color r = GUI.contentColor;
            GUI.contentColor = ms_contentClrStack.Pop();
            return r;
        }

        public static void PushGUIEnable(bool newState)
        {
            ms_enableStack.Push(GUI.enabled);
            GUI.enabled = newState;
        }

        public static bool PopGUIEnable()
        {
            bool r = GUI.enabled;
            GUI.enabled = ms_enableStack.Pop();
            return r;
        }

        public static bool Button(string msg, Color c)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(msg);
            EUtil.PopBackgroundColor();
            return bClick;
        }

        public static bool Button(string msg, string tips)
        {
            bool bClick = GUILayout.Button(new GUIContent(msg, tips));
            return bClick;
        }

        public static bool Button(string msg, string tips, Color c)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(new GUIContent(msg, tips));
            EUtil.PopBackgroundColor();
            return bClick;
        }

        public static bool Button(string msg, string tips, Color c, params GUILayoutOption[] options)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(new GUIContent(msg, tips), options);
            EUtil.PopBackgroundColor();
            return bClick;
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void AlignViewToPos(Vector3 pos, float dist = 40f)
        {
            var target = new GameObject();
            target.transform.position = pos + new Vector3(0, dist, -dist);
            target.transform.rotation = Quaternion.Euler(26.57f, 0, 0);
            SceneView.currentDrawingSceneView.AlignViewToObject(target.transform);
            GameObject.DestroyImmediate(target);
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void AlignViewToObj(GameObject go, float dist = 40f)
        {
            Transform tr = go.transform;
            Vector3 pos = tr.position;
            Renderer render = go.renderer;

            if (render != null)
            {
                Bounds bd = render.bounds;
                dist = Mathf.Max(bd.size.x, Mathf.Max(bd.size.y, bd.size.z));
            }

            var target = new GameObject();
            target.transform.position = pos + new Vector3(0, dist, dist);
            target.transform.rotation = Quaternion.Euler(26.57f, 180f, 0);
            SceneView.lastActiveSceneView.AlignViewToObject(target.transform);
            GameObject.DestroyImmediate(target);
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void FrameSelected()
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void SceneViewLookAt(Vector3 pos)
        {
            SceneView.lastActiveSceneView.LookAt(pos);
        }

        public static SceneView GetSceneView()
        {
            return SceneView.lastActiveSceneView == null ?
                EditorWindow.GetWindow<SceneView>() :
                SceneView.lastActiveSceneView;
        }

        public static void ShowNotification(string msg, float duration = 1.5f)
        {
            if (ms_notificationHideTime < 0)
                EditorApplication.update += _NotificationUpdate;

            EUtil.GetSceneView().ShowNotification(new GUIContent(msg));
            ms_notificationHideTime = EditorApplication.timeSinceStartup + duration;
            EUtil.GetSceneView().Repaint();
        }

        public static void HideNotification()
        {
            EUtil.GetSceneView().RemoveNotification();
            EUtil.GetSceneView().Repaint();
        }

        private static void _NotificationUpdate()
        {
            if (EditorApplication.timeSinceStartup > ms_notificationHideTime)
            {
                HideNotification();
                EditorApplication.update -= _NotificationUpdate;
                ms_notificationHideTime = double.MinValue;
            }
        }

        public static Vector2 DrawV2(Vector2 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                v.x = EditorGUILayout.FloatField("X", v.x, o);
                v.y = EditorGUILayout.FloatField("Y", v.y, o);
            }            
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }

        public static Vector3 DrawV3(Vector3 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                v.x = EditorGUILayout.FloatField("X", v.x, o);
                v.y = EditorGUILayout.FloatField("Y", v.y, o);
                v.z = EditorGUILayout.FloatField("Z", v.z, o);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }

        /// <summary>
        /// [HACK_TRICK]
        /// check if UAW(UnityAnimationWindow) is open
        /// </summary>
        public static bool IsUnityAnimationWindowOpen()
        {
            IList lst = (IList)RCall.CallMtd("UnityEditor.AnimationWindow", "GetAllAnimationWindows", null, null);
            return (lst.Count != 0);
        }

        /// <summary>
        /// [HACK TRICK]
        /// get UAW if there is, else null
        /// </summary>
        public static object GetUnityAnimationWindow()
        {
            IList lst = (IList)RCall.CallMtd("UnityEditor.AnimationWindow", "GetAllAnimationWindows", null, null);
            if (lst.Count > 0)
                return lst[0];
            else
                return null;
        }

        //public static object Call(string className, string mtd, params object[] ps)
        //{
        //    Type t = typeof(AssetDatabase);
        //    Dbg.Log(t);
        //    //Dbg.Assert(t != null, "failed to get class: {0}", className);
        //    //MethodInfo method
        //    //     = t.GetMethod(mtd, BindingFlags.Static | BindingFlags.Public);
        //    //Dbg.Assert(method != null, "failed to get method: {0}", mtd); 

        //    //return method.Invoke(null, ps);
        //    return null;
        //}

        public static void StartInputModalWindow(Action<string> onSuccess, Action onCancel, string prompt = "Input", string title = "", Texture2D bg = null)
        {
            InputModalWindow wndctrl = new InputModalWindow(onSuccess, onCancel, title, prompt, bg);
            GUIWindowMgr.Instance.Add(wndctrl);
        }

        public static void StartObjRefModalWindow(Action<Object> onSuccess, Action onCancel, Type tp, string prompt = "Object Reference", Texture2D bg = null)
        {
            if( tp == null )
                tp = typeof(Object);

            ObjectRefModalWindow wndctrl = new ObjectRefModalWindow(onSuccess, onCancel, tp, prompt, bg);
            GUIWindowMgr.Instance.Add(wndctrl);
        }
    }

    /// <summary>
    /// this is a default modal window used to get a object reference
    /// </summary>
    public class ObjectRefModalWindow : GUIWindow
    {
        private Action<UnityEngine.Object> m_onSuccess;
        private Action m_onCancel;
        private string m_Prompt = "Select An Object";
        private Texture2D m_background = null;
        private Type m_ObjType = null;

        private Object m_curInput = null;
        private State m_State = State.NONE;

        public ObjectRefModalWindow(Action<UnityEngine.Object> onSuccess, Action onCancel)            
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
            m_ObjType = typeof(UnityEngine.Object);
        }
        public ObjectRefModalWindow(Action<UnityEngine.Object> onSuccess, Action onCancel, 
            Type objType, string prompt, Texture2D bg)
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
            m_ObjType = objType;
            m_Prompt = prompt;
            m_background = bg;
        }

        public override EReturn OnGUI()
        {
            Rect rc = new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 50f, 300, 60);

            //GUI.ModalWindow(m_Index, rc, _Draw, m_Title);
            EUtil.PushGUIEnable(true);

            if (m_background != null)
                GUI.DrawTexture(rc, m_background);
            GUILayout.BeginArea(rc);
            {
                _Draw();
            }
            GUILayout.EndArea();

            EUtil.PopGUIEnable();

            if (m_State == State.OK)
            {
                if (m_onSuccess != null)
                    m_onSuccess(m_curInput);
                return EReturn.STOP;
            }
            else if (m_State == State.CANCEL)
            {
                if (m_onCancel != null)
                    m_onCancel();
                return EReturn.STOP;
            }

            return EReturn.MODAL;
        }

        private void _Draw()
        {
            GUILayout.Label(m_Prompt);

            m_curInput = EditorGUILayout.ObjectField(m_curInput, m_ObjType, true);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OK"))
                {
                    m_State = State.OK;
                }
                if (GUILayout.Button("Cancel"))
                {
                    m_State = State.CANCEL;
                }
            }
            GUILayout.EndHorizontal();
        }

        private enum State
        {
            NONE,
            OK,
            CANCEL,
        }
    }

    /// <summary>
    /// this is a default modal window used to get a text input
    /// </summary>
    public class InputModalWindow : GUIWindow
    {
        private Action<string> m_onSuccess;
        private Action m_onCancel;
        private string m_Title = "Input Modal Window";
        private string m_Prompt = "Input:";
        private Texture2D m_background = null;

        private string m_curInput = string.Empty;
        private State m_State = State.NONE;

        public InputModalWindow(Action<string> onSuccess, Action onCancel)
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
        }
        public InputModalWindow(Action<string> onSuccess, Action onCancel, string title, string prompt, Texture2D bg)
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
            m_Title = title;
            m_Prompt = prompt;
            m_background = bg;
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        public string Prompt
        {
            get { return m_Prompt; }
            set { m_Prompt = value; }
        }

        public override EReturn OnGUI()
        {
            Rect rc = new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 50f, 300, 60);

            //GUI.ModalWindow(m_Index, rc, _Draw, m_Title);
            EUtil.PushGUIEnable(true);

            if (m_background != null)
                GUI.DrawTexture(rc, m_background);
            GUILayout.BeginArea(rc);
            {
                _Draw();
            }
            GUILayout.EndArea();

            EUtil.PopGUIEnable();

            if (m_State == State.OK)
            {
                if (m_onSuccess != null)
                    m_onSuccess(m_curInput);
                return EReturn.STOP;
            }
            else if (m_State == State.CANCEL)
            {
                if (m_onCancel != null)
                    m_onCancel();
                return EReturn.STOP;
            }

            return EReturn.MODAL;
        }

        private void _Draw()
        {
            GUILayout.Label(m_Prompt);

            m_curInput = GUILayout.TextField(m_curInput);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OK"))
                {
                    m_State = State.OK;
                }
                if (GUILayout.Button("Cancel"))
                {
                    m_State = State.CANCEL;
                }
            }
            GUILayout.EndHorizontal();

            //Rect rc = new Rect(0, 0, Screen.width, Screen.height);
            //GUI.DrawTexture(rc, EditorGUIUtility.whiteTexture);
            //if( GUI.Button(rc, "XXSDFSDF") )
            //{
            //    Dbg.Log("xxx");
            //}
            //else
            //{
            //    Dbg.Log("yyy");
            //}
        }

        private enum State
        {
            NONE,
            OK,
            CANCEL,
        }
    }

}
