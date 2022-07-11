using Tridinet.Utilities.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// This class displays tridinet items in runtime for
/// instantiation
/// </summary>
public class TridnodeDisplay : MonoBehaviour
{
    public Button btn;
    public RawImage Icon;
    public Text _title;

    /// <summary>
    /// Entry point of the class
    /// </summary>
    /// <param name="item"></param>
    /// <param name="OnClick"></param>
    public void Init(TRepository item, UnityAction OnClick) {
        if (item.previewUrl != "")
        {
            ApiClient.main.FetchTexture(item.id, SetPreview);
        }
        Display(item.id, OnClick);
    }

    /// <summary>
    /// Displays an action with a callback
    /// </summary>
    /// <param name="title"></param>
    /// <param name="OnInvoke"></param>
    public void Display(string title, UnityAction OnInvoke)
    {
        if (OnInvoke != null)
        {
            btn.onClick.RemoveAllListeners();
            btn?.onClick.AddListener(OnInvoke);
        }
        _title.text = title;
    }

    /// <summary>
    /// A call back to set texture
    /// </summary>
    /// <param name="image"></param>
    public void SetPreview(Texture2D image)
    {
        Icon.texture = image;
    }
}
