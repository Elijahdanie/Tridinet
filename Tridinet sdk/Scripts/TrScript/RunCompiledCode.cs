using Assets.Scripts.UI.Modular_UI;
using Tridinet.Systems;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{

    public class RunCompiledCode
    {
        public static void SpawnTarget(CompiledCode code, INode node, UnityAction<INode, CompiledCode, string> Callback, string parentId)
        {
            if (code.headerTokens == null) return;
            var block = code.headerTokens.Find(x => x.key == "Spawn");
            if (block == null || block.parameters.Count == 0)
            {
                return;
            }
            Dictionary<string, string> value = block.parameters;
            string target = value["target"];
            if (target.Contains("ref["))
            {
                Debug.Log(target);
                var temptoken = target.Replace("ref[", "").Replace("]", "").Trim();
                if (temptoken.Contains(","))
                {
                    var temp = temptoken.Split(',');
                    ApiClient.main.fetchManifest(new TRepository() { id = temp[0] }, (manifest) => {
                        OnFetch(manifest, temp[1], temp[2], node, parentId, code);
                    });
                    return;
                }
            }
            TrScriptInterpreter.SpawnTargetHeader(value, target, (n) =>
            {
                Callback.Invoke(n, code, parentId);
            }, parentId, false);
        }

        private static void OnFetch(TManifest arg0, string presentationId, string codeKey, INode node, string parentId, CompiledCode code)
        {
            try
            {
            WorldBuilder.main.CreateFromManifest(arg0);
                foreach (var item in arg0.tridiObjects)
                {

                    var codecopy = JsonConvert.DeserializeObject<CompiledCode>(JsonConvert.SerializeObject(code));
                    var index = codecopy.customScripts.FindIndex(x => x.key == codeKey.Trim());
                    var headerindex = codecopy.headerTokens.FindIndex(x => x.key == "Spawn");
                    codecopy.customScripts[index].parameters["target"] = item.Key;
                    codecopy.customScripts[index].key = "Spawn";
                    codecopy.OtherTokens.Add(codecopy.customScripts[index]);
                    codecopy.headerTokens[headerindex].parameters["target"] = presentationId.Trim();
                    RunCompiledCode.Run(codecopy, null, "");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        private static void processDisplayTokens(CompiledCode code, INode node, string parentId)
        {
            if (UIUtils.main == null)
            {
                UIUtils.main = GameObject.FindObjectOfType<UIUtils>();
            }
            var menu = UIUtils.main.SpawnMenu(new Size() { x = 600, y = 700 });
            foreach (var item in code.DisplayTokens)
            {
                if (code.DisplayTokens.Count > 0)
                {
                    var functionName = item.key;
                    var paramas = item.parameters;
                    switch (functionName)
                    {
                        case "InputField":
                            TrScriptInterpreter.InputField(paramas, menu);
                            break;
                        case "Text":
                            TrScriptInterpreter.Text(paramas, menu, code.headerTokens.Find(x=>x.key== "Data").parameters);
                            break;
                        case "Image":
                            TrScriptInterpreter.Image(paramas, menu);
                            break;
                        case "OnClick":
                            TrScriptInterpreter.Button(paramas, menu, code.customScripts, node, parentId);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //Debug.Log(item);
                }
            }
        }


        public static void OnGetObject(INode node, CompiledCode code, string parentId)
        {
            if (node == null)
            {
                Debug.Log($"Your primary target with id {parentId} is compromised");
            }
            foreach (var x in code.OtherTokens)
            {
                Debug.Log(x.key);
                TrScriptInterpreter.ProcessFunctions(new KeyValuePair<string, Dictionary<string, string>>(x.key, x.parameters), node, parentId, code);
            }
            processDisplayTokens(code, node, parentId);
        }
        public static void Run(CompiledCode code, INode node, string parentId)
        {
            if (code == null)
            {
                return;
            }
            if (NodeBank.main == null)
            {
                NodeBank.main = GameObject.FindObjectOfType<NodeBank>();
            }
            if (ApiClient.main == null)
            {
                ApiClient.main = GameObject.FindObjectOfType<ApiClient>();
            }
            SpawnTarget(code, node, OnGetObject, parentId);
        }
    }
}