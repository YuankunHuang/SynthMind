using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.SandboxCore
{
    public class SandboxManager : MonoBehaviour
    {
        [Header("Sandbox Setup")]
        [SerializeField] private Camera _sandboxCam;
        [SerializeField] private RenderTexture _sandboxRenderTex;
        [SerializeField] private Transform _environmentRoot;
        [SerializeField] private Transform _groundPlane;
        [SerializeField] private Material _defaultMaterial;

        public static SandboxManager Instance { get; private set; }
        public Camera SandboxCam => _sandboxCam;
        public RenderTexture SandboxRenderTex => _sandboxRenderTex;
        public Transform EnvironmentRoot => _environmentRoot;

        private bool _isInitialized;
        private int _sandboxLayerIndex = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _sandboxLayerIndex = GetOrCreateSandboxLayer();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private int GetOrCreateSandboxLayer()
        {
            var layerIdx = LayerMask.NameToLayer(LayerNames.Sandbox);
            if (layerIdx == -1) // not found in existing layers
            {
                for (var i = 0; i < 32; ++i)
                {
                    var layerName = LayerMask.LayerToName(i);
                    if (string.IsNullOrEmpty(layerName)) // empty, available
                    {
                        LogHelper.LogError($"Sandbox layer not found! Please create a layer named {LayerNames.Sandbox} at index {i} in Project Settings > Tags and Layers");
                        return 0; // use default layer as fallback
                    }
                }

                LogHelper.LogError("No available layers found for Sandbox. Using Default layer.");
                return 0;
            }

            LogHelper.Log($"Using Sandbox layer at index: {layerIdx}");
            return layerIdx;
        }

        public void Initialize(RawImage rawImg, Action onComplete)
        {
            if (_isInitialized)
            {
                onComplete?.Invoke();
                return;
            }

            _isInitialized = true;

            InitRenderTexture(rawImg);
            InitCamera();
            InitEnvironment();

            LogHelper.Log("SandboxManager initialized successfully");
            onComplete?.Invoke();
        }

        private void InitRenderTexture(RawImage rawImg)
        {
            if (_sandboxRenderTex == null)
            {
                _sandboxRenderTex = new RenderTexture(Mathf.RoundToInt(rawImg.rectTransform.rect.width), Mathf.RoundToInt(rawImg.rectTransform.rect.height), 16, RenderTextureFormat.ARGB32);
                _sandboxRenderTex.name = "SandboxRenderTexture";
                _sandboxRenderTex.Create();
                rawImg.texture = _sandboxRenderTex;
            }
        }

        private void InitCamera()
        {
            if (_sandboxCam == null)
            {
                var camGO = new GameObject("SandboxCamera");
                _sandboxCam = camGO.AddComponent<Camera>();
                camGO.transform.SetParent(transform);
            }

            _sandboxCam.targetTexture = _sandboxRenderTex;
            _sandboxCam.cullingMask = 1 << _sandboxLayerIndex;
            _sandboxCam.clearFlags = CameraClearFlags.Skybox;

            // position to view the sandbox
            //_sandboxCam.transform.position = new Vector3(0, 5, -8);
            //_sandboxCam.transform.rotation = Quaternion.Euler(25, 0, 0);

            // must be "base" camera
            var urpData = _sandboxCam.GetUniversalAdditionalCameraData();
            urpData.renderType = CameraRenderType.Base;
        }

        private void InitEnvironment()
        {
            if (_environmentRoot != null && _environmentRoot.childCount > 0)
            {
                // assumbly all set
                return;
            }

            if (_environmentRoot == null)
            {
                var rootGO = new GameObject("Environment");
                rootGO.transform.SetParent(transform);
                _environmentRoot = rootGO.transform;
            }

            // create default ground plane if needed
            if (_groundPlane == null)
            {
                var groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
                groundGO.name = "Ground";
                groundGO.layer = LayerMask.NameToLayer(LayerNames.Sandbox);
                _groundPlane = groundGO.transform;
                _groundPlane.SetParent(_environmentRoot);
                _groundPlane.localScale = new Vector3(1000, 1, 1000);

                var renderer = _groundPlane.GetComponent<Renderer>();
                
                if (_defaultMaterial != null)
                {
                    renderer.material = _defaultMaterial;
                }
            }

            // add basic lighting
            var lightGO = new GameObject("Lighting");
            lightGO.transform.SetParent(_environmentRoot);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(45, 45, 0);
            light.cullingMask = 1 << _sandboxLayerIndex;

            LogHelper.Log("Sandbox environment initialized");
        }

        public GameObject CreateSimpleObject(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(_environmentRoot);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.layer = _sandboxLayerIndex >= 0 ? _sandboxLayerIndex : 0;
            obj.tag = TagNames.AISpawned;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (_defaultMaterial != null)
                {
                    renderer.material = _defaultMaterial;
                }
                renderer.material.color = color;
            }

            return obj;
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public void Clear()
        {
            foreach (Transform child in _environmentRoot)
            {
                if (child.CompareTag(TagNames.AISpawned))
                {
                    Destroy(child.gameObject);
                }
            }

            LogHelper.Log($"Sandbox Cleared");
        }

        private void Dispose()
        {
            if (_sandboxRenderTex != null)
            {
                Destroy(_sandboxRenderTex);
                _sandboxRenderTex = null;
            }

            _isInitialized = false;
        }

        public void OnDestroy()
        {
            Dispose();
        }
    }
}