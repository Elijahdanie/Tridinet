using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class resolves the local manifest for this project
/// and uploads files to the repository endpoint indicated by the user
/// </summary>
public class ManifestResolverConfig : MonoBehaviour
{
    /// <summary>
    /// Live server host address
    /// </summary>
    public string server;

    /// <summary>
    ///Live server host port
    /// </summary>
    public string port;

    /// <summary>
    /// Repository Endpoint
    /// </summary>
    public string repositoryEndpoint;


    /// <summary>
    /// Static instance of resolver config
    /// </summary>
    public static ManifestResolverConfig main;


    public virtual string OnUpload(string response)
    {
        Debug.Log(response);
        return response;
    }
}

#if UNITY_EDITOR

/// <summary>
/// This class Reolves, uploads and publishes both repository and world files
/// </summary>
public class ManifestResolver {

    public ManifestResolver() {
        manifestConfig = GameObject.FindObjectOfType<ManifestResolverConfig>();
    }
    public ManifestResolverConfig manifestConfig;
    public TManifest manifest;
    public string RepositoryPath = "./Assets/Tridinet/Repository";
    public string[] repoFiles;
    public UnityAction<string, bool> Completed;
    public World w;
    public int index;
    public string ptitile;

    /// <summary>
    /// Entry point for resolution
    /// </summary>
    /// <param name="Completed"></param>
    /// <param name="w"></param>
    /// <param name="ptitle"></param>
    public void Resolve(UnityAction<string, bool> Completed, World w = null, string ptitle = null) {
        this.ptitile = ptitile;
        this.Completed = Completed;
        NodeBank.main = GameObject.FindObjectOfType<NodeBank>();
        index = 0;
        this.w = w;
        var files = Directory.GetFiles(RepositoryPath).ToList();
        var parsedFiles = new List<string>();
        repoFiles = files.Select(x => x).Where(x => {
            var parsedinfo = x.Split('/');
            var nextparser = parsedinfo[parsedinfo.Length - 1].Split('.');
            var istridinet = nextparser[nextparser.Length - 1] == "tridinet";
            return istridinet;
        }
        ).ToArray();
        Debug.Log(repoFiles.Length);
        manifest = new TManifest();
        manifest.LoadLocalItem();
        Process();
    }


    /// <summary>
    /// This resolves and assign urls to tridinet object in the world
    /// </summary>
    /// <returns></returns>
    public string ProcessWorld() {
        Debug.Log("processing world");
        int count = 0;
        foreach (var sector in w.worldSectors)
        {
            foreach (var item in sector.children)
            {
                EditorUtility.DisplayProgressBar($"{ptitile}", "Preparing your world", count / sector.children.Count);
                var uri = manifest.getUri(item.assetId);
                    item.url = uri;
                Debug.Log(uri);
            }
        }
        var file = JsonConvert.SerializeObject(w);
        Debug.Log(file);
        return file;
    }

    /// <summary>
    /// This runs the resolution process recursively
    /// </summary>
    public void Process()
    {
        if (index == repoFiles.Length)
        {
            if (this.w != null)
            {
                var finaldata = ProcessWorld();
                this.Completed.Invoke(finaldata, this.w.id != null && this.w.id !="");
            }
            else
            {
                this.Completed.Invoke("", false);
            }
            return;
        }
        EditorUtility.DisplayProgressBar($"{ptitile}", "Please wait while tridinet resolves your manifest", index/repoFiles.Length);
        Debug.Log(repoFiles[index]);
        var item = NodeBank.main.FetchFromPath(repoFiles[index]);
        Debug.Log(manifest);
        if (!manifest.ContainsUri(item.assetId))
        {
            var data = File.ReadAllBytes(repoFiles[index]);
            ApiClient.main.FetchUri(manifestConfig.repositoryEndpoint, data, item.assetId, OnResponse);
        }
        else
        {
            index++;
            Process();
        }
    }


    /// <summary>
    /// Callback for resolution of a manifest
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="response"></param>
    private void OnResponse(string itemId, string response)
    {
        var uri = manifestConfig.OnUpload(response);
        manifest.RecordItem(itemId, uri);
        index++;
        Process();
    }
}
#endif