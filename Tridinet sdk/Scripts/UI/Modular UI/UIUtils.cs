using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Modular_UI
{
    /// <summary>
    /// This class manages the procedural provisioning of
    /// </summary>
    public class UIUtils : MonoBehaviour
    {
        public Sprite panel;
        public InputField InputField;
        public TMenu Mainprefab;
        public Transform MainCanvas;
        public Button btnprefab;

        public static UIUtils main;
        public HorizontalLayoutGroup HorizontalPrefab;

        /// <summary>
        /// Initialize the main menu window
        /// </summary>
        /// <param name="size"></param>
        public void BeginUI(Size size)
        {
            var menu = Instantiate(Mainprefab, MainCanvas);
            menu.InitMenu(size, Color.gray, panel);
            menu.AddText("Test", false, new Size() { x = 400, y = 50 }, 30, FontStyle.Bold);
        }

        /// <summary>
        /// Initialize the main menu window
        /// </summary>
        /// <param name="size"></param>
        public TMenu SpawnMenu(Size size)
        {
            var obj = FindObjectOfType<TMenu>();
            if (obj) DestroyImmediate(obj.gameObject);
            var menu = Instantiate(Mainprefab, MainCanvas);
            menu.InitMenu(size, Color.gray, panel);
            return menu;
        }

        /// <summary>
        /// This function adds a text
        /// </summary>
        /// <param name="content"></param>
        /// <param name="menu"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        public TMenu AddText(string content, TMenu menu, string type, Size size, int fontSize, FontStyle fontStyle)
        {
            switch (type)
            {
                case "inline":
                    menu.AddText(content, true, size, fontSize, fontStyle);
                    break;
                case "block":
                    break;
                default:
                    break;
            }
            return menu;
        }
    }

    public struct Size
    {
        public float x;
        public float y;
    }


    public class TImage
    {
        public RawImage cache;

        public void GetTextTure(string link)
        {
            ApiClient.main.FetchTextureGloabl(link, OnSetTexture);
        }

        private void OnSetTexture(Texture2D arg0)
        {
            cache.texture = arg0;
        }
    }
}