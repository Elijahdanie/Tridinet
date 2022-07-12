using Assets.Scripts;
using Tridinet.Systems;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Tridinet.Utilities.Data
{

    /// <summary>
    /// This class interfaces with the WorldBuilder and the api
    /// for prvisioning and managing tridinet objects
    /// </summary>
    public class NodeBank : MonoBehaviour
    {
        public static NodeBank main;
        public bool refresh;
        public Dictionary<string, TRepository> map = new Dictionary<string, TRepository>();
        public Dictionary<string, Func<INode>> cache = new Dictionary<string, Func<INode>>();
        string[] localItems;

        private void Awake()
        {
            main = this;
            localItems = Directory.GetFiles("./tri");
        }

        private void Start()
        {
            EventManager.main.OnGetItem.AddListener(probeItem);
        }


        /// <summary>
        /// This converts a Tridinet Gameobect to
        /// a unity game object
        /// </summary>
        /// <param name="tgameObject"> This is the tridinet Gameobject</param>
        /// <param name="onAdd"> Add Callback </param>
        public void Replicate(TGameObject tgameObject, UnityAction<INode, string, CompiledCode> onAdd)
        {
            var node = FetchCahe(tgameObject.assetId);
            if (node == null)
            {
                var tobject = new GameObject(tgameObject.name);
                node = tobject.AddComponent<Trinode>();
                if (!cache.ContainsKey(tgameObject.assetId))
                {
                    node.Init(tgameObject, onAdd).OnCache(Cache);
                }
                else
                {
                    node.Init(tgameObject, onAdd);
                }
            }
            else
            {
                onAdd.Invoke(node, tgameObject.assetId, tgameObject.OnLoad);
            }
        }


        /// <summary>
        /// This converts a Tridinet Gameobect to
        /// a unity game object
        /// </summary>
        /// <param name="level">The index of the world sector</param>
        /// <param name="data"> The node data of the tridinet gameObject </param>
        /// <param name="OnBuild"> OnBuild callback for WorldBuilder </param>
        public void Replicate(int level, NodeData data, UnityAction<INode, NodeData, int, CompiledCode> OnBuild)
        {
            var node = FetchCahe(data.assetId);
            if (node == null)
            {
                var tobject = new GameObject(data.assetId);
                node = tobject.AddComponent<Trinode>();
                if (!cache.ContainsKey(data.assetId))
                {
                    node.Init(data, level, OnBuild).OnCache(Cache);
                }
                else
                {
                    node.Init(data, level, OnBuild);
                }
            }
            else
            {
                OnBuild.Invoke(node, data, level, (node as INode).script);
            }
        }

        /// <summary>
        /// Checks if path points to a tridinet file
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool isTridineScript(string item) {
            var parsePath = item.Split('/');
            var filename = parsePath[parsePath.Length - 1];
            Debug.Log(filename);
            return filename.Split('.')[1] == "tr";
        }

        /// <summary>
        /// This replicates a gameobject from path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inode"></param>
        public void Replicate(string path, out INode inode)
        {
            var tgameObject = FetchFromPath(path);
            if (tgameObject != null)
            {
                var node = FetchCahe(tgameObject.assetId);
                if (node != null)
                {
                    inode = node;
                    return;
                }
                if (node == null)
                {
                    var tobject = new GameObject(tgameObject.name);
                    node = tobject.AddComponent<Trinode>();
                    node.itemId = tgameObject.assetId;
                    if (!cache.ContainsKey(tgameObject.assetId))
                    {
                        node.Init(tgameObject).OnCache(Cache);
                    }
                    else
                    {
                        node.Init(tgameObject);
                    }
                    inode = node;
                    return;
                }
            }
            inode = null;
        }

        /// <summary>
        /// deletes world from file
        /// </summary>
        /// <param name="path"></param>
        public void deleteWorld(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Comverts a tridinet GameObject
        /// </summary>
        /// <param name="path"></param>
        public void Replicate(string path)
        {
            if (isTridineScript(path))
            {
                var script = File.ReadAllText(path);
                var compiled = CompiledCode.Compile(script);
                RunCompiledCode.Run(compiled, null, "");
                return;
            }
            Debug.Log(path);
            var tgameObject = FetchFromPath(path);
            if (tgameObject != null)
            {
                var tobject = new GameObject(tgameObject.name);
                var node = tobject.AddComponent<Trinode>();
                node.itemId = tgameObject.assetId;
                if (!cache.ContainsKey(tgameObject.assetId))
                {
                    node.Init(tgameObject).OnCache(Cache);
                }
                else
                {
                    node.Init(tgameObject);
                }
            }
        }

        /// <summary>
        /// This retrieves a spawned tridinet gameObject
        /// reference from the cache
        /// </summary>
        /// <param name="itemId"> The unique id of the tridinet file</param>
        /// <returns></returns>
        public Trinode FetchCahe(string itemId)
        {
            if (cache.ContainsKey(itemId))
            {
                var inode = Instantiate(cache[itemId]().gameObject).GetComponent<Trinode>();
                Debug.Log("fetching from cache");
                return inode;
            }
            return null;
        }

        /// <summary>
        /// This function adds a tridinet file reference func
        /// to the cache
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="Itemcache"></param>
        public void Cache(string assetId, Func<INode> Itemcache)
        {
            if (!cache.ContainsKey(assetId))
            {
                cache.Add(assetId, Itemcache);
            }
        }

        /// <summary>
        /// This fetches tridinet Gameobject file from path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public TGameObject FetchFromPath(string path)
        {
            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<TGameObject>(file);
            }
            return null;
        }

        /// <summary>
        /// This clears the cache
        /// </summary>
        internal void clearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// This loads a tridinet GameObject file from
        /// the unique id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TGameObject FetchFromId(string id)
        {
            var path = $"{TManifest.RepositoryPath}/{id}.tridinet";
            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<TGameObject>(file);
            }
            return null;
        }


        /// <summary>
        /// This function saves the world file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="worldId"></param>
        public void SaveWorld(string data, string worldId)
        {
            var path = $"{TManifest.WorldsPath}/{worldId}.world";
            var payload = data != null ? data : "{}";
            File.WriteAllText(path, payload);
            //make api request to update
        }

        /// <summary>
        /// This saves the world from a callback
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public void SaveWorldResponse(string data, string path)
        {
            var payload = data != null ? data : "{}";
            File.WriteAllText(path, payload);
            //make api request to update
        }

        /// <summary>
        /// this loads the world from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public World fetchWorld(string path)
        {
            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                var world = JsonConvert.DeserializeObject<World>(file);
                return world;
            }
            //make api request
            return null;
        }


        /// <summary>
        /// This loads the world from unique id
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public World getWorld(string worldId) {
            var path = $"./trw/{worldId}.world";
            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                Debug.Log(file);
                var world = JsonConvert.DeserializeObject<World>(file);
                return world;
            }
            Debug.Log("cannot find it");
            //make api request
            return null;
        }

        /// <summary>
        /// This is for UI
        /// </summary>
        /// <param name="item"></param>
        internal void probeItem(TRepository item)
        {
            var path = $"./tri/{item.id}.tridinet";
            if (!localItems.Contains(path))
            {
                ApiClient.main.getItemBuild(item);
            }
        }

        /// <summary>
        /// This fetches a tridinet gameobject file from
        /// name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TGameObject FetchFromProjectFile(string name) {
            var path = $"{TManifest.RepositoryPath}/{name}.tridinet";
            Debug.Log(path);
            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<TGameObject>(file);
            }
            return null;
        }

        /// <summary>
        /// Spawn game object from a manifest file record
        /// </summary>
        /// <param name="record"></param>
        /// <param name="OnRecord"></param>
        public void Replicate(TObjectKeyPair record, UnityAction<TObjectKeyPair> OnRecord)
        {
            var tobject = FetchFromProjectFile(record.assetId);
            if (tobject == null)
            {
                ApiClient.main.getItem(record.Url, record, OnRecord);
            }
            else
            {
                BuildRecord(tobject);
                OnRecord.Invoke(record);
            }
        }

        /// <summary>
        /// This builds an GameObject from a tridinet GameObject file
        /// </summary>
        /// <param name="parserfile"></param>
        /// <param name="noSave"></param>
        /// <returns></returns>
        public INode BuildRecord(TGameObject parserfile, bool noSave = false)
        {
            var tobject = new GameObject(parserfile.name);
            var node = tobject.AddComponent<Trinode>();
            node.itemId = parserfile.assetId;
           if(!noSave) File.WriteAllText($"{TManifest.RepositoryPath}/{parserfile.assetId}", JsonConvert.SerializeObject(parserfile));
            if (!cache.ContainsKey(parserfile.assetId))
            {
                node.Init(parserfile).OnCache(Cache);
            }
            else
            {
                node.Init(parserfile);
            }
            return node;
        }
    }
}
