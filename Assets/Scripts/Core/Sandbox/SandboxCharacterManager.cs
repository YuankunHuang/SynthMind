using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class SandboxCharacterManager : MonoBehaviour
    {
        public static SandboxCharacterManager Instance { get; private set; }

        [SerializeField] private GameObject _characterPrefab;
        private GameObject _aiCharacter;
        private Coroutine _moveCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CreateAICharacter();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void CreateAICharacter()
        {
            if (_aiCharacter != null) return;

            if (_characterPrefab != null)
            {
                _aiCharacter = Instantiate(_characterPrefab);
            }
            else
            {
                // Create simple 3D character
                _aiCharacter = new GameObject("AI_Character");

                // Body (capsule)
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Character_Body";
                body.transform.SetParent(_aiCharacter.transform);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
                body.GetComponent<Renderer>().material.color = Color.blue;

                // Head (sphere)
                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Character_Head";
                head.transform.SetParent(_aiCharacter.transform);
                head.transform.localPosition = new Vector3(0, 1.2f, 0);
                head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                head.GetComponent<Renderer>().material.color = new Color(1f, 0.8f, 0.7f); // Skin tone

                // Eyes
                var leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leftEye.name = "Left_Eye";
                leftEye.transform.SetParent(head.transform);
                leftEye.transform.localPosition = new Vector3(-0.2f, 0.1f, 0.4f);
                leftEye.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                leftEye.GetComponent<Renderer>().material.color = Color.black;

                var rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rightEye.name = "Right_Eye";
                rightEye.transform.SetParent(head.transform);
                rightEye.transform.localPosition = new Vector3(0.2f, 0.1f, 0.4f);
                rightEye.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                rightEye.GetComponent<Renderer>().material.color = Color.black;
            }

            if (SandboxManager.Instance != null && SandboxManager.Instance.EnvironmentRoot != null)
            {
                _aiCharacter.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            }

            _aiCharacter.transform.position = Vector3.zero;
            _aiCharacter.tag = "AI_Character";

            // Set layer for character and all children
            SetLayerRecursively(_aiCharacter, LayerMask.NameToLayer(LayerNames.Sandbox));

            LogHelper.Log("AI Character created");
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (layer == -1) layer = 0; // Use default if Sandbox layer not found

            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public GameObject GetAICharacter()
        {
            if (_aiCharacter == null)
            {
                CreateAICharacter();
            }
            return _aiCharacter;
        }

        public void MoveCharacterTo(Vector3 target, float duration = 2f)
        {
            if (_aiCharacter == null) return;

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveCoroutine(target, duration));
        }

        public void MoveCharacterBy(Vector3 offset, float duration = 1.5f)
        {
            if (_aiCharacter == null) return;

            Vector3 target = _aiCharacter.transform.position + offset;
            MoveCharacterTo(target, duration);
        }

        private IEnumerator MoveCoroutine(Vector3 target, float duration)
        {
            if (_aiCharacter == null) yield break;

            var startPos = _aiCharacter.transform.position;
            var elapsed = 0f;

            // Look at target
            Vector3 direction = (target - startPos).normalized;
            if (direction != Vector3.zero)
            {
                _aiCharacter.transform.rotation = Quaternion.LookRotation(direction);
            }

            // Move with smooth animation
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _aiCharacter.transform.position = Vector3.Lerp(startPos, target, t);
                yield return null;
            }

            _aiCharacter.transform.position = target;
            _moveCoroutine = null;

            LogHelper.Log($"AI Character moved to {target}");
        }

        public void ResetCharacter()
        {
            if (_aiCharacter != null)
            {
                if (_moveCoroutine != null)
                {
                    StopCoroutine(_moveCoroutine);
                    _moveCoroutine = null;
                }
                _aiCharacter.transform.position = Vector3.zero;
                _aiCharacter.transform.rotation = Quaternion.identity;
            }
        }

        public Vector3 GetCharacterPosition()
        {
            return _aiCharacter != null ? _aiCharacter.transform.position : Vector3.zero;
        }
    }
}