using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.Core
{
    public class OffScreenUI_Cull : MonoBehaviour
    {
        [SerializeField] public RectTransform _viewportRectangle;
        [SerializeField, Space(15)] RectTransform _ownRectTransform;

        [SerializeField] public Graphic _localGraphicComponent;
        [SerializeField] public CanvasGroup _canvasGroup;
        [SerializeField] public GameObject[] _optionalGO_to_On_Off;

        void Reset()
        {
            _ownRectTransform = transform as RectTransform;
        }

        void OnValidate()
        {
            if (_viewportRectangle == null)
            {
                var root = GameObject.FindGameObjectWithTag(TagNames.WindowRoot).transform;
                _viewportRectangle = root.GetComponent<RectTransform>();
            }
            if (_viewportRectangle == null)
            {
                _viewportRectangle = (GetComponentInParent(typeof(Canvas)) as Canvas).transform as RectTransform;
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            //while in editor, this will discard null "optional game objects", automatically.
            int prevLength = _optionalGO_to_On_Off.Length;

            _optionalGO_to_On_Off = _optionalGO_to_On_Off.Where(go => go != null)
                                                         .ToArray();

            if (_optionalGO_to_On_Off.Length != prevLength)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false) { return; }
#endif

            Cull();
        }

        void Cull()
        {
            if (_viewportRectangle == null) { return; }

            bool overlaps = _ownRectTransform.rectTransfOverlaps_inScreenSpace(_viewportRectangle);

            if (overlaps == true)
            {
                toggleElements_ifNeeded(true);
            }
            else
            {
                toggleElements_ifNeeded(false);
            }
        }

        void toggleElements_ifNeeded(bool requiredValue)
        {
            for (int i = 0; i < _optionalGO_to_On_Off.Length; i++)
            {
                GameObject optionalGO = _optionalGO_to_On_Off[i];

                if (optionalGO.activeSelf != requiredValue)
                {
                    optionalGO.SetActive(requiredValue);
                }
            }

            if (_localGraphicComponent != null && _localGraphicComponent.enabled != requiredValue)
            {
                _localGraphicComponent.enabled = requiredValue;
            }

            if (_canvasGroup != null)
            {
                //_canvasGroup.alpha = requiredValue ? 1 : 0;
                if (requiredValue)
                {
                    _canvasGroup.CanvasGroupOn();
                }
                else
                {
                    _canvasGroup.CanvasGroupOff();
                }
            }
        }
    }

    static class Extensions
    {
        public static bool rectTransfOverlaps_inScreenSpace(this RectTransform rectTrans1, RectTransform rectTrans2)
        {
            Rect rect1 = rectTrans1.getScreenSpaceRect();
            Rect rect2 = rectTrans2.getScreenSpaceRect();

            return rect1.Overlaps(rect2);
        }

        //rect transform into coordinates expressed as seen on the screen (in pixels)
        //takes into account RectTrasform pivots
        // based on answer by Tobias-Pott
        // http://answers.unity3d.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
        public static Rect getScreenSpaceRect(this RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
            rect.x -= (transform.pivot.x * size.x);
            rect.y -= ((1.0f - transform.pivot.y) * size.y);
            return rect;
        }
    }
}