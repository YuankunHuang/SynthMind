using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.Core
{
    public class MonoManager : MonoBehaviour
    {
        public static MonoManager Instance { get; private set; }
        public static bool IsInitialized => Instance != null;

        public event Action OnTick;
        public event Action OnLateTick;
        public event Action OnFixedTick;
        public event Action OnTickPerSec;

        private float _tickPerSecTimer = 0f;

        public static void Initialize(MonoManager instance)
        {
            Instance = instance;
            LogHelper.Log("[MonoManager] Initialized");
        }

        public static void Dispose()
        {
            if (Instance != null)
            {
                Instance.Shutdown();
                Instance = null;
            }
            
            LogHelper.Log("[MonoManager] Disposed");
        }

        private void Shutdown()
        {
            OnTick = null;
            OnLateTick = null;
            OnFixedTick = null;
            OnTickPerSec = null;
            StopAllCoroutines();
        }

        private void Update()
        {
            OnTick?.Invoke();

            _tickPerSecTimer += Time.deltaTime;
            if (_tickPerSecTimer >= 1f)
            {
                _tickPerSecTimer -= 1f;
                OnTickPerSec?.Invoke();
            }
        }

        private void LateUpdate()
        {
            OnLateTick?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedTick?.Invoke();
        }

        // Firebase WebGL callback handler
        public void OnFirebaseCallback(string callbackData)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var parts = callbackData.Split('|', 2);
            if (parts.Length == 2)
            {
                YuankunHuang.Unity.FirebaseCore.WebGLFirebaseManager.OnJSCallback(parts[0], parts[1]);
            }
#endif
        }
    }
}