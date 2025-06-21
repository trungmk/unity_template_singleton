using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public class GameData : BaseSystem
    {
        private IGameDefinitionManager _gameDefinitionManager = null;

        private readonly ILocalDataManager _localDataManager;

        private static GameData _instance;

        public Action OnLoadDataCompleted { get; set; }

        public Action OnLoadLocalDataCompleted { get; set; }

        public bool IsInitialized { get; private set; }

        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameData>();
                    if (_instance == null)
                    {
                        var go = new GameObject();
                        _instance = go.AddComponent<GameData>();
                    }
                }

                return _instance;
            }
        }

        public IGameDefinitionManager GameDefinitionManager => _gameDefinitionManager;

        private void Awake()
        {
            _instance = this;
        }

        public void FetchDataFromRemoteServer()
        {

            // bypass
            Handle_OnFetchDataTimeOut();
            return;

            //// Analytic
            //OneShootAnalyticEvent oneShootAnalyticEvent = OneShootAnalyticEvent.GetInstance();
            //oneShootAnalyticEvent.SetUpEvent(FirebaseConstant.ConfigBegin);
            //EventManager.Instance.Dispatch(oneShootAnalyticEvent);
            //// End Analytic

            //FirebaseService.Instance.RemoteConfigController.OnFetchingDataCompleted -= Handle_OnFetchingDataCompleted;
            //FirebaseService.Instance.RemoteConfigController.OnFetchingDataCompleted += Handle_OnFetchingDataCompleted;
            //FirebaseService.Instance.RemoteConfigController.OnFetchDataTimeOut -= Handle_OnFetchDataTimeOut;
            //FirebaseService.Instance.RemoteConfigController.OnFetchDataTimeOut += Handle_OnFetchDataTimeOut;

            StartCoroutine(CheckNetwork((val) =>
            {
                if (val)
                {
                    Debug.Log("Network is good!");
                    //FirebaseService.Instance.RemoteConfigController.InitializeFirebaseRemoteConfig();
                }
                else
                {
                    Debug.Log("Network is poor!");
                    Handle_OnFetchDataTimeOut();
                }
            }));
        }

        private IEnumerator CheckNetwork(Action<bool> callback)
        {
            bool isNetworkAvailable;

            yield return new WaitForSeconds(1.5f);

            // ping to an address to check network
            using UnityWebRequest www = UnityWebRequest.Get("https://www.google.com/");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                isNetworkAvailable = true;
            }
            else
            {
                isNetworkAvailable = false;
            }

            if (callback != null)
            {
                callback(isNetworkAvailable);
            }
        }

        private void Handle_OnFetchDataTimeOut()
        {
            if (_gameDefinitionManager == null)
            {
                _gameDefinitionManager = new GameDefinitionManager();

                _gameDefinitionManager.OnInitCompleted = () =>
                {
                    IsInitialized = true;
                };

                _gameDefinitionManager.Init();
                Debug.Log("DEBUG: Init local data!");
            }
        }

        private void Handle_OnFetchingDataCompleted()
        {
            if (_gameDefinitionManager == null)
            {
                IsInitialized = true;

                //_gameDefinitionManager = FirebaseService.Instance.RemoteConfigController.RemoteConfigManager;

                //// Analytic
                //OneShootAnalyticEvent oneShootAnalyticEvent = OneShootAnalyticEvent.GetInstance();
                //oneShootAnalyticEvent.SetUpEvent(FirebaseConstant.ConfigDone);
                //EventManager.Instance.Dispatch(oneShootAnalyticEvent);
                //// End Analytic
            }
        }

        public void InitLocalData()
        {
            _localDataManager.OnLoadDataCompleted = () =>
            {
                if (OnLoadLocalDataCompleted != null)
                {
                    OnLoadLocalDataCompleted();
                }
            };

            _localDataManager.Init();
        }

        public List<T> LoadGameDefinition<T>() where T : IGameDefinition
        {
            return _gameDefinitionManager.GetDefinitions<T>();
        }

        public T LoadLocalData<T>() where T : ILocalData
        {
            return _localDataManager.GetData<T>();
        }

        public void SaveLocalData<T>() where T : ILocalData
        {
            _localDataManager.SaveData<T>();
        }

        public void SaveLocalData<T>(object obj) where T : ILocalData
        {
            _localDataManager.SaveData<T>(obj);
        }
    }
}
