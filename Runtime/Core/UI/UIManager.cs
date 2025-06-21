using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class UIManager : BaseSystem
    {
        [SerializeField] private UIViewStack[] _uiViewStacks = null;

        private UIExecutionHandler _executionHandler;

        public List<BaseView> Views { get; } = new List<BaseView>();

        private static UIManager _instance;
        
        public static UIManager Instance 
        {
            get 
            {
                if (_instance == null) 
                {
                    _instance = FindFirstObjectByType<UIManager>();
                }
                
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
            _executionHandler = new UIExecutionHandler(this); 
        }

        public IShowingCallback Show<T>(params object[] args) where T : BaseView
        { 
            UIData uiData = UIHandler.GetUIData<T>();
            if(uiData == null)
            {
                Debug.LogError("Not found view type := " + typeof(T));
                return null;
            }

            int viewKey = uiData.Key;
            ViewExecution viewExecution = _executionHandler.SetupShowingExecution(viewKey, viewType: typeof(T), args: args);
            
            return viewExecution?.ShowingHandler.SetupShow(viewKey, isWidget: false, args: args);
        }
        
        public IShowingCallback ShowWidget<T>(params object[] args) where T : WidgetView
        {
            UIData uiData = UIHandler.GetUIData<T>();
            if(uiData == null)
            {
                Debug.LogError("Not found view type := " + typeof(T));
                return new ShowingUIHandler();
            }

            int viewKey = uiData.Key;
            ViewExecution viewExecution = _executionHandler.SetupWidgetShowingExecution(viewKey, viewType: typeof(T), args: args);
            
            return viewExecution?.ShowingHandler.SetupShow(viewKey, isWidget: true, args: args);
        }

        /// <summary>
        /// DO NOT use this function to show Widget
        /// </summary>
        /// <param name="viewKey">Key view</param>
        /// <param name="args">args</param>
        /// <returns>IShowingCallback</returns>
        public IShowingCallback Show(int viewKey, params object[] args)
        {
            UIData uiData = UIHandler.GetUIData(viewKey);
            if(uiData == null)
            {
                Debug.LogError("Cannot find view with key:= " + viewKey);
                return null;
            }

            ViewExecution viewExecution = _executionHandler.SetupWidgetShowingExecution(viewKey, uiData.ViewType, args);
    
            return viewExecution?.ShowingHandler.SetupShow(viewKey, isWidget: false, args: args);
        }

        public IHidingCallback Hide<T>(bool isDisable = false, bool isDestroy = false, params object[] args) where T : BaseView
        {
            ViewExecution viewExecution = _executionHandler.SetupHidingExecution(typeof(T), isDestroy, args);
            
            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + typeof(T) + " cause the ViewExecution instance is null.");
                return new HidingUIHandler();
            }
    
            return viewExecution.HidingHandler.SetupHide(viewExecution.ViewKey, isDisable: isDisable, isDestroy: isDestroy, isWidget: false, args: args);
        }
        
        public IHidingCallback HideAll<T>(bool isDisable = false, bool isDestroy = false, params object[] args) where T : BaseView
        {
            ViewExecution viewExecution = _executionHandler.SetupHidingExecution(typeof(T), isDestroy, args);
            
            List<BaseView> baseViews = new List<BaseView>();
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].GetType() == typeof(T))
                {
                    baseViews.Add(Views[i]);
                }
            }

            return viewExecution.HidingHandler.SetupHide(baseViews, isDisable: isDisable, isDestroy: isDestroy, isWidget: false, args: args);
        }
        
        public IHidingCallback HideWidget(WidgetView widgetView, bool isDisable = false, bool isDestroy = false, params object[] args)
        {
            if (widgetView == null || !widgetView.gameObject.activeSelf)
            {
                return new HidingUIHandler();
            }

            ViewExecution viewExecution = _executionHandler.SetupWidgetHidingExecution(widgetView, isDestroy, args);

            if (viewExecution.HidingHandler != null)
            {
                return viewExecution.HidingHandler.SetupHide(viewExecution.ViewKey, isDisable: isDisable, isDestroy: isDestroy, isWidget: true, args: args);
            }

            Debug.LogError("UIManager HideWidget:= viewExecution.HidingHandler is null");
            return new HidingUIHandler();
        }

        /// <summary>
        /// Hide UI 
        /// </summary>
        /// <param name="viewKey">View key is genarated</param>
        /// <param name="isDestroy">Hide and destroy this view</param>
        /// <param name="args">params</param>
        /// <returns></returns>
        public IHidingCallback Hide(int viewKey, bool isDisable = false, bool isDestroy = false, params object[] args)
        {
            ViewExecution viewExecution = _executionHandler.SetupHidingExecution(viewKey, isDestroy, args);
            
            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + viewKey + " cause the ViewExecution instance is null.");
                return new HidingUIHandler();
            }
    
            return viewExecution.HidingHandler.SetupHide(viewKey, isDisable: isDisable, isDestroy: isDestroy, isWidget: true, args: args);
        }
    
        public void HideAllInContext(string contextName)
        {
            List<BaseView> viewsShouldBeDeleted = GetAllViewsInContext(contextName);
    
            for (int i = 0; i < viewsShouldBeDeleted.Count; i++)
            {
                BaseView view = viewsShouldBeDeleted[i];
    
                if (view == null)
                {
                    continue;
                }
    
                _executionHandler.RemoveViewExecutionByKey(view.Key);      
                Views.Remove(view);
                
                view.DestroyThisView();
            }
        }
    
        public bool IsPresenting(int viewKey)
        {
            BaseView view = GetCache(viewKey);
            if (view == null)
            {
                return false;
            }
    
            return view.IsPresenting;
        }
        
        public bool IsPresenting<T>()  where T : BaseView
        {
            BaseView view = GetCache<T>();
            if (view == null)
            {
                return false;
            }
    
            return view.IsPresenting;
        }
        
        public bool IsDialogPresenting()
        {
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].IsPresenting && Views[i].GetType().IsSubclassOf(typeof(BaseDialog)))
                {
                    return true;
                }
            }

            return false;
        }
    
        private List<BaseView> GetAllViewsInContext(string contextName)
        {
            List<BaseView> views = new List<BaseView>();
            
            for (int i = 0; i < Views.Count; i++)
            {
                BaseView view = Views[i];
                
                if (view != null 
                    && !string.IsNullOrEmpty(view.ContextName)
                    && string.Equals(view.ContextName, contextName))
                {
                    views.Add(view);
                }
            }
    
            return views;
        }
        
        public void HideAllWidgets()
        {
            for (int i = 0; i < Views.Count; i++)
            {
                BaseView view = Views[i];

                if (view != null
                    && view.IsPresenting
                    && view.Layer == UILayer.Widget)
                {
                    HideWidget((WidgetView) view);
                }
            }
        }
        
        public UIViewStack GetLayer(UILayer layer)
        {
            for (int i = 0; i < _uiViewStacks.Length; i++)
            {
                if(_uiViewStacks[i].Layer == layer)
                {
                    return _uiViewStacks[i];
                }
            }
    
            return null;
        }
    
        public BaseView GetCache(int uiKey)
        {
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].Key == uiKey)
                {
                    return Views[i];
                }
            }
    
            return null;
        }
        
        public T GetCache<T>() where T : BaseView
        {
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].GetType() == typeof(T))
                {
                    return Views[i] as T;
                }
            }
    
            return null;
        }

        public override void OnUnloaded()
        {
            for (int i = 0; i < Views.Count; i++)
            {
                BaseView view = Views[i];

                if (!string.Equals(view.ContextName, CoreSceneManager.Instance.CurrentScene.SceneName)
                    || view.Layer == UILayer.ScreenTransition)
                {
                    continue;
                }

                if (view.Layer == UILayer.Panel)
                {
                    Hide(view.Key, isDisable: true , isDestroy: true);
                }
                else if (view.Layer == UILayer.Widget)
                {
                    HideWidget((WidgetView) view, isDisable: false);
                }
                else
                {
#if UNITY_EDITOR
                    view.name = view.name.Replace(" [ACTIVE]", "");
#endif
                    
                    view.ForceHide();
                    view.gameObject.SetActive(false);
                }
            }
        }
    }
}

