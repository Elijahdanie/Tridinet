using Assets.Scripts;
using Tridinet.Systems;
using Tridinet.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class encapsulate loading and spawning of tridinet files
/// </summary>
public class Trinode : BaseBuiltPrefab
{
    public string itemId;
    UnityAction<INode, string, CompiledCode> OnAdd;
    public int level;
    UnityAction<INode, NodeData, int, CompiledCode> Build;
    UnityAction<string, Func<INode>> cacheCallback;
    /// <summary>
    /// This builds from the world data
    /// </summary>
    /// <param name="data"></param>
    public Trinode Init(NodeData data, int level, UnityAction<INode, NodeData, int, CompiledCode> Build)
    {
        this.level = level;
        this.itemId = data.assetId;
        this.Build = Build;
        var node = NodeBank.main.FetchFromId(data.assetId);
        if (node == null)
        {
            data.url = WorldBuilder.main.getUri(data.assetId);
            if (data.url == "")
            {
                Debug.LogError($"Object {data.assetId} cannot be fetched");
                this.Build(null, data, level, null);
                DestroyImmediate(gameObject);
            }
            ApiClient.main.getTrinode(data.assetId, data, OnBuild);
        }
        else
        {
            OnBuild(node, data);
        }
        return this;
    }

    public void OnCache(UnityAction<string, Func<INode>> cacheCallback)
    {
        this.cacheCallback = cacheCallback;
    }

    /// <summary>
    /// This buids the GameObject
    /// </summary>
    /// <param name="Tnode"></param>
    /// <param name="data"></param>
    private void OnBuild(TGameObject Tnode, NodeData data)
    {
        var node = Replicator.BuildTriNode(gameObject, Tnode, data);
        node.data = data != null ? data : new NodeData("", node, Tnode.assetId, 0);
        node.script = Tnode.OnLoad;
        if (OnAdd != null)
            OnAdd.Invoke(node, itemId, Tnode.OnLoad);
        if (Build != null)
            Build(node, data, level, Tnode.OnLoad);
        if (cacheCallback != null)
        {
            cacheCallback.Invoke(this.itemId, Fetch);
        }
    }

    private INode Fetch() {
        return gameObject.GetComponent<INode>();
    }

    /// <summary>
    /// This builds from item selection
    /// </summary>
    /// <param name="item"></param>
    public Trinode Init(TGameObject item, UnityAction<INode, string, CompiledCode> onAdd)
    {
        this.itemId = item.assetId;
        this.OnAdd = onAdd;
        OnBuild(item, null);
        return this;
    }

    internal Trinode Init(TGameObject tgameObject)
    {
        OnBuild(tgameObject, null);
        return this;
    }
}
