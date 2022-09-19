using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class BaseDisposable : IDisposable
{
    protected bool isDisposed;
    private List<IDisposable> _disposables;
    private List<Object> _unityObjects;

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            if (_disposables != null)
            {
                List<IDisposable> disposables = _disposables;
                for (int i = _disposables.Count - 1; i >= 0 ; i--)
                {
                    _disposables[i]?.Dispose();
                }
                disposables.Clear();
            }
        }
            
        try
        {
            OnDispose();
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception when disposing {GetType().Name}: {e}");
        }
            
        if (_unityObjects != null)
            for (int index = _unityObjects.Count - 1; index >= 0; index--)
            {
                Object obj = _unityObjects[index];
                if (obj)
                    Object.Destroy(obj);
            }
    }
    
    protected virtual void OnDispose() { }
        
    protected TDisposable AddDispose<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
    {
        if (disposable == null)
            return default;
        if (_disposables == null)
            _disposables = new List<IDisposable>(1);
        _disposables.Add(disposable);
        return disposable;
    }

    protected TObject AddObject<TObject>(TObject obj) where TObject : Object
    {
        if (_unityObjects == null)
            _unityObjects = new List<Object>(1);
        _unityObjects.Add(obj);
        return obj;
    }
}