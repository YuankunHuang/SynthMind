using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.AssetCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.UICore
{
    public class InputBlocker : MonoBehaviour
    {
        public static InputBlocker Instance { get; private set; }
        public static bool IsInitialized => Instance != null;
        public static bool IsBlocking => Instance != null && Instance._isBlocking;

        [Header("Blocker Settings")]
        [SerializeField] private bool showLoadingIndicator = true;
        [SerializeField] private bool dimBackground = true;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.3f);

        [Header("Loading Indicator")]
        [SerializeField] private GameObject loadingIndicatorPrefab;
        [SerializeField] private bool useSimpleSpinner = true;
        [SerializeField] private float spinnerRotationSpeed = 360f;
        [SerializeField] private float loadingIndicatorDelay = 2f;

        private Canvas _blockerCanvas;
        private CanvasGroup _blockerCanvasGroup;
        private GraphicRaycaster _graphicRaycaster;
        private GameObject _currentLoadingIndicator;
        private GameObject _maskGO;
        private bool _isBlocking = false;
        private int _blockCount = 0; // Support nested blocking
        private Coroutine _currentAnimationCoroutine;

        public static void Initialize(InputBlocker instance)
        {
            Instance = instance;
            Instance.InitializeInternal();
        }

        private void InitializeInternal()
        {
            _blockerCanvas = gameObject.AddComponent<Canvas>();
            _blockerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _blockerCanvas.sortingOrder = 9999;

            // Add CanvasGroup to control visibility and interaction
            _blockerCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            _blockerCanvasGroup.CanvasGroupOff();

            // Use GraphicRaycaster to block input raycasts
            _graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            CreateBackgroundMask();

            if (showLoadingIndicator)
            {
                CreateLoadingIndicator();
            }

            LogHelper.Log("[InputBlocker] Initialized");
        }

        public static void Dispose()
        {
            if (Instance != null)
            {
                Instance.DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_currentAnimationCoroutine != null)
            {
                StopCoroutine(_currentAnimationCoroutine);
                _currentAnimationCoroutine = null;
            }
            StopAllCoroutines();

            if (_graphicRaycaster != null)
            {
                Destroy(_graphicRaycaster);
                _graphicRaycaster = null;
            }

            if (_blockerCanvasGroup != null)
            {
                Destroy(_blockerCanvasGroup);
                _blockerCanvasGroup = null;
            }

            if (_blockerCanvas != null)
            {
                Destroy(_blockerCanvas);
                _blockerCanvas = null;
            }

            if (_maskGO != null)
            {
                Destroy(_maskGO);
                _maskGO = null;
            }

            if (_currentLoadingIndicator != null)
            {
                Destroy(_currentLoadingIndicator);
                _currentLoadingIndicator = null;
            }

            Instance = null;
            LogHelper.Log("[InputBlocker] Disposed");
        }

        private void CreateBackgroundMask()
        {
            if (_maskGO == null)
            {
                _maskGO = new GameObject("BackgroundMask");
                _maskGO.transform.SetParent(transform);

                var rt = _maskGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var image = _maskGO.AddComponent<Image>();
                image.color = dimBackground ? backgroundColor : Color.clear;
                image.raycastTarget = true;
            }
        }

        private void CreateLoadingIndicator()
        {
            GameObject indicatorGO;

            if (loadingIndicatorPrefab != null)
            {
                indicatorGO = Instantiate(loadingIndicatorPrefab, transform);
            }
            else if (useSimpleSpinner)
            {
                indicatorGO = CreateSimpleSpinner();
            }
            else
            {
                indicatorGO = CreateSimpleTextIndicator();
            }

            _currentLoadingIndicator = indicatorGO;
            _currentLoadingIndicator.SetActive(false);
        }

        private GameObject CreateSimpleSpinner()
        {
            var spinnerGO = new GameObject("LoadingSpinner");
            spinnerGO.transform.SetParent(transform);

            var rt = spinnerGO.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(64, 64);

            var image = spinnerGO.AddComponent<Image>();

            try
            {
                var assetManager = ModuleRegistry.Get<IAssetManager>();
                if (assetManager != null)
                {
                    var loadingSprite = assetManager.GetAsset<Sprite>("LoadingIcon");
                    if (loadingSprite != null)
                    {
                        image.sprite = loadingSprite;
                    }
                    else
                    {
                        image.sprite = CreateDefaultSpinnerSprite();
                    }
                }
                else
                {
                    image.sprite = CreateDefaultSpinnerSprite();
                }
            }
            catch
            {
                image.sprite = CreateDefaultSpinnerSprite();
            }

            image.color = Color.white;
            image.raycastTarget = false;

            var spinner = spinnerGO.AddComponent<SimpleSpinner>();
            spinner.rotationSpeed = spinnerRotationSpeed;

            return spinnerGO;
        }

        private Sprite CreateDefaultSpinnerSprite()
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var colors = new Color[64 * 64];

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                    if (distance <= 30 && distance >= 20)
                    {
                        float angle = Mathf.Atan2(y - 32, x - 32) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360;

                        float alpha = Mathf.Clamp01((360 - angle) / 360f);
                        colors[y * 64 + x] = new Color(1, 1, 1, alpha);
                    }
                    else
                    {
                        colors[y * 64 + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        private GameObject CreateSimpleTextIndicator()
        {
            var textGO = new GameObject("LoadingText");
            textGO.transform.SetParent(transform);

            var rt = textGO.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(200, 50);

            var text = textGO.AddComponent<Text>();
            text.text = "Loading...";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            var blinker = textGO.AddComponent<TextBlinker>();

            return textGO;
        }

        public static void StartBlocking(float duration = -1f, float loadingDelay = -1f)
        {
            if (Instance == null)
            {
                LogHelper.LogError("[InputBlocker] Instance is null. Please initialize first.");
                return;
            }

            if (loadingDelay < 0)
            {
                loadingDelay = Instance.loadingIndicatorDelay;
            }

            Instance.StartBlockingInternal(duration, loadingDelay);
        }

        public static void StopBlocking()
        {
            if (Instance == null) return;
            Instance.StopBlockingInternal();
        }

        private void StartBlockingInternal(float duration, float loadingDelay)
        {
            if (_currentAnimationCoroutine != null)
            {
                StopCoroutine(_currentAnimationCoroutine);
                _currentAnimationCoroutine = null;
            }

            _blockCount++;

            if (!_isBlocking)
            {
                _isBlocking = true;

                _blockerCanvasGroup.alpha = 0f;
                _blockerCanvasGroup.blocksRaycasts = true;
                _blockerCanvasGroup.interactable = true;

                LogHelper.Log($"[InputBlocker] Started blocking (count: {_blockCount})");

                _currentAnimationCoroutine = StartCoroutine(FadeInCoroutine(loadingDelay));
            }

            if (duration > 0)
            {
                StartCoroutine(AutoStopBlocking(duration));
            }
        }

        private void StopBlockingInternal()
        {
            _blockCount = Mathf.Max(0, _blockCount - 1);

            if (_blockCount == 0 && _isBlocking)
            {
                _isBlocking = false;

                if (_currentAnimationCoroutine != null)
                {
                    StopCoroutine(_currentAnimationCoroutine);
                }

                _currentAnimationCoroutine = StartCoroutine(FadeOutCoroutine());

                LogHelper.Log("[InputBlocker] Stopped blocking");
            }
        }

        private IEnumerator AutoStopBlocking(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopBlockingInternal();
        }

        private IEnumerator FadeInCoroutine(float loadingDelay)
        {
            if (loadingDelay > 0)
            {
                yield return new WaitForSeconds(loadingDelay);
            }

            if (showLoadingIndicator && _currentLoadingIndicator != null)
            {
                _currentLoadingIndicator.SetActive(true);
            }

            _blockerCanvasGroup.alpha = 1f;

            LogHelper.Log("[InputBlocker] Fade in completed");
        }

        private IEnumerator FadeOutCoroutine()
        {
            if (showLoadingIndicator && _currentLoadingIndicator != null)
            {
                _currentLoadingIndicator.SetActive(false);
            }

            _blockerCanvasGroup.CanvasGroupOff();

            LogHelper.Log("[InputBlocker] Fade out completed");

            yield break;
        }

        /// <summary>
        /// Force stop blocking immediately, resetting the block count. 
        /// This should be used with caution, as it may interrupt ongoing operations.
        /// </summary>
        public static void ForceStopBlocking()
        {
            if (Instance != null)
            {
                Instance._blockCount = 0;
                Instance.StopBlockingInternal();
            }
        }
    }

    public class SimpleSpinner : MonoBehaviour
    {
        public float rotationSpeed = 360f;

        private void Update()
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.unscaledDeltaTime);
        }
    }

    public class TextBlinker : MonoBehaviour
    {
        private Text _text;
        private float _timer;

        private void Start()
        {
            _text = GetComponent<Text>();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            float alpha = (Mathf.Sin(_timer * 3f) + 1f) * 0.5f;
            alpha = Mathf.Lerp(0.3f, 1f, alpha);

            var color = _text.color;
            color.a = alpha;
            _text.color = color;
        }
    }
}