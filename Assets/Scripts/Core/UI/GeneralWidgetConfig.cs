using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.UICore
{
    /// @ingroup Core
    /// @class GeneralWidgetConfig
    /// @brief Configures and holds various widget elements used in a scene, including UI elements like buttons, text fields, and images.
    /// 
    /// The GeneralWidgetConfig class is designed to store and manage a collection of different types of UI elements
    /// and other scene-related objects, such as RectTransforms, TextMeshPro elements, buttons, game objects, images,
    /// cameras, and more. These elements are typically used in a grid-based UI layout, and this class helps manage them
    /// effectively within the scene.
    public class GeneralWidgetConfig : GridScrollViewElement
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _root;

        [Header("Extra")]
        [SerializeField] private List<GeneralWidgetConfig> _extraWidgetConfigList;
        [SerializeField] private List<RectTransform> _extraRectTransformList;
        [SerializeField] private List<TMP_Text> _extraTMPTextList;
        [SerializeField] private List<GeneralButton> _extraButtonList;
        [SerializeField] private List<Transform> _extraObjectList;
        [SerializeField] private List<GameObject> _extraGameObjectList;
        [SerializeField] private List<int> _extraIntList;
        [SerializeField] private List<CanvasGroup> _extraCanvasGroupList;
        [SerializeField] private List<Image> _extraImageList;
        [SerializeField] private List<SpriteAtlas> _extraSpriteAtlasList;
        [SerializeField] private List<string> _extraStringList;
        [SerializeField] private List<ScriptableObject> _extraScriptableObjectList;
        [SerializeField] private List<Camera> _extraCameraList;
        [SerializeField] private List<Sprite> _extraSpriteList;
        [SerializeField] private List<TMP_ColorGradient> _extraColorGradientList;
        [SerializeField] private List<Animator> _extraAnimatorList;
        [SerializeField] private List<Renderer> _extraRendererList;
        [SerializeField] private List<Canvas> _extraCanvasList;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public RectTransform Root => _root;

        // extra
        public List<GeneralWidgetConfig> ExtraWidgetConfigList => _extraWidgetConfigList;
        public List<RectTransform> ExtraRectTransformList => _extraRectTransformList;
        public List<TMP_Text> ExtraTextMeshProList => _extraTMPTextList;
        public List<GeneralButton> ExtraButtonList => _extraButtonList;
        public List<Transform> ExtraObjectList => _extraObjectList;
        public List<GameObject> ExtraGameObjectList => _extraGameObjectList;
        public List<int> ExtraIntList => _extraIntList;
        public List<CanvasGroup> ExtraCanvasGroupList => _extraCanvasGroupList;
        public List<Image> ExtraImageList => _extraImageList;
        public List<SpriteAtlas> ExtraSpriteAtlasList => _extraSpriteAtlasList;
        public List<string> ExtraStringList => _extraStringList;
        public List<ScriptableObject> ExtraScriptableObjectList => _extraScriptableObjectList;
        public List<Camera> ExtraCameraList => _extraCameraList;
        public List<Sprite> ExtraSpriteList => _extraSpriteList;
        public List<TMP_ColorGradient> ExtraColorGradientList => _extraColorGradientList;
        public List<Animator> ExtraAnimatorList => _extraAnimatorList;
        public List<Renderer> ExtraRendererList => _extraRendererList;
        public List<Canvas> ExtraCanvasList => _extraCanvasList;

        #region Mono Events
        public event Action OnEnableTriggered;
        public event Action OnDisableTriggered;
        public event Action OnDestroyTriggered;

        protected virtual void OnEnable()
        {
            OnEnableTriggered?.Invoke();
        }
        protected virtual void OnDisable()
        {
            OnDisableTriggered?.Invoke();
        }
        protected virtual void OnDestroy()
        {
            OnDestroyTriggered?.Invoke();
        }
        #endregion
    }
}