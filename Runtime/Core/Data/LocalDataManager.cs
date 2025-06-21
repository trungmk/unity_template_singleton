using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class LocalDataManagerBase : ILocalDataManager
    {
        public Action OnLoadDataCompleted { get; set; }

        private readonly Dictionary<Type, ILocalDataWrapper> _localDataWrapperDict = new Dictionary<Type, ILocalDataWrapper>();

        private bool _isInit;

        public void Init()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            ConfigureLocalData();

            using Dictionary<Type, ILocalDataWrapper>.Enumerator enumerator = _localDataWrapperDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ILocalDataWrapper localDataWrapper = enumerator.Current.Value;
                localDataWrapper.Init();
            }

            if (OnLoadDataCompleted != null)
            {
                OnLoadDataCompleted();
            }
        }

        public void SaveData<T>() where T : ILocalData
        {
            Type key = typeof(T);
            ILocalDataWrapper wrapper = _localDataWrapperDict[key];
            wrapper.SaveData<T>();
        }

        public void SaveData<T>(object obj) where T : ILocalData
        {
            Type key = typeof(T);
            ILocalDataWrapper wrapper = _localDataWrapperDict[key];
            wrapper.SaveData<T>(obj);
        }

        public T GetData<T>() where T : ILocalData
        {
            Type key = typeof(T);

            if (_localDataWrapperDict.TryGetValue(key, out ILocalDataWrapper wrapper))
            {
                if (wrapper != null)
                {
                    return wrapper.GetData<T>();
                }
            }

            return default;
        }

        protected virtual void ConfigureLocalData() { }

        protected virtual void AssignLocalData<T>(ILocalDataWrapper localDataWrapper) where T : ILocalData
        {
            Type key = typeof(T);
            if (!_localDataWrapperDict.ContainsKey(key))
            {
                _localDataWrapperDict.Add(key, localDataWrapper);
            }
            else
            {
                Debug.LogError($"The type {key} already exists in the dictionary.");
            }
        }
    }
}


