using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class TreeManager : MonoBehaviour
    {
        public static TreeManager Instance { get; private set; }

        private List<GameObject> _spawnedTrees = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterTree(GameObject tree)
        {
            _spawnedTrees.Add(tree);
        }

        public void ClearAllTrees()
        {
            foreach (var tree in _spawnedTrees)
            {
                if (tree != null)
                {
                    Destroy(tree.gameObject);
                }
            }

            _spawnedTrees.Clear();
        }

        public int GetTreeCount()
        {
            return _spawnedTrees.Count;
        }
    }
}