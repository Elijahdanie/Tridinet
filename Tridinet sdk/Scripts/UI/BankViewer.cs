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
