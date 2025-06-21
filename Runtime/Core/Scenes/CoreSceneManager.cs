using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Core
{
    public class CoreSceneManager : BaseSystem
    {
        [Header("Context configs")]
        public List<SceneSO> Scenes;
    
        public SceneSO InitializeScene;
        
        #if UNITY_EDITOR

        [Header("Only use for editor")] 
        public string GeneratePath;

        #endif

        public Action OnSceneUnloaded;
        
        public Action OnScenePreUnloaded;
        
        public Action OnSceneLoaded;

        public SceneSO CurrentScene { get; private set; }
        
        public static string NextSceneName { get; private set; }
        
        public static string CurrentSceneName { get; private set; }
    
        private SceneController _currentController;
        
        private static CoreSceneManager _instance;

        private AsyncOperationHandle<SceneInstance> _currentSceneAsyncOperation;
        
        public static CoreSceneManager Instance 
        {
            get 
            {
                if (_instance == null) 
                {
                    _instance = FindFirstObjectByType<CoreSceneManager>();
                }
                
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        public void LoadInitScene()
        {
            Timing.RunCoroutine(ProcessLoadScene(InitializeScene.SceneName));
        }

        private IEnumerator<float> UnloadScene()
        {
            if (_currentController != null)
            {
                _currentController.OnPreUnloaded();
                
                if (OnScenePreUnloaded != null)
                {
                    OnScenePreUnloaded();
                }

                yield return Timing.WaitForOneFrame;
            }

            Scene currentScene = SceneManager.GetSceneByName(CurrentSceneName);
            if (currentScene.isLoaded)
            {
                if (_currentController != null)
                {
                    _currentController.OnUnloaded();
                    
                    yield return Timing.WaitForOneFrame;
                }

                yield return Timing.WaitUntilDone(SceneManager.UnloadSceneAsync(CurrentSceneName));

                //  asyncTask1 = null;
                //
                // AsyncOperationHandle<SceneInstance> asyncTask = Addressables.UnloadSceneAsync(asyncTask1);

                yield return Timing.WaitForOneFrame;
                
                OnSceneUnloaded?.Invoke();
            }
            
            yield return Timing.WaitForOneFrame;
        }

        private IEnumerator<float> LoadContext(string sceneName)
        {
            NextSceneName = sceneName;

            yield return Timing.WaitUntilDone(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive));
            
            CurrentSceneName = sceneName;
            
            yield return Timing.WaitForOneFrame;
    
            if(_currentController != null)
            {
                CurrentScene = GetSceneSOByName(sceneName);
                _currentController.OnLoaded();
                
                if (OnSceneLoaded != null)
                {
                    OnSceneLoaded();
                }
            }
        }
    
        private IEnumerator<float> ProcessLoadScene(string sceneName, Action loadContextCompleted = null)
        { 
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(UnloadScene()));

            yield return Timing.WaitForOneFrame;
    
            Timing.RunCoroutine(LoadContext(sceneName));
    
            if (loadContextCompleted != null)
            {
                loadContextCompleted();
            }
        }
    
        private SceneSO GetSceneSOByName(string sceneName)
        {
            for (int i = 0; i < Scenes.Count; i++)
            {
                SceneSO context = Scenes[i];
                if(string.Equals(context.SceneName, sceneName))
                {
                    return context;
                }
            }
    
            return null;
        }

        public void SetContextController(SceneController controller)
        {
            _currentController = controller;
        }

        public void ChangeScene(int contextKey)
        {
            string contextName = SceneHandler.GetSceneName(contextKey);

            if (!string.IsNullOrEmpty(contextName))
            {
                Timing.RunCoroutine(ProcessLoadScene(contextName));
            }
        }

        public void ChangeGameToPause(bool isPause)
        {
            if (_currentController == null)
            {
                return;
            }

            if (isPause)
            {
                _currentController.OnPause();
            }
            else
            {
                _currentController.OnResume();
            }
        }

        public void UpdateContext(float deltaTime)
        {
            if (_currentController == null)
            {
                return;
            }

            _currentController.OnUpdate(deltaTime);
        }

        public void LateUpdateContext(float fixedDeltaTime)
        {
            if (_currentController == null)
            {
                return;
            }

            _currentController.OnLateUpdate(fixedDeltaTime);
        }

        public void FixedUpdateContext(float deltaTime)
        {
            if (_currentController == null)
            {
                return;
            }

            _currentController.OnFixedUpdate(deltaTime);
        }
        
        protected virtual void OnDestroy()
        {
            if (this == Instance)
            {
                _instance = null;
            }
        }
    }
}

