using Tridinet.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Tridinet.Systems
{
    public class OnSelectEventNodeObject : UnityEvent<TGameObject> { }
    //public class OnSelectEventNode : UnityEvent<INode, List<INode>, ActionList> { }

    public class OnDisplayNodeObejcts : UnityEvent<List<TGameObject>> { }


    public class BoolEvent : UnityEvent<bool> { }

    public class Vector2Event : UnityEvent<Vector2> { }

    public class ItemEvent : UnityEvent<TRepository> { }


    public class EventManager : MonoBehaviour
    {
        public static EventManager main;

        private void Awake()
        {
            main = this;
        }
        public OnDisplayNodeObejcts ondisplayNodes = new OnDisplayNodeObejcts();
        public UnityEvent OnSave = new UnityEvent();
        public ItemEvent OnSelectPrefab = new ItemEvent();
        //public OnSelectEventNode OnSelectNode = new OnSelectEventNode();
        //public OnSelectEventNode OnDeSelectNode = new OnSelectEventNode();
        public Vector2Event onNavigation = new Vector2Event();
        public BoolEvent onRun = new BoolEvent();
        public BoolEvent onJump = new BoolEvent();
        public OnSelectEventNodeObject OnFetchNodeObject = new OnSelectEventNodeObject();
        public ItemEvent OnGetItem = new ItemEvent();
        public OnSelectEventNodeObject OnDisplayNodeObject = new OnSelectEventNodeObject();
        public UnityEvent OnWorldLoaded = new UnityEvent();

        #region LOGGER
        string filename = "";
        void OnEnable() { Application.logMessageReceived += Log; }
        void OnDisable() { Application.logMessageReceived -= Log; }

        public void Log(string logString, string stackTrace, LogType type)
        {
            string d = Environment.CurrentDirectory + "/TRIDINET_LOGS";
            if (!Directory.Exists(d))
            {
                System.IO.Directory.CreateDirectory(d);
            }
            if (filename == "")
            {
                filename = d + "/tridinet_logs.txt";
            }

            try
            {
                File.AppendAllText(filename, logString + "\n stack trace" + stackTrace + "\n");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        #endregion

    }

}
