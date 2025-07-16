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
        public int Index { get; private set; }
        public GameObject GameObj { get; private set; }

        public GridScrollViewElement(int index, GameObject gameObj)
        {
            Index = index;
            GameObj = gameObj;
        }
    }

    public interface IGridHandler
    {
        int GetDataCount();
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
        public GameObject itemPrefab;

        [Header("Layout")]
        public GridLayoutType layoutType = GridLayoutType.Vertical;
        public int constraintCount = 1;
        public Vector2 spacing = Vector2.zero;
        public Vector2 padding = Vector2.zero;

        private Queue<GameObject> _pool;
        private List<GridScrollViewElement> _activeElements;
        private HashSet<int> _visibleIndices;
        private IGridHandler _handler;

        public void Activate()
        {
            _pool = new();
            _activeElements = new();
            _visibleIndices = new();

            scrollRect.onValueChanged.AddListener(OnValueChanged);
        }

        public void Deactivate()
        {
            _pool = null;
            _activeElements = null;
            _visibleIndices = null;

            scrollRect.onValueChanged.RemoveAllListeners();
        }

        private void OnValueChanged(Vector2 pos)
        {
            var viewportRect = GetWorldRect(scrollRect.viewport);
            for (int i = 0; i < _activeElements.Count; ++i)
            {
                var element = _activeElements[i];
                var elementRect = GetWorldRect((RectTransform)element.transform);
                bool isVisible = viewportRect.Overlaps(elementRect);
                bool wasVisible = _visibleIndices.Contains(element.Index);

                if (isVisible && !wasVisible)
                {
                    _visibleIndices.Add(element.Index);
                    _handler.OnElementShow(element);
                }
                else if (!isVisible && wasVisible)
                {
                    _visibleIndices.Remove(element.Index);
                    _handler.OnElementHide(element);
                }
            }
        }

        Rect GetWorldRect(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            return new Rect(corners[0], corners[2] - corners[0]);
        }

        public void SetHandler(IGridHandler handler)
        {
            _handler = handler;
        }

        public void Refresh()
        {
            // recycle all
            foreach (var element in _activeElements)
            {
                _handler.OnElementHide(element);
                element.gameObject.SetActive(false);
                _pool.Enqueue(element.gameObject);
            }
            _activeElements.Clear();

            // create new
            for (var i = 0; i < _handler.GetDataCount(); ++i)
            {
                var go = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(itemPrefab, content);
                go.SetActive(true);

                var index = i;
                var element = new GridScrollViewElement(index, go);
                if (_activeElements.Count < 1) // ?
                {
                    _handler.OnElementCreate(element);
                }
                _handler.OnElementShow(element);
                _activeElements.Add(element);
            }

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            var count = _activeElements.Count;
            if (count < 1)
            {
                return;
            }

            var itemSize = ((RectTransform)(itemPrefab.transform)).sizeDelta;

            for (var i = 0; i < count; ++i)
            {
                var rt = (RectTransform)_activeElements[i].transform;
                var pos = Vector2.zero;

                if (layoutType == GridLayoutType.Vertical)
                {
                    pos = new Vector2(padding.x, -padding.y - i * (itemSize.y + spacing.y));
                }
                else if (layoutType == GridLayoutType.Horizontal)
                {
                    pos = new Vector2(padding.x + i * (itemSize.x + spacing.x), -padding.y);
                }
                else if (layoutType == GridLayoutType.Grid)
                {
                    var row = i / constraintCount;
                    var col = i % constraintCount;
                    pos = new Vector2(padding.x + col * (itemSize.x + spacing.x), -padding.y - col * (itemSize.y + spacing.y));
                }

                rt.anchoredPosition = pos;
            }

            // content size
            if (layoutType == GridLayoutType.Vertical)
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, padding.y * 2 + count * itemSize.y + (count - 1) * spacing.y);
            }
            else if (layoutType == GridLayoutType.Horizontal)
            {
                content.sizeDelta = new Vector2(padding.x * 2 + count * itemSize.x + (count - 1) * spacing.x, content.sizeDelta.y);
            }
            else if (layoutType == GridLayoutType.Grid)
            {
                var rowCount = Mathf.CeilToInt((float)count / constraintCount);
                content.sizeDelta = new Vector2(
                    padding.x * 2 + constraintCount * (itemSize.x + spacing.x),
                    padding.y * 2 + rowCount * itemSize.y + (rowCount - 1) * spacing.y
                );
            }
        }

        public void ReleaseAll()
        {
            foreach (var element in _activeElements)
            {
                _handler.OnElementHide(element);
                _handler.OnElementRelease(element);
            }
            _activeElements.Clear();
        }
    }
}