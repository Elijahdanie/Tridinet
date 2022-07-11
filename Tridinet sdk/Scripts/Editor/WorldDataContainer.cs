using System.Collections;
using Tridinet.WorldEditor;
using UnityEngine;

namespace Tridinet.WorldEditor
{
    public class WorldDataContainer : ScriptableObject
    {
        public string id;
        public string description;
        public WorldAccess access;
        public string privateKey;
        public WorldType type;
        public string path;
        public string manifestPath;
        public string url;
        public string script;
        public string storeUri;
        public Vector3 startPosition;
    }
}
