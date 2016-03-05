using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using DelegateDict = System.Collections.Generic.Dictionary<string, System.Delegate>;
using TypeDict = System.Collections.Generic.Dictionary<string, System.Type>;
using System.Linq.Expressions;
using System.Text;

namespace MH
{

/// <summary>
/// To make reflection calls easier
/// </summary>
public class RCall 
{
	#region "data"
    // data

    private static DelegateDict ms_deleDict = new DelegateDict();
    private static TypeDict ms_typeDict = new TypeDict();

    #endregion "data"

	#region "public method"
    // public method

    /// <summary>
    /// given a type string, find in all assemblies for that type
    /// if not found, return null
    /// </summary>
    public static Type GetTypeFromString(string className, bool bSilent = false)
    {
        Type t;
        if (ms_typeDict.TryGetValue(className, out t))
        {
            return t;
        }
        else
        {
            // enum all assembly to find the class
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = assembly.GetType(className);
                if (t != null)
                {
                    ms_typeDict[className] = t;
                    return t;
                }
            }

            if( ! bSilent )
                Dbg.LogErr("RCall.GetTypeFromString: failed to find class: {0}", className);
            return null;
        }
    }
    
    public static object CallMtd(string className, string mtdName, object inst, params object[] ps)
    {
        string combineName = className + "|" + mtdName;
        Delegate dele = null;
        object ret = null;
        if (ms_deleDict.TryGetValue(combineName, out dele))
        {
            MethodInfo mi = dele.Method;
            return mi.Invoke(inst, ps);
        }
        else
        {
            //Dbg.Log("RCall.CallMtd: try creating new delegate for: {0}.{1}", className, mtdName);

            Type t = GetTypeFromString(className);
            if (null == t)
            {
                Dbg.LogErr("RCall.CallMtd: failed to find className: {0}", className);
            }

            MethodInfo mi = t.GetMethod(mtdName, InstFlags);
            if (null == mi)
            {
                mi = t.GetMethod(mtdName, StaticFlags);
                if( null == mi )
                {
                    Dbg.LogErr("RCall.CallMtd: failed to GetMethod \"{0}\" for class \"{1}\"", mtdName, className);
                    return null;
                }
            }

            ret = mi.Invoke(inst, ps);

            dele = Delegate.CreateDelegate(_GetDelegateTypeFromMethodInfo(mi), mi);
            ms_deleDict[combineName] = dele;

            return ret;

        }
    }

    /// <summary>
    /// can only call public method / public static method
    /// call overloaded method with specified parameters types
    /// </summary>
    public static object CallMtd1(string className, string mtdName, Type[] types, object inst, params object[] ps)
    {
        StringBuilder bld = new StringBuilder();
        bld.AppendFormat("{0}|{1}", className, mtdName);
        for(int idx =0; idx < types.Length; ++idx)
        {
            bld.AppendFormat("|{0}", types[idx].ToString());
        }
        string combineName = bld.ToString();

        Delegate dele = null;
        object ret = null;
        if (ms_deleDict.TryGetValue(combineName, out dele))
        {
            MethodInfo mi = dele.Method;
            return mi.Invoke(inst, ps);
        }
        else
        {
            //Dbg.Log("RCall.CallMtd: try creating new delegate for: {0}.{1}", className, mtdName);

            Type t = GetTypeFromString(className);
            if (null == t)
            {
                Dbg.LogErr("RCall.CallMtd: failed to find className: {0}", className);
            }

            MethodInfo mi = t.GetMethod(mtdName, types);
            if (null == mi)
            {
                mi = t.GetMethod(mtdName, StaticFlags);
                if (null == mi)
                {
                    Dbg.LogErr("RCall.CallMtd: failed to GetMethod \"{0}\" for class \"{1}\"", mtdName, className);
                    return null;
                }
            }

            ret = mi.Invoke(inst, ps);

            dele = Delegate.CreateDelegate(_GetDelegateTypeFromMethodInfo(mi), mi);
            ms_deleDict[combineName] = dele;

            return ret;

        }
    }

    public static object GetField(string className, string fieldName, object inst)
    {
        Type t = GetTypeFromString(className);

        // first public then non-pub
        FieldInfo fi = t.GetField(fieldName, InstFlags);
        if( null == fi )
        {
            fi = t.GetField(fieldName, StaticFlags);
            if( null == fi )
            {
                Dbg.LogErr("RCall.GetField: failed to get \"{0}.{1}\"", className, fieldName);
                return null;
            }
        }

        object ret = fi.GetValue(inst);

        return ret;
    }

    public static void SetField(string className, string fieldName, object inst, object val)
    {
        Type t = GetTypeFromString(className);

        // first public then non-pub
        FieldInfo fi = t.GetField(fieldName, InstFlags);
        if (null == fi)
        {
            fi = t.GetField(fieldName, StaticFlags);
            if (null == fi)
            {
                Dbg.LogErr("RCall.SetField: failed to get \"{0}.{1}\"", className, fieldName);
            }
        }

        fi.SetValue(inst, val);
    }

    public static object GetProp(string className, string propName, object inst)
    {
        string combineName = className + "|" + propName + "|get";
        Delegate dele = null;
        object ret = null;
        if (ms_deleDict.TryGetValue(combineName, out dele))
        {
            MethodInfo mi = dele.Method;
            return mi.Invoke(inst, null);
        }
        else
        {
            //Dbg.Log("RCall.GetProp: try creating new delegate for: {0}.{1}", className, propName);

            Type t = GetTypeFromString(className);
            if (null == t)
            {
                Dbg.LogErr("RCall.GetProp: failed to find className: {0}", className);
            }

            PropertyInfo pi = t.GetProperty(propName, InstFlags);
            if( pi == null )
            {
                pi = t.GetProperty(propName, StaticFlags);
                if( null == pi )
                {
                    Dbg.LogErr("RCall.GetProp: failed to get prop \"{0}.{1}\"", className, propName);
                    return null;
                }
            }

            MethodInfo mi = pi.GetGetMethod();
            ret = mi.Invoke(inst, null);

            dele = Delegate.CreateDelegate(_GetDelegateTypeFromMethodInfo(mi), mi);
            ms_deleDict.Add(combineName, dele);

            return ret;
        }
    }

    public static object SetProp(string className, string propName, object inst, object val)
    {
        string combineName = className + "|" + propName + "|set";
        Delegate dele = null;
        object ret = null;
        if (ms_deleDict.TryGetValue(combineName, out dele))
        {
            MethodInfo mi = dele.Method;
            return mi.Invoke(inst, null);
        }
        else
        {
            //Dbg.Log("RCall.SetProp: try creating new delegate for: {0}.{1}", className, propName);

            Type t = GetTypeFromString(className);
            if (null == t)
            {
                Dbg.LogErr("RCall.SetProp: failed to find className: {0}", className);
            }

            PropertyInfo pi = t.GetProperty(propName, InstFlags);
            if (pi == null)
            {
                pi = t.GetProperty(propName, StaticFlags);
                if (null == pi)
                {
                    Dbg.LogErr("RCall.SetProp: failed to get prop \"{0}.{1}\"", className, propName);
                    return null;
                }
            }

            MethodInfo mi = pi.GetSetMethod();
            ret = mi.Invoke(inst, new object[]{val});

            dele = Delegate.CreateDelegate(_GetDelegateTypeFromMethodInfo(mi), mi);
            ms_deleDict.Add(combineName, dele);

            return ret;
        }
    }

    #endregion "public method"

	#region "private method"
    // private method

    

    /// <summary>
    /// given the MethodInfo instance, return the delegate Type
    /// </summary>
    /// <param name="mi"></param>
    /// <returns></returns>
    private static Type _GetDelegateTypeFromMethodInfo(MethodInfo mi)
    {
        Type delegateType;
        var typeArgs = mi.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToList();
        if( !mi.IsStatic )
            typeArgs.Insert(0, mi.DeclaringType);

        // build Action/Func delegate type
        if (mi.ReturnType == typeof(void))
        {
            delegateType = Expression.GetActionType(typeArgs.ToArray());
        }
        else
        {
            typeArgs.Add(mi.ReturnType);
            delegateType = Expression.GetFuncType(typeArgs.ToArray());
        }

        return delegateType;
    }

    #endregion "private method"

	#region "constant data"
    // constant data

    public const BindingFlags InstFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    #endregion "constant data"




}

}
