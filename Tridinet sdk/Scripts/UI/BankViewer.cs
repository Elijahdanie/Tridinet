using Tridinet.Systems;
using Tridinet.UI;
using Tridinet.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BankViewer : MonoBehaviour
{
    public Transform viewerParent;
    public Button Open;
    public GameObject viewPanel;
    public TridnodeDisplay prefab;
    public Dictionary<string, TridnodeDisplay> displays = new Dictionary<string, TridnodeDisplay>();
    private void Start()
    {
        Open.onClick.AddListener(()=> {
            viewPanel.SetActive(true);
        });
        ApiClient.main.getRepositories(0, OnResponse);
    }

    private void OnResponse(string arg0)
    {
        Debug.Log(arg0);
        var tempres = JsonUtility.FromJson<ItemBuilds>(arg0);
        foreach (var Item in tempres.data)
        {
            Debug.Log("build");
            Display(Item);
            EventManager.main.OnGetItem.Invoke(Item);
        }
    }

    public void LoadLocal() { 
        
    }

    //public void Display(NodeObject node)
    //{
    //    var display = Instantiate(prefab, viewerParent);
    //    display.Display(node.name, () => EventManager.main.OnSelectPrefab.Invoke(node));
    //    if (node.previewUri != "")
    //    {
    //        apiClient.FetchTexture(node.assetId, display.SetPreview);
    //    }
    //}

    public void Display(TRepository node)
    {
        var display = Instantiate(prefab, viewerParent);
        display.Init(node, () => EventManager.main.OnSelectPrefab.Invoke(node));
    }
}
