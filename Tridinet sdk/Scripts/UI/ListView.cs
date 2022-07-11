using Assets.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Tridinet.Systems;
using Tridinet.Utilities.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tridinet.UI
{
    /// <summary>
    /// This class manages display or repositories and
    /// worlds
    /// </summary>
    public class ListView : MonoBehaviour
    {
        public ItemDisplay display;
        public List<ItemDisplay> displays = new List<ItemDisplay>();

        public Button next;
        public Button previous;
        public Transform parent;
        public int currentPage = 0;
        public UnityAction Next;
        public UnityAction Previous;

        public void DisplayItem(List<TRepository> listofRepos, int total)
        {
            Clear();
            listofRepos.ForEach(X =>
            {
                var temp = Instantiate(display, parent);
                displays.Add(temp);
                temp.Display(new DisplayData()
                {
                    name = X.name,
                    description = X.description,
                    additionalinfo = X.cost.ToString()
                }, () =>
                {
                    ApiClient.main.getWorld("tr://repository.world", X.id);
                });
            });
            next.onClick.RemoveAllListeners();
            previous.onClick.RemoveAllListeners();
            next.onClick.AddListener(() =>
            {
                if (currentPage < total)
                {
                    currentPage++;
                    ApiClient.main.getRepositories(currentPage, OnResponse);
                }
            });
            previous.onClick.AddListener(() =>
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    ApiClient.main.getRepositories(currentPage, OnResponse);
                }
            });
        }

        public void OnResponse(string arg0)
        {
            Debug.Log(arg0);
            var tempres = JsonConvert.DeserializeObject<ItemBuilds>(arg0);
            DisplayItem(tempres.data, tempres.total);
        }

        public void DisplayItem(List<worldList> worlds, int total)
        {
            Clear();
            worlds.ForEach(X =>
            {
                var temp = Instantiate(display, parent);
                displays.Add(temp);
                Debug.Log(total);
                temp.Display(new DisplayData()
                {
                    name = X.Name,
                    description = X.Description,
                    additionalinfo = X.url
                }, () =>
                {
                    WorldBrowser.main.SetURI(X.url);
                });
            });
            next.onClick.RemoveAllListeners();
            previous.onClick.RemoveAllListeners();
            next.onClick.AddListener(() =>
            {
                if (currentPage < total)
                {
                    currentPage++;
                    ApiClient.main.getWorlds(currentPage, OnResponseWorld);
                }
            });
            previous.onClick.AddListener(() =>
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    ApiClient.main.getWorlds(currentPage, OnResponseWorld);
                }
            });
        }

        public void OnResponseWorld(string arg0)
        {
            Debug.Log(arg0);
            var tempres = JsonConvert.DeserializeObject<WorldsList>(arg0);
            DisplayItem(tempres.data, tempres.total);
        }

        public void Clear()
        {
            displays.ForEach(x => { Destroy(x.gameObject); });
            displays.Clear();
        }
    }

    public class DisplayData
    {
        public string name;
        public string description;
        public string additionalinfo;
        public string imageuri;
    }
}