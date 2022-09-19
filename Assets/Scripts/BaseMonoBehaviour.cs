using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BaseMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake() { }

    protected virtual void OnDestroy()
    {
        FieldInfo[] allFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in allFields)
        {
            Type fieldType = field.FieldType;
            if (typeof(IList<>).IsAssignableFrom(fieldType))
                if (field.GetValue(this) is IList list)
                    list.Clear();
            if (typeof(IDictionary).IsAssignableFrom(fieldType))
                if (field.GetValue(this) is IDictionary dictionary)
                    dictionary.Clear();
            if (!fieldType.IsPrimitive)
                field.SetValue(this, null);
        }
    }
}