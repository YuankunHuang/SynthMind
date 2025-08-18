using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class MonoManager : MonoBehaviour
    {
        public static MonoManager Instance { get; private set; }

        public event Action OnTick;
        public event Action OnLateTick;
        public event Action OnFixedTick;
        public event Action OnTickPerSec;

        private float _tickPerSecTimer = 0f;

        public static void CreateInstance()
        {
            if (Instance != null)
            {
                Debug.LogWarning("MonoManager instance already exists. Destroying the old instance.");
                Destroy(Instance.gameObject);
            }
            GameObject go = new GameObject("MonoManager");
            Instance = go.AddComponent<MonoManager>();
            DontDestroyOnLoad(go);
        }

        public static void DestroyInstance()
        {
            if (Instance != null)
            {
                Instance.Shutdown();
                Destroy(Instance.gameObject);
                Instance = null;
            }
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
    }
}