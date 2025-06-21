using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using UnityEngine;

namespace Core
{
    public class GameHub : MonoSingleton<GameHub>
    {
        [SerializeField] 
        private CoreSceneManager _sceneManager = default;

        [SerializeField] 
        private Transform _gameSystemGroup = default;

        public static Action<bool> OnGamePause { get; set; }
        
        private const int FIXED_FRAME_RATE = 60;

        private bool _pauseStatus;

        private List<BaseSystem> _CoreBaseSystems;

        public List<BaseSystem> CoreBaseSystems => _CoreBaseSystems;

        private int _baseSystemCount = 0;

        private void Awake()
        {
            Application.targetFrameRate = FIXED_FRAME_RATE;
            _CoreBaseSystems = _gameSystemGroup.GetComponentsInChildren<BaseSystem>().ToList();
        }

        private void Start()
        {
            #if UNITY_ANDROID
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            #endif
            
            EnterPoint();
        }

        /// <summary>
        /// The simple entry point that init the game.
        /// </summary>
        public void EnterPoint()
        {
            if (_sceneManager == null)
            {
                Debug.LogError("ContextManager is NULL!");
            }

            _baseSystemCount = _CoreBaseSystems.Count;
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].Initialize();
            }

            _sceneManager.OnSceneLoaded = Handle_OnSceneLoaded;
            _sceneManager.OnSceneUnloaded = Handle_OnSceneUnloaded;
            _sceneManager.OnScenePreUnloaded = Handle_OnScenePreUnloaded;
            _sceneManager.LoadInitScene();
        }

        private void Handle_OnScenePreUnloaded()
        {
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnPreUnloaded();
            }
        }

        private void Handle_OnSceneLoaded()
        {
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnLoaded();
            }
        }
        
        private void Handle_OnSceneUnloaded()
        {
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnUnloaded();
            }
        }
        
// //#if UNITY_EDITOR || UNITY_IOS
//         private void OnApplicationFocus(bool hasFocus)
//         {
//             _pauseStatus = !hasFocus;
//
//             if (!_pauseStatus)
//             {
//                 if (_CoreBaseSystems == null)
//                 {
//                     return;
//                 }
//                 
//                 for (int i = 0; i < _baseSystemCount; i++)
//                 {
//                     _CoreBaseSystems[i].OnResume();
//                 }
//             }
//             else
//             {
//                 if (_CoreBaseSystems == null)
//                 {
//                     return;
//                 }
//                 
//                 for (int i = 0; i < _baseSystemCount; i++)
//                 {
//                     _CoreBaseSystems[i].OnPause();
//                 }
//             }
//             
//             _sceneManager.ChangeGameToPause(_pauseStatus);
//             
//             if (this != null)
//             {
//                 OnGamePause?.Invoke(_pauseStatus);
//             }
//             else
//             {
//                 Debug.LogError("OnApplicationPause with GameHub instance is null!");
//             }
//         }
// //#endif

        public void PauseGame()
        {
            if (_CoreBaseSystems == null)
            {
                return;
            }
                
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnPause();
            }
            
            _sceneManager.ChangeGameToPause(true);
        }
        
        public void ResumeGame()
        {
            if (_CoreBaseSystems == null)
            {
                return;
            }
                
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnResume();
            }
            
            _sceneManager.ChangeGameToPause(false);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _pauseStatus = pauseStatus;

            if (_pauseStatus)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
            
            if (this != null)
            {
                OnGamePause?.Invoke(pauseStatus);
            }
            else
            {
                Debug.LogError("OnApplicationPause with GameHub instance is null!");
            }
        }

        private void OnApplicationQuit()
        {
            GC.Collect();
        }
    }
}


