using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace T
{
    public static class NetworkBehaviourExtensions
    {
        public static bool TryGet(this ulong networkObjectID, out NetworkObject networkObject)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out networkObject))
            {
                return true;
            }

            return false;
        }
    }
}
