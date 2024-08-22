using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Struct;
using Utils;

namespace Prefabs
{
    [Serializable]
    public class ConfigData
    {
        public bool useFlooding;
    }

    public class ApplicationHandler : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (PlayerPrefs.HasKey("ConfigData"))
            {
                string json = PlayerPrefs.GetString("ConfigData");
                ConfigData data = JsonUtility.FromJson<ConfigData>(json);

                SystemData.Instance.SetUseFlooding(data.useFlooding);
            }
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("End Application");
            SaveConfig();
        }

        private void SaveConfig()
        {
            ConfigData data = new ConfigData()
            {
                useFlooding = SystemData.Instance.useFlooding,
            };

            string json = JsonUtility.ToJson(data, true);

            PlayerPrefs.SetString("ConfigData", json);
            PlayerPrefs.Save();
        }
    }
}
