using Tridinet.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Modular_UI
{
    /// <summary>
    /// This class represent a dynamic UI windows
    /// </summary>
    public class TMenu : MonoBehaviour
    {
        public LayoutGroup layoutGroup;
        Dictionary<string, string> data = new Dictionary<string, string>();
        public Transform container;
        public TMenu AddText(string content, bool lastGroup, Size size, int fontSize, FontStyle fontStyle)
        {
            if (!lastGroup)
            {
                var hl = getNew<HorizontalLayoutGroup>(size);
                hl.childAlignment = TextAnchor.UpperLeft;
                hl.padding = new RectOffset(0, 0, 5, 5);
            }
            var textObject = new GameObject("Text").AddComponent<RectTransform>();
            textObject.transform.SetParent(layoutGroup.transform);
            textObject.sizeDelta = new Vector2(textObject.sizeDelta.x, 50);
            var text = textObject.gameObject.AddComponent<Text>();
            text.text = content;
            var contentsize = text.gameObject.AddComponent<ContentSizeFitter>();
            contentsize.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = TextAnchor.MiddleLeft;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="Key"></param>
        public void AddInputField(Size size, string Key) {
            var hl = getNew<HorizontalLayoutGroup>(size);
            hl.childAlignment = TextAnchor.UpperLeft;
            hl.padding = new RectOffset(0, 0, 5, 5);
            data.Add(Key, "");
            AddText(Key, true, size, 20, FontStyle.Bold);
            var inputObject = Instantiate(UIUtils.main.InputField, layoutGroup.transform);
            inputObject.transform.SetParent(layoutGroup.transform);
            var rect = inputObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 50);
            inputObject.onEndEdit.AddListener((x) => {
                data[Key] = x;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="link"></param>
        /// <param name="size"></param>
        /// <param name="lastGroup"></param>
        public void AddImage(string link, Size size, bool lastGroup) {
            if (!lastGroup)
            {
                var hl = getNew<HorizontalLayoutGroup>(size);
                hl.childAlignment = TextAnchor.UpperLeft;
                hl.padding = new RectOffset(0, 0, 5, 5);
            }
            var rect = new GameObject("image").AddComponent<RectTransform>();
            var image = rect.gameObject.AddComponent<RawImage>();
            var proccersor = new TImage() { cache = image };
            rect.transform.SetParent(layoutGroup.transform);
            rect.sizeDelta = new Vector2(100, 100);
            proccersor.GetTextTure(link);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="size"></param>
        /// <returns></returns>
        public T getNew<T>(Size size) where T : LayoutGroup
        {
            var lgroup = Instantiate(UIUtils.main.HorizontalPrefab, container);
            lgroup.transform.SetParent(container);
            layoutGroup = lgroup;
            layoutGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y);
            return layoutGroup as T;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lastGroup"></param>
        /// <param name="bodyParamKeys"></param>
        /// <param name="paramKeys"></param>
        /// <param name="HTTPREQUES"></param>
        /// <param name="url"></param>
        public void AddButton(string name, bool lastGroup, List<string> bodyParamKeys, List<string> paramKeys, string HTTPREQUES, string url)
        { 
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lastGroup"></param>
        /// <param name="url"></param>
        public void AddButton(string name, bool lastGroup, string url)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lastGroup"></param>
        /// <param name="code"></param>
        /// <param name="Key"></param>
        /// <param name="node"></param>
        /// <param name="parentId"></param>
        public void AddButton(string name, bool lastGroup, Dictionary<string, string> code, string Key, INode node, string parentId)
        {
            //data.Add(Key, Script);
             TrScriptInterpreter.ProcessSpawn(code, node, parentId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <param name="DefaultPanel"></param>
        internal void InitMenu(Size size, Color color, Sprite DefaultPanel)
        {
            var rect = gameObject.GetComponent<RectTransform>();
            var vert = container.GetComponent<VerticalLayoutGroup>();
            vert.childControlHeight = false;
            rect.sizeDelta = new Vector2(size.x, size.y);
            vert.padding = new RectOffset(5, 5, 20, 20);
            vert.spacing = 10;
            vert.childControlHeight = true;
            vert.childControlWidth = true;
            vert.childForceExpandWidth = true;
            vert.gameObject.AddComponent<Image>().sprite = DefaultPanel;
            var parentRect = gameObject.GetComponent<RectTransform>().rect;
            parentRect.position = Vector2.zero;
            layoutGroup = vert;
        }
    }
}