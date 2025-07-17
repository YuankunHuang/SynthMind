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
        private float _restartTimer = 0f;

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

            _tickPerSecTimer += Time.deltaTime;
            if (_tickPerSecTimer >= 1f)
            {
                _tickPerSecTimer -= 1f;
                OnTickPerSec?.Invoke();
            }

            if (InputManager.GetKey(KeyCode.Escape))
            {
                if (_restartTimer > -0.1f) // only trigger once per press
                {
                    _restartTimer += Time.deltaTime;
                    if (_restartTimer >= 1f)
                    {
                        GameManager.Restart();
                        _restartTimer = -1f;
                    }
                }
            }
            else
            {
                _restartTimer = 0;
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