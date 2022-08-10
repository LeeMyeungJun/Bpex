using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 특정 스크립트에 종속되지 않는 유용한 기능들을 전역 함수가 정리된 클래스
/// </summary>
public class Utility
{
    public static GameObject ResourceLoad(string path)
    {
        GameObject request = Resources.Load<GameObject>(path);

        if (request == null)
        {
            LMJ.LogError("request.asset == null " + path);
            return null;
        }

        var go = UnityEngine.Object.Instantiate(request) as GameObject;
        return go;
    }

    /// <summary>
    /// 방향 벡터를 입력하면 해당 방향을 X축을 통해 바라보는 각도를 반환합니다. transform.eulerAngles의 Z값에 사용됩니다.
    /// </summary>
    /// <param name="isForwardY">해당 방향을 X축 대신 Y축으로 바라보는 각도를 반환합니다.</param>
    /// <returns></returns>
    public static float LookDirToAngle(Vector2 dir, bool isForwardY = false)
	{
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        return isForwardY ? (angle - 90f) : angle;
    }

    /// <summary>
    /// 오브젝트에서 지정한 컴포넌트를 찾아서 반환합니다. 없으면 컴포넌트를 추가한 후 반환합니다.
    /// </summary>
    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();

        return (component != null) ? component : obj.AddComponent<T>();
    }

    /// <summary>
    /// 입력한 이름의 자식 오브젝트를 찾아서 T 타입의 컴포넌트를 반환합니다.  
    /// UI Canvas 생성 시 산하 요소를 탐색하여 바인딩할 때 사용됩니다.
    /// </summary>
    /// <returns>타입이 GameObject일 경우 오브젝트를 반환하며, 그 외에는 오브젝트에 붙어 있는 T 타입의 컴포넌트를 반환합니다. 없을 경우 null을 반환합니다. 탐색에 실패한 경우 null을 반환합니다.</returns>
	public static T FindChild<T>(GameObject parent, string name) where T : UnityEngine.Object
    {
		if ((parent == null) || (string.IsNullOrEmpty(name)))
			return null;

        if(typeof(T) ==typeof(GameObject))
		{
            Transform tr = FindChild<Transform>(parent, name);
            if (tr == null)
                return null;

            return tr.gameObject as T;
        }

        foreach (T component in parent.GetComponentsInChildren<T>())
        {
            if (component.name == name)
                return component;
        }

        return null;
	}


    /// <summary>
    /// 사용예 : SendMessage("funcName", Utils.Params(1234, "string")); 
    /// </summary>
    public UnityEngine.Object[] Params(params UnityEngine.Object[] args)
    {
        return args;
    }


    #region TimeStamp
    public static int DateToTimeStamp()
    {
        System.DateTime d1 = System.DateTime.Parse("1970-01-01 09:00:00"); // 한국시간 기준
        System.DateTime d2 = System.DateTime.Now;
        System.TimeSpan d3 = d2 - d1;
        return System.Convert.ToInt32(d3.TotalSeconds);
    }

    public static int DateToTimeStamp(System.DateTime now)
    {
        System.DateTime d1 = System.DateTime.Parse("1970-01-01 09:00:00");
        System.DateTime d2 = now;
        System.TimeSpan d3 = d2 - d1;
        return System.Convert.ToInt32(d3.TotalSeconds);
    }

    public static System.DateTime TimeStampToDate(int TimeStamp)
    {
        System.DateTime d = System.DateTime.Parse("1970-01-01 09:00:00");
        return d.AddSeconds(TimeStamp);
    }

    public static Int64 DateToTimeStamp64()
    {
        System.DateTime d1 = System.DateTime.Parse("1970-01-01 09:00:00");
        System.DateTime d2 = System.DateTime.Now;
        System.TimeSpan d3 = d2 - d1;
        return System.Convert.ToInt64(d3.TotalMilliseconds);
    }

    public static Int64 DateToTimeStamp64(System.DateTime now)
    {
        System.DateTime d1 = System.DateTime.Parse("1970-01-01 09:00:00");
        System.DateTime d2 = now;
        System.TimeSpan d3 = d2 - d1;
        return System.Convert.ToInt64(d3.TotalMilliseconds);
    }

    public static System.DateTime TimeStampToDate64(Int64 TimeStamp)
    {
        System.DateTime d = System.DateTime.Parse("1970-01-01 09:00:00");
        return d.AddMilliseconds(TimeStamp);
    }
    #endregion //TimeStamp

    // 받침의 존재유무를 확인해서 조사를 선택해 붙인다.
    // a는 받침이 없을때 조사, b는 받침이 있을때 조사
    public static string AttachPostposition(string str, string a, string b)
    {
        if (IsContainFinalConsonant(str) == true)
            return str + b;

        return str + a;
    }

    // 마지막 글자에 받침이 존재하는지 확인
    public static bool IsContainFinalConsonant(string str)
    {
        char[] lastCharacter = null;

        int hangulCodeFirst = 0xAC00;
        int hangulCodeLast = 0xD7A3;

        int finalConsonantCount = 28;

        int code = 0;

        for (int i = str.Length - 1; i >= 0; i--)
        {
            lastCharacter = str.ToCharArray(i, 1);
            code = (int)lastCharacter[0];

            // 괄호 아닌 다른 문자면 끝, 그렇지 않으면 앞 글자
            if (lastCharacter[0] != '>' && lastCharacter[0] != ']' &&
                lastCharacter[0] != ')' && lastCharacter[0] != '}')
            {
                break;
            }
        }

        // 한글이 아니면 그냥 받침이 없는 취급한다.
        if (code < hangulCodeFirst && hangulCodeLast < code)
        {
            return false;
        }

        return ((code - hangulCodeFirst) % finalConsonantCount != 0);
    }

    public static float DamageCalculator(float _minDamge,float _maxDamage,float _criticalChance,out bool Critical)
    {
        float damage = UnityEngine.Random.Range(_minDamge, _maxDamage);
        Critical = UnityEngine.Random.Range(0.0f, 100.0f) <= _criticalChance;
        if (Critical)
            damage *= 1.5f;
        return damage;
    }
}

public static class CustomUtilityExtension
{
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        return Utility.GetOrAddComponent<T>(obj);
    }
}

public static class StringExtension
{

    // 널-빈문자열 확인
    public static bool IsNullOrEmpty(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return true;
        return false;
    }

    // 문자열 유효길이 확인
    public static bool IsVaildLength(this string s, int min = 0, int max = int.MaxValue)
    {
        if (min > max)
            return false;

        if (s.Length >= min && s.Length <= max)
            return true;
        return false;
    }

    // 문자열에서 특정문자 존재 여부
    public static bool IsExistChar(this string s, string chars)
    {
        bool r = false;
        for (int i = 0; i < chars.Length; i++)
        {
            if (s.Contains(chars[i].ToString()))
            {
                r = true;
                break;
            }
        }
        return r;
    }

    public static T ParseEnum<T>(this string s)
    {
        return (T)System.Enum.Parse(typeof(T), s);
    }

    public static T ParseEnum<T>(this string s, bool ignoreCase)
    {
        return (T)System.Enum.Parse(typeof(T), s, ignoreCase);
    }

    public static bool CompareToEnum<T>(this string s, T enumValue)
    {
        return s.CompareTo(enumValue.ToString()) == 0;
    }


    // null, "" 체크

    // 자릿수 콤마찍기

    // ....
}


public static class ListExtension
{
    public static object Random(this IList lst)
    {
        if (lst.Count <= 0)
            return null;

        return lst[UnityEngine.Random.Range(0, lst.Count)];
    }

    public static void Shuffle(this IList lst, int count = 0)
    {
        if (lst.Count == 0)
            return;

        if (count <= 0)
            count = lst.Count;

        for (int i = 0; i < count; i++)
        {
            int a = UnityEngine.Random.Range(0, lst.Count);
            int b = UnityEngine.Random.Range(0, lst.Count);

            if (a != b)
            {
                object tmp = lst[a];
                lst[a] = lst[b];
                lst[b] = tmp;
            }
        }
    }
}

