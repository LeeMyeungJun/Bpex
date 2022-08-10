using UnityEngine;
using System.Collections.Generic;

public class ZEvent
{
    public ZEvent()
    {

    }
    public ZEvent(object data)
    {
        _data = data;
    }

    public System.Action<ZEvent> _callback;
    public string _eventID;
    public object _data;
	public object _tag;
}

class EventObj
{
    public object _tag;
    public System.Action<ZEvent> _callback;
}


public class LmjEventManager
{
    static Dictionary<string, List<EventObj>> _listerners = new Dictionary<string, List<EventObj>>();

    static public void Log()
    {
        string msg = "";
        msg += "\n Current event listener!\n";
        foreach (KeyValuePair<string, List<EventObj>> pair in _listerners)
        {
            LMJ.Log("    " + pair.Key + " count :" + pair.Value.Count);
            msg += "    " + pair.Key + " count :" + pair.Value.Count + "\n";
        }

        LMJ.Log(msg);
    }

    //static public void Clear()
    //{
    //    if (_listerners != null)
    //    {
    //        if(_listerners.Count>0)
    //        {
    //            foreach (KeyValuePair<string, List<EventObj>> pair in _listerners)
    //            {
    //                LMJ.LogError("    " + pair.Key + " count :" + pair.Value.Count);
    //            }
    //        }
    //        _listerners.Clear();
    //    }
    //}

    static public bool isHaveEventListener(string eventID, System.Action<ZEvent> callback)
    {
        List<EventObj> arr;
        if (_listerners.ContainsKey(eventID))
            arr = _listerners[eventID];
        else
            return false;

        foreach (EventObj o in arr)
        {
            if (o._callback == callback)
                return true;
        }

        return false;
    }

    static public bool isHaveEventListener(string eventID, object tag)
    {
        List<EventObj> arr;
        if (_listerners.ContainsKey(eventID))
            arr = _listerners[eventID];
        else
            return false;

        foreach (EventObj o in arr)
        {
            if (o._tag == null)
                continue;
            if (o._tag.Equals(tag))
                return true;
        }

        return false;
    }


    static public void addEventListener(string eventID, System.Action<ZEvent> callback, object tag = null)
    {
        List<EventObj> arr;
        if (_listerners.ContainsKey(eventID))
        {
            arr = _listerners[eventID];
        }
        else
        {
            arr = new List<EventObj>();
            _listerners.Add(eventID, arr);
        }

        foreach (EventObj o in arr)
        {
            if (o._callback == callback &&
				o._tag == tag )
            {
                LMJ.Log("addEventListener 이미 등록된 거네.. " + eventID);
                return;
            }
        }

        EventObj eo = new EventObj();
        eo._tag = tag;
        eo._callback = callback;

        arr.Add(eo);
    }

    static public void removeEventListenerByTag(string eventID, object tag)
    {
        if (_listerners.ContainsKey(eventID) == false)
        {
            //LMJ.Log("can`t find listener!! " + eventID);
            return;
        }

        List<EventObj> arr = _listerners[eventID];
        List<EventObj> removeList = new List<EventObj>();

        foreach (EventObj o in arr)
        {
            if (o._tag == null)
                continue;
            if (o._tag.Equals(tag))
            {
                removeList.Add(o);
            }
        }

        foreach (EventObj o in removeList)
        {
            arr.Remove(o);
        }

        if (arr.Count == 0)
            _listerners.Remove(eventID);
    }

    static public void removeEventListenerByTag(object tag)
    {
        List<string> remove = new List<string>();
        List<EventObj> removeList = new List<EventObj>();
        foreach (KeyValuePair<string, List<EventObj>> pair in _listerners)
        {
            List<EventObj> arr = pair.Value;

            removeList.Clear();
            foreach (EventObj o in arr)
            {
                if (o._tag == null)
                    continue;
                if (o._tag.Equals(tag))
                {
                    removeList.Add(o);
                }
            }

            foreach (EventObj o in removeList)
            {
                arr.Remove(o);
            }

            if (arr.Count == 0)
                remove.Add(pair.Key);
        }

        foreach (string key in remove)
        {
            _listerners.Remove(key);
        }
    }


    static public void removeEventListener(ZEvent e)
    {
        if (e == null)
        {
            LMJ.LogError("ZEvent e==null");
            return;
        }

        if (_listerners.ContainsKey(e._eventID) == false)
        {
            LMJ.Log("can`t find listener!! " + e._eventID);
            return;
        }

        List<EventObj> arr = _listerners[e._eventID];
        removeByCallback(arr, e._callback);

        if (arr.Count == 0)
            _listerners.Remove(e._eventID);
    }


    static public void removeEventListener(string eventID, System.Action<ZEvent> callback)
    {
        if (_listerners.ContainsKey(eventID) == false)
        {
            LMJ.Log("can`t find listener!! " + eventID);
            return;
        }

        List<EventObj> arr = _listerners[eventID];
        removeByCallback(arr, callback);

        if (arr.Count == 0)
            _listerners.Remove(eventID);
    }

    static void removeByCallback(List<EventObj> arr, System.Action<ZEvent> callback)
    {
        foreach (EventObj o in arr)
        {
            if (o._callback == callback)
            {
                arr.Remove(o);
                return;
            }
        }
    }

    static public void clearEventListener(string eventID)
    {
        if (_listerners.ContainsKey(eventID) == false)
            return;

        _listerners.Remove(eventID);
    }


    static public void dispatchEvent(string eventID, ZEvent obj = null)
    {
        if (_listerners.ContainsKey(eventID) == false)
        {
            return;
        }

        List<EventObj> arrTmp = new List<EventObj>();
        arrTmp.Clear();
        List<EventObj> arr;
        arr = _listerners[eventID];
        for (int i = 0; i < arr.Count; i++)
        {
            arrTmp.Add(arr[i]);
        }

        for (int i = 0; i < arrTmp.Count; i++)
        {
            if (obj == null)
                obj = new ZEvent();

            obj._eventID = eventID;
			obj._tag = arrTmp[i]._tag;
			obj._callback = arrTmp[i]._callback;
            arrTmp[i]._callback(obj);
        }

        arrTmp.Clear();
    }
}