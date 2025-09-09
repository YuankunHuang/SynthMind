using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;

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

            // 1. Load blur material first
            try
            {
                _blurMaterial = await ResManager.LoadAssetAsync<Material>(AddressablePaths.UIGaussianBlurMaterial).WithLogging();
                if (_blurMaterial == null)
                {
                    LogHelper.LogWarning("[BlurBackground] Blur material not found");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[BlurBackground] Failed to load blur material: {ex.Message}");
                return false;
            }

            // 2. Create blur result texture
            _blurTexture = RenderTexture.GetTemporary(
                capturedTexture.width, capturedTexture.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

            // 3. Apply blur effect (single pass)
            Graphics.Blit(capturedTexture, _blurTexture, _blurMaterial);

            // 4. Create UI object
            _blurObject = new GameObject("BlurBackground");
            _blurObject.layer = LayerMask.NameToLayer(LayerNames.UI);
            var rectTransform = _blurObject.AddComponent<RectTransform>();
            var rawImage = _blurObject.AddComponent<RawImage>();

            // 5. Setup transform
            rectTransform.SetParent(parent);
            rectTransform.SetAsFirstSibling();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            // 6. Setup image
            rawImage.texture = _blurTexture;
            rawImage.raycastTarget = false;
            rawImage.uvRect = new Rect(0, 1, 1, -1);

            return true;
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
                ResManager.Release(AddressablePaths.UIGaussianBlurMaterial);
                _blurMaterial = null;
            }

            if (_blurTexture != null)
            {
                RenderTexture.ReleaseTemporary(_blurTexture);
                _blurTexture = null;
            }
        }
    }
}