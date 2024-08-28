using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Struct;
using Utils;

namespace Prefabs
{
    public class ApplicationHandler : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("End Application");
            SystemData.Instance.SaveSystemConfig();
            SystemData.Instance.SaveRoomConfig();
        }
    }
}
