using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

public class JSONUtilityManager : MonoBehaviour
{
    #region Instance
	// ----------------------Instance Section---------------------- //
    public static JSONUtilityManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<JSONUtilityManager>();

			return _instance;
		}
	}
	private static JSONUtilityManager _instance;
    // ----------------------Instance Section---------------------- //
#endregion


    private static Dictionary<Type, ValueWrapper> _wrapperTable;

    public static T FromJson<T>(string json) => (T)FromJson(json, typeof(T));

    public static object FromJson(string json, Type type)
    {
        object ret;

        if (ValueWrapper.IsSupport(type))
        {
            var wrapperType = ValueWrapper.GetWrapperType(type);
            var wrapper = JsonUtility.FromJson(json, wrapperType);

            ret = (wrapper as ValueWrapper)?.obj;
        }
        else
        {
            ret = JsonUtility.FromJson(json, type);
        }

        return ret;
    }


    public static string ToJson(object obj) => ToJson(obj, false);

    public static string ToJson(object obj, bool prettyPrint)
    {
        return JsonUtility.ToJson(WrapObject(obj), prettyPrint);
    }


    private static object WrapObject(object obj)
    {
        if (obj == null) return null;

        var type = obj.GetType();
        if (!ValueWrapper.IsSupport(type)) return obj;

        _wrapperTable ??= new();
        if (!_wrapperTable.TryGetValue(type, out var wrapObj))
        {
            wrapObj = (ValueWrapper)Activator.CreateInstance(ValueWrapper.GetWrapperType(type));
            _wrapperTable[type] = wrapObj;
        }


        wrapObj.obj = obj;
        return wrapObj;
    }

    private abstract class ValueWrapper
    {
        public static Type GetWrapperType(Type type) => typeof(ValueWrapper<>).MakeGenericType(type);

        private static readonly HashSet<Type> SupportedTypes = new()
        {
            typeof(string),
            typeof(Vector2Int), typeof(Vector3Int),
            typeof(Rect), typeof(RectOffset),
            typeof(Bounds), typeof(BoundsInt)
        };

        public static bool IsSupport(Type type)
        {
            return type.IsPrimitive
                   || SupportedTypes.Contains(type)
                   || type.IsArray
                   || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                ;
        }

        public abstract object obj { get; set; }
    }

    private class ValueWrapper<T> : ValueWrapper
    {
        // Public to serialize with JsonUtility
        // ReSharper disable once MemberCanBePrivate.Local
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public T value;

        public override object obj
        {
            get => value;
            set => this.value = (T)value;
        } 
    }
}
