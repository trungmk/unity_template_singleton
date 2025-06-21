using System;
using UnityEngine;

namespace Core
{
    public class InputFilter
    {
        private static InputFilter _instance = null;

        public static InputFilter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InputFilter();
                }

                return _instance;
            }
        }
        
        public void OnUserPress(Vector3 target)
        {
            EventManager.Instance.Dispatch(TouchDownEvent.GetInstance(target));
        }

        public void OnUserRelease(Vector3 target)
        {
            EventManager.Instance.Dispatch(TouchUpEvent.GetInstance(target));
        }

        public void OnUserDrag(Vector3 target)
        {
            EventManager.Instance.Dispatch(DragEvent.GetInstance(target));
        }
    }
}


