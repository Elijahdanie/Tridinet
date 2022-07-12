using Assets.Scripts;
using Tridinet.Systems;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tridinet.WorldEditor
{

    [CustomEditor(typeof(WorldDataContainer))]
    public class WorldContainerEditor : Editor {
        WorldDataContainer wc;
        private Vector2 scroll;
        public bool update = true;

        public string getDefault()
        {
            var temp = $"Spawn: {{ target =ref[manifestid, baseid, customscriptkey]; \n\t num = 1; \n\toffset = 1; \n\tpostion = (0,0,0); \n\tscale = (0,0,0); \n\trotation = (0,0,0); \n\tdirection = left;\n}}:\n";
            var data = $"Data: {{ YourdataKey=\"Your data value\"; }}:\n|";
            var customspawn = $"YourCustomSpawn: {{\n  target = placeholderid; \n\t num = 1; \n\toffset = 1; \n\tpostion = (0,0,0); \n\tscale = (0,0,0); \n\trotation = (0,0,0); \n\tdirection = left;\n}}:\n";
            return temp + data + customspawn;
        }
        private void OnEnable()
        {
            wc = (WorldDataContainer)target;
            WorldBuilder.main = FindObjectOfType<WorldBuilder>();
            NodeBank.main = FindObjectOfType<NodeBank>();
            ApiClient.main = FindObjectOfType<ApiClient>();
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(wc.name);
            if (wc.url != "")
            {
                EditorGUILayout.LabelField("Url", wc.url);
            }
            wc.description = EditorGUILayout.TextField("Description", wc.description);
            wc.access = (WorldAccess)EditorGUILayout.EnumPopup("Access", wc.access);
            if (wc.access == WorldAccess.@private)
            {
                wc.privateKey = EditorGUILayout.TextField("privateKey", wc.privateKey);
            }
            wc.type = (WorldType)EditorGUILayout.EnumPopup("Access", wc.type);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            wc.script = EditorGUILayout.TextArea(wc.script, GUI.skin.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Generate Default"))
            {
                wc.script = getDefault();
            }
                GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load"))
            {
                var file = File.ReadAllText($"{TManifest.WorldsPath}/{wc.name}.world");
                var data = JsonConvert.DeserializeObject<World>(file);
                WorldBuilder.main.Init(data);
            }
            if (GUILayout.Button("Publish"))
            {
                var w = NodeBank.main.fetchWorld(wc.path);
                ManifestResolver resolver = new ManifestResolver();
                EditorUtility.DisplayProgressBar($"Publishing {wc.name}", "Please wait while tridinet publishes your world", 0);
                resolver.Resolve(OnDone, w, "Publishing your world");
            }
            if (GUILayout.Button("Save Scene"))
            {
                var w = WorldBuilder.main.probe();
                NodeBank.main.SaveWorld(JsonConvert.SerializeObject(w), wc.name);
            }
            if (GUILayout.Button("Save Script"))
            {
                var w = NodeBank.main.fetchWorld(wc.path);
                if (wc.script != null && wc.script != "")
                {
                    w.script = CompiledCode.Compile(wc.script);
                }
                NodeBank.main.SaveWorld(JsonConvert.SerializeObject(w), wc.name);
            }
            if (GUILayout.Button("Bind"))
            {
                var w = WorldBuilder.main.probe();
                ManifestResolver resolver = new ManifestResolver();
                resolver.manifest = new TManifest();
                resolver.manifest.LoadLocalItem();
                resolver.w = w;
                NodeBank.main.SaveWorld(resolver.ProcessWorld(), wc.name);
            }
            if (GUILayout.Button("Delete"))
            {
                ApiClient.main.DeleteWorld(wc.id, OnResponse);
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Pull"))
            {
                ApiClient.main.DeleteItem(wc.id, OnResponse);
            }
            EditorGUILayout.HelpBox("Be carefull, this may override your progress", MessageType.Warning);
        }

        private void OnResponse(string arg0, bool arg1)
        {
            NodeBank.main.deleteWorld(wc.path);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(wc.GetInstanceID()));
            AssetDatabase.Refresh();
        }

        private void OnDone(string data, bool update)
        {
            NodeBank.main.SaveWorld(data, wc.name);
            Debug.Log(wc.id);
            if (!update)
            {
                ApiClient.main.CreateWorld(new WorldPayload()
                {
                    name = wc.name,
                    description = wc.description,
                    data = data,
                    privateKey = wc.privateKey,
                    access = wc.access.ToString(),
                    type = wc.type.ToString()
                }, OnCreated);
            }
            else
            {
                ApiClient.main.UpdateWorld(new WorldPayload()
                {
                    id = wc.id,
                    name = wc.name,
                    description = wc.description,
                    data = data,
                    privateKey = wc.privateKey,
                    access = wc.access.ToString(),
                    type = wc.type.ToString()
                }, OnCreated);
            }
            EditorUtility.ClearProgressBar();
        }

        private void OnCreated(WorldPayload arg0)
        {
            Debug.Log($"{arg0.id}");
            wc.id = arg0.id;
            wc.url = arg0.url;
            var w = NodeBank.main.fetchWorld(wc.path);
            w.id = arg0.id;
            NodeBank.main.SaveWorld(JsonConvert.SerializeObject(w), wc.name);
            EditorUtility.ClearProgressBar();
        }
    }

    public class RepositoryEditor : EditorWindow {
        public TRepository repository;
        ManifestResolver resolver;

        private void OnEnable()
        {
            repository = new TRepository();
            resolver = new ManifestResolver();
        }

        private void OnGUI()
        {
            repository.name = EditorGUILayout.TextField("Name", repository.name);
            repository.description = EditorGUILayout.TextField("description", repository.description);
            repository.category = EditorGUILayout.TextField("Category", repository.category);
            repository.cost = EditorGUILayout.FloatField("Cost", repository.cost);
            if (GUILayout.Button("Create Repository"))
            {
                EditorUtility.DisplayProgressBar($"Creating repository", "Please wait while tridinet publishes your repository", 0);
                resolver.Resolve(OnDone, null, "Creating Repository");
            }
        }

        private void OnDone(string arg0, bool isupdate)
        {
            ApiClient.main.createItem(repository, OnCreateRepo);
            Close();
            EditorUtility.ClearProgressBar();
        }

        public void OnCreateRepo(TRepository data)
        {
            
        }
    }

    public class ScriptEditor : EditorWindow
    {
        TGameObject tgameObject;
        string script;
        private TManifest currentManifest;

        [MenuItem("Tridinet/Script Editor")]
        public static void OpenEditor()
        {
            ScriptEditor window = ScriptEditor.CreateInstance<ScriptEditor>();
            window.minSize = new Vector2(10f, 50f);
            window.maxSize = new Vector2(500, 600);
            window.name = "Tridinet Editor";
            window.Show();
        }

        private void OnEnable()
        {
        }


        Vector2 scroll = new Vector2();
        private bool displayObject;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("TrScript Editor", EditorStyles.boldLabel, GUILayout.Height(50));
            if (WorldBuilder.main == null)
            {
                WorldBuilder.main = FindObjectOfType<WorldBuilder>();
            }
            if (currentManifest == null)
            {
                currentManifest = new TManifest();
                currentManifest.LoadLocalItem();
            }
            if (NodeBank.main == null)
            {
                NodeBank.main = FindObjectOfType<NodeBank>();
            }
            if (tgameObject != null && displayObject)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("save"))
                {
                    if (script != "" && script != null)
                    {
                        tgameObject.OnLoad = CompiledCode.Compile(script);
                        var data = JsonConvert.SerializeObject(tgameObject);
                        File.WriteAllText($"{TManifest.RepositoryPath}/{tgameObject.assetId}.tridinet", data);
                        File.WriteAllText($"{TManifest.ScriptPath}/{tgameObject.assetId}.tr", script);
                    }
                }
                if (GUILayout.Button("Quit"))
                {
                    script = "";
                    displayObject = false;
                    if (currentManifest == null)
                    {
                        currentManifest = new TManifest();
                        currentManifest.LoadLocalItem();
                    }
                }
                if (GUILayout.Button("Compile"))
                {
                    tgameObject.OnLoad = CompiledCode.Compile(script);
                }
                if (tgameObject.OnLoad != null)
                {
                    if (GUILayout.Button("Run"))
                    {
                        WorldBuilder.main.clear();
                        RunCompiledCode.Run(tgameObject.OnLoad, null, "");
                    }
                }
                GUILayout.EndHorizontal();
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
                script = EditorGUILayout.TextArea(script, GUI.skin.textArea, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
            else
            {
                if (GUILayout.Button("Reload Manifest"))
                {
                    currentManifest = new TManifest();
                    currentManifest.LoadLocalItem();
                }
                if (currentManifest.tridiObjects.Count == 0)
                {
                    EditorGUILayout.HelpBox("There are no objects in your repository, create from a model in your prpoject and reload to display them here", MessageType.Info);
                }
                foreach (var record in currentManifest.tridiObjects)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_TextScriptImporter Icon"), GUILayout.Width(40));
                    EditorGUILayout.LabelField(record.Value.name, EditorStyles.toolbarButton);
                    EditorGUILayout.TextField(record.Value.assetId, EditorStyles.toolbarButton);
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), GUILayout.Width(50)))
                    {
                        tgameObject = NodeBank.main.FetchFromProjectFile(record.Value.assetId);
                        var path = $"./{TManifest.ScriptPath}/{tgameObject.assetId}.tr";
                        if (File.Exists(path))
                        {
                            script = File.ReadAllText(path);
                        }
                        else
                        {
                            script = getDefault(tgameObject);
                        }
                        displayObject = true;
                    }
                    if (GUILayout.Button("Spawn", GUILayout.Width(50)))
                    {
                        //NodeBank.main.Replicate(record.Value, OnRecord);
                        var tobject = NodeBank.main.FetchFromProjectFile(record.Value.assetId);
                        var node = NodeBank.main.BuildRecord(tobject, true);
                        Selection.activeGameObject = node.gameObject;
                        SceneView.lastActiveSceneView.Frame(new Bounds(node.transform.position, Vector3.one), false);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        public string getDefault(TGameObject tgameObject)
        {
            var file = File.ReadAllText($"{TManifest.ScriptPath}/default.tr");
            var temp = $"Spawn: {{ target = {tgameObject.assetId}; \n\t num = 1; \n\toffset = 1; \n\tposition = {getStringVector(tgameObject.transform.position)}; \n\tscale = {getStringVector(tgameObject.transform.Scale)}; \n\trotation = {getStringVector(tgameObject.transform.Rotation)}; \n\tdirection = left;\n\tstyle=linear;\n}}:\n";
            return temp + file;
        }

        private string getStringVector(Evector4 vec)
        {
            return $"({vec.x},{vec.y},{vec.z})";
        }

        public string getStringVector(Evector3 vec)
        {
            return $"({vec.x},{vec.y},{vec.z})";
        }

        private void OnRecord(TObjectKeyPair record)
        {
            currentManifest.RecordItem(record.assetId, record.instanceId, record.name);
            currentManifest.RecordItem(record.assetId, record.Url);
            AssetDatabase.Refresh();
        }
    }


    public class WorldEditor : EditorWindow
    {
        public WorldDataContainer wc;
        public TManifest resolver;
        private void OnEnable()
        {
            wc = CreateInstance<WorldDataContainer>();
            WorldBuilder.main = FindObjectOfType<WorldBuilder>();
            resolver = new TManifest();
        }
        public void OnGUI()
        {
            EditorGUILayout.LabelField("TRIDINET EDITOR");
            GUILayout.Space(30);
            GUILayout.Label("World Options");
            wc.name = EditorGUILayout.TextField("Name", wc.name);
            wc.description = EditorGUILayout.TextField("Description", wc.description);
            wc.storeUri = EditorGUILayout.TextField("StoreURI", wc.storeUri);
            wc.access = (WorldAccess)EditorGUILayout.EnumPopup("Access", wc.access);
            wc.startPosition = EditorGUILayout.Vector3Field("startPosition", wc.startPosition);
            if (wc.access == WorldAccess.@private)
            {
                wc.privateKey = EditorGUILayout.TextField("privateKey", wc.privateKey);
            }
            wc.type = (WorldType)EditorGUILayout.EnumPopup("Access", wc.type);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            if (GUILayout.Button("Create"))
            {
                var data = WorldBuilder.main.createWorld(wc.name, wc.startPosition);
                var f1path = $"{TManifest.WorldsPath}/{wc.name}.world";
                File.WriteAllText(f1path, data);
                wc.path = f1path;
                AssetDatabase.CreateAsset(wc, $"Assets/Tridinet/{wc.name}.asset");
                AssetDatabase.Refresh();
                Close();
            }
            GUILayout.Space(30);
            GUILayout.EndHorizontal();
        }
    }
}
