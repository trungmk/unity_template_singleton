using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


namespace Core
{
    /// <summary>
    /// Menu path: Initialization Node/Create
    /// [CreateAssetMenu(fileName = "AdNode", menuName = "Initialization Node/Create")]
    /// </summary>
    public abstract class StartupTaskBase : ScriptableObject
    {
        public StartupTaskBase[] DependencyTasks;

        public bool IsDefault;

        public bool HasStarted { get; set; }
        
        public bool HasCompleted { get; set; }

        public virtual void Init()
        {
            HasStarted = false;
            HasCompleted = false;
        }

        public abstract void Execute();

        public bool CanExecution()
        {
            if (HasStarted || HasCompleted)
            {
                return false;
            }
            
            if (!HasStarted 
                && DependencyTasks == null
                && (DependencyTasks != null && DependencyTasks.Length == 0))
            {
                return true;
            }

            if (DependencyTasks != null)
            {
                for (int i = 0; i < DependencyTasks.Length; i++)
                {
                    StartupTaskBase node = DependencyTasks[i];

                    if (node == null)
                    {
                        Debug.LogError("InitializationNodeBase: " + name + " there is a dependency node is null");
                        continue;
                    }

                    if (!node.HasStarted
                        || (node.HasStarted && !node.HasCompleted))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}