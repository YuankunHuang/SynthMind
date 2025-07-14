using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace YuankunHuang.Unity.Core
{
    /// @ingroup Core
    /// @class GeneralWindowConfig
    /// @brief Provides configuration data for a UI window, including references to UI elements, widgets, and other UI objects.
    ///
    /// The GeneralWindowConfig class holds a variety of configurations related to a window, such as references to UI elements
    /// (like buttons, text fields, images), and other objects or data that may be needed for window initialization or manipulation.
    /// This class helps streamline scene setup by providing a centralized place for all window-related configuration data.
    public class GeneralWindowConfig : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Extra")]
        [SerializeField] private List<GeneralWidgetConfig> _extraWidgetConfigs;
        [SerializeField] private List<RectTransform> _extraRectTransforms;
        [SerializeField] private List<TMP_Text> _extraTextMeshPros;
        [SerializeField] private List<GeneralButton> _extraButtons;
        [SerializeField] private List<Transform> _extraObjects;
        [SerializeField] private List<GameObject> _extraGameObjects;
        [SerializeField] private List<int> _extraInts;
        [SerializeField] private List<CanvasGroup> _extraCanvasGroups;
        [SerializeField] private List<Image> _extraImages;
        [SerializeField] private List<SpriteAtlas> _extraSpriteAtlases;
        [SerializeField] private List<string> _extraStrings;
        [SerializeField] private List<ScriptableObject> _extraScriptableObjects;
        [SerializeField] private List<Camera> _extraCameras;
        [SerializeField] private List<Sprite> _extraSprites;
        [SerializeField] private List<TMP_ColorGradient> _extraColorGradients;
        [SerializeField] private List<Animator> _extraAnimators;
        [SerializeField] private List<Renderer> _extraRenderers;
        [SerializeField] private List<Canvas> _extraCanvases;

        public CanvasGroup CanvasGroup => _canvasGroup;

        // extra
        public List<GeneralWidgetConfig> ExtraWidgetConfigs => _extraWidgetConfigs;
        public List<RectTransform> ExtraRectTransforms => _extraRectTransforms;
        public List<TMP_Text> ExtraTextMeshPros => _extraTextMeshPros;
        public List<GeneralButton> ExtraButtons => _extraButtons;
        public List<Transform> ExtraObjects => _extraObjects;
        public List<GameObject> ExtraGameObjects => _extraGameObjects;
        public List<int> ExtraInts => _extraInts;
        public List<CanvasGroup> ExtraCanvasGroups => _extraCanvasGroups;
        public List<Image> ExtraImages => _extraImages;
        public List<SpriteAtlas> ExtraSpriteAtlases => _extraSpriteAtlases;
        public List<string> ExtraStrings => _extraStrings;
        public List<ScriptableObject> ExtraScriptableObjects => _extraScriptableObjects;
        public List<Camera> ExtraCameras => _extraCameras;
        public List<Sprite> ExtraSprites => _extraSprites;
        public List<TMP_ColorGradient> ExtraColorGradients => _extraColorGradients;
        public List<Animator> ExtraAnimators => _extraAnimators;
        public List<Renderer> ExtraRenderers => _extraRenderers;
        public List<Canvas> ExtraCanvases => _extraCanvases;
    }
}