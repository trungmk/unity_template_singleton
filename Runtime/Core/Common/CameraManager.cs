using UnityEngine;

namespace Core
{
    public class CameraManager : MonoSingleton<CameraManager>
    {
        [Header("Camera reference")]
        [SerializeField] private Camera _mainCamera;

        [Header("Reference resolution (width:height)")]
        [SerializeField] private float referenceWidth = 1080f;
        [SerializeField] private float referenceHeight = 1920f;

        [Header("Reference orthographic size (for vertical or horizontal)")]
        [SerializeField] private float referenceOrthoSize_Vertical = 10.5f;
        [SerializeField] private float referenceOrthoSize_Horizontal = 6.0f;

        [Header("Orientation priority")]
        [SerializeField] private bool isVertical = true;

        public bool IsVerticalScale { get; set; } = false;

        private void Awake()
        {
            AdjustCamera();
        }

        private void AdjustCamera()
        {
            float targetAspect = referenceWidth / referenceHeight;
            float windowAspect = (float)Screen.width / Screen.height;

            if (isVertical)
            {
                if (windowAspect > targetAspect)
                {
                    _mainCamera.orthographicSize = referenceOrthoSize_Vertical;
                    IsVerticalScale = false;
                }
                else
                {
                    float scaleFactor = targetAspect / windowAspect;
                    _mainCamera.orthographicSize = referenceOrthoSize_Vertical * scaleFactor;
                    IsVerticalScale = true;
                }
            }
            else
            {
                float reverseTargetAspect = referenceHeight / referenceWidth;
                float windowAspectInv = (float)Screen.height / Screen.width;

                if (windowAspectInv > reverseTargetAspect)
                {
                    _mainCamera.orthographicSize = referenceOrthoSize_Horizontal;
                }
                else
                {
                    float scaleFactor = reverseTargetAspect / windowAspectInv;
                    _mainCamera.orthographicSize = referenceOrthoSize_Horizontal * scaleFactor;
                }
            }
        }
    } 
}