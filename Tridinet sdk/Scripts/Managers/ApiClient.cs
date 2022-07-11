using UnityEngine;
using Proyecto26;
using System.Collections.Generic;
using UnityEngine.Networking;
using Tridinet.Utilities.Data;
using Tridinet.Systems;
using System.Linq;
using UnityEngine.Events;
using System.IO;
using Newtonsoft.Json;
using System.Text;

/// <summary>
/// This is the API client for 
/// tridinet
/// </summary>
public class ApiClient : MonoBehaviour
{
	public string basePath;
	private RequestHelper currentRequest;

	private void LogMessage(string title, string message)
	{
		Debug.Log(message);
	}

	public string itemId;
	public string email;
	public string password;
	public bool signin;

	public static ApiClient main;

    private void Awake()
    {
		if (main == null)
		{
			main = this;
			DontDestroyOnLoad(main);
		}
		else if (main != this)
		{
			Destroy(gameObject);
		}
	}

    private void Start()
    {
		if(signin)
			SignIn(email, password);
		//getItem(itemId);
    }

    public void createItem(TRepository item, UnityAction<TRepository> onCreateRepo)
    {
		var form = new WWWForm();
		form.AddField("name", item.name);
		form.AddField("description", item.description);
		form.AddField("cost", item.cost.ToString());
		form.AddField("cateogory", item.category);
		form.AddBinaryData("file", File.ReadAllBytes($"{TManifest.RepositoryFile}"));
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		currentRequest = new RequestHelper
		{
			Uri = basePath + "/market/create",
			FormData = form,
			EnableDebug = true
		};
		RestClient.Post<ResponseData>(currentRequest)
		.Then(res => {

			// And later we can clear the default query string params for all requests
			onCreateRepo.Invoke(JsonConvert.DeserializeObject<TRepository>(res.data));
			RestClient.ClearDefaultParams();
			this.LogMessage("Success", JsonUtility.ToJson(res, true));
		})
		.Catch(err => this.LogMessage("Error", err.Message));
	}

    internal void getWorlds(int page, UnityAction<string> onResponse)
    {
		var fileUrl = $"{basePath}/world/{page}";
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Get(new RequestHelper
		{
			Uri = fileUrl
		}).Then(res =>
		{
			Debug.Log(res.Text);
			onResponse.Invoke(res.Text);
		}).Catch(err =>
		{
			this.LogMessage("Error", err.Message);
		});
	}

    internal void FetchUri(string endpoint, byte [] file, string itemId, UnityAction<string, string> Callback)
	{
		var form = new WWWForm();
		form.AddField("id", itemId);
		form.AddBinaryData("file", file);
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		currentRequest = new RequestHelper
		{
			Uri = endpoint,
			FormData = form,
			EnableDebug = true
		};
		RestClient.Post<ResponseData>(currentRequest)
		.Then(res => {

			// And later we can clear the default query string params for all requests
			RestClient.ClearDefaultParams();
			Callback.Invoke(itemId, res.data);
			this.LogMessage("Success", JsonUtility.ToJson(res, true));
		})
		.Catch(err => {
			Callback.Invoke(itemId, "");
			this.LogMessage("Error", err.Message);
		});
	}

	public void Delete()
	{

		RestClient.Delete(basePath + "/posts/1", (err, res) => {
			if (err != null)
			{
				this.LogMessage("Error", err.Message);
			}
			else
			{
				this.LogMessage("Success", "Status: " + res.StatusCode.ToString());
			}
		});
	}

	public void AbortRequest()
	{
		if (currentRequest != null)
		{
			currentRequest.Abort();
			currentRequest = null;
		}
	}

	public void Register(string name, string email, string password)
	{

		currentRequest = new RequestHelper
		{
			Uri = basePath + "/user/register",
			Body = new SignProps
			{
				name = name,
				email = email,
				password = password
			},
			EnableDebug = true
		};
		RestClient.Post<ResponseData>(currentRequest)
		.Then(res => {
			RestClient.ClearDefaultParams();

			this.LogMessage("Success", JsonUtility.ToJson(res, true));
			Debug.Log(res.data);
			Debug.Log("Registered Successfully");
		})
		.Catch(err => this.LogMessage("Error", err.Message));
	}

	public void SignIn(string email, string password) {

		currentRequest = new RequestHelper
		{
			Uri = basePath + "/user/login",
			Body = new SignProps
			{
				email = email,
				password = password
			},
			EnableDebug = true
		};
		RestClient.Post<ResponseData>(currentRequest)
		.Then(res => {
			RestClient.ClearDefaultParams();

			this.LogMessage("Success", JsonUtility.ToJson(res, true));
			Debug.Log(res.data);
			PlayerPrefs.SetString("token", res.data);

		})
		.Catch(err => this.LogMessage("Error", err.Message));
	}

    internal void fetchObjectFromRecord(string url)
    {

    }

    public void getRepositories(int page, UnityAction<string> OnResponse)
	{
		var fileUrl = $"{basePath}/Repository/{page}";
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Get(new RequestHelper
		{
			Uri = fileUrl
		}).Then(res =>
		{
			OnResponse.Invoke(res.Text);
		}).Catch(err =>
		{
			if (err.Message == "HTTP/1.1 401 Unauthorized")
			{
				PlayerPrefs.SetString("token", "");
			}
			this.LogMessage("Error", err.Message);
		});
	}

	public void FetchTextureGloabl(string url, UnityAction<Texture2D> OnSetTexture)
	{
		RestClient.Get(new RequestHelper
		{
			Uri = url,
			DownloadHandler = new DownloadHandlerTexture()
		}).Then(res => {
			var image = ((DownloadHandlerTexture)res.Request.downloadHandler).texture;
			OnSetTexture.Invoke(image);
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}

	public void FetchTexture(string itemId, UnityAction<Texture2D> OnSetTexture)
	{
		var fileUrl = $"{basePath}/market/preview/{itemId}";
		RestClient.Get(new RequestHelper
		{
			Uri = fileUrl,
			DownloadHandler = new DownloadHandlerTexture()
		}).Then(res => {
			var image = ((DownloadHandlerTexture)res.Request.downloadHandler).texture;
			Debug.Log(OnSetTexture);
			OnSetTexture.Invoke(image);
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}

	public void DeleteWorld(string id, UnityAction<string, bool> OnResponse)
	{
		Debug.Log(id);
		var fileUrl = $"{basePath}/world/delete/{id}";
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Delete(new RequestHelper
		{
			Uri = fileUrl
		}).Then(res => {
			if (res.StatusCode == 200)
			{
				OnResponse.Invoke(id, true);
			}
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}

	public void DeleteItem(string id, UnityAction<string, bool> OnResponse)
    {
		Debug.Log(id);
		var fileUrl = $"{basePath}/market/item/{id}";
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Delete(new RequestHelper
		{
			Uri = fileUrl,
		}).Then(res => {
			if (res.StatusCode == 200)
			{
				OnResponse.Invoke(id, true);
			}
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}

	public void fetchManifest(TRepository item, UnityAction<TManifest> OnFetch)
	{
		Debug.Log(item.id);
		var fileUrl = $"{basePath}/market/item/{item.id}";
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Get(new RequestHelper
		{
			Uri = fileUrl,
			Body = new { id= item.id}
		}).Then(res =>
		{
			Debug.Log(res.Text);
			var parserfile = JsonConvert.DeserializeObject<TManifest>(res.Text);
			parserfile.name = item.name;
			OnFetch.Invoke(parserfile);
		}).Catch(err =>
		{
			this.LogMessage("Error", err.Message);
		});
	}

		public void getItemBuild(TRepository item)
	{
		Debug.Log(item.id);
		var fileUrl = $"{basePath}/market/item/{item.id}";
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Get(new RequestHelper
		{
			Uri = fileUrl
		}).Then(res => {
			var parserfile = JsonUtility.FromJson<TGameObject>(res.Text);
			parserfile.assetId = item.id;
			EventManager.main.OnFetchNodeObject.Invoke(parserfile);
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}

	public void getWorld(string url, string id)
	{
		currentRequest = new RequestHelper
		{
			Uri = basePath + "/world/fetchRepoWorld",
			Body = new
			{
				url = url,
				id = id
			},
			EnableDebug = true
		};
		RestClient.Post(currentRequest)
		.Then(data => {
			Debug.Log(data.Text);
			var res = JsonConvert.DeserializeObject<WorldResponse>(data.Text);
			Debug.Log(res.data);
			var payload = JsonConvert.DeserializeObject<World>(res.data.data);
			WorldBuilder.main.Init(payload);
			RestClient.ClearDefaultParams();
			this.LogMessage("Success", JsonUtility.ToJson(res, true));
		})
		.Catch(err => this.LogMessage("Error", err.Message));
	}

	public void getWorld(string url)
	{
		currentRequest = new RequestHelper
		{
			Uri = basePath + "/world/fetchworld",
			//Params = new Dictionary<string, string> {
			//	{ "param1", "value 1" },
			//	{ "param2", "value 2" }
			//},
			Body = new UrlPayload() { 
				url = url
			},
			EnableDebug = true
		};
		RestClient.Post(currentRequest)
		.Then(data => {
			Debug.Log(data.Text);
			var payload = JsonUtility.FromJson<World>(data.Text);
			WorldBuilder.main.InitViaBrower(payload);
			RestClient.ClearDefaultParams();
			this.LogMessage("Success", JsonUtility.ToJson(data, true));
		})
		.Catch(err => this.LogMessage("Error", err.Message));
	}

	public void CreateWorld(WorldPayload payload, UnityAction<WorldPayload> onCreated)
	{
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		var form = new WWWForm();
		form.AddField("name", payload.name);
		form.AddField("description", payload.description);
		form.AddField("privateKey", payload.privateKey);
		form.AddField("type", payload.type);
		form.AddField("access", payload.access);
		form.AddBinaryData("file", Encoding.ASCII.GetBytes(payload.data));
		currentRequest = new RequestHelper
		{
			Uri = basePath + "/world/create",
			FormData = form,
			EnableDebug = true
		};
		RestClient.Post<WorldResponse>(currentRequest)
		.Then(res => {
			onCreated.Invoke(res.data);
			EventManager.main.OnWorldLoaded.Invoke();
			RestClient.ClearDefaultParams();
			this.LogMessage("Success", JsonUtility.ToJson(res, true));

		})
		.Catch(err => { EventManager.main.OnWorldLoaded.Invoke(); this.LogMessage("Error", err.Message); });
	}

	public void UpdateWorld(WorldPayload payload, UnityAction<WorldPayload> onCreated)
	{
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		var form = new WWWForm();
		form.AddField("name", payload.name);
		form.AddField("description", payload.description);
		form.AddField("privateKey", payload.privateKey);
		form.AddField("type", payload.type);
		form.AddField("access", payload.access);
		form.AddBinaryData("file", Encoding.ASCII.GetBytes(payload.data));
		currentRequest = new RequestHelper
		{
			Uri = basePath + "/world/update",
			FormData = form,
			EnableDebug = true
		};
		RestClient.Post<WorldResponse>(currentRequest)
		.Then(res => {
			onCreated.Invoke(res.data);
			EventManager.main.OnWorldLoaded.Invoke();
			RestClient.ClearDefaultParams();
			this.LogMessage("Success", JsonUtility.ToJson(res, true));

		})
		.Catch(err => { EventManager.main.OnWorldLoaded.Invoke(); this.LogMessage("Error", err.Message); });
	}

	public void getItem(string url, TObjectKeyPair record, UnityAction<TObjectKeyPair> Onrecord)
    {
		Debug.Log(url);
        RestClient.Get(new RequestHelper
        {
            Uri = url
        }).Then(res =>
        {
            var parserfile = JsonConvert.DeserializeObject<TGameObject>(res.Text);
			NodeBank.main.BuildRecord(parserfile);
			Onrecord.Invoke(record);
        }).Catch(err =>
        {
            this.LogMessage("Error", err.Message);
        });
    }

    public void getTrinode(string itemId, NodeData data, UnityAction<TGameObject, NodeData> OnBuild)
	{
		RestClient.DefaultRequestHeaders["authorization"] = $"Bearer {PlayerPrefs.GetString("token")}";
		RestClient.Get(new RequestHelper
		{
			Uri = data.url
		}).Then(res => {
			Debug.Log("fetched data");
			var parserfile = JsonConvert.DeserializeObject<TGameObject>(res.Text);
			OnBuild(parserfile, data);
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}

	public void DownloadFile()
	{

		var fileUrl = "https://raw.githubusercontent.com/IonDen/ion.sound/master/sounds/bell_ring.ogg";
		var fileType = AudioType.OGGVORBIS;

		RestClient.Get(new RequestHelper
		{
			Uri = fileUrl,
			DownloadHandler = new DownloadHandlerAudioClip(fileUrl, fileType)
		}).Then(res => {
			AudioSource audio = GetComponent<AudioSource>();
			audio.clip = ((DownloadHandlerAudioClip)res.Request.downloadHandler).audioClip;
			audio.Play();
		}).Catch(err => {
			this.LogMessage("Error", err.Message);
		});
	}
}
