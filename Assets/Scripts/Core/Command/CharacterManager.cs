using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        [SerializeField] private GameObject _aiCharacterPrefab;
        private GameObject _aiCharacter;

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
            if (_aiCharacterPrefab != null)
            {
                _aiCharacter = Instantiate(_aiCharacterPrefab);
            }
            else // create a simple primitive if no prefab assigned
            {
                _aiCharacter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _aiCharacter.name = "AI_Character";
                _aiCharacter.transform.position = Vector3.zero;
                
                var renderer = _aiCharacter.GetComponent<Renderer>();
                renderer.material.color = Color.blue;
            }
        }

        public GameObject GetAICharacter()
        {
            return _aiCharacter;
        }

        public void MoveCharacterTo(GameObject character, Vector3 targetWorldPos)
        {
            if (character != null)
            {
                StartCoroutine(MoveCoroutine(character, targetWorldPos));
            }
        }

        private IEnumerator MoveCoroutine(GameObject character, Vector3 targetWorldPos)
        {
            var startPos = character.transform.position;
            var duration = 2f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                character.transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
                yield return null;
            }

            character.transform.position = targetWorldPos;
        }
    }
}