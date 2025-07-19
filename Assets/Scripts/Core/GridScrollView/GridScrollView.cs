using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YuankunHuang.Unity.Core
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
    }

    public interface IGridHandler
    {
        int GetDataCount();
        Vector2 GetElementSize(int index); // (width, height)
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
        public GameObject itemPrefab;

        [Header("Layout")]
        public GridLayoutType layoutType = GridLayoutType.Vertical;
        public int constraintCount = 1;

        [Header("Movement")]
        public bool enableSmoothScroll = true;
        [Range(0.01f, 1f)] public float smoothScrollDuration = 0.15f;

        private Queue<GameObject> _pool;
        private Dictionary<int, GridScrollViewElement> _activeElements;
        private IGridHandler _handler;
        private Dictionary<int, Vector2> _elementSizes = new();
        private Dictionary<int, float> _elementPositions = new();
        private float _totalContentSize = 0f;
        private Coroutine _scrollCoroutine;
        private int _visibleIndexMin = -1;
        private int _visibleIndexMax = -1;

        private bool _isFixingDeferredSizes = false;
        private readonly List<int> _deferredSizeFixIndices = new();

        private Vector2 spacing => layoutGroup switch
        {
            HorizontalOrVerticalLayoutGroup hv => hv.spacing * Vector2.one,
            GridLayoutGroup grid => grid.spacing,
            _ => Vector2.zero
        };
        private Vector2 padding => layoutGroup switch
        {
            HorizontalOrVerticalLayoutGroup hv => new(hv.padding.left, hv.padding.top),
            GridLayoutGroup grid => new(grid.padding.left, grid.padding.top),
            _ => Vector2.zero
        };
        public void SetHandler(IGridHandler handler) => _handler = handler;

        private void OnValidate()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponentInChildren<ScrollRect>();
            }
            if (content == null)
            {
                content = scrollRect != null ? scrollRect.content : null;
            }
            if (layoutGroup == null)
            {
                layoutGroup = content != null ? content.GetComponent<LayoutGroup>() : null;
            }
            if (contentSizeFitter == null)
            {
                contentSizeFitter = content != null ? content.GetComponent<ContentSizeFitter>() : null;
            }
        }

        public void Activate()
        {
            _pool = new();
            _activeElements = new();

            layoutGroup.enabled = false;
            contentSizeFitter.enabled = false;

            scrollRect.onValueChanged.AddListener(OnValueChanged);
        }

        public void Deactivate()
        {
            _pool = null;
            _activeElements = null;

            scrollRect.onValueChanged.RemoveAllListeners();
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
            {
                return;
            }

            var elementPos = CalculatePositionForIndex(index);

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
            {
                targetY = 0f; // Ensure we don't scroll above the top
            }
            var target = new Vector2(content.anchoredPosition.x, targetY);
            _scrollCoroutine = StartCoroutine(SmoothScrollTo(target, smoothScrollDuration));
        }

        public void SmoothScrollToElement(int index)
        {
            if (index < 0 || index >= _handler.GetDataCount())
            {
                return;
            }
            var targetY = -CalculatePositionForIndex(index);
            if (targetY < 0f)
            {
                targetY = 0f; // Ensure we don't scroll above the top
            }
            var targetPos = new Vector2(content.anchoredPosition.x, targetY);
            _scrollCoroutine = StartCoroutine(SmoothScrollTo(targetPos, smoothScrollDuration));
        }

        private IEnumerator SmoothScrollTo(Vector2 targePos, float duration)
        {
            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }

            var startPos = content.anchoredPosition;
            var time = 0f;

            scrollRect.StopMovement();
            while (time < duration)
            {
                content.anchoredPosition = Vector2.Lerp(startPos, targePos, Mathf.Clamp01(time / duration));
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            content.anchoredPosition = targePos;
            _scrollCoroutine = null;
        }

        private float CalculatePositionForIndex(int idx)
        {
            float offset = 0f;

            if (layoutType == GridLayoutType.Vertical)
            {
                for (int i = 0; i < idx; i++)
                {
                    offset += GetElementSize(i).y;
                }
                offset += spacing.y * idx;
                return -padding.y - offset;
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                for (int i = 0; i < idx; i++)
                {
                    offset += GetElementSize(i).x;
                }
                offset += spacing.x * idx;
                return padding.x + offset;
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
                return -padding.y - yOffset;
            }
            else
            {
                LogHelper.LogError($"[GridScrollView]::CalculatePositionForIndex: Unsupported layout type {layoutType}");
                return 0;
            }
        }

        private Vector2 CalculateElementSize()
        {
            var parentSize = content.rect.size;
            Vector2 size = Vector2.zero;

            switch (layoutType)
            {
                case GridLayoutType.Vertical:
                    size.x = parentSize.x - padding.x * 2;
                    size.y = -1; // let height be determined per index via handler
                    break;

                case GridLayoutType.Horizontal:
                    size.y = parentSize.y - padding.y * 2;
                    size.x = -1; // let width be determined per index via handler
                    break;

                case GridLayoutType.Grid:
                    int columnCount = constraintCount;
                    if (columnCount < 1)
                    {
                        columnCount = 1;
                    }

                    float totalSpacingX = spacing.x * (columnCount - 1);
                    float availableWidth = parentSize.x - padding.x * 2 - totalSpacingX;
                    float cellWidth = availableWidth / columnCount;

                    size.x = cellWidth;
                    size.y = cellWidth; // square, or you can customize this
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

        private (int min, int max) GetVisibleIndices()
        {
            int count = _handler.GetDataCount();
            if (count == 0) return (0, -1);

            float viewStart, viewEnd;

            switch (layoutType)
            {
                case GridLayoutType.Vertical:
                case GridLayoutType.Grid:
                    viewStart = -content.anchoredPosition.y;
                    viewEnd = viewStart - scrollRect.viewport.rect.height;
                    break;
                case GridLayoutType.Horizontal:
                    viewStart = -content.anchoredPosition.x;
                    viewEnd = viewStart + scrollRect.viewport.rect.width;
                    break;
                default:
                    return (0, -1);
            }

            int min = -1, max = -1;

            for (int i = 0; i < count; i++)
            {
                float pos = CalculatePositionForIndex(i);
                float size = layoutType == GridLayoutType.Horizontal ? GetElementSize(i).x : GetElementSize(i).y;

                float elementStart = pos;
                float elementEnd = layoutType == GridLayoutType.Horizontal ? pos + size : pos - size;

                bool isVisible = layoutType == GridLayoutType.Horizontal
                    ? (elementEnd >= viewStart && elementStart <= viewEnd)
                    : (elementEnd <= viewStart && elementStart >= viewEnd);

                if (isVisible)
                {
                    if (min == -1) min = i;
                    max = i;
                }
                else if (min != -1 && !isVisible)
                {
                    // Break early once we've exited the visible range
                    break;
                }
            }

            return (min == -1) ? (0, -1) : (min, max);
        }

        public void Refresh()
        {
            _elementSizes.Clear();
            _elementPositions.Clear();

            foreach (var element in _activeElements.Values)
            {
                element.IsVisible = false;
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);
                _pool.Enqueue(element.gameObject);
            }
            _activeElements.Clear();

            UpdateContentSize();

            var (min, max) = GetVisibleIndices();

            for (var idx = min; idx <= max; ++idx)
            {
                CreateElementForIndex(idx);
            }

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
            if (index != _handler.GetDataCount() - 1) // must be last logical item
                return;          

            EnsureSizeCacheForIndex(index); // size cache
            CreateElementForIndex(index); // create and position the new item
            UpdateContentSize();   // cheap - only rows/cols recalculated
        }

        public void AppendTop(int index, float keepViewportFraction = 1f)
        {
            if (index != 0) return;

            EnsureSizeCacheForIndex(index);
            var size = _elementSizes[index];

            float grow = 0f;
            if (layoutType == GridLayoutType.Vertical)
            {
                grow = _activeElements.Count > 0
                    ? size.y + spacing.y
                    : size.y;
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                grow = _activeElements.Count > 0
                    ? size.x + spacing.x
                    : size.x;
            }

            // 2. Shift all active elements
            foreach (var kv in _activeElements)
            {
                var rt = (RectTransform)kv.Value.transform;
                if (layoutType == GridLayoutType.Vertical)
                {
                    rt.anchoredPosition += new Vector2(0f, grow);
                }
                else if (layoutType == GridLayoutType.Horizontal)
                {
                    rt.anchoredPosition += new Vector2(grow, 0f);
                }
            }

            // 3. Shift scroll position to preserve view
            if (layoutType == GridLayoutType.Vertical)
            {
                content.anchoredPosition += new Vector2(0f, grow * keepViewportFraction);
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                content.anchoredPosition -= new Vector2(grow * keepViewportFraction, 0f);
            }

            // 4. Shift active element index mapping
            var shifted = new Dictionary<int, GridScrollViewElement>();
            foreach (var kv in _activeElements)
            {
                kv.Value.Index = kv.Key + 1;
                shifted[kv.Key + 1] = kv.Value;
            }
            _activeElements = shifted;

            // 5. Create new element at index 0
            CreateElementForIndex(0);

            UpdateContentSize();
        }

        public void GoToBottom()
        {
            if (enableSmoothScroll)
            {
                SmoothScrollToBottom();
            }
            else
            {
                SnapToBottom();
            }
        }

        private void CreateElementForIndex(int idx)
        {
            var isNewElement = _pool.Count < 1;
            GameObject go;
            if (isNewElement)
            {
                go = Instantiate(itemPrefab, content);
            }
            else
            {
                go = _pool.Dequeue();
                go.transform.SetParent(content, false);
                go.transform.localScale = Vector3.one;
            }
            go.SetActive(true);

            var element = go.GetComponent<GridScrollViewElement>();
            element.Index = idx;

            if (isNewElement)
            {
                _handler.OnElementCreate(element);
            }

            element.IsVisible = true;
            _handler.OnElementShow(element);
            _activeElements[idx] = element;

            _deferredSizeFixIndices.Add(idx);
            if (!_isFixingDeferredSizes)
            {
                _isFixingDeferredSizes = true;
                StartCoroutine(FixDeferredSizes());
            }

            SetElementPosition(element, idx);
        }

        private IEnumerator FixDeferredSizes()
        {
            if (_deferredSizeFixIndices.Count < 1)
            {
                yield break;
            }

            yield return null; // wait for layout pass

            foreach (int idx in _deferredSizeFixIndices)
            {
                var element = GetElementAtIndex(idx);
                if (element == null) continue;

                var rt = (RectTransform)element.transform;
                var actualHeight = rt.sizeDelta.y;

                _elementSizes[idx] = rt.sizeDelta;
            }

            foreach (var kv in _activeElements)
            {
                SetElementPosition(kv.Value, kv.Key);
            }

            UpdateContentSize();
            SnapToBottom();

            _deferredSizeFixIndices.Clear();
            _isFixingDeferredSizes = false;
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
                position = new Vector2(padding.x, CalculatePositionForIndex(idx));
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
                    CalculatePositionForIndex(idx)
                );
            }

            rt.anchoredPosition = position;
        }

        private void UpdateContentSize()
        {
            if (_handler == null)
            {
                return;
            }

            var width = 0f;
            var height = 0f;

            var count = _handler.GetDataCount();

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
                        if (i >= count) 
                            break;
                        maxRowHeight = Mathf.Max(maxRowHeight, GetElementSize(i).y);
                    }
                    yTotal += maxRowHeight;
                }
                yTotal += spacing.y * (rowCount - 1) + padding.y * 2;

                var colCount = Mathf.Min(constraintCount, count);
                var totalSpacingX = spacing.x * (colCount - 1);
                var cellWidth = (content.rect.width - padding.x * 2 - totalSpacingX) / colCount;
                width = content.rect.width;
                height = yTotal;

                content.sizeDelta = new Vector2(width, height);
            }
        }

        private void OnValueChanged(Vector2 value)
        {
            var (newVisibleIndexMin, newVisibleIndexMax) = GetVisibleIndices();

            if (newVisibleIndexMin == _visibleIndexMin && newVisibleIndexMax == _visibleIndexMax)
            {
                return; // no change in visible indices
            }

            _visibleIndexMin = newVisibleIndexMin;
            _visibleIndexMax = newVisibleIndexMax;

            // to hide
            var indicesToHide = new List<int>();
            foreach (var kvp in _activeElements)
            {
                var idx = kvp.Key;
                if (idx < newVisibleIndexMin || idx > newVisibleIndexMax)
                {
                    if (kvp.Value.IsVisible)
                    {
                        indicesToHide.Add(idx);
                    }
                }
            }

            foreach (var idx in indicesToHide)
            {
                var element = _activeElements[idx];
                element.IsVisible = false;
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);
                element.IsVisible = false;
            }

            // to show
            if (newVisibleIndexMin <= newVisibleIndexMax && newVisibleIndexMax >= 0)
            {
                for (var i = newVisibleIndexMin; i <= newVisibleIndexMax; ++i)
                {
                    if (!_activeElements.TryGetValue(i, out var element))
                    {
                        CreateElementForIndex(i);
                    }
                    else
                    {
                        if (!element.IsVisible)
                        {
                            element.gameObject.SetActive(true);
                            element.IsVisible = true;
                            _handler.OnElementShow(element);
                        }
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

            while (_pool != null && _pool.Count > 0)
            {
                var go = _pool.Dequeue();
                if (go != null)
                {
                    var element = go.GetComponent<GridScrollViewElement>();
                    if (element != null)
                    {
                        _handler.OnElementRelease(element);
                    }
                    Destroy(go);
                }
            }
        }

        public GridScrollViewElement GetElementAtIndex(int index)
        {
            return _activeElements.TryGetValue(index, out var element) ? element : null;
        }
    }
}