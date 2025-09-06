using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.UICore
{
    public class BlurBackground
    {
        private GameObject _blurObject;
        private RenderTexture _blurTexture;
        private Material _blurMaterial;

        public async Task<bool> CreateAsync(Transform parent, RenderTexture capturedTexture)
        {
            if (capturedTexture == null || parent == null) return false;

            _blurTexture = capturedTexture;

            // Create blur object
            _blurObject = new GameObject("BlurBackground");
            _blurObject.layer = LayerMask.NameToLayer(LayerNames.UI);
            var rectTransform = _blurObject.AddComponent<RectTransform>();
            var rawImage = _blurObject.AddComponent<RawImage>();

            // Setup transform
            rectTransform.SetParent(parent);
            rectTransform.SetAsFirstSibling();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            // Setup image
            rawImage.texture = _blurTexture;
            rawImage.raycastTarget = false;
            rawImage.uvRect = new Rect(0, 1, 1, -1);

            // Load and apply blur material
            try
            {
                _blurMaterial = await ResManager.LoadAssetAsync<Material>(AddressablePaths.UIBoxBlurMaterial);
                if (_blurMaterial != null)
                {
                    rawImage.material = _blurMaterial;
                    return true;
                }
                else
                {
                    rawImage.color = new Color(1, 1, 1, 0.8f);
                    LogHelper.LogWarning("[BlurBackground] Blur material not found, using fallback transparency");
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[BlurBackground] Failed to load blur material: {ex.Message}");
                Dispose();
                return false;
            }
        }

        public void Dispose()
        {
            if (_blurObject != null)
            {
                GameObject.Destroy(_blurObject);
                _blurObject = null;
            }

            if (_blurMaterial != null)
            {
                ResManager.Release(AddressablePaths.UIBoxBlurMaterial);
                _blurMaterial = null;
            }

            if (_blurTexture != null)
            {
                _blurTexture.Release();
                GameObject.Destroy(_blurTexture);
                _blurTexture = null;
            }
        }
    }
}