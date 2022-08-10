using UnityEngine;

public class MonoSingle<T> : MonoBehaviour where T : MonoSingle<T>
{
    static public T Inst;

    virtual protected void Awake()
    {
        if (Inst != null)
        {
            Debug.LogError("Inst != null " + gameObject.name);
            return;
        }
        Inst = (T)this;
    }


    public void DestroyImmediate()
    {
        if (Inst)
            GameObject.DestroyImmediate(Inst);

        LmjEventManager.removeEventListenerByTag(GetHashCode());
        Inst = null;
    }

    virtual protected void OnDestroy()
    {
        Inst = null;
    }

}