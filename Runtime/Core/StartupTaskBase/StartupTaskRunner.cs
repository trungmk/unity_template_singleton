using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;


namespace Core
{
    /// <summary>
    /// This class will execute all start up tasks
    /// This runner is put in Booting scene.
    /// </summary>
    public class StartupTaskRunner : BaseSystem
    {
        public StartupTaskBase[] StartupTasks = default;
        
        public Action OnInitializeCompleted = delegate { };
    
        private readonly List<StartupTaskBase> _nodesDefault = new List<StartupTaskBase>();
        
        private readonly List<StartupTaskBase> _nodes = new List<StartupTaskBase>();

        private static StartupTaskRunner _instance;
        
        public static StartupTaskRunner Instance 
        {
            get 
            {
                if (_instance == null) 
                {
                    _instance = FindFirstObjectByType<StartupTaskRunner>();
                }
                
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        /// <summary>
        /// Init configs, bundles, ads, ...
        /// </summary>
        public void Init()
        {
            SetupNodes();
            Timing.RunCoroutine(ProcessNodes());
        }

        private void SetupNodes()
        {
            for (int i = 0; i < StartupTasks.Length; i++)
            {
                StartupTaskBase node = StartupTasks[i];
                node.Init();
    
                if (node.IsDefault)
                {
                    _nodesDefault.Add(node);
                }
                else
                {
                    _nodes.Add(node);
                }
            }
        }
        
        private IEnumerator<float> ProcessNodes()
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Process(_nodesDefault)));

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Process(_nodes)));

            OnInitializeCompleted?.Invoke();
        }
    
        private IEnumerator<float> Process(List<StartupTaskBase> nodes)
        {
            int count = 0;

            while (count < nodes.Count)
            {
                StartupTaskBase currentNodesRunning = null;    
    
                for (int i = 0; i < nodes.Count; i++)
                {
                    currentNodesRunning = nodes[i];
    
                    if (currentNodesRunning == null)
                    {
                        Debug.LogError("InitializationRunner: Initialization node null at index:= " + i);
                        continue;
                    }
    
                    if (currentNodesRunning.CanExecution())
                    {
                        currentNodesRunning.HasStarted = true;
                        currentNodesRunning.Execute();
                        break;
                    }
                }
                
                if (currentNodesRunning != null)
                {
                    count++;
                    while (!currentNodesRunning.HasCompleted)
                    {
                        yield return Timing.WaitForOneFrame;
                    }
                }
    
                yield return Timing.WaitForOneFrame;
            }
        }
    }
}

