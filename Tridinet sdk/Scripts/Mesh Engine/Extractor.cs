using Tridinet.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class extracts 3d model data from GameObjects
/// </summary>
public class Extractor : MonoBehaviour
{
    public TRepository item;

    public static TGameObject FetchParentMesh(GameObject parent, TGameObject TObect) {
        var filter = parent.GetComponent<MeshFilter>();
        var renderer = parent.GetComponent<Renderer>();
        if (filter && renderer)
        {
            TObect.parentMesh = GrabChild(filter, renderer, true);
        }
        return TObect;
    }


    public static TGameObject createItemLocal(GameObject model)
    {
        var target = new TGameObject();
        target.assetId = Guid.NewGuid().ToString();
        target.instanceId = model.GetInstanceID().ToString();
        target.transform = new Etransform().Save(model.transform);
        target = FetchParentMesh(model, target);
        target.name = model.name;
        for (int i = 0; i < model.transform.childCount; i++)
        {
            var filter = model.transform.GetChild(i).GetComponent<MeshFilter>();
            var renderer = model.transform.GetChild(i).GetComponent<Renderer>();
            if (filter && renderer)
            {
                target.meshes.Add(GrabChild(filter, renderer));
            }
        }
        return target;
    }

    public static TModel GrabChild(MeshFilter filter, Renderer renderer, bool isParent = false) {
        Mesh mesh = filter.sharedMesh;
        var triangles = mesh.triangles;
        var model = new TModel()
        {
            triangle = triangles,
            indices = Evector3.getArray(mesh.vertices),
            name = filter.name,
            ShaderName = renderer.sharedMaterial.shader.name,
            materials = getMaterials(renderer.sharedMaterials),
            transform = !isParent ? new Etransform().SaveLocal(filter.transform) : new Etransform().Save(filter.transform),
            uvs = getUvs(mesh),
            subMeshes = getSubMeshes(mesh)
        };
        return model;
    }

    private static List<Evector2[]> getUvs(Mesh mesh)
    {
        List<Evector2[]> uvs = new List<Evector2[]>();
        uvs.Add(Evector2.getArray(mesh.uv));
        //uvs.Add(Evector2.getArray(mesh.uv2));
        //uvs.Add(Evector2.getArray(mesh.uv3));
        //uvs.Add(Evector2.getArray(mesh.uv4));
        return uvs;
    }


    public static List<int[]> getSubMeshes(Mesh mesh) {
        List<int[]> submeshes = new List<int[]>();
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            var temp = mesh.GetTriangles(i);
            submeshes.Add(temp);
        }
        return submeshes;
    }

    private static TMaterial[] getMaterials(Material[] sharedMaterials)
    {
        List<TMaterial> tempMAT = new List<TMaterial>();
        foreach (var item in sharedMaterials)
        {
            TMaterial mat = new TMaterial();
            mat.color = new Evector4(item.color);
            mat.name = item.name;
            mat.ShaderName = item.shader.name;
            //smat.texture.mainTexture = ((Texture2D)item.mainTexture).EncodeToPNG();
            tempMAT.Add(mat);
        }
        return tempMAT.ToArray();
    }
}
