using Tridinet.Systems;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Tridinet.Utilities.Data
{

    [System.Serializable]
    public class Evector3 {
        public float x;
        public float y;
        public float z;

        public Evector3 Save(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
            return this;
        }

        public static Evector3[] getArray(Vector3[] vectors)
        {
            return vectors.Select(x => {
                return new Evector3().Save(x);
            }).ToArray();
        }

        public static Vector3[] getArray(Evector3[] vectors)
        {
            return vectors.Select(x => {
                return new Vector3(x.x, x.y, x.z);
            }).ToArray();
        }

        public Vector3 get() {
            return new Vector3(x, y, z);
        }
    }

    [System.Serializable]
    public class Evector2
    {
        public float x;
        public float y;

        public Evector2 Save(Vector2 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            return this;
        }

        public static Evector2[] getArray(Vector2[] vectors)
        {
            return vectors.Select(x => {
                return new Evector2().Save(x);
            }).ToArray();
        }

        public static Vector2[] getArray(Evector2[] vectors)
        {
            return vectors.Select(x => {
                return new Vector2(x.x, x.y);
            }).ToArray();
        }

        public Vector2 get()
        {
            return new Vector3(x, y);
        }
    }

    [System.Serializable]
    public class Etransform {
        public Evector3 position = new Evector3();
        public Evector3 Scale = new Evector3();
        public Evector4 Rotation = new Evector4();

        public Etransform Save(Transform transform) {
            position.Save(transform.position);
            Scale.Save(transform.localScale);
            Rotation.Save(transform.rotation);
            return this;
        }

        public Transform get(Transform transform)
        {
            transform.position = position.get();
            transform.localScale = Scale.get();
            transform.rotation = Rotation.getRotation();
            return transform;
        }

        public Transform getLocal(Transform transform)
        {
            transform.localPosition = position.get();
            transform.localScale = Scale.get();
            transform.localRotation = Rotation.getRotation();
            return transform;
        }

        internal Etransform SaveLocal(Transform transform)
        {
            position.Save(transform.localPosition);
            Scale.Save(transform.localScale);
            Rotation.Save(transform.localRotation);
            return this;
        }

        internal void Save(Vector3 pos, Vector3 setRot, Vector3 setScale)
        {
            position.Save(pos);
            Scale.Save(setScale);
            Rotation.Save(Quaternion.Euler(setRot));
        }
    }

    [System.Serializable]
    public class Evector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public void Save(Quaternion vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
            this.w = vector.w;
        }

        public Evector4() { }
        public Evector4 (Color vector)
        {
            this.x = vector.r;
            this.y = vector.g;
            this.z = vector.b;
            this.w = vector.a;
        }

        public Color getColor()
        {
            return x==0 && y == 0 && z == 0? new Color(1, 1, 1, w) : new Color(x, y, z, w);
        }

        public Quaternion getRotation()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    [System.Serializable]
    public class SignProps
    {
        public string name;
        public string email;
        public string password;
    }

    [System.Serializable]
    public class TRepository
    {
        public string id;
        public string name;
        public string description;
        public string manifestUrl;
        public string previewUrl;
        public float cost;
        public string category;
    }

    [System.Serializable]
    public class worldList
    {
        public string Name;
        public string Description;
        public string url;
        public string previewUri;
    }

    [System.Serializable]
    public class WorldPayload
    {
        public string name;
        public string description;
        public string data;
        public string access;
        public string privateKey;
        public string type;
        public string id;
        public string url;
    }

    [System.Serializable]
    public class ResponseData
    {
        public bool success;
        public string data;
    }

    [System.Serializable]
    public class WorldResponse
    {
        public bool success;
        public WorldPayload data;
    }

    [System.Serializable]
    public class ItemBuilds
    {
        public bool success;
        public int total;
        public List<TRepository> data = new List<TRepository>();
    }

    [System.Serializable]
    public class WorldsList
    {
        public bool success;
        public string message;
        public int total;
        public List<worldList> data = new List<worldList>();
    }

    [System.Serializable]
    public class UrlPayload
    {
        public string url;
        public string password;
    }

    public class ObjectData  {
        Etransform transform;
    }

    [System.Serializable]
    public class EObject
    {
        public string uid { get; set; }
        public Etransform transform { get; set; }
    }

    public enum OwnerShip {
        Global, User
    }

    /// <summary>
    /// This is a data interface
    /// </summary>
    public interface IUser {
        WorldSector world { get; set; }
    }

    [System.Serializable]
    public class World {
        public string id;
        public string name;
        public string server;
        public string port;
        public Evector3 startPosition;
        public List<WorldSector> worldSectors = new List<WorldSector>();
        public CompiledCode script;
    }

    [System.Serializable]
    public class WorldSector {
        public Evector3 averagePosition = new Evector3();
        public List<NodeData> children = new List<NodeData>();
        public int Id;

        public WorldSector() { }

        public WorldSector(int Id, ref World world)
        {
            this.Id = Id;
            world.worldSectors.Add(this);
        }
    }

    public interface IDamage {
        float health { get;}
    }

    public interface IOffensive {
        float damageAmmount { get; set; }
    }

    public interface INode : IDamage {

        CompiledCode script { get; set; }
        NodeData data { get; set; }

        Vector3 offset { get; set; }
        GameObject gameObject { get; }
        Transform transform { get;}

        void InitNode(NodeData node);

        void DeleteNode();
        //void SetOffset(ActionList actionlist);
        //Vector3 Calculate(Axis axis, ActionList actionList, Vector3 difference, float multiplier);
    }

    [System.Serializable]
    public class TGameObject
    {
        public string assetId;
        public string instanceId;
        public string url;
        public string name;
        public TModel parentMesh;
        public List<TModel> meshes = new List<TModel>();
        public string assetPreview;
        public CompiledCode OnLoad;
        public ScriptEvents scriptEvents;
        public Etransform transform;
        public string repositoryId;
        public string nodeServerHostAddress;
        public string nodeServerPort;
    }

    [System.Serializable]
    public class ScriptEvents
    {
        public string OnEnter;
        public string OnExit;
        public string OnInteract;
    }

    [System.Serializable]
    public class TModel
    {
        public List<Evector2[]> uvs;
        public Etransform transform;
        public string name;
        public string ShaderName;
        public int[] triangle;
        public Evector3[] indices;
        public TMaterial[] materials;
        public List<int[]> subMeshes;
    }

    [System.Serializable]
    public class TMaterial
    {
        public string name;
        public string ShaderName;
        public Evector4 color;
    }

    public class TMatexture
    {
        public Evector2 mainTextureOffset;
        public Evector2 mainTextureScale;
        public string mainTexture;
    }

    [System.Serializable]
    public class TManifest {
        public TManifest() {
            setDirectory();
        }

        public void ReloadLocalRepo() {
            var result = Directory.GetFiles(TManifest.RepositoryPath);
            foreach (var item in result)
            {
                Debug.Log(item);
            }
            List<string> ids = new List<string>();
            List<string> currkeys = tridiObjects.Keys.ToList();
            foreach (var item in result)
            {
                if (isTridinetFile(item, out string itemId))
                {
                    if (!currkeys.Contains(itemId))
                    {
                        if (NodeBank.main == null)
                        {
                            NodeBank.main = GameObject.FindObjectOfType<NodeBank>();
                        }
                        var data = NodeBank.main.FetchFromProjectFile(itemId);
                        Debug.Log(data);
                        RecordItem(data.assetId, data.instanceId, data.name);
                    }
                    ids.Add(itemId);
                }
            }
            var obsoleteKeys = currkeys.Except(ids).ToList();
            foreach (var item in obsoleteKeys)
            {
                tridiObjects.Remove(item);
            }
        }

        private static bool isTridinetFile(string item, out string id) {
            try
            {
                var result = item.Split('\\')[1].Split('.');
                if (result[1] == "tridinet")
                {
                    id = result[0];
                    return true;
                }
                else
                {
                    id = "";
                    return false;
                }
            }
            catch (Exception)
            {
                id = "";
                return false;
            }
        }

        public void LoadLocalItem()
        {
            if (File.Exists(RepositoryFile))
            {
                var file = File.ReadAllText(RepositoryFile);
                var data = JsonConvert.DeserializeObject<TManifest>(file);
                name = data.name;
                tridiObjects = data.tridiObjects;
                tridinetUnityMapping = data.tridinetUnityMapping;
            }
            else
            {
                name = "manifest";
                tridiObjects = new Dictionary<string, TObjectKeyPair>();
                tridinetUnityMapping = new Dictionary<string, string>();
            }
        }

        public Dictionary<string, TObjectKeyPair> tridiObjects = new Dictionary<string, TObjectKeyPair>();
        public Dictionary<string, string> tridinetUnityMapping = new Dictionary<string, string>();
        public string name;
        public string id;
        public static string RepositoryPath = "./Assets/Tridinet/Repository";
        public static string otherManifest = "./Assets/Tridinet/Repository/otherManifest";
        public static string RepositoryFile = "./Assets/Tridinet/Repository/manifest.json";
        public static string rootPath = "./Assets/Tridinet";
        public static string WorldsPath = "./Assets/Tridinet/Worlds";

        public static string ScriptPath = "./Assets/Tridinet/Repository/Scripts";

        public static void setDirectory() {
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            if (!Directory.Exists(WorldsPath))
            {
                Directory.CreateDirectory(WorldsPath);
            }
            if (!Directory.Exists(RepositoryPath))
            {
                Directory.CreateDirectory(RepositoryPath);
            }
            if (!Directory.Exists(otherManifest))
            {
                Directory.CreateDirectory(otherManifest);
            }
            if (!Directory.Exists(ScriptPath))
            {
                Directory.CreateDirectory(ScriptPath);
            }
        }

        public static void Save(TManifest manifest)
        {
            var file = JsonConvert.SerializeObject(manifest);
            File.WriteAllText(RepositoryFile, file);
        }

        public bool ValidateInstanceId(int instanceId)
        {
            return !tridinetUnityMapping.ContainsKey(instanceId.ToString());
        }

        public bool ValidateInstanceIdExternalManifest(string instanceId)
        {
            return !tridinetUnityMapping.ContainsKey(instanceId);
        }

        public void RecordItem(string itemId, string instanceId, string _name)
        {
            if (!tridinetUnityMapping.ContainsKey(instanceId))
            {
                TObjectKeyPair record = new TObjectKeyPair();
                record.assetId = itemId;
                record.instanceId = instanceId.ToString();
                record.name = _name;
                tridiObjects.Add(itemId, record);
                tridinetUnityMapping.Add(instanceId, itemId);
                TManifest.Save(this);
            }
        }

        public void RecordItem(string itemId, string Uri ="") {
            if (tridiObjects.ContainsKey(itemId))
            {
                var record = tridiObjects[itemId];
                record.Url = Uri;
                TManifest.Save(this);
            }
        }

        public bool ContainsUri(string id)
        {
            if (tridiObjects.ContainsKey(id))
            {
                var record = tridiObjects[id];
                if (record.Url != null && record.Url != "")
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public string getUri(string assetId)
        {
            Debug.Log(tridiObjects.Count);
            if(tridiObjects.ContainsKey(assetId))
                return tridiObjects[assetId].Url;
            return "";
        }

        public static TManifest reRoute(TManifest arg0)
        {
            arg0.tridinetUnityMapping = new Dictionary<string, string>();
            foreach (var item in arg0.tridiObjects)
            {
                item.Value.instanceId = $"{item.Value.instanceId}_{item.Value.assetId}";
                arg0.tridinetUnityMapping.Add(item.Value.assetId, item.Value.instanceId);
            }
            return arg0;
        }
    }

    public class TObjectKeyPair {
        public string assetId;
        public string Url;
        public string instanceId;
        public string name;
    }

    [System.Serializable]
    public class NodeData : EObject
    {
        public string assetId;
        public string url;
        public float health;
        public int sectorId;

        public NodeData() { }

        public NodeData(string uid, INode prefab, string assetId, int id)
        {
            this.uid = uid;
            this.assetId = assetId;
            transform = new Etransform();
            this.sectorId = id;
            Save(prefab);
        }

        public void Save(INode prefab) {
            transform.Save(prefab.transform);
        }


        OwnerShip ownerShip { get; }

    }
}
