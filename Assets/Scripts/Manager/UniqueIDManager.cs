using UnityEngine;

namespace Manager
{
    public class UniqueIDManager : MonoBehaviour
    {
        private int _version;

        public int GetVersion() { return _version; }
        public void IncrementVersion() { _version++;}
    }
}