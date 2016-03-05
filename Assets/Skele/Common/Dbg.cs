#define ALLOW_LOG

using UnityEngine;
using System.Collections.Generic;
using System;

using Object = UnityEngine.Object;
using System.Text;
using System.Collections;

public class Dbg {

    #region Log functions

    public static void Log<T>(T msg)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            Debug.Log(msg);             
        }
#endif
    }

    public static void Log<T1, T2>(T1 msg, T2 context) where T2 : Object
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            Debug.Log(msg, context);
        }
#endif
    }

    public static void Log<T>(string fmt, T par1)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            string msg = string.Format(fmt, par1);
            Dbg.Log(msg);
        }
#endif
    }

    public static void Log<T1, T2>(string fmt, T1 par1, T2 par2)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            string msg = string.Format(fmt, par1, par2);
            Dbg.Log(msg);
        }
#endif
    }

    public static void Log<T1, T2, T3>(string fmt, T1 par1, T2 par2, T3 par3)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            string msg = string.Format(fmt, par1, par2, par3);
            Dbg.Log(msg);
        }
#endif
    }

    public static void Log<T1, T2, T3, T4>(string fmt, T1 par1, T2 par2, T3 par3, T4 par4)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            string msg = string.Format(fmt, par1, par2, par3, par4);
            Dbg.Log(msg);
        }
#endif
    }

    public static void Log<T1, T2, T3, T4, T5>(string fmt, T1 par1, T2 par2, T3 par3, T4 par4, T5 par5)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            string msg = string.Format(fmt, par1, par2, par3, par4, par5);
            Dbg.Log(msg);
        }
#endif
    }

    public static void Log<T1, T2, T3, T4, T5, T6>(string fmt, T1 par1, T2 par2, T3 par3, T4 par4, T5 par5, T6 par6)
    {
#if ALLOW_LOG
        if (Debug.isDebugBuild)
        {
            string msg = string.Format(fmt, par1, par2, par3, par4, par5, par6);
            Dbg.Log(msg);
        }
#endif
    }

    public static void LogWarn<T>(T msg)
    {
        Debug.LogWarning(msg);
    }

    public static void LogWarn<T1, T2>(T1 msg, T2 context) where T2 : Object
    {
        Debug.LogWarning(msg, context);
    }

    public static void LogWarn<T1>(string fmt, T1 par1)
    {
        string msg = string.Format(fmt, par1);
        Dbg.LogWarn(msg);
    }

    public static void LogWarn<T1, T2>(string fmt, T1 par1, T2 par2)
    {
        string msg = string.Format(fmt, par1, par2);
        Dbg.LogWarn(msg);
    }

    public static void LogWarn<T1, T2, T3>(string fmt, T1 par1, T2 par2, T3 par3)
    {
        string msg = string.Format(fmt, par1, par2, par3);
        Dbg.LogWarn(msg);
    }

    public static void LogWarn<T1, T2, T3, T4>(string fmt, T1 par1, T2 par2, T3 par3, T4 par4)
    {
        string msg = string.Format(fmt, par1, par2, par3, par4);
        Dbg.LogWarn(msg);
    }
    
    public static void LogErr<T>(T msg)
    {
        Debug.LogError(msg);
#if !UNITY_EDITOR
        //Application.Quit();
#else
        Debug.Break();
#endif
    }

    public static void LogErr<T1, T2>(T1 msg, T2 context) where T2 : Object
    {
        Debug.LogError(msg, context);
#if !UNITY_EDITOR
        //Application.Quit();
#else
        Debug.Break();
#endif
    }

    public static void LogErr<T1>(string fmt, T1 par1)
    {
        string msg = string.Format(fmt, par1);
        Dbg.LogErr(msg);
    }

    public static void LogErr<T1, T2>(string fmt, T1 par1, T2 par2)
    {
        string msg = string.Format(fmt, par1, par2);
        Dbg.LogErr(msg);
    }

    public static void LogErr<T1, T2, T3>(string fmt, T1 par1, T2 par2, T3 par3)
    {
        string msg = string.Format(fmt, par1, par2, par3);
        Dbg.LogErr(msg);
    }

    public static void LogErr<T1, T2, T3, T4>(string fmt, T1 par1, T2 par2, T3 par3, T4 par4)
    {
        string msg = string.Format(fmt, par1, par2, par3, par4);
        Dbg.LogErr(msg);
    }

    public static void LogErr<T1, T2, T3, T4, T5>(string fmt, T1 par1, T2 par2, T3 par3, T4 par4, T5 par5)
    {
        string msg = string.Format(fmt, par1, par2, par3, par4, par5);
        Dbg.LogErr(msg);
    }

    #endregion log functinos

    #region Stack functions
    public string GetStack()
    {
        return System.Environment.StackTrace;
    }

    public void LogStack()
    {
        if(Debug.isDebugBuild)
        {
            Debug.Log(GetStack());
        }
    }

    // log the stack if condition is false
    public void LogStack(bool bCond)
    {
        if (! bCond) 
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log(GetStack());
            }
        }
    }

    #endregion

    #region Assert functions

    public static void Assert<T1>(bool cond, T1 msg)
    {
        if (cond)
            return;

        Dbg.LogErr(msg);
    }

    public static void Assert<T1>( bool cond, string fmt, T1 par1)
    {
        if (cond)
            return;        

        Dbg.LogErr(fmt, par1);
    }

    public static void Assert<T1, T2>(bool cond, string fmt, T1 par1, T2 par2)
    {
        if (cond)
            return;

        Dbg.LogErr(fmt, par1, par2);
    }

    public static void Assert<T1, T2, T3>( bool cond, string fmt, T1 par1, T2 par2, T3 par3)
    {
        if (cond)
            return;

        Dbg.LogErr(fmt, par1, par2, par3);
    }

    public static void Assert<T1, T2, T3, T4>(bool cond, string fmt, T1 par1, T2 par2, T3 par3, T4 par4)
    {
        if (cond)
            return;

        Dbg.LogErr(fmt, par1, par2, par3, par4);
    }

    public static void Assert<T1, T2, T3, T4, T5>(bool cond, string fmt, T1 par1, T2 par2, T3 par3, T4 par4, T5 par5)
    {
        if (cond)
            return;

        Dbg.LogErr(fmt, par1, par2, par3, par4, par5);
    }

    #endregion

    

}
