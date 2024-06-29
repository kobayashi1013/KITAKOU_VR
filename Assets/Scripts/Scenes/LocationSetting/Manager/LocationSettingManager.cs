using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Constant;
using TMPro;

namespace Scenes.LocationSetting.Manager
{
    public class LocationSettingManager : MonoBehaviour
    {
        [Header("SceneComponent")]
        [SerializeField] private TMP_Text _roomName;

        public static LocationSettingManager Instance;

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }

        public void PushBasicSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.BasicSettingScene);
        }

        public void PushBackButton()
        {
            SceneManager.LoadScene((int)SceneName.StartScene);
        }

        public void PushRoomButtons(string id)
        {
            _roomName.text = id;
        }
    }
}
