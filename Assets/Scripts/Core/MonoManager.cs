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

        private float _timer = 0f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            OnTick?.Invoke();

            _timer += Time.deltaTime;
            if (_timer >= 1f)
            {
                _timer -= 1f;
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