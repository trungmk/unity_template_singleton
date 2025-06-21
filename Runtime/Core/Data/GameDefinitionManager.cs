using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
    public class GameDefinitionManager : IGameDefinitionManager
    {
        private readonly Dictionary<Type, IGameDefinitionWrapper> _gameDefinitionWrapperDict = new Dictionary<Type, IGameDefinitionWrapper>();
        
        private const string GAME_DEFINITION_LABEL = "GameDefinition";
        
        public Action OnInitCompleted { get; set; }

        public void Init()
        {
            LoadGameDefinitions();
        }

        public void Init(IDictionary<string, object> values)
        {
            
        }

        public List<T> GetDefinitions<T>() where T : IGameDefinition
        {
            Type key = typeof(T);
            IGameDefinitionWrapper wrapper = _gameDefinitionWrapperDict[key];

            if (wrapper != null)
            {
                return wrapper.GetDefinitions<T>();
            }
            
            return null;
        }

        private async void LoadGameDefinitions()
        {
            IList<Object> objs = await AssetManager.Instance.LoadAssetsAsync(GAME_DEFINITION_LABEL);
            
            for (int i = 0; i < objs.Count; i++)
            {
                IGameDefinitionWrapper wrapper = objs[i] as IGameDefinitionWrapper;
                if (wrapper != null)
                {
                    wrapper.Init();
                    Type type = wrapper.GetGameDefinitionType();
                    _gameDefinitionWrapperDict.Add(type, wrapper);
                }
            }

            if (OnInitCompleted != null)
            {
                OnInitCompleted();
            }
        }
    }

}

