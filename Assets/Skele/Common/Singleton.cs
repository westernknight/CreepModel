using System;
using UnityEngine;

namespace MH
{

    /// <summary>
    /// Singleton class is base class for all singletons
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>, new()
    {

        // the static instance, you know...
        private static T sm_instance = null;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when the instance not created yet.
        /// </exception>
        public static T Instance
        {
            get
            {
                if (sm_instance == null)
                {
                    Dbg.LogErr("Singleton.sm_instance is null: " + typeof(T).Name);
                }
                return sm_instance;
            }
        }

        /// <summary>
        /// check if already created
        /// </summary>
        public static bool HasInst
        {
            get { return sm_instance != null; }
        }

        /// <summary>
        /// Awake this instance.
        /// create the instance
        /// </summary>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when instance already created
        /// </exception>
        public void Awake()
        {
            if (null != sm_instance)
            {
                throw new InvalidOperationException("Instance already exists: " + typeof(T).ToString());
            }

            sm_instance = this as T;
            Dbg.Log("Singleton.Awake: {0}", typeof(T).Name);
            sm_instance.Init();
        }

        /// <summary>
        /// call the callback Fini
        /// </summary>
        public void OnDestroy()
        {
            Fini();
        }

        /// <summary>
        /// Init this instance. for subclasses
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// destroy callback
        /// </summary>
        public virtual void Fini() { }



        /// <summary>
        /// Destroy this instance.
        /// </summary>	
        public static void Destroy()
        {
            Dbg.Log("Singleton.Destroy: {0}", typeof(T).Name);
            sm_instance = null;
        }
    }

    /// <summary>
    /// this class will not inherit behaviour
    /// </summary>
    public class NonBehaviourSingleton<T> where T : NonBehaviourSingleton<T>, new()
    {
        // the static instance, you know...
        private static T sm_instance = null;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when the instance not created yet.
        /// </exception>
        public static T Instance
        {
            get
            {
                if (sm_instance == null)
                {
                    Dbg.LogErr("NonBehaviourSingleton.sm_instance is null: " + typeof(T).Name);
                }
                return sm_instance;
            }
        }

        /// <summary>
        /// check if already created
        /// </summary>
        public static bool HasInst
        {
            get { return sm_instance != null; }
        }

        /// <summary>    
        /// create the instance
        /// </summary>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when instance already created
        /// </exception>
        public static void Create()
        {
            if (null != sm_instance)
            {
                throw new InvalidOperationException("Instance already exists: " + typeof(T).ToString());
            }

            sm_instance = new T();
            //Dbg.Log("NonBehaviourSingleton.Create: {0}", typeof(T).Name);
            sm_instance.Init();
        }

        /// <summary>
        /// Init this instance. for subclasses
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// destroy callback
        /// </summary>
        public virtual void Fini() { }

        /// <summary>
        /// Destroy this instance.
        /// </summary>	
        public static void Destroy()
        {
            if (sm_instance != null)
            {
                Dbg.Log("NonBehaviourSingleton.Destroy: {0}", typeof(T).Name);

                sm_instance.Fini();
                sm_instance = null;
            }
        }
    }

}