using Tridinet.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Assets.Scripts;
using System.Linq;

namespace Tridinet.Systems
{
    public class WorldBuilder : MonoBehaviour
    {
        //public static Dictionary<string, Func<INode>> prefabObjects = new Dictionary<string, Func<INode>>();
        public World world;
        public WorldSector currentWorldSector;

        public static WorldBuilder main;

        private void Awake()
        {
            main = this;
        }

        internal void GetUri(string uri)
        {
            ApiClient.main.getWorld(uri);
        }

        public void Init(World w = null)
        {
            clean();
            this.world = w;
            if (this.world.script != null)
            {
                RunCompiledCode.Run(this.world.script, null, "");
            }
            world?.worldSectors.ForEach(sector =>
            {
                for (int i = 0; i < sector.children.Count; i++)
                {
                    Build(sector.children[i], i);
                }
            });
        }

        public void InitViaBrower(World w = null)
        {
            clean();
            var user = FindObjectOfType<UserController>();
            user.transform.position = w.startPosition != null ?  w.startPosition.get() : Vector3.zero;
            StartCoroutine(startInit(w));
        }

        public IEnumerator startInit(World w = null)
        {
            this.world = w;
            if (this.world.script != null)
            {
                RunCompiledCode.Run(this.world.script, null, "");
            }
            if (this.world != null)
            {
                foreach (var sector in world.worldSectors)
                {
                    for (int i = 0; i < sector.children.Count; i++)
                    {
                        Build(sector.children[i], i);
                        yield return null;
                    }
                }
            }
            EventManager.main.OnWorldLoaded.Invoke();
        }

        public void clear()
        {
            var objects = FindObjectsOfType<Trinode>();
            foreach (var item in objects)
            {
                DestroyImmediate(item.gameObject);
            }
            world = null;
            currentWorldSector = null;
        }

        public void SelectSector(int id)
        {
            if (world != null && id < world.worldSectors.Count)
            {
                this.currentWorldSector = world.worldSectors[id];
                //iterate through all prefabs and set as children
            }
            else
            {
                Debug.LogError("world does not exist");
            }
        }

        public string createWorld(string name, Vector3 startPosition)
        {
            var world = new World();
            world.startPosition = new Evector3().Save(startPosition);
            world.name = name;
            var f1 = JsonConvert.SerializeObject(world);
            return f1;
        }

        public World probe()
        {
            world = null;
            this.currentWorldSector = null;
            var objects = FindObjectsOfType<Trinode>();
            currentWorldSector = createSector();
            foreach (var prefab in objects)
            {
                //var uid = prefabObjects.Count.ToString();
                //prefabObjects.Add(uid, () => prefab);
                prefab.data = new NodeData("", prefab, prefab.itemId, currentWorldSector.Id);
                this.currentWorldSector.children.Add(prefab.data);
            }
            return world;
        }

        public void Build(NodeData data, int level)
        {
            Debug.Log(data.transform);
            NodeBank.main.Replicate(level, data, OnBuild);
        }

        public void OnBuild(INode prefab, NodeData data, int level, CompiledCode script)
        {
            prefab.transform.localScale = data.transform.Scale.get();
            prefab.data = data;
            data.uid = level.ToString();
            Debug.Log("Done Building data");
            if (script != null)
            {
                RunCompiledCode.Run(script, prefab, "");
            }
            //prefabObjects.Add(prefab.data.uid, () => prefab);
        }

        public string getUri(string id)
        {
            if (world != null && world.worldSectors != null && world.worldSectors.Count > 0)
            {
                var nodedata = world.worldSectors[0].children.Find(x => {
                    return x.assetId == id;
                    });
                if (nodedata.url == null)
                {
                    return "";
                }
                return nodedata.url;
            }
            return "";
        }

        public WorldSector createSector()
        {
            if (world == null)
            {
                world = new World();
            }
            WorldSector sec = new WorldSector(world.worldSectors.Count, ref world);
            return sec;
        }

        //public void Add(NodeObject nodeObject)
        //{
        //    if (currentWorldSector == null)
        //    {
        //        currentWorldSector = createSector();
        //    }
        //    var prefab = bank.duplicate(nodeObject, currentWorldSector.averagePosition.get(), Quaternion.identity);
        //    var uid = prefabObjects.Count.ToString();
        //    prefabObjects.Add(uid, () => prefab);
        //    prefab.data = new NodeData(uid, prefab, nodeObject.assetId, currentWorldSector.Id);
        //    this.currentWorldSector.children.Add(prefab.data);
        //    Save();
        //}

        public void Add(TGameObject item)
        {
            if (currentWorldSector == null)
            {
                currentWorldSector = createSector();
            }
            NodeBank.main.Replicate(item, OnAdd); // bank.duplicate(nodeObject, currentWorldSector.averagePosition.get(), Quaternion.identity);
        }

        public void OnAdd(INode prefab, string id, CompiledCode script) {
            //var uid = prefabObjects.Count.ToString();
            //prefabObjects.Add(uid, () => prefab);
            prefab.data = new NodeData("", prefab, id, currentWorldSector.Id);
            this.currentWorldSector.children.Add(prefab.data);
            if (script != null)
            {
                RunCompiledCode.Run(script, prefab, "");
            }
            Save();
        }

        public void clean()
        {
            var objects = FindObjectsOfType<Trinode>();
            foreach (var item in objects)
            {
                DestroyImmediate(item.gameObject);
            }
            NodeBank.main.clearCache();
            world = null;
            currentWorldSector = null;
        }

        internal void CreateFromManifest(TManifest arg0)
        {
            clear();
            world = new World();
            world.worldSectors = new List<WorldSector>();
            world.worldSectors.Add(new WorldSector(0, ref world));
            for (int i = 0; i < arg0.tridiObjects.Count; i++)
            {
                world.worldSectors[0].children.Add(new NodeData() { 
                    url = arg0.tridiObjects.ElementAt(i).Value.Url,
                    assetId = arg0.tridiObjects.ElementAt(i).Key
                });
            }
            Debug.Log($"created world {JsonConvert.SerializeObject(world)}");
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public void Save()
        {
            if (this.world == null) return;
            WorldBrowser.browserCache.CacheWorld(this.world, "current");
        }

        public void DeleteNode(INode node)
        {
            //prefabObjects.Remove(node.data.uid);
            
            node.DeleteNode();
        }
    }
}