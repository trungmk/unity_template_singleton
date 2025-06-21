using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Core
{
    public class AssetManager : BaseSystem
    {
        private static AssetManager _instance;
        
        public static AssetManager Instance 
        {
            get 
            {
                if (_instance == null) 
                {
                    _instance = FindFirstObjectByType<AssetManager> ();
                }
                
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        public async UniTask<UnityEngine.Object> LoadAssetAsync(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            AsyncOperationHandle<UnityEngine.Object> async = Addressables.LoadAssetAsync<UnityEngine.Object>(address);
            UnityEngine.Object gameObjAsset = await async.Task.AsUniTask();

            if (gameObjAsset != null)
            {
                return gameObjAsset;
            }

            Debug.LogError("Could not load asset from: " + address);
            return null;
        }
        
        public async UniTask<UnityEngine.Object> LoadAssetAsync<T>(string address) where T : Component
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            AsyncOperationHandle<UnityEngine.Object> async = Addressables.LoadAssetAsync<UnityEngine.Object>(address);
            UnityEngine.Object gameObjAsset = await async.Task.AsUniTask();

            if (gameObjAsset != null)
            {
                return gameObjAsset;
            }

            Debug.LogError("Could not load asset from: " + address);
            return null;
        }
        
        public async UniTask<T> LoadFromTextAssetAsync<T>(string address) where T : class
        {
            TextAsset gameObjAsset = await LoadAssetAsync(address) as TextAsset;

            T data = default(T);
            if (gameObjAsset != null && !string.Equals(gameObjAsset.text, String.Empty))
            {
                data = JsonConvert.DeserializeObject<T>(gameObjAsset.text);
            }

            return data;
        }

        /// <summary>
        /// NOTE: This function will automatically add the object that instantiated into CoreObjectManager and Service Locator
        /// </summary>
        /// <param name="address">Address of the object that is set in addressable system</param>
        /// <param name="addToCoreObjectManager">true: means this object is a part of CoreObject life cycle.</param>
        /// <param name="useServiceLocator">true: means this object is injected by Zenject.</param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateAsync(string address)
        {
            Object asset = await LoadAssetAsync(address);

            if (asset == null)
            {
                return null;
            }
            
            GameObject prefab = GetPrefabAsGameObject(asset);
            prefab.SetActive(false);
            GameObject go = UnityEngine.Object.Instantiate(prefab);

            prefab.SetActive(true);
            go.SetActive(true);
            return go;
        }
        
        public async UniTask<T> InstantiateAsync<T> (string address) where  T : Component
        {
            GameObject go = await InstantiateAsync(address);
            T component = null;
            
            if (go != null)
            {
                component = go.GetComponent<T>();
            }

            return component;
        }

        public async UniTask<T> InstantiateAsync<T> (string address, 
            Transform parent,
            bool worldSpace = true) where  T : Component
        {
            GameObject go = await InstantiateAsync(address);
            T component = null;
            
            if (go != null)
            {
                go.transform.SetParent(parent, worldSpace);
                component = go.GetComponent<T>();
            }

            return component;
        }

        public async UniTask<GameObject> InstantiateAsync(string address, Transform parent, bool worldSpace = true)
        {
            GameObject go = await InstantiateAsync(address);

            if (go != null)
            {
                go.transform.SetParent(parent, worldSpace);
            }

            return go;
        }
        
        public async UniTask<GameObject> InstantiateAsync(AssetReference assetReference, Transform parent = null)
        {
            GameObject goInstance = await assetReference.InstantiateAsync(parent).Task.AsUniTask();
            return goInstance;
        }
        
        public async UniTask<T> InstantiateAsync<T> (AssetReference assetReference, Transform parent = null) where  T : Component
        {
            GameObject go = await InstantiateAsync(assetReference, parent);
            T component = null;
            
            if (go != null)
            {
                component = go.GetComponent<T>();
            }

            return component;
        }
        
        public async UniTask<Object> InstantiateAsync(string address, AssetLabelReference assetLabelReference)
        {
            Object obj = await InstantiateAsync(address, assetLabelReference.labelString);
            return obj;
        }

        public async UniTask<Object> InstantiateAsync(string address, string label)
        {
            if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(label))
            {
                return null;
            }
            
            List<string> addresses = new List<string>
            {
                address,
                label
            };
            
            AsyncOperationHandle<Object> async = Addressables.LoadAssetAsync<Object>(addresses);
            Object obj = await async.Task.AsUniTask();
            return obj;
        }
        
        public async UniTask<IList<Object>> LoadAssetsAsync(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return null;
            }

            IEnumerable<object> labels = new List<object>()
            {
                label
            };

            return await Addressables.LoadAssetsAsync<Object>(labels, null, Addressables.MergeMode.Union).Task.AsUniTask();
        }
        
        public async UniTask<IList<Object>> LoadAssetsAsync(IEnumerable<object> addresses)
        {
            if (addresses == null)
            {
                return null;
            }

            return await Addressables.LoadAssetsAsync<Object>(addresses, null, Addressables.MergeMode.Union).Task.AsUniTask();
        } 

        public async UniTask LoadResourceAsync<T>(string resourcePath, Action<bool, T> callback) where T : Object
        {
             ResourceRequest resourceRequest = Resources.LoadAsync<T>(resourcePath);
             await resourceRequest.ToUniTask();

             GameObject go = resourceRequest.asset as GameObject;
             T asset = go != null ? go.GetComponent<T>() : null;
             
             callback?.Invoke(asset != null, asset);
        }

        public bool UnloadAsset(Object asset)
        {
            if (asset == null)
            {
                return false;
            }
            
            Addressables.Release(asset);
            
            return true;
        }

        public void ReleaseInstance(GameObject go)                                           
        {
            Addressables.ReleaseInstance(go);
        }

        public GameObject InstantiateGameObject(Object unityObj, bool isActive = true)
        {
            GameObject gameObj = UnityEngine.Object.Instantiate(unityObj) as GameObject;
            gameObj.gameObject.SetActive(isActive);
            return gameObj;
        }
        
        public GameObject InstantiateGameObject(Object unityObj, Transform parent, bool isActive = true, bool worldPositionStays = true)
        {
            GameObject prefab = GetPrefabAsGameObject(unityObj);
            prefab.SetActive(false);
            
            GameObject gameObj = UnityEngine.Object.Instantiate(prefab);

            if (gameObj != null)
            {
                gameObj.transform.SetParent(parent, worldPositionStays);
                
                return gameObj;
            }

            prefab.SetActive(true);
            gameObj.gameObject.SetActive(isActive);
            
            return null;
        }
        
        private GameObject GetPrefabAsGameObject(UnityEngine.Object prefab)
        {
            if (prefab is GameObject gameObj)
            {
                return gameObj;
            }

            return ((Component) prefab).gameObject;
        }
    }
}


