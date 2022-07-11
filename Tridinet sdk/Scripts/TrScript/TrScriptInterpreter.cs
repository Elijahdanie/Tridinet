using Assets.Scripts;
using Assets.Scripts.UI.Modular_UI;
using Tridinet.Utilities.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrScriptInterpreter
{
    private static List<string> DefaultFunctions = new List<string> {
    "Spawn", "Rotate", "Scale", "Translate", "Display"};

    public static void ProcessFunctions(KeyValuePair<string, Dictionary<string, string>> funcToken, INode node, string parentId, CompiledCode blocks)
    {
        switch (funcToken.Key)
        {
            case "Spawn":
                ProcessSpawn(funcToken.Value, node, parentId, blocks);
                break;
            case "Rotate":
                ProcessRotate(funcToken.Value, node);
                break;
            case "Scale":
                ProcessScaling(funcToken.Value, node);
                break;
            case "Translate":
                ProcessMotion(funcToken.Value, node);
                break;
            default:
                break;
        }
    }

    public static Vector3 getVec3(string val)
    {
        var parsedNum = val.Split(',');
        return new Vector3(getFloat(parsedNum[0]), getFloat(parsedNum[1]), getFloat(parsedNum[2]));
    }

    public static Vector3 getVec3(Dictionary<string, string> value, string key, Vector3 thisVec)
    {
        if (value.ContainsKey(key))
        {
            var val = value[key];
            var numbers = val.Replace("(", "");
            numbers = numbers.Replace(")", "");
            numbers = numbers.Replace(" ", "");
            if (numbers.Contains("+"))
            {
                var addtions = numbers.Split('+');
                var finalVec = new Vector3();
                for (int i = 0; i < addtions.Length; i++)
                {
                    finalVec += addtions[i] == "this" ? thisVec : getVec3(addtions[i]);
                }
                return finalVec;
            }
            else
            {
                if (numbers == "this")
                {
                    return thisVec;
                }
                return getVec3(numbers);
            }

        }
        else
        {
            return Vector3.zero;
        }
    }

    public static bool getBoolean(Dictionary<string, string> value, string key)
    {
        if (value.ContainsKey(key))
        {
            var val = value[key];
            return val == "True" ? true : false;
        }
        else
        {
            return false;
        }
    }
    public static float getFloat(string key)
    {
        try
        {
            return float.Parse(key);
        }
        catch (System.Exception)
        {
            Debug.LogError($"Invalid Format of float {key}");
            return 0f;
        }
    }

    public static float getFloat(Dictionary<string, string> value, string key)
    {
        return value.ContainsKey(key) ? float.Parse(value[key]) : 0;
    }

    public static int getInt(Dictionary<string, string> value, string key)
    {
        return value.ContainsKey(key) ? int.Parse(value[key]) : 0;
    }

    public static void ProcessSpawn(Dictionary<string, string> value, INode node, string parentId, CompiledCode code=null)
    {
        Debug.Log(parentId);
        if (value.Count == 0)
        {
            return;
        }
        string target = value["target"];
        if (target == node.data.assetId)
        {
            Debug.LogError("You can't spawn a copy of thie object from itself");
            return;
        }
        if (target == parentId)
        {
            Debug.LogError($"The id {target} is referenced by child {node.data.assetId} with parent {parentId}, this could lead to recursion, children cannot spawn their parents");
            return;
        }
        float offset = getFloat(value, "offset");
        int num = (int)getFloat(value, "num");
        Vector3 startPos = getVec3(value, "position", node.transform.position);
        Vector3 setRot = getVec3(value, "rotation", node.transform.eulerAngles);
        Vector3 setScale = getVec3(value, "scale", node.transform.localScale);
        Vector3 direction = getDirection(value, "direction");
        int layers = getInt(value, "layers");
        string style = getString(value, "style");
        string layerdirection = getString(value, "direction");
        if (layers == 0)
        {
            layers = 1;
        }

        Debug.Log(target);
        for (int j = 0; j < layers; j++)
        {
            Debug.Log(target);
            for (int i = 0; i < num; i++)
            {
                if (style == "linear")
                {
                    var targetPos = startPos + (direction * offset * i);
                    Debug.Log(startPos);
                    SpawnTarget(target, targetPos, setRot, setScale, OnResponse, node.data.assetId, true);
                }
                else if (style == "radial")
                {
                    PlaceCirlce(target, offset * (j + 1), (360 / num) * i, setRot, setScale, startPos, node.data.assetId);
                }
            }
        }
    }


    private static void OnResponse(INode arg0)
    {

    }

    private static string getString(Dictionary<string, string> value, string v)
    {
        return value.ContainsKey(v) ? value[v] : "";
    }

    public static void SpawnTargetHeader(Dictionary<string, string> value, string target, UnityAction<INode> OnResponse, string parentId, bool v = false)
    {
        try
        {
            Vector3 Pos = getVec3(value, "position", Vector3.zero);
            Vector3 setRot = getVec3(value, "rotation", Vector3.zero);
            Vector3 setScale = getVec3(value, "scale", Vector3.one);
            int layers = getInt(value, "layers");
            string style = getString(value, "style");
            string layerdirection = getString(value, "layerdirection");
            float offset = getFloat(value, "offset");
            int num = (int)getFloat(value, "num");
            Vector3 direction = getDirection(value, "direction");
            if (layers == 0)
            {
                layers = 1;
            }

            Debug.Log(target);
            for (int j = 0; j < layers; j++)
            {
                Debug.Log(target);
                for (int i = 0; i < num; i++)
                {
                    if (style == "linear")
                    {
                        var targetPos = Pos + (direction * offset * i);
                        Debug.Log(Pos);
                        SpawnTarget(target, targetPos, setRot, setScale, OnResponse, "", v);
                    }
                    else if (style == "radial")
                    {
                        PlaceCirlce(target, offset * (j + 1), (360 / num) * i, setRot, setScale, Pos, "", v);
                    }
                }
            }
            //NodeData data = new NodeData();
            //data.assetId = target;
            //data.transform = new Etransform();
            //data.transform.Save(Pos, setRot, setScale);
            //NodeBank.main.Replicate(0, data, (node, d, level, script) => {
            //    OnResponse.Invoke(node);
            //    Debug.Log(script);
            //    if (v)
            //    {
            //        RunCompiledCode.Run(script, node, parentId);
            //    }
            //});
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public static void SpawnTarget(string target, Vector3 Pos, Vector3 setRot, Vector3 setScale, UnityAction<INode> OnResponse, string parentId, bool v=false)
    {
        try
        {
        NodeData data = new NodeData();
        data.assetId = target;
        data.transform = new Etransform();
        data.transform.Save(Pos, setRot, setScale);
        NodeBank.main.Replicate(0, data, (node, d, level, script) => {
            OnResponse.Invoke(node);
            Debug.Log(script);
            if (v)
            {
                RunCompiledCode.Run(script, node, parentId);
            }
        });
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public static void PlaceCirlce(string itemid, float offset, float angle, Vector3 setRot, Vector3 setScale, Vector3 pos, string parentId, bool v = false)
    {
        NodeData data = new NodeData();
        data.assetId = itemid;
        data.transform = new Etransform();
        data.transform.Save(getPosition(angle, offset, pos.y), setRot, setScale);
        NodeBank.main.Replicate(0, data, (node, d, level, script) =>
        {
            if (v)
            {
                RunCompiledCode.Run(script, node, parentId);
            }
        });
    }

    public static Vector3 getPosition(float angle, float offset, float y)
    {
        Vector2 positionXY = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * offset;

        Vector3 positionxyz = new Vector3(positionXY.x, y, positionXY.y);
        return positionxyz;
    }

    public static Vector3 getDirection(Dictionary<string, string> value, string key)
    {
        if (value.ContainsKey(key))
        {
            var val = value[key];
            if (val.Contains(","))
            {
                return getVec3(val);
            }
            switch (val)
            {
                case "up":
                    return Vector3.up;
                case "down":
                    return Vector3.down;
                case "right":
                    return Vector3.right;
                case "left":
                    return Vector3.left;
                case "forward":
                    return Vector3.forward;
                case "back":
                    return Vector3.back;
                default:
                    return Vector3.forward;
            }
        }
        return Vector3.forward;
    }

    public static void ProcessRotate(Dictionary<string, string> line, INode node)
    {
        Vector3 axis = getVec3(line, "axis", Vector3.zero);
        bool loop = getBoolean(line, "loop");
        float angle = getFloat(line, "angle");
        if (!loop)
        {
            node.transform.Rotate(axis, angle);
        }
        else
        {
            var trotate = node.gameObject.GetComponent<TRotate>();
            if (trotate == null)
            {
                node.gameObject.AddComponent<TRotate>().setRot(axis, angle);
            }
            else
            {
                trotate.setRot(axis, angle);
            }
        }
    }

    public static void ProcessMotion(Dictionary<string, string> line, INode node)
    {

    }

    public static void ProcessScaling(Dictionary<string, string> line, INode node)
    {
        Vector3 value = getVec3(line, "value", node.transform.localScale);
        float time = line.ContainsKey("time") ? getFloat(line["time"]) : 0f;
        node.transform.localScale = value;
    }


    public static Dictionary<string, string> processFinalTokens(string line)
    {
        string lineparsed = line.Replace("{", "").Replace("}", "").Trim();
        string[] tokens = lineparsed.Split(';');
        Dictionary<string, string> subtokens = new Dictionary<string, string>();
        foreach (var item in tokens)
        {
            item.Trim();
            var parsed = item.Split('=');
            if (parsed.Length > 1)
            {
                subtokens.Add(parsed[0].Trim(), parsed[1].Trim());
            }
            else
            {
                //Debug.Log(item);
            }
        }
        return subtokens;
    }

    public static Dictionary<string, string> processFinalTokensData(string line)
    {
        string lineparsed = line.Replace("{", "").Replace("}", "").Trim();
        string[] tokens = lineparsed.Split(';');
        Dictionary<string, string> subtokens = new Dictionary<string, string>();
        foreach (var item in tokens)
        {
            item.Trim();
            var parsed = item.Split('=');
            if (parsed.Length > 1)
            {
                subtokens.Add(parsed[0].Trim(), parsed[1]);
            }
            else
            {
                //Debug.Log(item);
            }
        }
        return subtokens;
    }

    //public static void SpawnTarget(string token, string scriptBody,
    //    UnityAction<INode, Dictionary<string, string>, string> OnCallback, INode node)
    //{
    //    Dictionary<string, string> data = new Dictionary<string, string>();
    //    var functions = token.Split(':');
    //    if (functions.Length % 2 != 0)
    //    {
    //        Debug.Log("Error, a function missing its implementation body");
    //    }
    //    var value = processFinalTokens(functions[1]);
    //    string target = value["target"];
    //    float offset = getFloat(value, "offset");
    //    int num = (int)getFloat(value, "num");
    //    Vector3 startPos = getVec3(value, "startPos", Vector3.zero);
    //    Vector3 setRot = getVec3(value, "setRot", Vector3.zero);
    //    Vector3 setScale = getVec3(value, "setScale", Vector3.one);
    //    Vector3 direction = getDirection(value, "direction");
    //    if (functions.Length == 4)
    //    {
    //        data = processFinalTokensData(functions[3]);
    //    }
    //    else
    //    {
    //        data = new Dictionary<string, string>();
    //    }
    //    SpawnTarget(target, startPos, setRot, setScale, (n) => {
    //        OnCallback.Invoke(n, data, scriptBody);
    //    });
    //}

    //public static void OnGetObject(INode node, Dictionary<string, string> data, string scriptBody)
    //{
    //    var functions = scriptBody.Split(':');
    //    if (functions.Length % 2 != 0)
    //    {
    //        Debug.Log("Error, a function missing its implementation body");
    //    }
    //    int count = 0;
    //    int length = functions.Length / 2;
    //    string DisplayScript = "";
    //    Dictionary<string, string> customScripts = new Dictionary<string, string>();
    //    for (int i = 0; i < length; i++)
    //    {
    //        var key = functions[count].Trim();
    //        if (key == "Display")
    //        {
    //            DisplayScript = functions[count + 1];
    //        }
    //        else
    //        {
    //            if (DefaultFunctions.Contains(key))
    //            {
    //                ProcessFunctions(new KeyValuePair<string, Dictionary<string, string>>(key, processFinalTokens(functions[count + 1])), node);
    //            }
    //            else
    //            {
    //                customScripts.Add(key, functions[count + 1]);
    //            }
    //        }
    //        count += 2;
    //    }
    //    processDisplayTokens(DisplayScript, customScripts, data, node);
    //}

    //public static void Run(string script, INode node = null)
    //{
    //    if (NodeBank.main == null)
    //    {
    //        NodeBank.main = GameObject.FindObjectOfType<NodeBank>();
    //    }
    //    if (script == "" || script == null) return;
    //    script = script.Replace("\n", "").Replace("\t", "").Replace("\r", "");
    //    var firstToken = script.Split('|');
    //    var target = firstToken[0];
    //    SpawnTarget(target, firstToken[1], OnGetObject, node);
    //}

    public static string getRefKey(string target, Dictionary<string, string> data)
    {
        if (target.Contains("ref"))
        {
            var key = target.Replace("ref[", "").Replace("]", "").Trim();
            return data[key];
        }
        return target;
    }

    public static TMenu Text(string input, TMenu menu, Dictionary<string, string> data)
    {
        input = input.Replace("(", "").Replace(")", "");
        var parsed = input.Split(',');
        if (parsed.Length != 6) 
        {
            Debug.LogError($"Inpufied input is invalid {input}");
            return menu;
        }
        menu.AddText(getRefKey(parsed[2], data), parsed[3] == "inline" ? true : false, new Size() { x = getFloat(parsed[0]), y = getFloat(parsed[1]) },
            int.Parse(parsed[4]), getFontstyle(parsed[5]));
        return menu;
    }

    public static FontStyle getFontstyle(string v)
    {
        switch (v)
        {
            case "bold":
                return FontStyle.Bold;
            case "normal":
                return FontStyle.Normal;
            default:
                return FontStyle.Normal;
        }
    }

    public static TMenu Image(string input, TMenu menu)
    {
        input = input.Replace("(", "").Replace(")", "");
        var parsed = input.Split(',');
        if (parsed.Length != 4)
        {
            Debug.LogError($"Inpufied input is invalid {input}");
            return menu;
        }
        menu.AddImage(parsed[2], new Size() { x = getFloat(parsed[0]), y = getFloat(parsed[1]) },
            parsed[3] == "inline" ? true : false);
        return menu;
    }

    public static TMenu Button(string input, TMenu menu, List<CodeBlocks> data, INode node, string parentId)
    {
        var parsed = input.Replace(";", "").Split('(');
        var functionName = parsed[0];
        var parameters = parsed[1].Replace(")", "").Split(',');
        if (functionName == "Run")
        {
            parameters[2] = parameters[2].Replace(" ", "");
            menu.AddButton(parameters[0], parameters[2] == "inline" ? true : false, data.Find(x=>x.key == parameters[2]).parameters, parameters[3], node, parentId);
        }
        if (functionName == "Open")
        {
            menu.AddButton(parameters[0], parameters[1] == "inline" ? true : false, parameters[2]);
        }
        if (functionName.Contains("["))
        {
            Debug.Log(parsed[1]);
        }
        return menu;
    }

    public static TMenu InputField(string input, TMenu menu)
    {
        input = input.Replace("(", "").Replace(")", "");
        var parsed = input.Split(',');
        if (parsed.Length != 3)
        {
            Debug.LogError($"Inpufied input is invalid {input}");
            return menu;
        }
        menu.AddInputField(new Size() { x = getFloat(parsed[0]), y = getFloat(parsed[1]) }, parsed[2]);
        return menu;
    }
    private static void processDisplayTokens(string line, Dictionary<string, string> customScripts,
        Dictionary<string, string> data, INode node)
    {
        string lineparsed = line.Replace("{", "").Replace("}", "").Trim();
        string[] tokens = lineparsed.Split(';');
        Dictionary<string, string> subtokens = new Dictionary<string, string>();
        if (UIUtils.main == null)
        {
            UIUtils.main = GameObject.FindObjectOfType<UIUtils>();
        }
        var menu = UIUtils.main.SpawnMenu(new Size() { x = 600, y = 700 });
        foreach (var item in tokens)
        {
            var parsed = item.Split('=');
            if (parsed.Length > 1)
            {
                var functionName = parsed[0].Trim();
                var paramas = parsed[1].Trim();
                switch (functionName)
                {
                    case "InputField":
                        InputField(paramas, menu);
                        break;
                    case "Text":
                        Text(paramas, menu, data);
                        break;
                    case "Image":
                        Image(paramas, menu);
                        break;
                    case "OnClick":
                       // Button(paramas, menu, customScripts, node);
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
}

[System.Serializable]
public class CodeBlock {
    public string key;
    public string parameters;
}

[System.Serializable]
public class CodeBlocks
{
    public string key;
    public Dictionary<string, string> parameters;
}

[System.Serializable]
public class CompiledCode {
    public List<CodeBlocks> headerTokens = new List<CodeBlocks>();
    public List<CodeBlocks> OtherTokens = new List<CodeBlocks>();
    public List<CodeBlock> DisplayTokens = new List<CodeBlock>();
    public List<CodeBlocks> customScripts = new List<CodeBlocks>();
    private static List<string> DefaultFunctions = new List<string> {
    "Spawn", "Rotate", "Scale", "Translate", "Display"};

    private static List<CodeBlock> CompileDisplay(string line, List<CodeBlocks> headerTokens)
    {
        string lineparsed = line.Replace("{", "").Replace("}", "").Trim();
        string[] tokens = lineparsed.Split(';');
        List<CodeBlock> codes = new List<CodeBlock>();
        foreach (var item in tokens)
        {
            var parsed = item.Split('=');
            if (parsed.Length > 1)
            {
                var functionName = parsed[0].Trim();
                var paramas = parsed[1].Trim();
                codes.Add(new CodeBlock() {
                    key = functionName,
                    parameters = paramas
                });
            }
            else
            {
                //Debug.Log(item);
            }
        }
        return codes;
    }

    public static List<CodeBlocks> CompileHeaderTokens(string token) {
        var functions = token.Split(':');
        var data = new List<CodeBlocks>();
        var value = processFinalTokens(functions[1]);
        data.Add(new CodeBlocks() { key = "Spawn", parameters = value });
        if (functions.Length == 5)
        {
            data.Add(new CodeBlocks() { key = "Data", parameters = processFinalTokensData(functions[3]) });
        }
        else
        {
            data.Add(new CodeBlocks() { key = "Data", parameters = new Dictionary<string, string>() });
        }
        return data;
    }

    public static Dictionary<string, string> processFinalTokensData(string line)
    {
        string lineparsed = line.Replace("{", "").Replace("}", "").Trim();
        string[] tokens = lineparsed.Split(';');
        Dictionary<string, string> subtokens = new Dictionary<string, string>();
        foreach (var item in tokens)
        {
            item.Trim();
            var parsed = item.Split('=');
            if (parsed.Length > 1)
            {
                subtokens.Add(parsed[0].Trim(), parsed[1]);
            }
            else
            {
                //Debug.Log(item);
            }
        }
        return subtokens;
    }

    public static CompiledCode Compile(string script)
    {
        var compiled = new CompiledCode();
        if (script == "" || script == null) return null;
        script = script.Replace("\n", "").Replace("\t", "").Replace("\r", "");
        var firstToken = script.Split('|');
        var target = firstToken[0];
        compiled.headerTokens = CompileHeaderTokens(target);
        var functions = firstToken[1].Split(':');
        //if (functions.Length % 2 != 0)
        //{
        //    Debug.Log("Error, a function missing its implementation body");
        //}
        int count = 0;
        int length = functions.Length / 2;
        string DisplayScript = "";
        for (int i = 0; i < length; i++)
        {
            var key = functions[count].Trim();
            if (key == "Display")
            {
                DisplayScript = functions[count + 1];
            }
            else
            {
                if (DefaultFunctions.Contains(key))
                {
                    compiled.OtherTokens.Add(new CodeBlocks() { key = key, parameters = processFinalTokens(functions[count + 1]) });
                }
                else
                {
                    compiled.customScripts.Add(new CodeBlocks() { key = key, parameters = processFinalTokens(functions[count + 1]) });
                }
            }
            count += 2;
        }
        compiled.DisplayTokens = CompileDisplay(DisplayScript, compiled.headerTokens);
        return compiled;
    }

    public static Dictionary<string, string> processFinalTokens(string line)
    {
        string lineparsed = line.Replace("{", "").Replace("}", "").Trim();
        string[] tokens = lineparsed.Split(';');
        Dictionary<string, string> subtokens = new Dictionary<string, string>();
        foreach (var item in tokens)
        {
            item.Trim();
            var parsed = item.Split('=');
            if (parsed.Length > 1)
            {
                subtokens.Add(parsed[0].Trim(), parsed[1].Trim());
            }
            else
            {
                //Debug.Log(item);
            }
        }
        return subtokens;
    }

    public static string Text(string input,  Dictionary<string, string> data)
    {
        var parsed = input.Split(',');
        List<string> parameters = new List<string>();
        if (parsed.Length != 6)
        {
            Debug.LogError($"Inpufied input is invalid {input}");
        }
        var compiledText = new TextData() { 
            content = TrScriptInterpreter.getRefKey(parsed[2], data),
            lastGroup = parsed[3] == "inline" ? true : false,
            size = new Size() { x = TrScriptInterpreter.getFloat(parsed[0]), y = TrScriptInterpreter.getFloat(parsed[1]) },
            fontSize = int.Parse(parsed[4]),
            fontStyle = TrScriptInterpreter.getFontstyle(parsed[5])
        };
        return JsonConvert.SerializeObject(compiledText);
    }
}

[System.Serializable]
public class TextData {
    public string content;
    public bool lastGroup;
    public Size size;
    public int fontSize;
    public FontStyle fontStyle;
}

