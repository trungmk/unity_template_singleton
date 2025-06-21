using System.ComponentModel.Design;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Ref link: https://bf.wtf/event-driven-unity-one/
    /// </summary>

    public class EventManager
    {
        private readonly ServiceContainer _serviceContainer;
        
        private readonly IList<Action> _pendingActions;

        private int _isDispatching = 0;

        private static EventManager _instance;

        public static EventManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new EventManager();
                }

                return _instance;
            }
        }

        private EventManager()
        {
            _serviceContainer = new ServiceContainer();
            _pendingActions = new List<Action>();
        }

        private IList<Action<T>> GetListenerList<T>() where T : IEvent
        {
            return _serviceContainer.GetService(typeof(IList<Action<T>>)) as IList<Action<T>>;   
        }

        public void AddListener<T>(Action<T> action) where T : IEvent
        {
            var listenerList = GetListenerList<T>();
            if (listenerList == null) 
            {
                _serviceContainer.AddService(typeof(IList<Action<T>>), new List<Action<T>>());
                listenerList = GetListenerList<T>();
            }

            if (_isDispatching > 0) 
            {
                _pendingActions.Add(() => listenerList.Add(action));
            } 
            else 
            {
                listenerList.Add(action);
            }
        }

        public void RemoveListener<T>(Action<T> action) where T : IEvent
        {
            var listenerList = GetListenerList<T>();
            
            if (listenerList != null) 
            {
                if (_isDispatching > 0) 
                {
                    _pendingActions.Add(() => listenerList.Remove(action));
                } 
                else 
                {
                    listenerList.Remove(action);
                }
            }
        }

        private void RunPendingActions() 
        {
            for (int i = 0; i < _pendingActions.Count; i++)
            {
                _pendingActions[i].Invoke();
            }

            for (int i = 0; i < _pendingActions.Count; i++)
            {
                if (_pendingActions[i] != null)
                {
                    _pendingActions[i].Invoke();
                }
            }

            _pendingActions.Clear();
        }
        
        public void Dispatch<T>(T eventData) where T : IEvent
        {
            var listenerList = GetListenerList<T>();

            if (_isDispatching == 0) 
            {
                RunPendingActions();
            }

            if (listenerList != null && listenerList.Count > 0) 
            {
                _isDispatching++;
                
                for (int i = listenerList.Count - 1; i >= 0 ; i--)
                {
                    if (listenerList[i] == null)
                    {
                        listenerList.RemoveAt(i);
                        continue;
                    }
                    
                    listenerList[i].Invoke(eventData);
                }
                
                _isDispatching--;
            }
            
            if (eventData != null)
            {
                eventData.Reset();
            }
        }
    }
}