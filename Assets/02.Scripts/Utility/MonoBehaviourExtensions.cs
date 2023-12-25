using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace T.Utility
{
    public static class MonoBehaviourExtensions
    {
        public static T OfType<T>(this MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetComponent<T>();
        }
    }
}
