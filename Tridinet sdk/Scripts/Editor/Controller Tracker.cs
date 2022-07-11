using Assets.Scripts;
using Tridinet.Systems;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System;
using System.IO;
using Tridinet.WorldEditor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TridinetEditor : EditorWindow
{

    string userName = "";
    string userPassword = "";
    string userEmail = "";
    private Vector2 scroll;

    static Extractor original;
    static SerializedObject itemObject;
    static WorldBuilder worldBuilder;
    static NodeBank nodeBank;
    static ItemBuilds items = new ItemBuilds();
    static ApiClient apiClient;
    static ManifestResolverConfig manifestResolver;
    static TManifest resolver = new TManifest();
    DragAndDrop dragdrop = new DragAndDrop();

    string Worldname, Worlddescription, Worlddata, WorldprivateKey = "";
    WorldType worldType;
    WorldAccess worldAccess;

    [MenuItem("Tridinet/Editor")]
    public static void OnOpenTracker() {
        TridinetEditor window = (TridinetEditor)GetWindow(typeof(TridinetEditor));
        window.minSize = new Vector2(10f, 50f);
        window.maxSize = new Vector2(400, 300f);
        window.name = "Tridinet Editor";
        window.Show();
    }

    [MenuItem("Assets/Tridinet/Create", isValidateFunction:true)]
    public static bool validateCreateItem() {
        var currentObject = Selection.activeGameObject;
        if (currentObject == null) return false;
        if (currentObject.GetComponent<Trinode>())
        {
            return false;
        }
        if (currentObject.GetComponent<MeshFilter>())
        {
            return true;
        }
        if (currentObject.GetComponentInChildren<MeshFilter>())
        {
            return true;
        }
        return false;
    }

    [MenuItem("Assets/Tridinet/Create")]
    public static void createItem()
    {
        CreateFromSelection();
    }

    private static void CreateItem(GameObject gameObject, bool addTrinode) {
        if (resolver == null)
        {
            resolver = new TManifest();
        }   
        TGameObject tobject = Extractor.createItemLocal(gameObject);
        if (addTrinode && !gameObject.GetComponent<Trinode>())
        {
            var trinode = gameObject.AddComponent<Trinode>();
            trinode.data = new NodeData("", trinode, tobject.assetId, 0);
            trinode.itemId = tobject.assetId;
        }
        var preview = AssetPreview.GetAssetPreview(gameObject);
        byte[] previewdata = preview.EncodeToPNG();
        if (previewdata != null)
        {
            string base64textture = Convert.ToBase64String(previewdata);
            tobject.assetPreview = base64textture;
        }
        if (!resolver.ValidateInstanceId(gameObject.GetInstanceID()))
        {
            Debug.LogWarning("Already created an item for this, Update instead");
            return;
        }
        resolver.RecordItem(tobject.assetId, tobject.instanceId, tobject.name);
        var data = JsonConvert.SerializeObject(tobject);
        Debug.Log(data);
        var finalPath = $"{TManifest.RepositoryPath}/{tobject.assetId}.tridinet";
        File.WriteAllText(finalPath, data);
        AssetDatabase.Refresh();
    }

    [MenuItem("Tridinet/Reload Manifest")]
    public static void RefreshManifest() {
        resolver = new TManifest();
        resolver.ReloadLocalRepo();
        Debug.Log("Reoloaded Manifest");
    }

    [MenuItem("GameObject/Tridinet/Create", true, -10)]
    public static bool ValidateCreateItemFromHeiracy() {
        var currentObject = Selection.activeGameObject;
        if (currentObject == null) return false;
        if (currentObject.GetComponent<Trinode>())
        {
            return false;
        }
        if (currentObject.GetComponent<MeshFilter>())
        {
            return true;
        }
        if (currentObject.GetComponentInChildren<MeshFilter>())
        {
            return true;
        }
        return false;
    }

    public static void CreateFromSelection(bool addTrinode=false) {
        TManifest.setDirectory();
        if (Selection.objects.Length > 0)
        {
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var gameObject = (GameObject)Selection.objects[i];
                if (gameObject)
                {
                    CreateItem(gameObject, addTrinode);
                }
            }
        }
    }

    [MenuItem("GameObject/Tridinet/Create", false, -10)]
    public static void CreateItemFromHeiracy()
    {
        CreateFromSelection(true);
    }

    [MenuItem("Tridinet/Publish Scene", false, -10)]
    public static void ProcessCurrentWorld() { 
        
    }

    [MenuItem("Assets/Tridinet/Run", true, -10)]
    public static bool RunFileValidate()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        var check = path.Split('.');
        if (check.Length > 0 && check[check.Length - 1] == "tr")
        {
            return true;
        }
        return false;
    }

    [MenuItem("Assets/Tridinet/Run", false, -10)]
    public static void RunFile()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        Debug.Log(path);
        var file = File.ReadAllText($"./{path}");
        Debug.Log(file);
        var compiled = CompiledCode.Compile(file);
        RunCompiledCode.Run(compiled, null, "");
    }

    [MenuItem("Tridinet/Create World")]
    public static void CreateWorld() {
        WorldEditor window = (WorldEditor)GetWindow(typeof(WorldEditor));
        window.minSize = new Vector2(10f, 50f);
        window.maxSize = new Vector2(400, 300f);
        window.name = "World Creator";
        window.Show();
    }

    [MenuItem("Tridinet/Create Repository")]
    public static void CreateRepository()
    {
        RepositoryEditor window = (RepositoryEditor)GetWindow(typeof(RepositoryEditor));
        window.minSize = new Vector2(10f, 50f);
        window.maxSize = new Vector2(400, 300f);
        window.name = "World Creator";
        window.Show();
    }

    public void DropAreaGUI()
    {
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, "Drop Tridinet Objects here", EditorStyles.objectField);
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    var data = DragAndDrop.paths;
                    foreach (var item in data)
                    {
                        NodeBank.main.Replicate(item);
                    }
                }
                break;
        }
        return;
    }

    private static void OnResponse(string arg0)
    {
        items = JsonConvert.DeserializeObject<ItemBuilds>(arg0);
    }
    Trinode currentNode;
    private int isController;

    public void OnGUI()
    {
        if (original == null)
        {
            original = Fetch<Extractor>();
            itemObject = new SerializedObject(original);
        }
        if (nodeBank == null)
        {
            nodeBank = Fetch<NodeBank>();
            NodeBank.main = nodeBank;
        }
        if (worldBuilder == null)
        {
            worldBuilder = Fetch<WorldBuilder>();
            worldBuilder.probe();
        }
        if (apiClient == null)
        {
            apiClient = Fetch<ApiClient>();
            ApiClient.main = apiClient;
        }
        if (manifestResolver == null)
        {
            manifestResolver = Fetch<ManifestResolverConfig>();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Repository", EditorStyles.toolbarButton))
        {
            isController = 0;
        }
        if (GUILayout.Button("Account", EditorStyles.toolbarButton))
        {
            isController = 1;
        }
        if (GUILayout.Button("Inspector", EditorStyles.toolbarButton))
        {
            isController = 2;
        }
        GUILayout.EndHorizontal();
        if (isController == 0)
        {
            Repository();
        }
        else if (isController == 1)
        {
            Accounts();
        }
        else if (isController == 2)
        {
            TridinetInspector();

        }
    }

    private T Fetch<T>() where T:MonoBehaviour
    {
        var instance = FindObjectOfType<T>();
        if (!instance)
        {
            var managers = GameObject.Find("TRIDINET");
            if (managers)
            {
                instance = managers.GetComponent<T>();
                if (instance) return instance;
                return managers.AddComponent<T>();
            }
            if (!managers)
            {
                var manager = new GameObject("TRIDINET");
                instance = manager.AddComponent<T>();
                return instance;
            }

        }
        return instance;
    }

    #region MARKET PLACE

    private int page;
    public bool isMax;
    public bool isLocal;
    private void Repository()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("TRIDINET REPOSITORY");
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        if (items.data.Count != 0)
        {
            GUILayout.Space(20);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true), GUILayout.Height(200));
            items.data.ForEach(x =>
            {
                DisplayItem(x);
            });
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                currentManifest = null;
                OpenRepository = false;
            }
        }
        else
        {
            if (GUILayout.Button("Fetch Remote Repository"))
            {
                items.data.Clear();
                ApiClient.main.getRepositories(page, OnResponse);
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.Space(50);
        if (items.data.Count != 0)
        {
            if (GUILayout.Button("Next"))
            {
                ApiClient.main.getRepositories(page++, OnResponse);
            }
            if (GUILayout.Button("Previous"))
            {
                ApiClient.main.getRepositories(page--, OnResponse);
            }
        }
        GUILayout.Space(50);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    public void DisplayRepository() { 
        
    }

    public bool OpenRepository = false;
    TManifest currentManifest;
    public void DisplayItem(TRepository item)
    {
        if (currentManifest == null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(item.name))
            {
                ApiClient.main.fetchManifest(item, OnFetchManifest);
                OpenRepository = true;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            if (OpenRepository)
            {
                EditorGUILayout.LabelField("Loading manifest.........");
            }
            foreach (var record in currentManifest.tridiObjects)
            {
                if (GUILayout.Button(record.Value.name))
                {
                    resolver.LoadLocalItem();
                    if (resolver.ValidateInstanceIdExternalManifest(record.Value.instanceId))
                        NodeBank.main.Replicate(record.Value, OnRecord);
                    else
                        Debug.LogWarning("Already created this item");
                }
            }
        }
    }

    private void OnRecord(TObjectKeyPair record)
    {
        resolver.RecordItem(record.assetId, record.instanceId, record.name);
        resolver.RecordItem(record.assetId, record.Url);
        AssetDatabase.Refresh();
    }

    private void OnFetchManifest(TManifest arg0)
    {
        OpenRepository = false;
        arg0 = TManifest.reRoute(arg0);
        var file = JsonConvert.SerializeObject(arg0);
        File.WriteAllText(TManifest.otherManifest + "/" + arg0.name + ".json", file);
        currentManifest = arg0;
        AssetDatabase.Refresh();
    }

    private void OnDeleteResponse(string arg0, bool arg1)
    {
        if (arg1)
        {
            var it = items.data.Find((x) => x.id == arg0);
            items.data.Remove(it);
        }
    }
    #endregion

    public void TridinetInspector() {
        DropAreaGUI();
        var objects = FindObjectsOfType<Trinode>();
        foreach (var item in objects)
        {
            if (GUILayout.Button(item.name))
            {
                Selection.activeGameObject = item.gameObject;
            }
        }
    }

    public void Accounts() {
        var check = PlayerPrefs.GetString("token");
        if (check != "")
        {
            if (GUILayout.Button("LogOut"))
            {
                PlayerPrefs.DeleteKey("token");
            }
        }
        if (check != "") return;
        if (check == "")
        {
            userName = EditorGUILayout.TextField("Name", userName);
        }
        userPassword = EditorGUILayout.TextField("Password", userPassword);
        userEmail = EditorGUILayout.TextField("Email", userEmail);

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Space(50);
        if (GUILayout.Button("Register"))
        {
            apiClient.Register(userName, userEmail, userPassword);
        }
        if (GUILayout.Button("Login"))
        {
            apiClient.SignIn(userEmail, userPassword);
        }
        GUILayout.Space(50);
        GUILayout.EndHorizontal();
    }
}