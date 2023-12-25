using UnityEditor;
using UnityEngine;
using T.Utility;
using T.GameSystems;
using System.Linq;

namespace T
{
    public class SceneBaker : Editor
    {
        [MenuItem("SceneBaker/Bake Initializables")]
        public static void Bake()
        {
            MonoBehaviourBase[] monos = FindObjectsOfType<MonoBehaviourBase>();
            Initializer.Create(monos.ToList());
        }
    }
}