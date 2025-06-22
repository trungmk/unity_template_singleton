using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YNWA;
using Cysharp.Threading.Tasks;

namespace Core
{
    public class ObjectPooling : BaseSystem
    {
        [SerializeField] 
        private PoolItem[] _poolItems = default;
        
        private readonly Dictionary<PoolItemType, List<PooledMono>> _pooledDict = new Dictionary<PoolItemType, List<PooledMono>>();
        
        private Transform _transform;

        private Action _onPreUnloadedEvent;
        
        private static ObjectPooling _instance;

        private bool _isInit;
        
        public event Action OnLoadPoolsCompleted {get; set;}
        
        public static ObjectPooling Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ObjectPooling>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            _transform = transform;
            _instance = this;
        }
        
        public void Init(Action<List<PooledMono>> callback)
        {
            if (_isInit)
            {
                if (callback != null)
                {
                    callback(_pooledDict[PoolItemType.Player]);
                }
                
                return;
                
            }
            
            _isInit = true;
            InitInternal(callback);
        }

        public override void OnPreUnloaded()
        {
            _onPreUnloadedEvent?.Invoke();
        }
        
        public void GetAllAllottedPooledObjects()
        {
            _onPreUnloadedEvent?.Invoke();
        }

        public async UniTask<T> Get<T>(string addressName, PoolItemType poolItemType = PoolItemType.General, bool isActive = true) where T : Component
        {
            if (_pooledDict.TryGetValue(poolItemType, out List<PooledMono> pooledMonos))
            {
                for (int i = 0; i < pooledMonos.Count; i++)
                {
                    PooledMono pooledMono = pooledMonos[i];
                    if (string.Equals(pooledMono.AddressName, addressName))
                    {
                        pooledMonos.RemoveAt(i);
                        pooledMono.gameObject.SetActive(isActive);
                        return pooledMono.GetComponent<T>();
                    }
                }
            }

            PooledMono mono = await CreateObject(addressName, poolItemType);
            if (mono != null)
            {
                mono.gameObject.SetActive(isActive);
                return mono.GetComponent<T>();
            }

            Debug.LogError($"* ObjectPooling: Could not get object with type: { typeof(T) } at address: { addressName } with pool type: { poolItemType }");
            return default(T);
        }
        
        public async UniTask<T> Get<T>(string addressName, Transform parentTransform, PoolItemType poolItemType = PoolItemType.General, bool isActive = true) where T : Component
        {
            T genericObject = await Get<T>(addressName, poolItemType, isActive);
            
            if (genericObject != null)
            {
                genericObject.transform.SetParent(parentTransform);
            }

            return genericObject;
        }
        
        public async UniTask<GameObject> Get(string addressName, PoolItemType poolItemType = PoolItemType.General, bool isActive = true)
        {
            if (_pooledDict.TryGetValue(poolItemType, out List<PooledMono> pooledMonos))
            {
                for (int i = 0; i < pooledMonos.Count; i++)
                {
                    PooledMono pooledMono = pooledMonos[i];
                    if (string.Equals(pooledMono.AddressName, addressName))
                    {
                        pooledMonos.RemoveAt(i);
                        GameObject gameObj;
                        (gameObj = pooledMono.gameObject).SetActive(isActive);
                        return gameObj;
                    }
                }
            }

            PooledMono mono = await CreateObject(addressName, poolItemType);
            if (mono != null)
            {
                GameObject gameObj;
                (gameObj = mono.gameObject).SetActive(isActive);
                return gameObj;
            }

            Debug.LogError($"* ObjectPooling: Could not get object at address: { addressName } with pool type: { poolItemType }");
            return null;
        }

        public async UniTask<T> Get<T>(PoolItemType poolItemType = PoolItemType.General, bool isActive = true) where T : Component
        {
            if (_pooledDict.TryGetValue(poolItemType, out List<PooledMono> pooledMonos))
            {
                for (int i = 0; i < pooledMonos.Count; i++)
                {
                    PooledMono pooledMono = pooledMonos[i];

                    T componentOutCome = pooledMono.GetComponent<T>();
                    if (componentOutCome != null)
                    {
                        pooledMonos.RemoveAt(i);
                        componentOutCome.gameObject.SetActive(isActive);
                        return componentOutCome;
                    }
                }
            }

            for (int i = 0; i < _poolItems.Length; i++)
            {
                PoolItem poolItem = _poolItems[i];
                if (poolItem.PoolType != poolItemType)
                {
                    continue;
                }
                
                PooledMono mono = await CreateObject(poolItem.AddressName, poolItem.PoolType);
                if (mono != null)
                {
                    GameObject gameObj;
                    (gameObj = mono.gameObject).SetActive(isActive);
                    return gameObj.GetComponent<T>();
                }
            }

            Debug.LogError($"* ObjectPooling: Could not get object with type: { typeof(T) } with type: { poolItemType }");
            return default(T);
        }

        public async UniTask<T> Get<T>(Transform parent, PoolItemType poolItemType = PoolItemType.General, bool isActive = true, bool resetTransform = false) where T : Component
        {
            T pooledObject = await Get<T>(poolItemType, isActive);
            if(pooledObject == null)
            {
                Debug.LogError(typeof(T).ToString() + " type does NOT exist in pool");
                return null;
            }

            pooledObject.transform.SetParent(parent);
            if (resetTransform)
            {
                var pooledTransform = pooledObject.transform;
                pooledTransform.localPosition = Vector3.zero;
                pooledTransform.localRotation = Quaternion.identity;
            }

            return pooledObject;
        }
        
        public void DestroyObject(PoolItemType itemType)
        {
            if (_pooledDict.TryGetValue(itemType, out List<PooledMono> pooledMonos))
            {
                for (int i = pooledMonos.Count - 1; i >= 0 ; i--)
                {
                    pooledMonos.RemoveAt(i);
                    Destroy(pooledMonos[i].gameObject);
                }
                
                pooledMonos.Clear();
            }
        }
        
        public void DestroyAllObjects()
        {
            using (var pooledGroup = _pooledDict.GetEnumerator())
            {
                List<PooledMono> pooledMonos = pooledGroup.Current.Value;
                
                for (int i = pooledMonos.Count - 1; i >= 0 ; i--)
                {
                    pooledMonos.RemoveAt(i);
                    Destroy(pooledMonos[i].gameObject);
                }
                
                pooledMonos.Clear();
            }
        }
        
        public async UniTask Preload(string addressName, 
            int count, 
            PoolItemType poolItemType = PoolItemType.General, 
            bool isActive = false)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject pooledObject = await Get(addressName, poolItemType, isActive);
                if(pooledObject == null)
                {
                    Debug.LogError("addressName:= " + addressName + " is NOT exist in pool");
                    continue;
                }
                
                AddToPooledDict(poolItemType, pooledObject.GetComponent<PooledMono>());
            }
        }

        private async UniTask InitInternal(Action<List<PooledMono>> callback)
        {
            for (int i = 0; i < _poolItems.Length; i++)
            {
                PoolItem poolItem = _poolItems[i];

                for (int j = 0; j < poolItem.AmountToPool; j++)
                {
                    PooledMono pooledMono = await CreateObject(poolItem.AddressName, poolItem.PoolType);
                    AddToPooledDict(pooledMono.PoolItemType, pooledMono);
                }

                if (poolItem.NeedModifyAfterInit && callback != null)
                {
                    callback(GetFromPooledDict(poolItem.PoolType));
                }
            }

            if (OnLoadPoolsCompleted != null)
            {
                OnLoadPoolsCompleted();
            }
        }
        
        private List<PooledMono> GetFromPooledDict(PoolItemType poolItemType)
        {
            if (_pooledDict.TryGetValue(poolItemType, out List<PooledMono> pooledMonos))
            {
                return pooledMonos;
            }
            
            return pooledMonos;
        }

        private void AddToPooledDict(PoolItemType poolItemType, PooledMono pooledMono)
        {
            if (_pooledDict.TryGetValue(poolItemType, out List<PooledMono> pooledMonos))
            {
                if (!pooledMonos.Contains(pooledMono))
                {
                    pooledMonos.Add(pooledMono);
                }
            }
            else
            {
                _pooledDict.Add(poolItemType, new List<PooledMono>
                {
                    pooledMono
                });
            }
        }

        private PooledMono CreateObject(PooledMono objectToPool, PoolItemType poolItemType)
        {
            GameObject gameObj = AssetManager.Instance.InstantiateGameObject(objectToPool, transform, isActive: false);
            PooledMono pooledItem = gameObj.GetComponent<PooledMono>();
            pooledItem.OnBackToPoolEvent = ReturnToPool;
            _onPreUnloadedEvent += pooledItem.Handle_UnPreload;
            pooledItem.Init(objectToPool.name, poolItemType);
            
            return pooledItem;
        }
        
        private async UniTask<PooledMono> CreateObject(string addressName, PoolItemType poolItemType)
        {
            GameObject gameObj = await AssetManager.Instance.InstantiateAsync(addressName, transform);

            if (gameObj == null)
            {
                Debug.LogError($"* ObjectPooling: could not load object at address: { addressName }");
                return null;
            }
            
            PooledMono pooledItem = gameObj.GetComponent<PooledMono>();
            
            if (pooledItem == null)
            {
                pooledItem = gameObj.AddComponent<PooledMono>();
            }
            
            pooledItem.OnBackToPoolEvent = ReturnToPool;
            _onPreUnloadedEvent += pooledItem.Handle_UnPreload;
            pooledItem.Init(addressName, poolItemType);
            gameObj.SetActive(false);

            return pooledItem;
        }
        
        private PooledMono CreateObject(GameObject gameObjectPrefab, PoolItemType poolItemType)
        {
            if (gameObjectPrefab == null)
            {
                Debug.LogError("* ObjectPooling: could not create object because object prefab is null");
                return null;
            }
            
            GameObject gameObj = AssetManager.Instance.InstantiateGameObject(gameObjectPrefab, transform, false);
            
            if (gameObj == null)
            {
                Debug.LogError($"* ObjectPooling: ObjectPooling: could not create object { gameObjectPrefab.name }");
                return null;
            }
            
            gameObj.SetActive(false);
            PooledMono pooledItem = gameObj.GetComponent<PooledMono>();

            if (pooledItem == null)
            {
                pooledItem = gameObj.AddComponent<PooledMono>();
            }

            pooledItem.OnBackToPoolEvent = ReturnToPool;
            _onPreUnloadedEvent += pooledItem.Handle_UnPreload;
            pooledItem.Init(string.Empty, poolItemType);
            
            return pooledItem;
        }
        
        private void ReturnToPool(PooledMono objectToReturn)
        {
            Transform objTransform = objectToReturn.transform;
            if (objTransform.parent == transform)
            {
                return;
            }

            objectToReturn.gameObject.SetActive(false);
            objTransform.SetParent(transform);
            objTransform.localPosition = Vector3.zero;
            AddToPooledDict(objectToReturn.PoolItemType, objectToReturn);
        }
        
        public void ReturnToPool(GameObject objectToReturn)
        {
            if (objectToReturn == null 
                || objectToReturn.gameObject == null
                || objectToReturn.transform.parent == this.transform)
            {
                return;
            }

            PooledMono[] pooledMonos = objectToReturn.GetComponentsInChildren<PooledMono>();

            if (pooledMonos == null)
            {
                objectToReturn.gameObject.AddComponent<PooledMono>();
            }

            if (pooledMonos != null)
            {
                for (int i = 0; i < pooledMonos.Length; i++)
                {
                    PooledMono pooledMono = pooledMonos[i];
                    pooledMono.ReturnToPool();
                }
            }
        }
    }
}

