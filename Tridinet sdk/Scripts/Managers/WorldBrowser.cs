using Tridinet.Systems;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using Tridinet.UI;

namespace Tridinet.Systems
{
    public class WorldBrowser : MonoBehaviour
    {
        public string URI;
        public string BookMarkKey;
        public int RepositoryCacheSize;
        public int WorldCacheSize;

        public TMP_Text loadingtext;
        public GameObject loadingPanel;

        public List<HistoryRecord> BookMarks = new List<HistoryRecord>();
        public static BrowserCache browserCache;
        public ListView listView;
        internal static WorldBrowser main;
        public TMP_InputField inputField;

        private void Awake()
        {
            main = this;
        }

        private void Start()
        {
            browserCache = new BrowserCache(WorldCacheSize, RepositoryCacheSize);
            EventManager.main.OnWorldLoaded.AddListener(OnWorldLoaded);
        }

        private void OnWorldLoaded()
        {
            loadingPanel.SetActive(false);
        }

        public void SendURI(string uri)
        {
            this.URI = uri;
            var world = browserCache.getWorld(uri);
            loadingtext.text = $"loading {uri}...";
            loadingPanel.SetActive(true);
            if (world == null)
            {
                WorldBuilder.main.GetUri(uri);
            }
            else
            {
                WorldBuilder.main.Init(world);
            }
        }

        public void ShowWorlds() {
            ApiClient.main.getWorlds(0, onResponse);
        }

        public void ShowRepositories()
        {
            ApiClient.main.getRepositories(0, onResponseRepositries);
        }

        private void onResponseRepositries(string arg0)
        {
            listView.OnResponse(arg0);
        }

        private void onResponse(string arg0)
        {
            listView.OnResponseWorld(arg0);
        }

        public void ReloadPage() {
            WorldBuilder.main.GetUri(this.URI);
        }

        internal void SetURI(string url)
        {
            inputField.text = url;
            SendURI(url);
            listView.transform.parent.gameObject.SetActive(false);
        }
    }
}

//create cache
public class BrowserCache {
    public int worldMax;
    public int RepositoryMax;
    public string cacheWorldPath = "./trw";
    public string cacheTriPath = "./tri";
    public List<string> RepositoryFiles;
    public List<string> WorldFiles;
    public BrowserCache(int worldMax, int repositoryMax) {
        if (!Directory.Exists(cacheTriPath))
        {
            Directory.CreateDirectory(cacheTriPath);
        }
        if (!Directory.Exists(cacheWorldPath))
        {
            Directory.CreateDirectory(cacheWorldPath);
        }
        this.worldMax = worldMax;
        this.RepositoryMax = repositoryMax;
        this.RepositoryFiles = Directory.GetFiles(cacheTriPath).ToList();
        this.WorldFiles = Directory.GetFiles(cacheWorldPath).ToList();
    }
    public void CacheTObject(TGameObject item) {
        var data = JsonConvert.SerializeObject(item);
        File.WriteAllText($"{cacheTriPath}/{item.assetId}", data);
    }

    public World getWorld(string uri) {
        if (WorldFiles.Contains(uri))
        {
            var data = File.ReadAllText($"{cacheWorldPath}/{uri}");
            var world = JsonConvert.DeserializeObject<World>(data);
            return world;
        }
        return null;
    }


    public TGameObject getItem(string itemId)
    {
        if (WorldFiles.Contains(itemId))
        {
            var data = File.ReadAllText($"{cacheTriPath}/{itemId}");
            var tobject = JsonConvert.DeserializeObject<TGameObject>(data);
            return tobject;
        }
        return null;
    }

    public void CacheWorld(World world, string link) {
        var data = JsonConvert.SerializeObject(world);
        var path = $"{cacheTriPath}/{world.name}";
        File.WriteAllText(path, data);
    }
}


public class HistoryRecord {
    public string name;
    public string uri;
}
