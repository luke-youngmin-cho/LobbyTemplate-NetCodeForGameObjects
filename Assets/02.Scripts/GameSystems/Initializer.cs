using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace T.GameSystems
{
    public class Initializer : MonoBehaviour
    {
        public static bool hasInitialized;
        public List<MonoBehaviourBase> targets;

#if UNITY_EDITOR
        public static Initializer Create(IEnumerable<MonoBehaviourBase> initializables)
        {
            Initializer initializer = FindObjectOfType<Initializer>();
            if (initializer != null)
                DestroyImmediate(initializer.gameObject);

            initializer = new GameObject($"Initializer").AddComponent<Initializer>();
            initializer.targets = new List<MonoBehaviourBase>(initializables);
            return initializer;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneChangedCallbacks()
        {
            SceneManager.sceneUnloaded += (scene) => hasInitialized = false;
        }

        private void Awake()
        {
            foreach (MonoBehaviourBase target in targets)
            {
                target.Init();
            }
            hasInitialized = true;
        }
    }
}