using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tridinet.UI
{
    public class IconDisplay : MonoBehaviour
    {
        public Button btn;
        public RawImage Icon;
        public Text _title;
        public string itemId;

        public void Display(string title, UnityAction OnInvoke)
        {
            if (OnInvoke != null)
            {
                btn.onClick.RemoveAllListeners();
                btn?.onClick.AddListener(OnInvoke);
            }
            _title.text = title;
        }

        public void SetPreview(Texture2D image)
        {
            Icon.texture = image;
        }
    }

}