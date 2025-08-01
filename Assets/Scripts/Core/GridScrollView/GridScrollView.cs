using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YuankunHuang.SynthMind.Core
{
    public enum GridLayoutType
    {
        Vertical,
        Horizontal,
        Grid,
    }

    public class GridScrollViewElement : MonoBehaviour
    {
        public int Index { get; set; }
        public bool IsVisible { get; set; }
        public int PrefabType { get; set; } // Track which prefab type this element uses
    }

    public interface IGridHandler
    {
        int GetDataCount();
        Vector2 GetElementSize(int index); // (width, height)
        int GetPrefabType(int index); // NEW: Return prefab type for this index
        void OnElementShow(GridScrollViewElement element);
        void OnElementHide(GridScrollViewElement element);
        void OnElementCreate(GridScrollViewElement element);
        void OnElementRelease(GridScrollViewElement element);
    }

    public class GridScrollView : MonoBehaviour
    {
        [Header("Basic")]
        public ScrollRect scrollRect;
        public RectTransform content;
        public ContentSizeFitter contentSizeFitter;
        public LayoutGroup layoutGroup;

        [Header("Prefabs")]
        public GameObject[] itemPrefabs; // Multiple prefabs array

        [Header("Layout")]
        public GridLayoutType layoutType = GridLayoutType.Vertical;
        public int constraintCount = 1;

        [Header("Movement")]
        public bool enableSmoothScroll = true;
        [Range(0.01f, 1f)] public float smoothScrollDuration = 0.15f;

        [Header("Performance")]
        [Range(1, 10)] public int bufferSize = 2; // Extra elements to render outside viewport
        [Range(0.1f, 1f)] public float updateThreshold = 0.1f; // Minimum scroll distance before update

        // Pool per prefab type for better performance
        private Dictionary<int, Queue<GameObject>> _pools;
        private Dictionary<int, GridScrollViewElement> _activeElements;
        private IGridHandler _handler;

        // Cached values for performance
        private readonly Dictionary<int, Vector2> _elementSizes = new();
        private readonly Dictionary<int, float> _cumulativePositions = new(); // Cache cumulative positions
        private float _totalContentSize = 0f;
        private Vector2 _cachedSpacing;
        private Vector2 _cachedPadding;
        private Vector2 _lastScrollPosition;
        private bool _cachesDirty = true;

        private Coroutine _scrollCoroutine;
        private int _visibleIndexMin = -1;
        private int _visibleIndexMax = -1;

        private bool _isFixingDeferredSizes = false;
        private readonly List<int> _deferredSizeFixIndices = new();

        // Performance: Cache these properties instead of calculating every frame
        private Vector2 spacing
        {
            get
            {
                if (_cachesDirty)
                    UpdateCachedValues();
                return _cachedSpacing;
            }
        }

        private Vector2 padding
        {
            get
            {
                if (_cachesDirty)
                    UpdateCachedValues();
                return _cachedPadding;
            }
        }

        private void UpdateCachedValues()
        {
            _cachedSpacing = layoutGroup switch
            {
                HorizontalOrVerticalLayoutGroup hv => hv.spacing * Vector2.one,
                GridLayoutGroup grid => grid.spacing,
                _ => Vector2.zero
            };

            _cachedPadding = layoutGroup switch
            {
                HorizontalOrVerticalLayoutGroup hv => new(hv.padding.left, hv.padding.top),
                GridLayoutGroup grid => new(grid.padding.left, grid.padding.top),
                _ => Vector2.zero
            };

            _cachesDirty = false;
        }

        public void SetHandler(IGridHandler handler) => _handler = handler;

        private void OnValidate()
        {
            if (scrollRect == null)
                scrollRect = GetComponentInChildren<ScrollRect>();
            if (content == null)
                content = scrollRect?.content;
            if (layoutGroup == null)
                layoutGroup = content?.GetComponent<LayoutGroup>();
            if (contentSizeFitter == null)
                contentSizeFitter = content?.GetComponent<ContentSizeFitter>();
        }

        public void Activate()
        {
            // Initialize pools for each prefab type
            _pools = new Dictionary<int, Queue<GameObject>>();
            for (int i = 0; i < GetPrefabCount(); i++)
            {
                _pools[i] = new Queue<GameObject>();
            }

            _activeElements = new();
            _lastScrollPosition = content.anchoredPosition;

            layoutGroup.enabled = false;
            contentSizeFitter.enabled = false;

            scrollRect.onValueChanged.AddListener(OnValueChanged);
            UpdateCachedValues();
        }

        public void Deactivate()
        {
            _pools = null;
            _activeElements = null;
            scrollRect.onValueChanged.RemoveAllListeners();
        }

        private int GetPrefabCount()
        {
            if (itemPrefabs != null && itemPrefabs.Length > 0)
                return itemPrefabs.Length;
            return 0;
        }

        private GameObject GetPrefabForType(int prefabType)
        {
            if (itemPrefabs != null && itemPrefabs.Length > 0)
            {
                if (prefabType >= 0 && prefabType < itemPrefabs.Length)
                    return itemPrefabs[prefabType];
            }
            return null;
        }

        public void SnapToBottom()
        {
            var targetY = layoutType switch
            {
                GridLayoutType.Vertical => Mathf.Max(_totalContentSize - scrollRect.viewport.rect.height, 0),
                GridLayoutType.Horizontal => Mathf.Min(scrollRect.viewport.rect.height - _totalContentSize, 0),
                GridLayoutType.Grid => Mathf.Max(_totalContentSize - scrollRect.viewport.rect.height, 0),
                _ => 0f
            };

            SetScrollPositionInstantly(new Vector2(content.anchoredPosition.x, targetY));
        }

        public void SnapToElement(int index)
        {
            if (index < 0 || index >= _handler.GetDataCount())
                return;

            var elementPos = GetCachedPositionForIndex(index);

            var anchoredPos = layoutType switch
            {
                GridLayoutType.Vertical => new Vector2(content.anchoredPosition.x, -elementPos),
                GridLayoutType.Horizontal => new Vector2(-elementPos, content.anchoredPosition.y),
                GridLayoutType.Grid => new Vector2(content.anchoredPosition.x, -elementPos),
                _ => content.anchoredPosition,
            };

            SetScrollPositionInstantly(anchoredPos);
        }

        private void SetScrollPositionInstantly(Vector2 anchoredPos)
        {
            scrollRect.StopMovement();
            content.anchoredPosition = anchoredPos;
            scrollRect.StopMovement();
        }

        public void SmoothScrollToBottom()
        {
            var targetY = _totalContentSize - scrollRect.viewport.rect.height;
            if (targetY < 0f)
                targetY = 0f;

            var target = new Vector2(content.anchoredPosition.x, targetY);

            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }

            _scrollCoroutine = StartCoroutine(SmoothScrollTo(target, smoothScrollDuration));
        }

        public void SmoothScrollToElement(int index)
        {
            if (index < 0 || index >= _handler.GetDataCount())
                return;

            var targetY = -GetCachedPositionForIndex(index);
            if (targetY < 0f)
                targetY = 0f;

            var targetPos = new Vector2(content.anchoredPosition.x, targetY);

            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }

            _scrollCoroutine = StartCoroutine(SmoothScrollTo(targetPos, smoothScrollDuration));
        }

        private IEnumerator SmoothScrollTo(Vector2 targetPos, float duration)
        {
            var startPos = content.anchoredPosition;
            var time = 0f;

            scrollRect.StopMovement();
            while (time < duration)
            {
                content.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.Clamp01(time / duration));
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            content.anchoredPosition = targetPos;
            _scrollCoroutine = null;
        }

        // Performance: Use cumulative position caching for O(1) lookups
        private float GetCachedPositionForIndex(int idx)
        {
            if (_cumulativePositions.TryGetValue(idx, out var cached))
                return cached;

            return CalculateAndCachePositionForIndex(idx);
        }

        private float CalculateAndCachePositionForIndex(int idx)
        {
            float position = 0f;

            if (layoutType == GridLayoutType.Vertical)
            {
                float offset = 0f;
                for (int i = 0; i < idx; i++)
                {
                    offset += GetElementSize(i).y;
                }
                offset += spacing.y * idx;
                position = -padding.y - offset;
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                float offset = 0f;
                for (int i = 0; i < idx; i++)
                {
                    offset += GetElementSize(i).x;
                }
                offset += spacing.x * idx;
                position = padding.x + offset;
            }
            else if (layoutType == GridLayoutType.Grid)
            {
                int row = idx / constraintCount;
                float yOffset = 0f;
                for (int r = 0; r < row; r++)
                {
                    float maxRowHeight = 0f;
                    for (int c = 0; c < constraintCount; c++)
                    {
                        int i = r * constraintCount + c;
                        if (i >= _handler.GetDataCount()) break;
                        maxRowHeight = Mathf.Max(maxRowHeight, GetElementSize(i).y);
                    }
                    yOffset += maxRowHeight;
                }
                yOffset += spacing.y * row;
                position = -padding.y - yOffset;
            }

            _cumulativePositions[idx] = position;
            return position;
        }

        private Vector2 CalculateElementSize()
        {
            var parentSize = content.rect.size;
            Vector2 size = Vector2.zero;

            switch (layoutType)
            {
                case GridLayoutType.Vertical:
                    size.x = parentSize.x - padding.x * 2;
                    size.y = -1;
                    break;

                case GridLayoutType.Horizontal:
                    size.y = parentSize.y - padding.y * 2;
                    size.x = -1;
                    break;

                case GridLayoutType.Grid:
                    int columnCount = Mathf.Max(1, constraintCount);
                    float totalSpacingX = spacing.x * (columnCount - 1);
                    float availableWidth = parentSize.x - padding.x * 2 - totalSpacingX;
                    float cellWidth = availableWidth / columnCount;

                    size.x = cellWidth;
                    size.y = cellWidth;
                    break;
            }

            return size;
        }

        private Vector2 GetElementSize(int index)
        {
            if (!_elementSizes.TryGetValue(index, out var size))
            {
                size = _handler.GetElementSize(index);
                _elementSizes[index] = size;
            }

            return size;
        }

        // Performance: Use binary search for large datasets and add buffer
        private (int min, int max) GetVisibleIndices()
        {
            int count = _handler.GetDataCount();
            if (count == 0) return (0, -1);

            float viewStart, viewEnd;

            switch (layoutType)
            {
                case GridLayoutType.Vertical:
                case GridLayoutType.Grid:
                    viewStart = -content.anchoredPosition.y + bufferSize * 100f; // Add buffer
                    viewEnd = viewStart - scrollRect.viewport.rect.height - bufferSize * 100f;
                    break;
                case GridLayoutType.Horizontal:
                    viewStart = -content.anchoredPosition.x - bufferSize * 100f;
                    viewEnd = viewStart + scrollRect.viewport.rect.width + bufferSize * 100f;
                    break;
                default:
                    return (0, -1);
            }

            int min = -1, max = -1;

            // For large datasets, consider using binary search here
            if (count > 100)
            {
                min = BinarySearchVisible(0, count - 1, viewStart, viewEnd, true);
                max = BinarySearchVisible(0, count - 1, viewStart, viewEnd, false);
            }
            else
            {
                // Linear search for smaller datasets
                for (int i = 0; i < count; i++)
                {
                    if (IsElementVisible(i, viewStart, viewEnd))
                    {
                        if (min == -1) min = i;
                        max = i;
                    }
                    else if (min != -1)
                    {
                        break;
                    }
                }
            }

            return (min == -1) ? (0, -1) : (min, max);
        }

        private bool IsElementVisible(int index, float viewStart, float viewEnd)
        {
            float pos = GetCachedPositionForIndex(index);
            float size = layoutType == GridLayoutType.Horizontal ? GetElementSize(index).x : GetElementSize(index).y;

            float elementStart = pos;
            float elementEnd = layoutType == GridLayoutType.Horizontal ? pos + size : pos - size;

            return layoutType == GridLayoutType.Horizontal
                ? (elementEnd >= viewStart && elementStart <= viewEnd)
                : (elementEnd <= viewStart && elementStart >= viewEnd);
        }

        private int BinarySearchVisible(int left, int right, float viewStart, float viewEnd, bool findFirst)
        {
            int result = -1;

            while (left <= right)
            {
                int mid = (left + right) / 2;

                if (IsElementVisible(mid, viewStart, viewEnd))
                {
                    result = mid;
                    if (findFirst)
                        right = mid - 1;
                    else
                        left = mid + 1;
                }
                else
                {
                    float pos = GetCachedPositionForIndex(mid);
                    bool beforeView = layoutType == GridLayoutType.Horizontal ? pos < viewStart : pos > viewStart;

                    if (beforeView)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }
            }

            return result;
        }

        public void Refresh()
        {
            // Clear all caches
            _elementSizes.Clear();
            _cumulativePositions.Clear();
            _cachesDirty = true;

            // Batch hide all elements and return to appropriate pools
            var elementsToHide = new List<GridScrollViewElement>(_activeElements.Values);
            foreach (var element in elementsToHide)
            {
                element.IsVisible = false;
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);

                // Return to the correct pool based on prefab type
                int prefabType = element.PrefabType;
                if (_pools.ContainsKey(prefabType))
                {
                    _pools[prefabType].Enqueue(element.gameObject);
                }
            }
            _activeElements.Clear();

            var (min, max) = GetVisibleIndices();
            for (var idx = min; idx <= max; ++idx)
            {
                CreateElementForIndex(idx);
            }

            UpdateContentSize();
            GoToBottom();
        }

        private void EnsureSizeCacheForIndex(int index)
        {
            if (!_elementSizes.ContainsKey(index))
            {
                _elementSizes[index] = _handler.GetElementSize(index);
            }
        }

        public void AppendBottom(int index)
        {
            if (index != _handler.GetDataCount() - 1)
                return;

            EnsureSizeCacheForIndex(index);
            CreateElementForIndex(index);
            UpdateContentSize();
        }

        public void AppendTop(int index, float keepViewportFraction = 1f)
        {
            if (index != 0) return;

            EnsureSizeCacheForIndex(index);
            var size = _elementSizes[index];

            float grow = _activeElements.Count > 0
                ? (layoutType == GridLayoutType.Vertical ? size.y + spacing.y : size.x + spacing.x)
                : (layoutType == GridLayoutType.Vertical ? size.y : size.x);

            // Batch update positions
            var updates = new List<(RectTransform rt, Vector2 newPos)>();
            foreach (var kv in _activeElements)
            {
                var rt = (RectTransform)kv.Value.transform;
                var currentPos = rt.anchoredPosition;
                var newPos = layoutType == GridLayoutType.Vertical
                    ? currentPos + new Vector2(0f, grow)
                    : currentPos + new Vector2(grow, 0f);
                updates.Add((rt, newPos));
            }

            // Apply all position updates at once
            foreach (var (rt, newPos) in updates)
            {
                rt.anchoredPosition = newPos;
            }

            // Update scroll position
            var scrollOffset = layoutType == GridLayoutType.Vertical
                ? new Vector2(0f, grow * keepViewportFraction)
                : new Vector2(-grow * keepViewportFraction, 0f);
            content.anchoredPosition += scrollOffset;

            // Shift indices
            var shifted = new Dictionary<int, GridScrollViewElement>();
            foreach (var kv in _activeElements)
            {
                kv.Value.Index = kv.Key + 1;
                shifted[kv.Key + 1] = kv.Value;
            }
            _activeElements = shifted;

            // Clear position cache as indices have shifted
            _cumulativePositions.Clear();

            CreateElementForIndex(0);
            UpdateContentSize();
        }

        public void GoToBottom()
        {
            if (enableSmoothScroll)
                SmoothScrollToBottom();
            else
                SnapToBottom();
        }

        private void CreateElementForIndex(int idx)
        {
            _deferredSizeFixIndices.Add(idx);
            if (!_isFixingDeferredSizes)
            {
                _isFixingDeferredSizes = true;
                StartCoroutine(FixDeferredSizes(IsNearBottom()));
            }

            // Get the prefab type for this index
            int prefabType = _handler.GetPrefabType(idx);

            // Ensure we have a pool for this prefab type
            if (!_pools.ContainsKey(prefabType))
            {
                _pools[prefabType] = new Queue<GameObject>();
            }

            var isNewElement = _pools[prefabType].Count < 1;
            GameObject go;

            if (isNewElement)
            {
                var prefab = GetPrefabForType(prefabType);
                if (prefab == null)
                {
                    Debug.LogError($"No prefab found for type {prefabType}");
                    return;
                }
                go = Instantiate(prefab, content);
            }
            else
            {
                go = _pools[prefabType].Dequeue();
                go.transform.SetParent(content, false);
                go.transform.localScale = Vector3.one;
            }

            go.SetActive(true);

            var element = go.GetComponent<GridScrollViewElement>();
            element.Index = idx;
            element.PrefabType = prefabType; // Store the prefab type

            if (isNewElement)
                _handler.OnElementCreate(element);

            element.IsVisible = true;
            _handler.OnElementShow(element);
            _activeElements[idx] = element;

            SetElementPosition(element, idx);
        }

        private IEnumerator FixDeferredSizes(bool isNearBottom)
        {
            if (_deferredSizeFixIndices.Count < 1)
                yield break;

            yield return null; // Wait for layout pass

            bool sizesChanged = false;
            foreach (int idx in _deferredSizeFixIndices)
            {
                var element = GetElementAtIndex(idx);
                if (element == null) continue;

                var rt = (RectTransform)element.transform;
                var newSize = rt.sizeDelta;

                if (!_elementSizes.TryGetValue(idx, out var oldSize) || oldSize != newSize)
                {
                    _elementSizes[idx] = newSize;
                    sizesChanged = true;
                }
            }

            if (sizesChanged)
            {
                // Clear position cache since sizes changed
                _cumulativePositions.Clear();

                // Batch position updates
                var positionUpdates = new List<(GridScrollViewElement element, int index)>();
                foreach (var kv in _activeElements)
                {
                    positionUpdates.Add((kv.Value, kv.Key));
                }

                foreach (var (element, index) in positionUpdates)
                {
                    SetElementPosition(element, index);
                }

                UpdateContentSize();

                if (isNearBottom)
                    GoToBottom();
            }

            _deferredSizeFixIndices.Clear();
            _isFixingDeferredSizes = false;
        }

        public bool IsNearBottom(float threshold = 0.01f)
        {
            return content.rect.size.y <= scrollRect.viewport.rect.size.y ||
                   content.anchoredPosition.y - (content.rect.size.y - scrollRect.viewport.rect.size.y) >= threshold;
        }

        private void SetElementPosition(GridScrollViewElement element, int idx)
        {
            var rt = (RectTransform)element.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);

            var layoutSize = CalculateElementSize();
            var handlerSize = GetElementSize(idx);
            var width = layoutSize.x > 0 ? layoutSize.x : handlerSize.x;
            var height = layoutSize.y > 0 ? layoutSize.y : handlerSize.y;
            rt.sizeDelta = new Vector2(width, height);

            Vector2 position;
            if (layoutType == GridLayoutType.Vertical)
            {
                position = new Vector2(padding.x, GetCachedPositionForIndex(idx));
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                position = new Vector2(padding.x + idx * (width + spacing.x), -padding.y);
            }
            else // Grid
            {
                var row = idx / constraintCount;
                var col = idx % constraintCount;
                position = new Vector2(
                    padding.x + col * (width + spacing.x),
                    GetCachedPositionForIndex(idx)
                );
            }

            rt.anchoredPosition = position;
        }

        private void UpdateContentSize()
        {
            if (_handler == null) return;

            var count = _handler.GetDataCount();
            if (count == 0)
            {
                content.sizeDelta = Vector2.zero;
                return;
            }

            float width = 0f, height = 0f;

            if (layoutType == GridLayoutType.Vertical)
            {
                for (var i = 0; i < count; i++)
                {
                    height += GetElementSize(i).y;
                }
                height += spacing.y * (count - 1) + padding.y * 2;
                _totalContentSize = height;
                content.sizeDelta = new Vector2(content.sizeDelta.x, height);
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                for (var i = 0; i < count; i++)
                {
                    width += GetElementSize(i).x;
                }
                width += spacing.x * (count - 1) + padding.x * 2;
                _totalContentSize = width;
                content.sizeDelta = new Vector2(width, content.sizeDelta.y);
            }
            else if (layoutType == GridLayoutType.Grid)
            {
                var rowCount = Mathf.CeilToInt((float)count / constraintCount);
                var yTotal = 0f;

                for (var row = 0; row < rowCount; row++)
                {
                    var maxRowHeight = 0f;
                    for (var col = 0; col < constraintCount; col++)
                    {
                        var i = row * constraintCount + col;
                        if (i >= count) break;
                        maxRowHeight = Mathf.Max(maxRowHeight, GetElementSize(i).y);
                    }
                    yTotal += maxRowHeight;
                }
                yTotal += spacing.y * (rowCount - 1) + padding.y * 2;

                var colCount = Mathf.Min(constraintCount, count);
                var totalSpacingX = spacing.x * (colCount - 1);
                width = content.rect.width;
                height = yTotal;

                _totalContentSize = height;
                content.sizeDelta = new Vector2(width, height);
            }
        }

        private void OnValueChanged(Vector2 value)
        {
            // Performance: Only update if scroll distance exceeds threshold
            var currentPos = content.anchoredPosition;
            var scrollDelta = Vector2.Distance(currentPos, _lastScrollPosition);

            if (scrollDelta < updateThreshold * 100f) // Scale threshold appropriately
                return;

            _lastScrollPosition = currentPos;

            var (newVisibleIndexMin, newVisibleIndexMax) = GetVisibleIndices();

            if (newVisibleIndexMin == _visibleIndexMin && newVisibleIndexMax == _visibleIndexMax)
                return;

            _visibleIndexMin = newVisibleIndexMin;
            _visibleIndexMax = newVisibleIndexMax;

            // Batch hide operations and return to correct pools
            var indicesToHide = new List<int>();
            foreach (var kvp in _activeElements)
            {
                var idx = kvp.Key;
                if (idx < newVisibleIndexMin || idx > newVisibleIndexMax)
                {
                    if (kvp.Value.IsVisible)
                        indicesToHide.Add(idx);
                }
            }

            // Hide elements in batch and return to appropriate pools
            foreach (var idx in indicesToHide)
            {
                var element = _activeElements[idx];
                element.IsVisible = false;
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);

                // Return to the correct pool based on prefab type
                int prefabType = element.PrefabType;
                if (_pools.ContainsKey(prefabType))
                {
                    _pools[prefabType].Enqueue(element.gameObject);
                }

                // Remove from active elements since it's now pooled
                _activeElements.Remove(idx);
            }

            // Show new elements
            if (newVisibleIndexMin <= newVisibleIndexMax && newVisibleIndexMax >= 0)
            {
                for (var i = newVisibleIndexMin; i <= newVisibleIndexMax; ++i)
                {
                    if (!_activeElements.ContainsKey(i))
                    {
                        CreateElementForIndex(i);
                    }
                }
            }
        }

        public void ReleaseAll()
        {
            foreach (var element in _activeElements.Values)
            {
                element.IsVisible = false;
                _handler.OnElementHide(element);
                _handler.OnElementRelease(element);
                Destroy(element.gameObject);
            }
            _activeElements.Clear();

            // Clear all pools
            if (_pools != null)
            {
                foreach (var pool in _pools.Values)
                {
                    while (pool.Count > 0)
                    {
                        var go = pool.Dequeue();
                        if (go != null)
                        {
                            var element = go.GetComponent<GridScrollViewElement>();
                            element.IsVisible = false;
                            _handler.OnElementHide(element);
                            _handler.OnElementRelease(element);
                            Destroy(element.gameObject);
                        }
                    }
                }
            }
        }

        public GridScrollViewElement GetElementAtIndex(int index)
        {
            return _activeElements.TryGetValue(index, out var element) ? element : null;
        }
    }
}