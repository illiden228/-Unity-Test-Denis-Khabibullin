using System;
using UnityEngine;

public interface IPoolObject<T> : IDisposable
{
    public T Get();
    void Return(GameObject prefab);
}