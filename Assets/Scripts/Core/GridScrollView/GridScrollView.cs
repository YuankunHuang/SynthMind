using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.HotUpdate;
using YuankunHuang.Unity.Util;

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
    }

    public interface IGridHandler
    {
        int GetDataCount();
        float GetElementHeight(int index);
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
        public LayoutGroup layoutGroup;
        public GameObject itemPrefab;

        [Header("Layout")]
        public GridLayoutType layoutType = GridLayoutType.Vertical;
        public int constraintCount = 1;
        
        private Vector2 spacing
        {
            get
            {
                Vector2 spacing;

                if (layoutGroup is HorizontalOrVerticalLayoutGroup hvLayoutGroup)
                {
                    spacing = hvLayoutGroup.spacing * Vector2.one;
                }
                else if (layoutGroup is GridLayoutGroup gridLayoutGroup)
                {
                    spacing = new Vector2(gridLayoutGroup.spacing.x, gridLayoutGroup.spacing.y);
                }
                else
                {
                    spacing = Vector2.zero;
                }

                return spacing;
            }
        }
        private Vector2 padding
        {
            get
            {
                Vector2 padding;

                if (layoutGroup is HorizontalOrVerticalLayoutGroup hvLayoutGroup)
                {
                    padding = new Vector2(hvLayoutGroup.padding.left, hvLayoutGroup.padding.top);
                }
                else if (layoutGroup is GridLayoutGroup gridLayoutGroup)
                {
                    padding = new Vector2(gridLayoutGroup.padding.left, gridLayoutGroup.padding.top);
                }
                else
                {
                    padding = Vector2.zero;
                }

                return padding;
            }
        }

        private Queue<GameObject> _pool;
        private Dictionary<int, GridScrollViewElement> _activeElements;
        private IGridHandler _handler;
        private Dictionary<int, float> _elementHeights = new();
        private Dictionary<int, float> _elementPositions = new();
        private float _totalContentHeight = 0f;

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
        }

        public void Activate()
        {
            _pool = new();
            _activeElements = new();

            scrollRect.onValueChanged.AddListener(OnValueChanged);
        }

        public void Deactivate()
        {
            _pool = null;
            _activeElements = null;

            scrollRect.onValueChanged.RemoveAllListeners();
        }






        private float CalculatePositionForIndex(int idx)
        {
            float yOffset = 0f;
            for (int i = 0; i < idx; i++)
            {
                yOffset += GetElementHeight(i) + spacing.y;
            }
            return -padding.y - yOffset;
        }

        private float GetElementHeight(int idx)
        {
            if (_elementHeights.TryGetValue(idx, out var height))
                return height;

            // 首次获取，从用户处获取并缓存
            height = _handler.GetElementHeight(idx);
            _elementHeights[idx] = height;
            return height;
        }

        private List<int> GetVisibleIndices()
        {
            var visibleIndices = new List<int>();
            var viewportRect = scrollRect.viewport.GetWorldRect();
            var contentOffset = content.anchoredPosition;

            // 使用高度信息快速计算可见范围
            float currentY = 0f;
            for (int i = 0; i < _handler.GetDataCount(); i++)
            {
                float elementHeight = GetElementHeight(i);
                float elementY = currentY + contentOffset.y;

                if (elementY + elementHeight >= viewportRect.yMin && elementY <= viewportRect.yMax)
                {
                    visibleIndices.Add(i);
                }

                currentY += elementHeight + spacing.y;
            }

            return visibleIndices;
        }

        public void Refresh()
        {
            _elementHeights.Clear();
            _elementPositions.Clear();

            foreach (var element in _activeElements.Values)
            {
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);
                _pool.Enqueue(element.gameObject);
            }
            _activeElements.Clear();

            UpdateContentHeight();

            var visibleIndices = GetVisibleIndices();
            foreach (var idx in visibleIndices)
            {
                CreateElementForIndex(idx);
            }
        }

        private void CreateElementForIndex(int idx)
        {
            var go = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(itemPrefab, content);
            go.SetActive(true);

            var element = go.GetComponent<GridScrollViewElement>();
            element.Index = idx;

            var isNewElement = !_activeElements.ContainsKey(idx);
            if (isNewElement)
            {
                _handler.OnElementCreate(element);
            }

            _handler.OnElementShow(element);
            _activeElements[idx] = element;

            SetElementPosition(element, idx);
        }

        private void SetElementPosition(GridScrollViewElement element, int idx)
        {
            var rt = (RectTransform)element.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);

            var elementHeight = GetElementHeight(idx);
            var itemSize = new Vector2(rt.sizeDelta.x, elementHeight);
            rt.sizeDelta = itemSize;

            Vector2 position;
            if (layoutType == GridLayoutType.Vertical)
            {
                position = new Vector2(padding.x, CalculatePositionForIndex(idx));
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                position = new Vector2(padding.x + idx * (itemSize.x + spacing.x), -padding.y);
            }
            else // Grid
            {
                var row = idx / constraintCount;
                var col = idx % constraintCount;
                position = new Vector2(
                    padding.x + col * (itemSize.x + spacing.x),
                    CalculatePositionForIndex(idx)
                );
            }

            rt.anchoredPosition = position;
        }

        private void UpdateContentHeight()
        {
            if (_handler == null) return;

            _totalContentHeight = 0f;
            for (int i = 0; i < _handler.GetDataCount(); i++)
            {
                _totalContentHeight += GetElementHeight(i) + spacing.y;
            }
            _totalContentHeight += padding.y * 2;

            // 更新 content 高度
            content.sizeDelta = new Vector2(content.sizeDelta.x, _totalContentHeight);
        }

        public void OnElementHeightChanged(int idx)
        {
            var oldHeight = GetElementHeight(idx);
            _elementHeights.Remove(idx);
            var newHeight = GetElementHeight(idx);

            if (Mathf.Abs(oldHeight - newHeight) > 0.01f)
            {
                for (int i = idx + 1; i < _handler.GetDataCount(); i++)
                {
                    if (_activeElements.ContainsKey(i))
                    {
                        var element = _activeElements[i];
                        SetElementPosition(element, i);
                    }
                }

                UpdateContentHeight();
            }
        }

        private void OnValueChanged(Vector2 value)
        {
            var visibleIndices = GetVisibleIndices();
            var newVisibleIndexMin = visibleIndices.Count > 0 ? visibleIndices[0] : 0;
            var newVisibleIndexMax = visibleIndices.Count > 0 ? visibleIndices[visibleIndices.Count - 1] : 0;

            // to hide
            var indicesToRemove = new List<int>();
            foreach (var kvp in _activeElements)
            {
                var idx = kvp.Key;
                if (idx < newVisibleIndexMin || idx > newVisibleIndexMax)
                {
                    indicesToRemove.Add(idx);
                }
            }

            foreach (var idx in indicesToRemove)
            {
                var element = _activeElements[idx];
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);
                _pool.Enqueue(element.gameObject);
                _activeElements.Remove(idx);
            }

            // to show
            for (var i = newVisibleIndexMin; i <= newVisibleIndexMax; ++i)
            {
                if (!_activeElements.ContainsKey(i))
                {
                    CreateElementForIndex(i);
                }
            }
        }

        public void ReleaseAll()
        {
            foreach (var element in _activeElements.Values)
            {
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

        public void ScrollToIndex(int index)
        {
            if (index < 0 || index >= _handler.GetDataCount()) return;

            var targetY = -CalculatePositionForIndex(index);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
        }

        public void ScrollToBottom()
        {
            var targetY = -_totalContentHeight + scrollRect.viewport.rect.height;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
        }

        public GridScrollViewElement GetElementAtIndex(int index)
        {
            return _activeElements.TryGetValue(index, out var element) ? element : null;
        }
    }
}