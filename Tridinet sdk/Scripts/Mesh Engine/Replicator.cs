using Assets.Scripts;
using Tridinet.Utilities.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class creates GameObjects from tridinet files
/// </summary>
public class Replicator : MonoBehaviour
{

    public static INode Build(TGameObject tgameObject, NodeData data = null) {
        var Tobject = new GameObject(tgameObject.name);
        tgameObject = SetParent(Tobject, tgameObject);
        Tobject.AddComponent<BaseBuiltPrefab>();
        Tobject.tag = "object";
        for (int i = 0; i < tgameObject.meshes.Count; i++)
        {
            var x = tgameObject.meshes[i];
            SetVoid(x, Tobject.transform);
        }
        Tobject.AddComponent<BoxCollider>();
        if (data != null)
        {
            Tobject.transform.position = data.transform.position.get();
            Tobject.transform.rotation = data.transform.Rotation.getRotation();
        }
        return Tobject.GetComponent<INode>();
    }

    public static INode BuildTriNode(GameObject Tobject, TGameObject tgameObject, NodeData data = null)
    {
        tgameObject = SetParent(Tobject, tgameObject);
        //tgameObject.transform.get(Tobject.transform);
        Tobject.tag = "object";
        for (int i = 0; i < tgameObject.meshes.Count; i++)
        {
            var x = tgameObject.meshes[i];
            SetVoid(x, Tobject.transform);
        }
        Tobject.AddComponent<BoxCollider>();
        if (data != null && data.transform != null)
        {
            Tobject.transform.position = data.transform != null ? data.transform.position.get() : Vector3.zero;
            Tobject.transform.rotation = data.transform.Rotation.getRotation();
            Tobject.transform.localScale = data.transform.Scale.get();
        }
        return Tobject.GetComponent<INode>();
    }

    //IEnumerator build(TGameObject tgameObject) {
    //    for (int i = 0; i < tgameObject.meshes.Count; i++)
    //    {
    //        var x = tgameObject.meshes[i];
    //        SetVoid(x, this.transform);
    //        yield return new WaitForSeconds(0.2f);
    //    }
    //    gameObject.AddComponent<BoxCollider>();
    //}

    public static TGameObject SetParent(GameObject Tobject, TGameObject tgameObject) {
        if (tgameObject.parentMesh != null)
        {
            var model = tgameObject.parentMesh;
            var filter = Tobject.AddComponent<MeshFilter>();
            var renderer = Tobject.AddComponent<MeshRenderer>();
            filter.sharedMesh = new Mesh();
            filter.sharedMesh.vertices = Evector3.getArray(model.indices);
            filter.sharedMesh.triangles = model.triangle;
            filter.sharedMesh.RecalculateNormals();
            filter.sharedMesh = getUvs(model.uvs, filter.sharedMesh);
            filter.sharedMesh = getSubMeshes(model.subMeshes, filter.sharedMesh);
            Tobject.transform.SetParent(Tobject.transform.parent);
            renderer.sharedMaterials = setMaterials(model.materials);
            model.transform.getLocal(Tobject.transform);
        }
        return tgameObject;
    }

    public static void SetVoid(TModel model, Transform parent) {
        var Tobject = new GameObject(model.name);
        var filter = Tobject.AddComponent<MeshFilter>();
        var renderer = Tobject.AddComponent<MeshRenderer>();
        filter.sharedMesh = new Mesh();
        filter.sharedMesh.vertices = Evector3.getArray(model.indices);
        filter.sharedMesh.triangles = model.triangle;
        filter.sharedMesh.RecalculateNormals();
        filter.sharedMesh = getUvs(model.uvs, filter.sharedMesh);
        filter.sharedMesh = getSubMeshes(model.subMeshes, filter.sharedMesh);
        Tobject.transform.SetParent(parent);
        renderer.sharedMaterials = setMaterials(model.materials);
        model.transform.getLocal(Tobject.transform);
    }

    private static Mesh getUvs(List<Evector2[]> uvs, Mesh mesh)
    {
        if (uvs != null && uvs.Count > 0)
        {
            mesh.uv = Evector2.getArray(uvs[0]);
            //mesh.uv2 = Evector2.getArray(uvs[1]);
            //mesh.uv3 = Evector2.getArray(uvs[2]);
            //mesh.uv4 = Evector2.getArray(uvs[3]);
        }
        return mesh;
    }

    public static Mesh getSubMeshes(List<int[]> submeshes, Mesh mesh)
    {
        mesh.subMeshCount = submeshes.Count;
        for (int i = 0; i < submeshes.Count; i++)
        {
            mesh.SetTriangles(submeshes[i], i);
        }
        return mesh;
    }

    private static Material[] setMaterials(TMaterial[] sharedMaterials)
    {
        List<Material> tempMAT = new List<Material>();
        foreach (var item in sharedMaterials)
        {
            Material mat = new Material(Shader.Find(item.ShaderName));
            mat.color = item.color.getColor();
            mat.name = item.name;
            tempMAT.Add(mat);
        }
        return tempMAT.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
