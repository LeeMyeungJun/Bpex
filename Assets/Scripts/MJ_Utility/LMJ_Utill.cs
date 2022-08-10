using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Experimental.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

//using fastJSON;
//32-126,44032-55203,12593-12643,8200-9900  font 문자셋 영어 한국어 특수문자
//영어 32-126
//한글 44032-55203
//한글 자모 12593-12643
//특수문자 8200-9900
public class BitFlag
{
    System.Action<int, int> OnChangeUserFlags;

    public BitFlag()
    {
    }
    public BitFlag(System.Action<int, int> _OnChangeUserFlags)
    {
        OnChangeUserFlags = _OnChangeUserFlags;
    }

    protected int m_Flags = 0;
    public int Flags
    {
        get { return m_Flags; }
        set
        {
            if (m_Flags == value)
                return;

            int prev = m_Flags;
            m_Flags = value;

            if (OnChangeUserFlags != null)
                OnChangeUserFlags(prev, m_Flags);
        }
    }
    public int SetUserFlags(int bitFlag)
    {
        if (bitFlag > 31)
        {
            LMJ.LogError("bitFlag>31");
            return Flags;
        }

        int bit = 1 << bitFlag;
        Flags |= bit;
        return Flags;
    }
    public int RemoveUserFlags(int bitFlag)
    {
        int bit = 1 << bitFlag;
        Flags &= ~bit;
        return Flags;
    }
    public bool isSetUserFlags(int bitFlag)
    {
        int bit = 1 << bitFlag;
        return (Flags & bit) != 0;
    }
}

public static class Extension
{
    public static string NameOf(this object o)
    {
        return o.GetType().Name;
    }
}
public class TimeChecker 
{
    System.DateTime _start;
    string _name;
    bool _isUsing = false;
    public TimeChecker(string name)
    {
        Start(name);
    }

    void Start(string name)
    {
        _name = name;
        _isUsing = true;
        _start = System.DateTime.Now;
        if(name != null)
            LMJ.LogWarning("start : " + name);
    }

    public void Print()
    {
        Print(_name);
    }
    public void Print(string name)
    {
        _isUsing = false;
        System.TimeSpan span = System.DateTime.Now - _start;
        LMJ.LogWarning("complete : " + name + " dur : " + span.TotalMilliseconds);
    }


    static List<TimeChecker> list = new List<TimeChecker>();
    static public TimeChecker Get(string name = null)
    {
        for(int i = 0; i < list.Count; i++)
        {
            if (list[i]._isUsing == false)
            {
                list[i].Start(name);
                return list[i];
            }
        }

        TimeChecker n = new TimeChecker(name);
        list.Add(n);
        return n;
    }
}

public class CamInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public float fov;
    public float nearClipPlane;
    public float farClipPlane;


    public float maxExtent;
    public float distanceToObj ;

    public CamInfo(Camera cam)
    {
        Get(cam);
    }

    public void Get(Camera cam)
    {
        position = cam.transform.position;
        rotation = cam.transform.rotation;
        fov = cam.fieldOfView;
        nearClipPlane = cam.nearClipPlane;
        farClipPlane = cam.farClipPlane;
    }

    public void Set(Camera cam)
    {
        cam.transform.position = position;
        cam.transform.rotation = rotation;
        cam.fieldOfView = fov;
        cam.nearClipPlane = nearClipPlane;
        cam.farClipPlane = farClipPlane;
    }
}

public class LMJ_Utill : MonoSingleton<LMJ_Utill>
{
    //static LMJ_Utill _instance = null;
    //public static LMJ_Utill Get()
    //{
    //    if (_instance == null) 
    //    {
    //        GameObject go = new GameObject("LMJ_Utill");
    //        GameObject.DontDestroyOnLoad(go);
    //    }
    //    return _instance;
    //}


    public static T GetComponentRecursively<T>(GameObject from) where T : Component
    {
        T comp = from.GetComponent<T>();
        if (comp != null)
            return comp;

        for (int n = from.transform.childCount - 1; 0 <= n; n--)
        {
            if (n < from.transform.childCount)
            {
                GameObject go = from.transform.GetChild(n).gameObject;

                comp = go.GetComponent<T>();
                if (comp != null)
                    return comp;

                comp = LMJ_Utill.GetComponentRecursively<T>(go);
                if (comp != null)
                    return comp;
            }
        }
        return null;
    }

    public static T GetComponentToParent<T>(MonoBehaviour from) where T : Component
    {
        return GetComponentToParent<T>(from.gameObject);
    }
    public static T GetComponentToParent<T>(GameObject from) where T : Component
    {
        T comp = from.GetComponent<T>();
        if (comp != null)
            return comp;

        if (from.transform.parent != null)
            return GetComponentToParent<T>(from.transform.parent.gameObject);

        return null;
    }

    public static List<T> GetComponentsRecursively<T>(GameObject from) where T : Component
    {
        List<T> result = new List<T>();
        GetComponentsRecursively<T>(from, result);
        return result;
    }

    public static void SetMonoBehaviourEnable<T>(GameObject from, bool isEnable) where T : Component
    {
        List<T> result = GetComponentsRecursively<T>(from);
        for (int i = 0; i < result.Count; i++)
        {
            MonoBehaviour mono = result[i] as MonoBehaviour;
            mono.enabled = isEnable;
        }
    }

    public static void GetComponentsRecursively<T>(GameObject from, List<T> result) where T : Component
    {
        T comp;
        comp = from.GetComponent<T>();
        if (comp != null)
            result.Add(comp);

        for (int n = from.transform.childCount - 1; 0 <= n; n--)
        {
            if (n < from.transform.childCount)
            {
                GameObject go = from.transform.GetChild(n).gameObject;
                //comp = go.GetComponent<T>();
                //if (comp != null)
                //    result.Add(comp);

                LMJ_Utill.GetComponentsRecursively<T>(go, result);
            }
        }
    }


    public static T FindGameObjectGetComponent<T>(GameObject goRoot, string nameGgameObj) where T : Component
    {
        GameObject go = LMJ_Utill.findGameObjectRecursively(goRoot, nameGgameObj);
        if (go == null)
            return null;
        return go.GetComponent<T>();
    }


    public static GameObject findGameObjectRecursivelyToParent(GameObject from, string name)
    {
        if (from.name == name)
            return from;

        if (from.transform.parent != null)
            return findGameObjectRecursivelyToParent(from.transform.parent.gameObject, name);
        return null;
    }

    public static void Destroy(GameObject go, float delayTime = 0f)
    {
        if (delayTime == 0f)
            GameObject.Destroy(go);
        else
            GameObject.Destroy(go, delayTime);
    }


    public static GameObject findGameObjectRecursively(GameObject from, string name)
    {
        if (from == null)
            return null;

        if (from.name == name)
            return from;

        for (int n = from.transform.childCount - 1; 0 <= n; n--)
        {
            if (n < from.transform.childCount)
            {
                GameObject go = from.transform.GetChild(n).gameObject;
                if (go.name == name)
                    return go;

                go = findGameObjectRecursively(go, name);
                if (go != null)
                    return go;
            }
        }
        return null;
    }

    public static List<GameObject> findGameObjectsRecursively(GameObject from, string name)
    {
        List<GameObject> result = new List<GameObject>();
        findGameObjectsRecursively(from, name, result);
        return result;
    }

    public static void findGameObjectsRecursively(GameObject from, string name, List<GameObject> result)
    {
        if (from == null)
            return;

        if (from.name == name)
            result.Add(from);

        for (int i = 0; i < from.transform.childCount; i++)
        {
            GameObject go = from.transform.GetChild(i).gameObject;
            findGameObjectsRecursively(go, name, result);
        }
    }

    public static string GetCallStack()
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame[] stackFrames = stackTrace.GetFrames();

        string callstack = "!!!!!!!!!!!! callstack !!!!!!!!!!!!\n";
        foreach (StackFrame stackFrame in stackFrames)
        {
            string strLine =
                stackFrame.GetMethod().ReflectedType.ToString() + "   " +
                stackFrame.GetMethod().Name + "(" +
                stackFrame.GetFileLineNumber().ToString() + ")\n";
            callstack += strLine;
        }

        return callstack;
    }

    public static void PrintCallStack()
    {
        LMJ.Log(GetCallStack());
    }

    public static string ColorToHex(Color32 color, bool alpah = false)
    {
        string hex = color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2");
        if (alpah)
            hex = color.a.ToString("x2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        if (hex.Length < 6)
            return Color.red;

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    public static string DictToString<T>(IDictionary<string, T> items, string format = null)
    {
        format = string.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

        StringBuilder itemString = new StringBuilder();
        foreach (var item in items)
            itemString.AppendFormat(format, item.Key, item.Value);

        return itemString.ToString();
    }

    static public int maxRenderQ(GameObject go)
    {
        int renderQ = -1;
        List<MeshRenderer> meshRenderers = LMJ_Utill.GetComponentsRecursively<MeshRenderer>(go);
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null && mr.sharedMaterial != null)
            {
                if (renderQ < mr.sharedMaterial.renderQueue)
                    renderQ = mr.sharedMaterial.renderQueue;
            }
        }
        return renderQ;
    }

    static public void setRenderQ(GameObject go, int q)
    {
        List<MeshRenderer> meshRenderers = LMJ_Utill.GetComponentsRecursively<MeshRenderer>(go);
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null && mr.sharedMaterial != null)
            {
                mr.sharedMaterial.renderQueue = q;    // 디폴트 값 3000 
            }
        }
    }

    static public int GetMaxRenderQ(GameObject go)
    {
        int renderQ = 0;
        List<MeshRenderer> meshRenderers = LMJ_Utill.GetComponentsRecursively<MeshRenderer>(go);
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null && mr.sharedMaterial != null)
            {
                if (renderQ < mr.sharedMaterial.renderQueue)
                    renderQ = mr.sharedMaterial.renderQueue;
            }
        }

        if (renderQ == 0)
        {
            LMJ.LogError("renderQ == 0");
            renderQ = 3000;
        }

        return renderQ;
    }


    static public string _myExternalIP = "";
    public void GetIP(System.Action<string> func)
    {
        if (string.IsNullOrEmpty(_myExternalIP) == false)
        {
            func(_myExternalIP);
            return;
        }

        /*
        LMJ_Utill.GetWebText("http://checkip.dyndns.org/", (str) =>
        {
            UnityEngine.Debug.Log("GetIP " + str);
            if (str.IndexOf("Current IP Address: ") > -1)
            {
                string tag = "Current IP Address: ";
                int ingNO = str.IndexOf(tag) + tag.Length;
                int ingNO2 = str.IndexOf("</body>");
                _myExternalIP = str.Substring(ingNO, ingNO2 - ingNO);
                func(_myExternalIP);
            }
            else
                func("");
        },
        null
        ,
        (msg) =>
        {
            LMJ.LogError(string.Format("[{0}] {1}", "GetIP", msg));
        });
        */
        DownLoadText("http://checkip.dyndns.org/", (b, o) =>
        {
            string result = o as string;
            UnityEngine.Debug.Log("GetIP " + result);

            if (result.IndexOf("Current IP Address: ") > -1)
            {
                string tag = "Current IP Address: ";
                int ingNO = result.IndexOf(tag) + tag.Length;
                int ingNO2 = result.IndexOf("</body>");
                _myExternalIP = result.Substring(ingNO, ingNO2 - ingNO);
                func(_myExternalIP);
            }
            else
                func("");
        });
    }

    static public string _myContryCode = "";
    public void GetContryCode(System.Action<string> func)
    {
        if (string.IsNullOrEmpty(_myContryCode) == false)
        {
            func(_myContryCode);
            return;
        }

        DownLoadText("http://ip2c.org/self", (b, o) =>
        {
            string result = o as string;
            UnityEngine.Debug.Log("GetContryCode " + result);

            string[] listnation = result.Split(';');

            _myContryCode = listnation[2];
            func(_myContryCode);
        });
    }


    static public string StripPath(string path)
    {
        string name = path;
        int nT = name.LastIndexOf('/');
        if (nT > 0)
            return path.Substring(nT + 1, path.Length - nT - 1);
        return path;
    }


    public static Texture2D LoadTextureFile(string fileName)
    {
        Texture2D Result = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        byte[] bytes = File.ReadAllBytes(fileName);
        if (!Result.LoadImage(bytes)) return null;
        else return Result;
    }

    public static void SaveTextureFile(string fileName, Texture2D SaveImage)
    {
        byte[] bytes = SaveImage.EncodeToPNG();
        File.WriteAllBytes(fileName, bytes);
    }

    public void DownLoadText(string url, WWWForm wform, System.Action<bool, string> onLoad)
    {
        StartCoroutine(LoadWebText(url, wform, onLoad));
    }
    public void DownLoadText(string url, System.Action<bool, string> onLoad)
    {
        StartCoroutine(LoadWebText(url, null, onLoad));
    }
    IEnumerator LoadWebText(string url, WWWForm wform, System.Action<bool, string> onLoad)
    {
        WWW www;
        if (wform != null)
            www = new WWW(url, wform);
        else
            www = new WWW(url);

        yield return www;

        if (www.error == null)
        {
            //LMJ.Log(www.text);
            //object obj = JsonConvert.DeserializeObject(www.text);
            onLoad(true, System.Text.Encoding.UTF8.GetString(www.bytes).Trim());
        }
        else
        {
            //object obj = JsonConvert.DeserializeObject(www.text);
            LMJ.LogError(url);
            LMJ.LogError(www.error);
            onLoad(false, www.error);
        }
        www.Dispose();
    }


    public void DownLoadText(int ReturnIndex, string url, WWWForm wform, System.Action<bool, int, string> onLoad)
    {
        StartCoroutine(LoadWebText(ReturnIndex, url, wform, onLoad));
    }
    public void DownLoadText(int ReturnIndex, string url, System.Action<bool, int, string> onLoad)
    {
        StartCoroutine(LoadWebText(ReturnIndex, url, null, onLoad));
    }
    IEnumerator LoadWebText(int ReturnIndex, string url, WWWForm wform, System.Action<bool, int, string> onLoad)
    {
        LMJ.Log(url);

        WWW www;
        if (wform != null)
            www = new WWW(url, wform);
        else
            www = new WWW(url);
        yield return www;
        if (www.error == null)
        {
            onLoad(true, ReturnIndex, System.Text.Encoding.UTF8.GetString(www.bytes).Trim());
        }
        else
        {
            onLoad(false, ReturnIndex, www.error);
        }
        www.Dispose();
    }



    public void DownloadImage(int index, string url, System.Action<bool, int, Texture2D> onLoad)
    {
        StartCoroutine(LoadWebTexture(index, url, onLoad));
    }
    IEnumerator LoadWebTexture(int ReturnIndex, string TextureUrl, System.Action<bool, int, Texture2D> onLoad)
    {
        WWW www = new WWW(TextureUrl);
        yield return www;
        if (www.error == null)
            onLoad(true, ReturnIndex, www.texture);
        else
            onLoad(false, ReturnIndex, null);
        www.Dispose();
    }

    static public void GetWebText(string url, System.Action<string> onResult, System.Action<float> onProgress = null, System.Action<string> onError = null)
    {
        LMJ_Utill.GetWebContentAsync(url, (bytes, ind) =>
        {
            string ss = System.Text.Encoding.UTF8.GetString(bytes).Trim();
            onResult(ss);
        },
        onProgress, onError);
    }

    static public void GetWebContentAsync(string url, System.Action<byte[], string> onResult, System.Action<float> onProgress = null, System.Action<string> onError = null, string id = "")
    {
        try
        {
            HttpWebRequest _request = WebRequest.Create(url) as HttpWebRequest;
            IAsyncResult _responseAsyncResult = null;
            _responseAsyncResult = _request.BeginGetResponse((state) =>
           {
               var response = _request.EndGetResponse(_responseAsyncResult) as HttpWebResponse;
               long contentLength = response.ContentLength;
               if (contentLength == -1)
               {
                   if (onError != null)
                       onError("contentLength == -1");
                   return;
               }

               Stream responseStream = response.GetResponseStream();
               if (onProgress != null)
                   onProgress(0);

               // Allocate space for the content
               var data = new byte[contentLength];
               int currentIndex = 0;
               int bytesReceived = 0;
               var buffer = new byte[256];
               do
               {
                   /*
                   if (ClientNet.IsInternetReachability == false )
                   {
                       if (onError != null)
                           onError(e.Message);
                       return;
                   }
				   */

                   bytesReceived = responseStream.Read(buffer, 0, 256);
                   Array.Copy(buffer, 0, data, currentIndex, bytesReceived);
                   currentIndex += bytesReceived;

                   // Report percentage
                   double percentage = (double)currentIndex / contentLength;
                   if (onProgress != null)
                       onProgress((float)percentage);
               } while (currentIndex < contentLength);

               if (onProgress != null)
                   onProgress(1f);
               response.Close();

               if (onResult != null)
                   onResult(data, id);

           }, null);
        }
        catch (System.Exception e)
        {
            if (onError != null)
                onError(e.Message);
        }
    }


    static public void GetWebContent(string url, System.Action<byte[]> onResult, System.Action<string> onError = null)
    {
        try
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(url);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(( (HttpWebResponse)response ).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();

            var data = new byte[response.ContentLength];
            dataStream.Read(data, 0, (int)response.ContentLength);

            dataStream.Close();
            response.Close();

            if (onResult != null)
                onResult(data);
        }
        catch (System.Exception e)
        {
            if (onError != null)
                onError(e.Message);
        }
    }

    static public void ChangeLayerMask(GameObject go, string layer)
    {
        ChangeLayerMask(go, LayerMask.NameToLayer(layer));
    }

    static public void ChangeLayerMask(GameObject go, int layer)
    {
        Renderer[] renderer = go.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderer.Length; i++)
            renderer[i].gameObject.layer = layer;
    }

    static public void ChangeAllLayerMask(GameObject go, int layer)
    {
        int childCount = go.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = go.transform.GetChild(i);

            ChangeAllLayerMask(child.gameObject, layer);
        }

        go.layer = layer;
    }

    static public int GetLayerMask(GameObject go)
    {
        Renderer[] renderer = go.GetComponentsInChildren<Renderer>();

        if (renderer.Length > 0)
            return renderer[0].gameObject.layer;

        return -1;
    }

    static public void DestroyAllChild(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject itm = go.transform.GetChild(i).gameObject;
            if (itm != null && itm.activeSelf)
            {
                GameObject.Destroy(itm);
            }
        }
    }

    static public void DestroyImmediateAllChild(GameObject go)
    {
        List<GameObject> destroyObjects = new List<GameObject>();

        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject itm = go.transform.GetChild(i).gameObject;
            if (itm != null && itm.activeSelf)
            {
                destroyObjects.Add(itm);
            }
        }

        destroyObjects.ForEach(itm => DestroyImmediate(itm));
    }

    static public void SetActiveAllChild(GameObject go, bool isActive)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject itm = go.transform.GetChild(i).gameObject;
            if (itm != null)
                itm.gameObject.SetActive(isActive);
        }
    }


    static public TimeSpan SecToTime(double currentSec, bool noSec = true, bool day = true)
    {
        int nDay = 0;
        int nHour = (int)( currentSec / 3600 );
        int nMin = (int)( ( currentSec / 60 ) % 60 );
        int nSec = (int)( currentSec % 60 );

        if (noSec)
            return new TimeSpan(nHour, nMin, 0);

        if (nHour > 0)
        {
            if (day && nHour > 24)
            {
                nDay = (int)( nHour / 24 );
                nHour = nHour - nDay * 24;
                return new TimeSpan(nDay, nHour, nMin, nSec);
            }
            else
                return new TimeSpan(nHour, nMin, nSec);
        }

        return new TimeSpan(nDay, nHour, nMin, nSec);
    }
    static public string SecToTimeStr(double currentSec, bool noSec = true, bool day = true)
    {
        TimeSpan span = SecToTime(currentSec, noSec, day);

        //int nDay = 0;
        //int nHour = (int)( currentSec / 3600 );
        //int nMin = (int)( ( currentSec / 60 ) % 60 );
        //int nSec = (int)( currentSec % 60 );
        if (noSec)
            return string.Format("{0:D2} : {1:D2}", span.Hours , span.Minutes);

        if (span.Hours > 0)
        {
            if (day && span.Days > 0)
            {
                return string.Format("{0} Day {1:D2} : {2:D2} : {3:D2}", span.Days, span.Hours, span.Minutes, span.Seconds);
            }
            else
            {
                int hours = span.Days * 24 + span.Hours;
                return string.Format("{0:D2} : {1:D2} : {2:D2}", hours, span.Minutes, span.Seconds);
            }
        }

        return string.Format("{0:D2} : {1:D2}", span.Minutes, span.Seconds);
    }

    static public void CopyComponentValue(Component original, Component destination)
    {
        System.Type type = original.GetType();
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(destination, field.GetValue(original));
        }
    }


    static public Component CopyComponent(Component original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);

        CopyComponentValue(original, copy);
        return copy;
    }

    //static public T CopyComponent<T>(T original, GameObject destination) where T : Component
    //{
    //    System.Type type = original.GetType();
    //    Component copy = destination.AddComponent(type);
    //    CopyComponentValue(original, copy);
    //    return copy as T;
    //}


    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();

        var dst = destination.GetComponent(type) as T;
        if (!dst) dst = destination.AddComponent(type) as T;

        var fields = GetAllFields(type);
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }

        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }

        return dst as T;
    }
    public static void RemoveAllChildObject(GameObject parent, bool bImmediate)
    {
        for (int n = parent.transform.childCount - 1; 0 <= n; n--)
        {
            if (n < parent.transform.childCount)
            {
                Transform obj = parent.transform.GetChild(n);
                if (bImmediate)
                    GameObject.DestroyImmediate(obj.gameObject);
                else
                    GameObject.Destroy(obj.gameObject);
            }
            // 			obj.parent = null;
            // 			Object.Destroy(obj.gameObject);
        }
    }

    public static IEnumerable<FieldInfo> GetAllFields(System.Type t)
    {
        if (t == null)
        {
            return Enumerable.Empty<FieldInfo>();
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                             BindingFlags.Static | BindingFlags.Instance |
                             BindingFlags.DeclaredOnly;
        return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
    }

    public static void CopyRectTrandform(RectTransform from, RectTransform to)
    {
        to.position = from.position;
        to.rotation = from.rotation;
        to.anchoredPosition = from.anchoredPosition;
        to.offsetMax = from.offsetMax;
        to.offsetMin = from.offsetMin;
        to.anchoredPosition3D = from.anchoredPosition3D;
        to.anchorMin = from.anchorMin;
        to.anchorMax = from.anchorMax;
        to.pivot = from.pivot;
        to.sizeDelta = from.sizeDelta;
    }

    static public void GameQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }


    static public void SendLogMail(string title, string msg)
    {
#if UNITY_EDITOR
        SaveText("AllLog.log", msg);
#endif

        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("LMJ.Log.recv@gmail.com");
        mail.To.Add("LMJ.Log.recv@gmail.com");
        mail.Subject = title;
        mail.Body = msg;

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential("LMJ.Log.recv@gmail.com", "Eoqkrsksek!") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);
        LMJ.Log("SendLogMail success : to LMJ.Log.recv@gmail.com ");
    }


    static public string RandomString(int size)
    {
        string rStr = Path.GetRandomFileName();
        rStr = rStr.Replace(".", ""); // For Removing the .
        rStr = rStr.Substring(0, size);
        return rStr;
    }

    static public void UnloadUnusedAssetsGC()
    {
        try
        {

            UnityEngine.Debug.Log("UnloadUnusedAssetsGC");
            Resources.UnloadUnusedAssets();
            System.GC.Collect(0, System.GCCollectionMode.Forced);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("UnloadUnusedAssetsGC : " + e.Message);
        }
    }

    public static string BuildQueryString(Dictionary<string, object> post)
    {
        StringBuilder builder = new StringBuilder(20);

        int nc = 0;
        foreach (KeyValuePair<string, object> post_arg in post)
        {
            builder.Append(post_arg.Key);
            builder.Append("=");
            builder.Append(System.Uri.EscapeDataString(post_arg.Value.ToString()));

            nc++;
            if (nc == post.Count)
                break;

            builder.Append("&");
        }

        return builder.ToString(); ;
    }


    //static public Color GetMousePointColor(UITexture txtr)
    //{
    //    RaycastHit hit;
    //    if (!Physics.Raycast(NGUIScene.GetCamera().ScreenPointToRay(Input.mousePosition), out hit))
    //        return Color.cyan;

    //    Vector3 localHit = txtr.transform.InverseTransformPoint(hit.point);
    //    UIWidget w = txtr.GetComponent<UIWidget>();
    //    localHit.x += w.width / 2;
    //    localHit.y += w.height / 2;

    //    localHit.x /= w.width;
    //    localHit.y /= w.height;

    //    Texture2D tex = txtr.mainTexture as Texture2D;
    //    Vector2 pixelUV = hit.textureCoord;
    //    Color hitPixel = tex.GetPixelBilinear(localHit.x, localHit.y);

    //    //LMJ.Log(string.Format("hitPixel {0}  {1}", localHit, hitPixel));

    //    return hitPixel;//.a > 0f;
    //}


    static public Coroutine TransformLerf(Transform from, Transform to, float fSec, bool isLocal, System.Action onComplete, AnimationCurve aniCurve = null)
    {
        Vector3 fromPos;
        Quaternion fromRot;
        Vector3 toPos;
        Quaternion toRot;
        if (isLocal)
        {
            fromPos = from.localPosition;
            fromRot = from.localRotation;
            toPos = to.localPosition;
            toRot = to.localRotation;
        }
        else
        {
            fromPos = from.position;
            fromRot = from.rotation;
            toPos = to.position;
            toRot = to.rotation;
        }
        return Get().StartCoroutine(eTransformLerf(from, fromPos, fromRot, toPos, toRot, fSec, isLocal, onComplete, aniCurve));
    }
    static public Coroutine TransformLerf(Transform from, Vector3 toPos, Quaternion toRot, float fSec, bool isLocal, System.Action onComplete, AnimationCurve aniCurve = null)
    {
        Vector3 fromPos;
        Quaternion fromRot;
        if (isLocal)
        {
            fromPos = from.localPosition;
            fromRot = from.localRotation;
        }
        else
        {
            fromPos = from.position;
            fromRot = from.rotation;
        }
        return Get().StartCoroutine(eTransformLerf(from, fromPos, fromRot, toPos, toRot, fSec, isLocal, onComplete, aniCurve));
    }
    static public IEnumerator eTransformLerf(Transform mvTr,
                                        Vector3 fromPos, Quaternion fromRot,
                                        Vector3 toPos, Quaternion toRot,
                                        float fSec, bool isLocal, System.Action onComplete, AnimationCurve aniCurve = null)
    {
        float p = 0, t = 0;
        while (p < 1)
        {
            p = Mathf.Clamp01(( t += Time.deltaTime ) / fSec);
            if (aniCurve != null)
                p = aniCurve.Evaluate(p);
            if (isLocal)
            {
                mvTr.localPosition = Vector3.Lerp(fromPos, toPos, p);
                mvTr.localRotation = Quaternion.Lerp(fromRot, toRot, p);
            }
            else
            {
                mvTr.position = Vector3.Lerp(fromPos, toPos, p);
                mvTr.rotation = Quaternion.Lerp(fromRot, toRot, p);
            }
            yield return null;
        }

        if (onComplete != null)
            onComplete();
    }

    static public Coroutine LerpFloat(float fromPos, float toPos, float fSec, System.Action<float> onUpdate, System.Action onComplete = null, AnimationCurve aniCurve = null, bool unscaledTime = false)
    {
        return Get().StartCoroutine(eLerpFloat(fromPos, toPos, fSec, onUpdate, onComplete, aniCurve));
    }
    static public Coroutine LerpFloat(MonoBehaviour owner, float fromPos, float toPos, float fSec, System.Action<float> onUpdate, System.Action onComplete = null, AnimationCurve aniCurve = null, bool unscaledTime = false)
    {
        return owner.StartCoroutine(eLerpFloat(fromPos, toPos, fSec, onUpdate, onComplete, aniCurve));
    }
    static public IEnumerator eLerpFloat(float from, float to, float fSec, System.Action<float> onUpdate = null, System.Action onEnd = null, AnimationCurve aniCurve = null, bool unscaledTime = false)
    {
        //AnimationCurve aniCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        float p = 0, t = 0;
        while (p < 1)
        {
            if (unscaledTime)
                p = Mathf.Clamp01(( t += Time.unscaledDeltaTime ) / fSec);
            else
                p = Mathf.Clamp01(( t += Time.deltaTime ) / fSec);
            if (aniCurve != null)
                p = aniCurve.Evaluate(p);
            float val = Mathf.Lerp(from, to, p);
            onUpdate(val);
            yield return null;
        }

        if (onEnd != null)
            onEnd();
    }

    static public Coroutine LerpTransform(Vector3 fromPos, Quaternion fromPRot, float fromFov,
                                 Vector3 toPos, Quaternion toRot, float toFov,
                                 float fSec, System.Action<Vector3, Quaternion, float> onUpdate, System.Action onComplete = null, AnimationCurve aniCurve = null)
    {
        return Get().StartCoroutine(eLerpTransform(fromPos, fromPRot, fromFov,
                                        toPos, toRot, toFov, fSec, onUpdate, onComplete, aniCurve));
    }
    static public IEnumerator eLerpTransform(Vector3 fromPos, Quaternion fromRot, float fromFov,
                                 Vector3 toPos, Quaternion toRot, float toFov, float fSec, System.Action<Vector3, Quaternion, float> onUpdate = null, System.Action onEnd = null, AnimationCurve aniCurve = null)
    {
        float p = 0, t = 0;
        while (p < 1)
        {
            p = Mathf.Clamp01(( t += Time.deltaTime ) / fSec);
            if (aniCurve != null)
                p = aniCurve.Evaluate(p);

            Vector3 pos = Vector3.Lerp(fromPos, toPos, p);
            Quaternion rot = Quaternion.Lerp(fromRot, toRot, p);
            float fov = Mathf.Lerp(fromFov, toFov, p);

            onUpdate(pos, rot, fov);
            yield return null;
        }

        if (onEnd != null)
            onEnd();
    }

    static public Coroutine TransformLerpPos(Transform from, Vector3 fromPos, Vector3 toPos, float fSec, bool isLocal, System.Action onComplete, AnimationCurve aniCurve = null)
    {
        return Get().StartCoroutine(Get().ePosLerf(from, fromPos, toPos, fSec, isLocal, onComplete, aniCurve));
    }
    public IEnumerator ePosLerf(Transform target, Vector3 from, Vector3 to, float fSec, bool isLocal, System.Action onEnd = null, AnimationCurve aniCurve = null)
    {
        float p = 0, t = 0;
        while (p < 1)
        {
            p = Mathf.Clamp01(( t += Time.deltaTime ) / fSec);
            if (aniCurve != null)
                p = aniCurve.Evaluate(p);

            if (isLocal)
                target.localPosition = Vector3.Lerp(from, to, p);
            else
                target.position = Vector3.Lerp(from, to, p);

            yield return null;
        }

        if (onEnd != null)
            onEnd();
    }

    static public Coroutine LerpVector3(Vector3 fromPos, Vector3 toPos, float fSec, System.Action<Vector3> onUpdate, System.Action onComplete, AnimationCurve aniCurve = null)
    {
        return Get().StartCoroutine(Get().ePosVector3(fromPos, toPos, fSec, onUpdate, onComplete, aniCurve));
    }
    public IEnumerator ePosVector3(Vector3 from, Vector3 to, float fSec, System.Action<Vector3> onUpdate, System.Action onEnd = null, AnimationCurve aniCurve = null)
    {
        float p = 0, t = 0;
        while (p < 1)
        {
            p = Mathf.Clamp01(( t += Time.deltaTime ) / fSec);
            if (aniCurve != null)
                p = aniCurve.Evaluate(p);

            onUpdate(Vector3.Lerp(from, to, p));
            yield return null;
        }

        if (onEnd != null)
            onEnd();
    }

    static public Coroutine TransformLerpRot(Transform from, Quaternion fromRot, Quaternion toRot, float fSec, bool isLocal, System.Action onComplete = null, System.Action onUpdate = null, AnimationCurve aniCurve = null)
    {
        return Get().StartCoroutine(eRotLepf(from, fromRot, toRot, fSec, isLocal, onComplete, onUpdate, aniCurve));
    }
    static public IEnumerator eRotLepf(Transform target, Quaternion from, Quaternion to, float fSec, bool isLocal, System.Action onEnd = null, System.Action onUpdate = null, AnimationCurve aniCurve = null)
    {
        float p = 0, t = 0;
        while (p < 1)
        {
            p = Mathf.Clamp01(( t += Time.deltaTime ) / fSec);
            if (aniCurve != null)
                p = aniCurve.Evaluate(p);

            if (isLocal)
            {
                target.localRotation = Quaternion.Lerp(from, to, p);
            }
            else
            {
                try
                {
                    target.rotation = Quaternion.Lerp(from, to, p);
                }
                catch (System.Exception e)
                {
                    LMJ.LogError(e.Message);
                    break;
                }

            }

            if (onUpdate != null)
                onUpdate();
            yield return null;
        }

        if (onEnd != null)
            onEnd();
    }




    static public bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        return IsVisibleFrom(renderer.bounds, camera);
    }

    public static bool visibleCheckTrue = false;
    static public bool IsVisibleFrom(Bounds bounds, Camera camera)
    {
        if (visibleCheckTrue) //for performance check!!
            return true;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }



    static public void SaveText(string path, string contents)
    {
        StringBuilder sb = new StringBuilder();
        if(path.Contains(":") == false)
            sb.Append(Application.dataPath);
        sb.Append(path);// "/Resources/TranslateDic.txt");

        FileStream file = new FileStream(sb.ToString(), FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(file, System.Text.Encoding.UTF8);

        sw.Write(contents);
        sw.Close();
        file.Close();
    }


    static public string LoadText(string path)
    {
        StreamReader reader = new StreamReader(path);
        string str = reader.ReadToEnd();
        reader.Close();
        return str;
    }



    static public void SaveBinary(string path, byte[] contents)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(Application.dataPath);
        sb.Append(path);// "/Resources/TranslateDic.txt");

        FileStream file = new FileStream(sb.ToString(), FileMode.Create, FileAccess.Write);

        file.Write(contents, 0, contents.Length);

        file.Close();
    }

    public static void ValueContinuesChange(int from, int to, System.Action<int> onChange, float sec = 0f, System.Action onEnd = null)
    {
        if (from == to)
        {
            onChange(to);
            if (onEnd != null)
                onEnd();
            return;
        }

        Get().StartCoroutine(Get().eValueContinuesChange(from, to, onChange, sec, onEnd));
    }
    IEnumerator eValueContinuesChange(int from, int to, System.Action<int> onChange, float sec = 0f, System.Action onEnd = null)
    {
        int prev = from;

        int diff = Math.Abs(to - prev);
        float playTime = sec;
        if (playTime == 0)
            playTime = Mathf.Min(diff / (float)to, 1.0f);  // max 1초로.

        playTime = Mathf.Abs(playTime);

        int curr = prev;
        float time = 0;

        while (true)
        {
            time += Time.deltaTime;
            curr = (int)Mathf.Lerp(prev, to, time / playTime);

            onChange(curr);

            if (curr == to)
                break;
            yield return null;
        }

        if (onEnd != null)
            onEnd();
    }

    public static string ADID = "";
    static public void GetIDFA_ADID()
    {
        GetADID((id) =>
        {
            ADID = id;
        });
    }

    static public void GetADID(System.Action<string> onRecv)
    {
        LMJ.Log("■■■■■GetID()■■■■■");
        // IDFA
        Application.RequestAdvertisingIdentifierAsync(
                (string advertisingId, bool trackingEnabled, string error) =>
                {
                    LMJ.Log("advertisingId=" + advertisingId + " enabled=" + trackingEnabled + " error=" + error);
                    if (onRecv != null)
                        onRecv(advertisingId);
                }
        );
    }

    static public string GetUniqueID()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }





    public enum ImageFilterMode : int
    {
        Nearest = 0,
        Biliner = 1,
        Average = 2
    }
    public static Texture2D ResizeTexture(Texture2D pSource, ImageFilterMode pFilterMode, float pScale)
    {

        //*** Variables
        int i;

        //*** Get All the source pixels
        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);

        //*** Calculate New Size
        float xWidth = Mathf.RoundToInt((float)pSource.width * pScale);
        float xHeight = Mathf.RoundToInt((float)pSource.height * pScale);

        //*** Make New
        Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

        //*** Make destination array
        int xLength = (int)xWidth * (int)xHeight;
        Color[] aColor = new Color[xLength];

        Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

        //*** Loop through destination pixels and process
        Vector2 vCenter = new Vector2();
        for (i = 0; i < xLength; i++)
        {

            //*** Figure out x&y
            float xX = (float)i % xWidth;
            float xY = Mathf.Floor((float)i / xWidth);

            //*** Calculate Center
            vCenter.x = ( xX / xWidth ) * vSourceSize.x;
            vCenter.y = ( xY / xHeight ) * vSourceSize.y;

            //*** Do Based on mode
            //*** Nearest neighbour (testing)
            if (pFilterMode == ImageFilterMode.Nearest)
            {

                //*** Nearest neighbour (testing)
                vCenter.x = Mathf.Round(vCenter.x);
                vCenter.y = Mathf.Round(vCenter.y);

                //*** Calculate source index
                int xSourceIndex = (int)( ( vCenter.y * vSourceSize.x ) + vCenter.x );

                //*** Copy Pixel
                aColor[i] = aSourceColor[xSourceIndex];
            }

            //*** Bilinear
            else if (pFilterMode == ImageFilterMode.Biliner)
            {

                //*** Get Ratios
                float xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
                float xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

                //*** Get Pixel index's
                int xIndexTL = (int)( ( Mathf.Floor(vCenter.y) * vSourceSize.x ) + Mathf.Floor(vCenter.x) );
                int xIndexTR = (int)( ( Mathf.Floor(vCenter.y) * vSourceSize.x ) + Mathf.Ceil(vCenter.x) );
                int xIndexBL = (int)( ( Mathf.Ceil(vCenter.y) * vSourceSize.x ) + Mathf.Floor(vCenter.x) );
                int xIndexBR = (int)( ( Mathf.Ceil(vCenter.y) * vSourceSize.x ) + Mathf.Ceil(vCenter.x) );

                //*** Calculate Color
                aColor[i] = Color.Lerp(
                    Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
                    Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
                    xRatioY
                );
            }

            //*** Average
            else if (pFilterMode == ImageFilterMode.Average)
            {

                //*** Calculate grid around point
                int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - ( vPixelSize.x * 0.5f )), 0);
                int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + ( vPixelSize.x * 0.5f )), vSourceSize.x);
                int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - ( vPixelSize.y * 0.5f )), 0);
                int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + ( vPixelSize.y * 0.5f )), vSourceSize.y);

                //*** Loop and accumulate
                Vector4 oColorTotal = new Vector4();
                Color oColorTemp = new Color();
                float xGridCount = 0;
                for (int iy = xYFrom; iy < xYTo; iy++)
                {
                    for (int ix = xXFrom; ix < xXTo; ix++)
                    {

                        //*** Get Color
                        oColorTemp += aSourceColor[(int)( ( (float)iy * vSourceSize.x ) + ix )];

                        //*** Sum
                        xGridCount++;
                    }
                }

                //*** Average Color
                aColor[i] = oColorTemp / (float)xGridCount;
            }
        }

        //*** Set Pixels
        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        //*** Return
        return oNewTex;
    }

    public enum ImageSlice : int
    {
        LEFT = 0,
        RIGHT ,
        TOP ,
        BOTTOM ,
        LEFT_TOP ,
        LEFT_BOTTOM,
        RIGHT_TOP,
        RIGHT_BOTTOM,
    }

    public static Texture2D SliceTexture(Texture2D pSource, ImageSlice type, int size, int pad = 0)
    {
        int i;

        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2Int vSourceSize = new Vector2Int(pSource.width, pSource.height);

        Texture2D oNewTex = null;
        int xWidth = 0;
        int xHeight = 0;
        size += pad;
        if (type == ImageSlice.LEFT || type == ImageSlice.RIGHT)
        {
            xWidth = size;
            xHeight = pSource.height;
            oNewTex = new Texture2D(xWidth, xHeight, TextureFormat.RGBA32, false);
        }
        else if (type == ImageSlice.TOP || type == ImageSlice.BOTTOM)
        {
            xWidth = pSource.width;
            xHeight = size;
            oNewTex = new Texture2D(xWidth, xHeight, TextureFormat.RGBA32, false);
        }

        int xLength = xWidth * xHeight;
        Color[] aColor = new Color[xLength];

        for (i = 0; i < xLength; i++)
        {
            int xY = 0;
            int xX = 0;
            int xSourceIndex = 0;
            if (type == ImageSlice.LEFT)
            {
                xX = i % xWidth;
                xY = (int)( i / xWidth );

                xSourceIndex = ( xY * vSourceSize.x ) + xX;
            }
            else if (type == ImageSlice.RIGHT)
            {
                xX = vSourceSize.x - ( xWidth - i % xWidth );
                xY = (int)( i / xWidth );

                xSourceIndex = ( xY * vSourceSize.x ) + xX;
            }
            else if (type == ImageSlice.BOTTOM)
            {
                xX = i % xWidth;
                xY = (int)( i / xWidth );

                xSourceIndex = ( xY * vSourceSize.x ) + xX;
            }
            else if (type == ImageSlice.TOP)
            {
                xX = i % xWidth;
                xY = (int)( i / xWidth );

                int nJump = xWidth * ( vSourceSize.y - size );
                xSourceIndex = nJump + ( xY * vSourceSize.x ) + xX;
            }

            try
            {
                aColor[i] = aSourceColor[xSourceIndex];
            }
            catch (System.Exception e)
            {
                LMJ.LogError(e.Message);
            }
        }

        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        return oNewTex;
    }

    public static Texture2D SliceTextureCorner(Texture2D pSource, ImageSlice type, int xSize, int ySize, int pad = 0)
    {
        int i;

        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2Int vSourceSize = new Vector2Int(pSource.width, pSource.height);

        Texture2D oNewTex = null;
        int xWidth = xSize + pad;
        int xHeight = ySize + pad;
        oNewTex = new Texture2D(xWidth, xHeight, TextureFormat.RGBA32, false);
        
        int xLength = xWidth * xHeight;
        Color[] aColor = new Color[xLength];

        for (i = 0; i < xLength; i++)
        {
            int xY = 0;
            int xX = 0;
            int xSourceIndex = 0;
            if (type == ImageSlice.LEFT_TOP)
            {
                xX = i % xWidth;
                xY = (int)( i / xWidth );

                int nJump = vSourceSize.x * ( vSourceSize.y - xHeight );
                xSourceIndex = nJump + ( xY * vSourceSize.x ) + xX;
            }
            else if (type == ImageSlice.LEFT_BOTTOM)
            {
                xX = i % xWidth;
                xY = (int)( i / xWidth );

                xSourceIndex = ( xY * vSourceSize.x ) + xX;
            }
            else if (type == ImageSlice.RIGHT_BOTTOM)
            {
                xX = vSourceSize.x - ( xWidth - i % xWidth );
                xY = (int)( i / xWidth );

                xSourceIndex = ( xY * vSourceSize.x ) + xX;
            }
            else if (type == ImageSlice.RIGHT_TOP)
            {
                xX = vSourceSize.x - ( xWidth - i % xWidth );
                xY = (int)( i / xWidth );

                int nJump = vSourceSize.x * ( vSourceSize.y - xHeight );
                xSourceIndex = nJump + ( xY * vSourceSize.x ) + xX;
            }

            try
            {
                aColor[i] = aSourceColor[xSourceIndex];
            }
            catch(System.Exception e)
            {
                LMJ.LogError(e.Message);
            }
        }

        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        return oNewTex;
    }

    public static Texture2D MergeTexture(Texture2D Top, Texture2D bottom, bool vertical )
    {
        int xWidth = 0;
        int xHeight = 0;
        if (vertical)
        {
            xWidth = Math.Max(Top.width, bottom.width);
            xHeight = Top.height + bottom.height;
        }
        else
        {
            xHeight = Math.Max(Top.height, bottom.height);
            xWidth = Top.width + bottom.width;
        }

        Texture2D oNewTex = null;
        oNewTex = new Texture2D(xWidth, xHeight, TextureFormat.RGBA32, false);

        int xLength = xWidth * xHeight;
        Color[] aColor = new Color[xLength];

        Color[] sourceColor1 = Top.GetPixels(0);
        Color[] sourceColor2 = bottom.GetPixels(0);
        if (vertical)
        {
            int nIndex = 0;
            for (int i = 0; i < sourceColor2.Length; i++)
            {
                aColor[nIndex] = sourceColor2[i];
                nIndex++;
            }
            for (int i = 0; i < sourceColor1.Length; i++)
            {
                aColor[nIndex] = sourceColor1[i];
                nIndex++;
            }
        }
        else
        {
            int nIndex = 0;
            for (int i = 0; i < sourceColor1.Length; i++)
            {
                int xX = i % Top.width;
                int xY = (int)( i / Top.width );
                nIndex = xX + xY * ( bottom.width + Top.width );
                aColor[nIndex] = sourceColor1[i];
            }

            nIndex = 0;
            for (int i = 0; i < sourceColor2.Length; i++)
            {
                int xX = i % bottom.width;
                int xY = (int)( i / bottom.width );
                nIndex = Top.width + xX + xY * ( bottom.width + Top.width );
                try
                {
                    aColor[nIndex] = sourceColor2[i];
                }
                catch (System.Exception e)
                {
                    LMJ.LogError(e.Message);
                }
            }
        }

        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        return oNewTex;
    }

    

    static public float CalcAngleBetweenVector(Vector3 vec1, Vector3 vec2)
    {
        float angle = Vector3.Angle(vec1, vec2); // calculate angle
        // assume the sign of the cross product's Y component:
        return angle * Mathf.Sign(Vector3.Cross(vec1, vec2).y);
    }

    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        //return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
        return Vector3.Magnitude(ClosestPointOnLineSegment(point, lineStart, lineEnd) - point);
    }
    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;
        Vector3 lhs = vector2;
        if (magnitude > 1E-06f)
        {
            lhs = (Vector3)( lhs / magnitude );
        }
        float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
        return ( lineStart + ( (Vector3)( lhs * num2 ) ) );
    }

    public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        // Shift the problem to the origin to simplify the math.    
        var wander = point - segmentStart;
        var span = segmentEnd - segmentStart;

        // Compute how far along the line is the closest approach to our point.
        float t = Vector3.Dot(wander, span) / span.sqrMagnitude;

        // Restrict this point to within the line segment from start to end.
        t = Mathf.Clamp01(t);

        // Return this point.
        return segmentStart + t * span;
    }

    public static Vector3 ClosestPointOnInfiniteLine(Vector3 p, Vector3 a, Vector3 b)
    {
        return a + Vector3.Project(p - a, b - a);
    }

    //public static TextAsset LoadTextAsset(string path)
    //{
    //    TextAsset textAsset = ResourceLoader.Load<TextAsset>(path);
    //    if (textAsset == null)
    //    {
    //        LMJ.LogError("Can`t load file ! " + path);
    //    }
    //    return textAsset;
    //}
    //public static T LoadJsnFile<T>(string path)
    //{
    //    TextAsset textAsset = ResourceLoader.Load<TextAsset>(path);
    //    if (textAsset == null)
    //    {
    //        LMJ.LogError("Can`t load file ! " + path);
    //    }
    //    return JsonConvert.DeserializeObject<T>(textAsset.text);
    //}

    public static void ReAssignShader(GameObject obj)
    {
        //Log("find object : " + obj);

        Renderer[] renderers = obj.transform.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer item in renderers)
        {
            if (item.materials != null)
            {
                foreach (Material mat in item.materials)
                {
                    Shader sha = mat.shader;
                    mat.shader = Shader.Find(sha.name);
                    // Debuger.Log(item.gameObject.name + " : " + mat.name, item.gameObject);
                }
            }
        }
    }






    static public string UrlToLocalPath(string url)
    {
        if (url == null)
            return "";

        string[] tmp = url.Split('/');
        string fName = tmp[tmp.Length - 1];
#if UNITY_EDITOR
        string localPath = Application.dataPath + "/../Download/";

        if (Directory.Exists(localPath) == false)
        {
            Directory.CreateDirectory(localPath);
        }

        localPath += fName;
#else
		string localPath = Application.temporaryCachePath + "/" + fName;
#endif
        return localPath;
    }

    static public string LocalPathRoot()
    {
#if UNITY_EDITOR
        string localPath = Application.dataPath + "/../Download/";

        if (Directory.Exists(localPath) == false)
            Directory.CreateDirectory(localPath);
#else
		string localPath = Application.temporaryCachePath + "/" ;
#endif
        return localPath;
    }


    public static bool isHaveDownLoadedFile(string url)
    {
        string localPath = UrlToLocalPath(url);
        if (File.Exists(localPath))
            return true;
        return false;
    }

    static public float cumulativeAverage(float prevAvg, float newNumber, float count)
    {
        float oldWeight = ( count - 1f ) / count;
        float newWeight = 1f / count;
        return ( prevAvg * oldWeight ) + ( newNumber * newWeight );
    }

    public static void RunParticles(GameObject from, bool isPlay)
    {
        if (isPlay)
            from.SetActive(true);

        List<ParticleSystem> parts = GetComponentsRecursively<ParticleSystem>(from);
        for (int n = 0; n < parts.Count; n++)
        {
            if (isPlay)
                parts[n].Play();
            else
                parts[n].Stop();
        }
    }

    public static void SetOrderLayer(GameObject gameObject, int layer)
    {
        List<Renderer> rs = LMJ_Utill.GetComponentsRecursively<Renderer>(gameObject);
        for (int i = 0; i < rs.Count; i++)
        {
            rs[i].sortingOrder = layer;
        }
    }

    static public float GetAngle2Vector(Vector3 fwd, Vector3 targetDir)
    {
        float angle = Vector3.Angle(fwd, targetDir);

        if (AngleDir(fwd, targetDir, Vector3.up) == -1)
        {
            angle = 360.0f - angle;
            if (angle > 359.9999f)
                angle -= 360.0f;
            return angle;
        }
        else
            return angle;
    }

    static public int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0)
            return 1;
        else if (dir < 0.0)
            return -1;
        else
            return 0;
    }

    //예각 만들기
    static public float MakeAcuteAngle(float fDeg)
    {
        if (fDeg > 180)
            fDeg -= 360;
        else if (fDeg < -180)
            fDeg += 360;
        return fDeg;
    }


    public static Bounds GetBoundsWithChildren(GameObject gameObject)
    {
        Renderer parentRenderer = gameObject.GetComponent<Renderer>();

        Renderer[] childrenRenderers = gameObject.GetComponentsInChildren<Renderer>();

        if (childrenRenderers.Length == 0)
            return new Bounds();

        Bounds bounds = parentRenderer != null
           ? parentRenderer.bounds
           : childrenRenderers.FirstOrDefault(x => x.enabled).bounds;

        if (childrenRenderers.Length > 0)
        {
            foreach (Renderer renderer in childrenRenderers)
            {
                if (renderer.enabled)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        return bounds;
    }

    public static CamInfo GetCameraFocusOnInfo(Camera camera, GameObject focusedObject, float marginPercentage, float minimumDist = 0f)
    {
        Bounds bounds = GetBoundsWithChildren(focusedObject);
        return GetCameraFocusOnInfo(camera, bounds, marginPercentage, minimumDist);
    }

    public static CamInfo GetCameraFocusOnInfo(Camera camera, Bounds bounds, float marginPercentage, float minimumDist = 0f)
    {
        float maxExtent = bounds.extents.magnitude;
        return GetCameraFocusOnInfo(camera, bounds.center, maxExtent, marginPercentage, minimumDist);
    }

    public static CamInfo GetCameraFocusOnInfo(Camera camera, Vector3 center, float size, float marginPercentage, float minimumDist = 0f)
    {
        CamInfo cInfo = new CamInfo(camera);
        float minDistance = ( size * marginPercentage ) / Mathf.Sin(Mathf.Deg2Rad * camera.fieldOfView / 2f);

        if (minDistance < minimumDist)
            minDistance = minimumDist;

        cInfo.maxExtent = size;
        cInfo.distanceToObj = minDistance;
        cInfo.position = center - camera.transform.forward * minDistance;
        cInfo.nearClipPlane = minDistance - size;

        return cInfo;
    }

    public static void CameraFocusOn(Camera camera, GameObject focusedObject, float marginPercentage)
    {
        CamInfo cInfo = GetCameraFocusOnInfo(camera, focusedObject, marginPercentage);

        cInfo.Set(camera);
    }

    public static float FrustumHeightAtDistance(float fov, float distance)
    {
        return 2.0f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
    }

    // Calculate the FOV needed to get a given frustum height at a given distance.
    public static float FOVForHeightAndDistance(float height, float distance)
    {
        return 2.0f * Mathf.Atan(height * 0.5f / distance) * Mathf.Rad2Deg;
    }

    public static float FovFocusOn(Camera camera, GameObject focusedObject)
    {
        Bounds bounds = GetBoundsWithChildren(focusedObject);
        float maxExtent = bounds.extents.magnitude;
        //float minDistance = ( maxExtent * marginPercentage ) / Mathf.Sin(Mathf.Deg2Rad * camera.fieldOfView / 2f);

        var currDistance = Vector3.Distance(camera.transform.position, focusedObject.transform.position);
        return FOVForHeightAndDistance(maxExtent, currDistance);

        //FrustumHeightAtDistance(float fov, float distance)
        //float camera.fieldOfView = 2 * Mathf.Atan(frustumHeight * 0.5f / distance) * Mathf.Rad2Deg;
    }

    public static void CameraFocusOnWithFOV(Camera camera, GameObject focusedObject)
    {
        camera.fieldOfView = FovFocusOn(camera, focusedObject);
    }

    public static GameObject ResourceLoad(string path)
    {
        GameObject request = Resources.Load<GameObject>(path);

        if (request == null)
        {
            LMJ.LogError("request.asset == null " + path);
            return null;
        }

        var go = Instantiate(request) as GameObject;
        return go;
    }

    public static void ResourceLoadAsync(string path, System.Action<GameObject> onLoaded)
    {
        Get().StartCoroutine(Get().eLoadResourceAsync(path, onLoaded));
    }

    public IEnumerator eLoadResourceAsync(string path, Action<GameObject> onOpened)
    {
        var request = Resources.LoadAsync<GameObject>(path);
        while (!request.isDone)
            yield return null;

        if (request.asset == null)
        {
            LMJ.LogError("request.asset == null " + path);
            onOpened(null);
            yield break;
        }

        var go = Instantiate(request.asset) as GameObject;
        onOpened(go);
    }

    public static void ResourceLoadAsync<T>(string path, System.Action<T> onLoaded) where T : MonoBehaviour
    {
        Get().StartCoroutine(Get().LoadResourceAsync<T>(path, onLoaded));
    }

    private IEnumerator LoadResourceAsync<T>(string path, Action<T> onOpened) where T : MonoBehaviour
    {
        var request = Resources.LoadAsync<GameObject>(path);
        while (!request.isDone)
            yield return null;

        var go = Instantiate(request.asset) as GameObject;
        var comp = go.GetComponent<T>();
        onOpened(comp);
    }

    /// <summary>
    /// Convenience extension that destroys all children of the transform.
    /// </summary>

    static public void DestroyChildren(Transform t)
    {
        bool isPlaying = Application.isPlaying;

        while (t.childCount != 0)
        {
            Transform child = t.GetChild(0);

            if (isPlaying)
            {
                child.parent = null;
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    //좌하단 0,0 우상단 1,1
    public static bool ViewIntersection(out Vector3 intersectionScreenResult, Vector3 screenPos, float gapX = 0.1f, float gapY = 0.1f)
    {
        intersectionScreenResult = Vector3.zero;
        Vector3 intersection = Vector3.zero;

        Vector3 center = new Vector3(0.5f, 0.5f, 0);
        Vector3 dir = screenPos - center;

        float startX = gapX;
        float endX = 1f - gapX;
        float startY = gapY;
        float endY = 1f - gapY;
        //top
        if (screenPos.y > startY &&
            LineLineIntersection(out intersection, center, dir, new Vector3(startX, endY), new Vector3(1f, 0f)))
        {
            if (startX < intersection.x && intersection.x < endX)
            {
                intersectionScreenResult = intersection;
            }
        }

        //bottom
        if (intersectionScreenResult == Vector3.zero &&
            screenPos.y <= startY &&
            LineLineIntersection(out intersection, center, dir, new Vector3(startX, startY), new Vector3(1f, 0f)))
        {
            if (startX < intersection.x && intersection.x < endX)
            {
                intersectionScreenResult = intersection;
            }
        }

        if (intersectionScreenResult == Vector3.zero)
        {
            //left
            if (screenPos.x < startX &&
                LineLineIntersection(out intersection, center, dir, new Vector3(startX, startY), new Vector3(0f, 1f)))
            {
                if (startY < intersection.y && intersection.y < endY)
                {
                    intersectionScreenResult = intersection;
                }
            }

            //right
            if (screenPos.x >= startX &&
                intersectionScreenResult == Vector3.zero &&
                LineLineIntersection(out intersection, center, dir, new Vector3(endX, startY), new Vector3(0f, 1f)))
            {
                if (startY < intersection.y && intersection.y < endY)
                {
                    intersectionScreenResult = intersection;
                }
            }
        }

        if (intersectionScreenResult != Vector3.zero)
        {
            return true;
        }

        return false;
    }

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + ( lineVec1 * s );
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    static public void LoadSceneFromAssetBundle(string assetPath, string sceneName, bool isAdditive)
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetPath);

        // 에셋 번들 내에 존재하는 씬의 경로를 모두 가져오기
        string[] scenes = assetBundle.GetAllScenePaths();
        string loadScenePath = null;

        foreach (string sname in scenes)
        {
            if (sname.Contains(sceneName))
            {
                loadScenePath = sname;
            }
        }


        if (loadScenePath == null)
            return;

        UnityEngine.SceneManagement.LoadSceneMode loadMode;

        if (isAdditive)
            loadMode = UnityEngine.SceneManagement.LoadSceneMode.Additive;
        else
            loadMode = UnityEngine.SceneManagement.LoadSceneMode.Single;

        UnityEngine.SceneManagement.SceneManager.LoadScene(loadScenePath, loadMode);
    }


    public static bool GetReflectionMemberValue<T>(object target, string propName, out T val)
    {
        FieldInfo fi = target.GetType().GetField(propName);
        if (( fi != null ) && ( fi.FieldType == typeof(T) ))
        {
            val = (T)fi.GetValue(target);
            return true;
        }

        PropertyInfo pi = target.GetType().GetProperty(propName);
        if (( pi != null ) && ( pi.PropertyType == typeof(T) ))
        {
            val = (T)pi.GetValue(target, null);
            return true;
        }

        val = default(T);
        return false;
    }

    public static bool SetReflectionMemberValue<T>(object target, string propName, T val)
    {
        FieldInfo fi = target.GetType().GetField(propName);
        if (( fi != null ) && ( fi.FieldType == typeof(T) ))
        {
            fi.SetValue(target, val);
            return true;
        }

        PropertyInfo pi = target.GetType().GetProperty(propName);
        if (( pi != null ) && ( pi.PropertyType == typeof(T) ))
        {
            pi.SetValue(target, val);
            return true;
        }

        val = default(T);
        return false;
    }

    //public static bool IsDiasableObj(GameObject btn)
    //{
    //    MetaBase meta = btn.GetComponent<MetaBase>();
    //    if (meta == null)
    //        return false;

    //    if (meta.isContain("DiasableObj"))
    //        return true;
    //    return false;
    //}
    //public static void DisableObj(GameObject btn, bool isDisable)
    //{
    //    MetaBase meta = btn.GetComponent<MetaBase>();
    //    if (meta == null)
    //        meta = btn.AddComponent<MetaBase>();

    //    if (meta.isContain("DiasableObj") && isDisable)
    //        return;

    //    if (meta.isContain("DiasableObj") == false && !isDisable)
    //        return;

    //    if (isDisable)
    //        meta.AddOrUpdate("DiasableObj", 1);
    //    else
    //        meta.Remove("DiasableObj");

    //    LMJ.AnimatedButton[] aniBtns = btn.GetComponentsInChildren<LMJ.AnimatedButton>();
    //    Button[] btns = btn.GetComponentsInChildren<Button>();
    //    Image[] imgs = btn.GetComponentsInChildren<Image>();
    //    Text[] txts = btn.GetComponentsInChildren<Text>();
    //    Outline[] outlines = btn.GetComponentsInChildren<Outline>();

    //    for (int i = 0; i < btns.Length; i++)
    //    {
    //        btns[i].enabled = !isDisable;
    //    }

    //    for (int i = 0; i < aniBtns.Length; i++)
    //    {
    //        aniBtns[i].enabled = !isDisable;
    //    }

    //    for (int i = 0; i < imgs.Length; i++)
    //    {
    //        Color clr = imgs[i].color;
    //        if (isDisable)
    //        {
    //            clr *= 0.5f;
    //            clr.a = 255;
    //            imgs[i].color = clr;
    //        }
    //        else
    //        {
    //            clr *= 2f;
    //            clr.a = 255;
    //            imgs[i].color = clr;
    //        }
    //        //imgs[i].color *= 2f;
    //    }

    //    for (int i = 0; i < txts.Length; i++)
    //    {
    //        Color clr = txts[i].color;
    //        if (isDisable)
    //        {
    //            clr *= 0.5f;
    //            clr.a = 255;
    //            txts[i].color = clr;
    //        }
    //        else
    //        {
    //            clr *= 2f;
    //            clr.a = 255;
    //            txts[i].color = clr;
    //        }
    //    }

    //    for (int i = 0; i < outlines.Length; i++)
    //    {
    //        Color clr = outlines[i].effectColor;
    //        if (isDisable)
    //        {
    //            clr *= 0.5f;
    //            clr.a = 255;
    //            outlines[i].effectColor = clr;
    //        }
    //        else
    //        {
    //            clr *= 2f;
    //            clr.a = 255;
    //            outlines[i].effectColor = clr;
    //        }
    //    }
    //}

    public static bool IsEqual(float a, float b, float tolerance = 0.001f)
    {
        return Math.Abs(a - b) < tolerance;
    }

}