using System.Collections;
using UnityEngine;

namespace Tridinet.WorldEditor
{

    /// <summary>
    /// this script contains data to manage world creation
    /// </summary>
    public class WorldDataContainer : ScriptableObject
    {
        public string description;
        public string data;
        public WorldAccess access;
        public string privateKey;
        public WorldType type;
        public string path;
        public string manifestPath;
    }

    /// <summary>
    /// This refers to the world type
    /// root means base url
    /// mask means it replaces the root entirely
    /// appendix means its being added to the root or mask
    /// </summary>
    public enum WorldType
    {
        root, mask, appendix
    }


    /// <summary>
    /// This describes the access level of the world
    /// </summary>
    public enum WorldAccess
    {
        @public, @private
    }
}