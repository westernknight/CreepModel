//#define HAS_ZIP
//#define HAS_EVTSET
#define HAS_JSON
//#define HAS_MAPUTIL
//#define HAS_POOL

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if HAS_ZIP
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
#endif

#if HAS_JSON
using LitJson;
#endif

#if HAS_MAPUTIL
using Pathfinding;
using Path = System.IO.Path;
#endif

namespace MH
{
    /// <summary>
    /// some methods for Vector3
    /// </summary>
    public class V3Ext
    {
        public static void ReplaceXY(ref Vector3 v, float x, float y)
        {
            v.x = x;
            v.y = y;
        }
        public static void ReplaceXY2(ref Vector3 v3, ref Vector2 v2)
        {
            v3.x = v2.x;
            v3.y = v2.y;
        }
        public static void ReplaceXY(ref Vector3 v3, ref Vector3 src)
        {
            v3.x = src.x;
            v3.y = src.y;
        }
        public static void ReplaceXZ(ref Vector3 v3, float x, float z)
        {
            v3.x = x;
            v3.z = z;
        }
        public static void ReplaceXZ2(ref Vector3 v3, ref Vector2 v2)
        {
            v3.x = v2.x;
            v3.z = v2.y;
        }
        public static void ReplaceXZ(ref Vector3 v3, ref Vector3 src)
        {
            v3.x = src.x;
            v3.y = src.y;
        }

        public static Vector3 MultiplyComp(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 DivideComp(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }

    public class Misc
    {

        public static string
        GetTransformPath(Transform tr)
        {
            StringBuilder bld = new StringBuilder();
            while (tr != null)
            {
                if (bld.Length != 0)
                    bld.Insert(0, '/');
                bld.Insert(0, tr.name);
                tr = tr.parent;
            }
            return bld.ToString();
        }

        /// <summary>
        /// use GameObject to Find specified GO,
        /// if not found, create one
        /// </summary>
        public static GameObject
        ForceGetGO(string gopath, GameObject baseGO = null)
        {
            if (gopath == null || gopath.Length == 0)
                return null;
             
            GameObject go = null;
            Transform tr = baseGO == null ? null : baseGO.transform;

            string[] paths = gopath.Split('/');
            for (int idx = 0; idx < paths.Length; ++idx)
            {
                go = _ForceGetGO(paths[idx], tr);
                tr = go.transform;
            }

            Dbg.Assert(go != null, "Misc.ForceGetGO: failed to get go for path: {0}", gopath);

            return go;
        }

        private static GameObject
        _ForceGetGO(string onePath, Transform baseTR)
        {
            if( baseTR == null )
            {
                string pureName = onePath;
                if (pureName.StartsWith("/"))
                    pureName = pureName.Substring(1);

                GameObject go = GameObject.Find("/" + pureName);
                if (go == null)
                    go = new GameObject(pureName);

                return go;
            }
            else
            {
                Transform tr = baseTR.Find(onePath);
                if( tr == null )
                {
                    GameObject go = new GameObject(onePath);
                    Misc.AddChild(baseTR, go);
                    return go;
                }
                else
                {
                    return tr.gameObject;
                }
            }
        }

        /// <summary>
        /// add `child' as children of `parent'
        /// </summary>
        public static void
        AddChild(GameObject parent, GameObject child, bool keepLocalTr = false)
        {
            AddChild(parent.transform, child.transform, keepLocalTr);
        }        
        public static void
        AddChild(Transform parent, GameObject child, bool keepLocalTr = false )
        {
            AddChild(parent, child.transform, keepLocalTr);
        }
        public static void
        AddChild(Transform parent, Transform child, bool keepLocalTr = false)
        {
            XformData data = null;
            if( keepLocalTr )
            {
                data = new XformData();
                data.CopyFrom(child);
            }
            child.parent = parent;
            if( keepLocalTr )
            {
                data.Apply(child);
            }
        }

        /// <summary>
        /// convert a byte[] to a Base64 string
        /// </summary>
        public static string ToBase64(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }
        public static byte[] FromBase64(string es)
        {
            return System.Convert.FromBase64String(es);
        }

        /// <summary>
        /// count the consecutive zero bits on the right side
        /// </summary>
        public static int CountZeroOnRight(uint v)
        {
            int c = 32;
            if (v != 0) c--;
            if ((v & 0x0000FFFF) != 0) { c -= 16; v &= 0x0000FFFF; }
            if ((v & 0x00FF00FF) != 0) { c -= 8; v &= 0x00FF00FF; }
            if ((v & 0x0F0F0F0F) != 0) { c -= 4; v &= 0x0F0F0F0F; }
            if ((v & 0x33333333) != 0) { c -= 2; v &= 0x33333333; }
            if ((v & 0x55555555) != 0) { c -= 1; v &= 0x55555555; }

            return c;
        }

        /// <summary>
        /// swap two elements in a IList<T>
        /// </summary>
        public static void Swap<T>(IList<T> lst, int idx1, int idx2)
        {
            T tmp = lst[idx1];
            lst[idx1] = lst[idx2];
            lst[idx2] = tmp;
        }

        /// <summary>
        /// given a rotation radian(-inf, inf),
        /// transform it to [0, 2*PI]
        /// </summary>
        public static float NormalizeRotation(float radian)
        {
            float pi2 = Mathf.PI * 2f;
            float r = radian % pi2;
            r = (r < 0) ? (r + pi2) : r;
            return r;
        }

        /// <summary>
        /// given a rotation angle (-inf, inf)
        /// transform it to [0, 360]
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeAngle(float angle)
        {
            float pi2 = 360f;
            float r = angle % pi2;
            r = (r < 0) ? (r + pi2) : r;
            return r;
        }

        /// <summary>
        /// get the distance of two angles
        /// </summary>
        public static float AngleDist(float lhs, float rhs)
        {
            float a = Misc.NormalizeAngle(lhs);
            float b = Misc.NormalizeAngle(rhs);
            float d1 = Mathf.Abs(a - b);
            float d2 = Mathf.Abs(a - b - 360f);
            float d3 = Mathf.Abs(a - b + 360f);

            return Mathf.Min(d3, Mathf.Min(d1, d2));
        }

    }

    /// <summary>
    /// data conversion
    /// </summary>
    public class DataUtil
    {
        public static int ParseAsInt(string s, int def = 0)
        {
            int ret = 0;
            bool bOk = Int32.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static uint ParseAsUint(string s, uint def = 0)
        {
            uint ret = 0;
            bool bOk = UInt32.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static float ParseAsFloat(string s, float def = 0)
        {
            float ret = 0;
            bool bOk = Single.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static double ParseAsDouble(string s, double def = 0)
        {
            double ret = 0;
            bool bOk = Double.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static T ParseAsEnum<T>(string s, Type eType)
        {
            try{
                T ret = (T)Enum.Parse(eType, s);
                return ret;
            }catch(ArgumentException){
                Dbg.LogWarn("DataUtil.ParseAsEnum: failed to parse {0} as {1}", s, eType.ToString());
                return default(T);
            }
        }
    }

    #if HAS_ZIP
    /// <summary>
    /// utility about zip/unzip
    /// </summary>
    public class ZUtil
    {
        public static void ZipFile(string inputFile, string outputFile)
        {
            Dbg.Assert(File.Exists(inputFile), "ZUtil.ZipFile: the inputfile not exist: {0}", inputFile);

            FastZip fzip = new FastZip();
            fzip.CreateZip(outputFile, Path.GetDirectoryName(inputFile), true, Path.GetFileName(inputFile));
        }

        public static void ZipDir(string inputDir, string outputFile)
        {
            Dbg.Assert(Directory.Exists(inputDir), "ZUtil.ZipDir: the inputDir not exist: {0}", inputDir);

            FastZip fzip = new FastZip();
            fzip.CreateZip(outputFile, inputDir, true, null);
        }

        public static void UnzipFile(string inputFile, string outBaseDir)
        {
            Dbg.Assert(File.Exists(inputFile), "ZUtil.UnzipFile: the inputFile not exist: {0}", inputFile);

            FastZip fzip = new FastZip();
            if (!Directory.Exists(outBaseDir))
            {
                Directory.CreateDirectory(outBaseDir);
            }
            fzip.ExtractZip(inputFile, outBaseDir, FastZip.Overwrite.Always, null, null, null, false);
        }

        public static void UnzipStream(Stream iStream, string outBaseDir)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outBaseDir)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outBaseDir));
            }

            ZipInputStream zipInputStream = new ZipInputStream(iStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                // Optionally match entrynames against a selection list here to skip as desired.
                // The unpacked length is available in the zipEntry.Size property.

                byte[] buffer = new byte[4096];     // 4K is optimum

                // Manipulate the output filename here as desired.
                String fullZipToPath = Path.Combine(outBaseDir, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                // of the file, but does not waste memory.
                // The "using" will close the stream even if an exception occurs.
                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
                zipEntry = zipInputStream.GetNextEntry();
            }

        }

    }
    #endif

#if HAS_POOL
    /// <summary>
    /// alloc/dealloc object by pool
    /// </summary>
    public class Mem
    {
        public static T New<T>() where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            return pool.Spawn();
        }

        public static void Del<T>(T obj) where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            pool.Despawn(obj);
        }

        public static void SetResetAction<T>(Action<T> resetAction) where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            pool.ResetAction = resetAction;
        }

        public static void SetFirstInitAction<T>(Action<T> initAction) where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            pool.InitAction = initAction;
        }

        //public static GameObject NewPrefabGO(string poolName, Transform parent)
        //{
        //    ResPrefabPool<GameObject> pool = PoolMgr.Instance.Get(poolName) as ResPrefabPool<GameObject>;
        //    if (pool == null)
        //    {
        //        pool = new ResPrefabPool<GameObject>(poolName);
        //        PoolMgr.Instance.Add(pool);
        //    }

        //    GameObject obj = pool.Spawn();
        //    obj.transform.parent = parent;

        //    return obj;
        //}

        //public static T NewPrefab<T>(string poolName) where T : UnityEngine.Object
        //{
        //    ResPrefabPool<T> pool = PoolMgr.Instance.Get(poolName) as ResPrefabPool<T>;
        //    if (pool == null)
        //    {
        //        pool = new ResPrefabPool<T>(poolName);
        //        PoolMgr.Instance.Add(pool);
        //    }

        //    T obj = pool.Spawn();

        //    return obj;
        //}

        //public static void DelPrefabGO(GameObject obj)
        //{
        //    PoolTicket ticket = obj.GetComponent<PoolTicket>();
        //    Dbg.Assert(ticket != null, "Mem.DelPrefabGO: the obj doesn't have PoolTicket");
        //    ResPrefabPool<GameObject> pool = PoolMgr.Instance.Get(ticket.PoolName) as ResPrefabPool<GameObject>;
        //    Dbg.Assert(null != pool, "Mem.DelPrefabGO: failed to get pool: {0}", ticket.PoolName);
        //    pool.Despawn(obj);
        //}
    }
#endif

    /// <summary>
    /// get UID in a multi-queue style
    /// </summary>
    public class UID
    {        
        public enum Q{
            CR,  //coroutine            
            Q_CNT
        };

        public static uint[] s_ids = Enumerable.Repeat((uint)0, (int)Q.Q_CNT).ToArray();

        public static uint Get(Q que)
        {
            return s_ids[(int)que]++;
        }
        public static string GetAsString(Q que)
        {
            uint v = Get(que);
            return v.ToString();
        }

        /// <summary>
        /// peek next id of specified queue
        /// </summary>
        public static uint Peek(Q que)
        {
            return s_ids[(int)que];
        }
    }

    /// <summary>
    /// utility pair
    /// </summary>
    public class Pair<T, U>
    {
        public T first;
        public U second;

        public Pair(T f, U s) { first = f; second = s; }

        public T x { get { return first; } set { first = value; } }
        public U y { get { return second; } set { second = value; } }
    }
    
    /// <summary>
    /// some extra utilities on IO
    /// </summary>
    public class IOUtil
    {
        /// <summary>
        /// write to file, automatically create directories
        /// </summary>
        public static void WriteAllText(string pathName, string content)
        {
#if !UNITY_WEBPLAYER
            Directory.CreateDirectory(Path.GetDirectoryName(pathName));
            File.WriteAllText(pathName, content, Encoding.UTF8);
#endif
        }

        /// <summary>
        /// write to file, automatically create directories
        /// </summary>
        public static void WriteAllBytes(string pathName, byte[] bytes)
        {
#if !UNITY_WEBPLAYER
            Directory.CreateDirectory(Path.GetDirectoryName(pathName));
            File.WriteAllBytes(pathName, bytes);
#endif
        }
    }


    public class PathUtil
    {
        public static string GetDocumentPath(string filename)
        {
            #if UNITY_IPHONE
            return Path.Combine(Application.persistentDataPath, filename);
            #elif UNITY_ANDROID
            return Path.Combine(Application.persistentDataPath, filename);
            #else
            return Path.Combine (Application.persistentDataPath, filename);
            #endif
        }

        /// <summary>
        /// combine the paths, and force use the directory separator used by Resources.Load ('/')
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            string combined = Path.Combine(path1, path2);
            return ForceForwardSlash(combined);
        }

        /// <summary>
        /// strip extension if the path has
        /// </summary>
        public static string StripExtension(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string nameNoExt = Path.GetFileNameWithoutExtension(path);
            if( dir.Length == 0 )
            {
                return nameNoExt;
            }
            else
            {
                return dir + "/" + nameNoExt;
            }
        }

        /// <summary>
        /// convert all "\" to "/" in path
        /// </summary>
        public static string ForceForwardSlash(string path)
        {
            return path.Replace('\\', '/');
        }

        #if UNITY_EDITOR
        /// <summary>
        /// editor used method;
        /// given "XXXXX", return "f:/Project/Game/Assets/Resources/XXXXX"
        /// </summary>
        public static string ResourcesPath2FullPath(string pathName)
        {
            return string.Format("{0}/Resources/{1}", Application.dataPath, pathName);
        }

        /// <summary>
        /// given "XXXXX", return "Assets/Resources/XXXXX"
        /// </summary>
        public static string ResourcesPath2ProjectPath(string pathName)
        {
            return Path.Combine("Assets/Resources", pathName);
        }

        /// <summary>
        /// given "Assets/Resources/XXXXX", return "XXXXX"
        /// </summary>
        public static string ProjectPath2ResourcesPath(string Prjpath)
        {
            Dbg.Assert(Prjpath.StartsWith("Assets/Resources"), "PathUtil.ProjectPath2ResourcesPath: invalid path");
            return Prjpath.Substring("Assets/Resources".Length+1);
        }
        
        /// <summary>
        /// given "f:/Project/Game/Assets/xxx" return "Assets/xxx"
        /// </summary>
        public static string FullPath2ProjectPath(string fullpath)
        {
            Dbg.Assert(fullpath.StartsWith(Application.dataPath), "PathUtil.FullPath2ProjectPath: The path is not in Assets directory: {0}", fullpath);
            return fullpath.Substring(Application.dataPath.Length - 6);
        }

        #endif
    }

    #if HAS_JSON
    public class Json
    {
        private static JsonWriter s_PrettyWriter;

        static Json(){
            s_PrettyWriter = new JsonWriter();
            s_PrettyWriter.PrettyPrint = true;
        }

        /// <summary>
        /// convert object to json string
        /// </summary>
        public static string ToStr(object obj) 
        {
            return ToStr(obj, true);
        }
        public static string ToStr(object obj, bool bIndented)
        {
            if( bIndented )
            {
                s_PrettyWriter.Reset();
                JsonMapper.ToJson(obj, s_PrettyWriter);
                string o = s_PrettyWriter.ToString();
                return o;
            }
            else
            {
                string o = JsonMapper.ToJson(obj);
                return o;
            }
        }

        /// <summary>
        /// convert json string to object
        /// </summary>
        public static T ToObj<T>(string str)
        {
            T o = JsonMapper.ToObject<T>(str);
            return o;
        }

        public static object ToObj(string str, Type t)
        {
            object o = JsonMapper.ToObject(str, t);
            return o;
        }

    }

    /// <summary>
    /// dump object to file
    /// </summary>
    public class Dumper
    {
        public static void Dump<T>(T obj)
        {
            string filename = DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".dump";
            Dump(obj, filename);
        }

        public static void Dump<T>(T obj, string path)
        {
            //string serialized = JsonConvert.SerializeObject(obj, Formatting.Indented);
            string serialized = Json.ToStr(obj, true);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(serialized);
            }
        }

       
    }
    #endif
    
    /// <summary>
    /// used by QUtil.LookAtXXX series function, 
    /// to specify how to calc Quaternion with given parameters
    /// </summary>
    public enum LookAtMtd
    {
        XY, //lookat x, reference y 
        XZ, //lookat x, reference z
        YX, //lookat y, reference x
        YZ, //lookat y, reference z
        ZX, //lookat z, reference x
        ZY  //lookat z, reference y
    }

    public class QUtil
    {
        public static Quaternion LookAt(LookAtMtd mtd, Vector3 lookDir, Vector3 refDir)
        {
            switch (mtd)
            {
                case LookAtMtd.XY: return LookAtXY(lookDir, refDir);
                case LookAtMtd.XZ: return LookAtXZ(lookDir, refDir);
                case LookAtMtd.YX: return LookAtYX(lookDir, refDir);
                case LookAtMtd.YZ: return LookAtYZ(lookDir, refDir);
                case LookAtMtd.ZX: return LookAtZX(lookDir, refDir);
                case LookAtMtd.ZY: return LookAtZY(lookDir, refDir);
                default:
                    Dbg.LogErr("QUtil.LookAt: unexpected method: {0}", mtd);
                    return Quaternion.identity;
            }
        }

        public static Quaternion LookAtZY(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newZ = lookDir;
            Vector3 newY = refDir; //ref
            Vector3 newX; //calc

            if (newZ == Vector3.up && newY == Vector3.up)
            {
                newY = -Vector3.forward; //ref
                newX = Vector3.right; //calc
            }
            else
            {
                newX = Vector3.Cross(newY, newZ); //calc
                if (newX == Vector3.zero)
                {
                    newY = Vector3.up; //ref
                    newX = Vector3.Cross(newY, newZ); //calc
                }
                newY = Vector3.Cross(newZ, newX); //recalc ref
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtZX(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newZ = lookDir;
            Vector3 newX = refDir; //ref
            Vector3 newY; //calc

            if (newZ == Vector3.right && newX == Vector3.right)
            {
                newX = -Vector3.forward; //ref
                newY = Vector3.up; //calc
            }
            else
            {
                newY = Vector3.Cross(newZ, newX); //calc
                if (newY == Vector3.zero)
                {
                    newX = Vector3.right; //ref
                    newY = Vector3.Cross(newZ, newX); //calc
                }
                newX = Vector3.Cross(newY, newZ); //recalc ref
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtXY(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newX = lookDir;
            Vector3 newY = refDir;
            Vector3 newZ; //calc-vec

            if (newX == Vector3.up && newY == Vector3.up)
            {
                newY = -Vector3.forward; //ref
                newZ = -Vector3.right; //calc
            }
            else
            {
                newZ = Vector3.Cross(newX, newY); //calc
                if (newZ == Vector3.zero)
                {
                    newY = Vector3.up; //ref
                    newZ = Vector3.Cross(newX, newY); //calc
                }
                newY = Vector3.Cross(newZ, newX); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtXZ(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newX = lookDir;
            Vector3 newZ = refDir;
            Vector3 newY; //calc-vec

            if (newX == Vector3.forward && newZ == Vector3.forward)
            {
                newZ = -Vector3.right; //ref-vec
                newY = Vector3.up; //calc-vec
            }
            else
            {
                newY = Vector3.Cross(newZ, newX); //calc-vec
                if (newY == Vector3.zero)
                {
                    newZ = Vector3.forward; //ref-vec
                    newY = Vector3.Cross(newZ, newX); //calc-vec
                }
                newZ = Vector3.Cross(newX, newY); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtYX(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newY = lookDir;
            Vector3 newX = refDir;
            Vector3 newZ; //calc-vec

            if (newY == Vector3.right && newX == Vector3.right)
            {
                newX = Vector3.down; //ref-vec
                newZ = Vector3.forward; //calc-vec
            }
            else
            {
                newZ = Vector3.Cross(newX, newY); //calc-vec
                if (newZ == Vector3.zero) //calc-vec
                {
                    newX = Vector3.right; //ref-vec
                    newZ = Vector3.Cross(newX, newY); //calc-vec
                }
                newX = Vector3.Cross(newY, newZ); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtYZ(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newY = lookDir;
            Vector3 newZ = refDir;
            Vector3 newX; //calc-vec

            if (newY == Vector3.forward && newZ == Vector3.forward)
            {
                newZ = Vector3.down; //ref-vec
                newX = Vector3.right; //calc-vec
            }
            else
            {
                newX = Vector3.Cross(newY, newZ); //calc-vec
                if (newX == Vector3.zero) //calc-vec
                {
                    newZ = Vector3.forward; //ref-vec
                    newX = Vector3.Cross(newY, newZ); //calc-vec
                }
                newZ = Vector3.Cross(newX, newY); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }
        
        /// <summary>
        /// make quaternion's fwd and up vector to reflect along (1,0,0)
        /// </summary>
        public static Quaternion ReflectX_YZ(Quaternion q)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 up = q * Vector3.up;

            Vector3 newFwd = Vector3.Reflect(fwd, Vector3.right);
            Vector3 newUp = Vector3.Reflect(up, Vector3.right);

            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion ReflectX_XZ(Quaternion q)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 right = q * Vector3.right;

            Vector3 newFwd = Vector3.Reflect(fwd, Vector3.right);
            Vector3 newRight = Vector3.Reflect(right, Vector3.right);

            Vector3 newUp = Vector3.Cross(newFwd, newRight);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion ReflectX_XY(Quaternion q)
        {
            Vector3 right = q * Vector3.right;
            Vector3 up = q * Vector3.up;

            Vector3 newRight = Vector3.Reflect(right, Vector3.right);
            Vector3 newUp = Vector3.Reflect(up, Vector3.right);

            Vector3 newFwd = Vector3.Cross(newRight, newUp);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Reflect_YZ(Quaternion q, Vector3 planeNormal)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 up = q * Vector3.up;

            Vector3 newFwd = Vector3.Reflect(fwd, planeNormal);
            Vector3 newUp = Vector3.Reflect(up, planeNormal);

            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Reflect_XZ(Quaternion q, Vector3 planeNormal)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 right = q * Vector3.right;

            Vector3 newFwd = Vector3.Reflect(fwd, planeNormal);
            Vector3 newRight = Vector3.Reflect(right, planeNormal);

            Vector3 newUp = Vector3.Cross(newFwd, newRight);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Reflect_XY(Quaternion q, Vector3 planeNormal)
        {
            Vector3 right = q * Vector3.right;
            Vector3 up = q * Vector3.up;

            Vector3 newRight = Vector3.Reflect(right, planeNormal);
            Vector3 newUp = Vector3.Reflect(up, planeNormal);

            Vector3 newFwd = Vector3.Cross(newRight, newUp);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Normalize(Quaternion rot)
        {
            float norm = Mathf.Sqrt(rot.x * rot.x + rot.y * rot.y + rot.z * rot.z + rot.w * rot.w);
            Dbg.Assert(norm != 0, "QUtil.Normalize: the given Quaternion is all 0");
            rot.x /= norm;
            rot.y /= norm;
            rot.z /= norm;
            rot.w /= norm;

            return rot;
        }
    }

    
}

namespace ExtMethods
{
    public static class DictExt
    {
        /// <summary>
        /// try to get the value of given key, return null if not found
        /// </summary>
        public static V TryGet<K,V>(this Dictionary<K,V> dict, K key) where V : class
        {
            V ret;
            if( dict.TryGetValue(key, out ret))
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// simulate C++ map's operator[], if the key
        /// </summary>
        public static V ForceGet<K,V>(this Dictionary<K,V> dict, K key, V defVal) where V : class
        {
            V ret = null;
            if( dict.TryGetValue(key, out ret) )
            {
                return ret;
            }
            else
            {
                dict.Add(key, defVal);
                return defVal;
            }
        }
        public static V ForceGet<K,V>(this Dictionary<K,V> dict, K key) where V : class
        {
            return ForceGet(dict, key, null);
        }
    }

    public static class TransformExt
    {
        public static void CopyLocal(this Transform tr, Transform from)
        {
            tr.localPosition = from.localPosition;
            tr.localRotation = from.localRotation;
            tr.localScale = from.localScale;
        }

        /// <summary>
        /// scale will still use local
        /// </summary>
        public static void CopyWorld(this Transform tr, Transform from)
        {
            tr.position = from.position;
            tr.rotation = from.rotation;
            tr.localScale = from.localScale;
        }

        /// <summary>
        /// 6 lookat functions
        /// </summary>
        public static void LookAtZY(this Transform tr, Vector3 targetPos, Vector3 newY)
        {
            tr.LookAt(targetPos, newY);
        }
        public static void LookAtZX(this Transform tr, Vector3 targetPos, Vector3 newX)
        {
            Vector3 newZ = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtZX(newZ, newX);
            tr.rotation = q;
        }
        public static void LookAtXY(this Transform tr, Vector3 targetPos, Vector3 newY)
        {
            Vector3 newX = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtXY(newX, newY);
            tr.rotation = q;
        }
        
        public static void LookAtXZ(this Transform tr, Vector3 targetPos, Vector3 newZ)
        {
            Vector3 newX = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtXZ(newX, newZ);
            tr.rotation = q;
        }
        public static void LookAtYX(this Transform tr, Vector3 targetPos, Vector3 newX)
        {
            Vector3 newY = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtYX(newY, newX);
            tr.rotation = q; 
        }
        public static void LookAtYZ(this Transform tr, Vector3 targetPos, Vector3 newZ)
        {
            Vector3 newY = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtYZ(newY, newZ);
            tr.rotation = q; 
        }

        /// <summary>
        /// like Transform.Find, but will not take `/' as hierachy separator.
        /// will just recursive find all children for exact matching name,
        /// if not found, return null
        /// </summary>
        public static Transform FindByName(this Transform tr, string name)
        {
            if (tr.name == name)
                return tr;

            for( int idx = 0; idx < tr.childCount; ++idx )
            {
                Transform ctr = tr.GetChild(idx);
                Transform retTr = FindByName(ctr, name);
                if( retTr != null )
                {
                    return retTr;
                }
            }

            return null;
        }

        /// <summary>
        /// find the first transform that has given tag, and is child or self of given `parent'
        /// </summary>
        public static Transform
        FindWithTag(this Transform parent, string tag)
        {
            GameObject[] withTags = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < withTags.Length; ++i)
            {
                Transform tr = withTags[i].transform;
                if (tr.IsChildOf(parent))
                {
                    return tr;
                }
            }
            return null;
        }

        /// <summary>
        /// find the first transform of self and self's ancestors, which has the given tag
        /// </summary>
        public static Transform
        FindParentWithTag(this Transform self, string tag)
        {
            Transform tr = self;
            while (tr != null )
            {
                if( tr.tag == tag )
                {
                    return tr;
                }
                else
                {
                    tr = tr.parent;
                }
            }
            return null;
        }

    }

    public static class GOExt
    {
        public enum FIELD{NONE = 0, X=1, Y=2, XY=3, Z=4, XZ=5, YZ=6, XYZ=7};
        private static int[] FIELDCNT = {0, 1, 1, 2, 1, 2, 2, 3 };

        public static T ForceGetComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if( comp == null )
            {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        public static void SilentDestroyComponent<T> (this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if( comp != null )
            {
                GameObject.Destroy(comp);
            }
        }

        public static void SetLocalPosition(this GameObject go, FIELD f, params float[] p)
        {
            Vector3 pos = go.transform.localPosition;
            Vector3 oldPos = pos;
            int fieldCnt = FIELDCNT[(uint)f];
            if (fieldCnt == 0)
                return;
            Dbg.Assert(p.Length == fieldCnt, "GOExt.SetLocalPosition: param cnt not fit: {0} != {1}", p.Length, fieldCnt);

            int idx = 0;
            int bits = (int)f;
            if( (bits & (int)FIELD.X) != 0 )
            {
                pos.x = p[idx++];
            }
            if( (bits & (int)FIELD.Y) != 0)
            {
                pos.y = p[idx++];
            }
            if( (bits & (int)FIELD.Z) != 0)
            {
                pos.z = p[idx++];
            }

            if( oldPos == pos )
            {
                go.transform.localPosition = pos;
            }
        }

        public static void SetLocalScale(this GameObject go, FIELD f, params float[] p)
        {
            Transform trans = go.transform;
            Vector3 oldscale = trans.localScale;
            Vector3 scale = oldscale;

            int fieldCnt = FIELDCNT[(uint)f];
            if (fieldCnt == 0)
                return;
            Dbg.Assert(p.Length == fieldCnt, "GOExt.SetLocalScale: param cnt not fit: {0} != {1}", p.Length, fieldCnt);

            int idx = 0;
            int bits = (int)f;
            if ((bits & (int)FIELD.X) != 0)
            {
                scale.x = p[idx++];
            }
            if ((bits & (int)FIELD.Y) != 0)
            {
                scale.y = p[idx++];
            }
            if ((bits & (int)FIELD.Z) != 0)
            {
                scale.z = p[idx++];
            }

            if( scale != oldscale )
            {
                go.transform.localScale = scale;
            }
        }
    }

    public static class ListExt
    {
        public static void AddMany<T>(this List<T> lst, params T[] objs)
        {
            for(int idx = 0; idx < objs.Length; ++idx)
            {
                lst.Add(objs[idx]);
            }
        }

        /// <summary>
        /// enable to get element by negative index
        /// </summary>
        public static T ExtGet<T>(this List<T> lst, int idx)
        {
            if( idx < 0 ) { idx += lst.Count; }
            Dbg.Assert(0 <= idx && idx < lst.Count, "ListExt.ExtGet: invalid idx: {0}, count: {1}", idx, lst.Count);
            return lst[idx];
        }

        public static bool Contains<T>(this T[] arr, T v)
        {
            for( int i=0; i<arr.Length; ++i)
            {
                if( v.Equals(arr[i]) )
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class BitArrayExt
    {
        /// <summary>
        /// test if all value in `arr' are all `bVal'
        /// </summary>
        public static bool IsAll(this BitArray arr, bool bVal)
        {
            for( int idx = 0; idx < arr.Count; ++idx )
            {
                if (bVal != arr.Get(idx))
                    return false;
            }
            return true;
        }
    }
}