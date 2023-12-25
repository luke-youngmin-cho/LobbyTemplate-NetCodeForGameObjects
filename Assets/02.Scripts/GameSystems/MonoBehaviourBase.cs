using UnityEngine;

namespace T.GameSystems
{
    public abstract class MonoBehaviourBase : MonoBehaviour, IInitializable
    {
        public virtual void Init()
        {
        }
    }
}
