
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tridinet.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class ItemDisplay : MonoBehaviour
    {
        public TMP_Text name;
        public TMP_Text description;
        public TMP_Text additiondata;
        public RawImage image;
        public Button btn;
        internal void Display(DisplayData displayData, UnityAction p)
        {
            name.text = displayData.name;
            description.text = displayData.description;
            additiondata.text = displayData.additionalinfo;
            if (displayData.imageuri != "")
            {
                ApiClient.main.FetchTexture(displayData.imageuri, OnSetTexture);
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(p);
        }

        private void OnSetTexture(Texture2D arg0)
        {
            image.texture = arg0;
        }
    }
}