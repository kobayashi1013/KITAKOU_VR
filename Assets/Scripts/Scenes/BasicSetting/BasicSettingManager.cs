using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Constant;
using Utils;
using Struct;

namespace Scenes.BasicSetting
{
    public class BasicSettingManager : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _mortonModelButton;
        [SerializeField] private Toggle _useFlooding;
        [SerializeField] private Toggle _useEvent;

        private void Start()
        {
            LoadSetting();
        }

        public void PushLocationSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.LocationSettingScene);
        }

        public void PushFileSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.FileSettingScene);
        }

        public void PushBackButton()
        {
            SceneManager.LoadScene((int)SceneName.StartScene);
        }

        public void ChangeMortonModelDepth()
        {
            if (_mortonModelButton.value == 0)
                SystemData.Instance.SetMortonModelDepth(MortonModelDepth.depth4);
            else if (_mortonModelButton.value == 1)
                SystemData.Instance.SetMortonModelDepth(MortonModelDepth.depth8);
            else if (_mortonModelButton.value == 2)
                SystemData.Instance.SetMortonModelDepth(MortonModelDepth.depth16);
            else if (_mortonModelButton.value == 3)
                SystemData.Instance.SetMortonModelDepth(MortonModelDepth.depth32);
            else if (_mortonModelButton.value == 4)
                SystemData.Instance.SetMortonModelDepth(MortonModelDepth.depth64);
        }

        public void PushUseFlooding()
        {
            SystemData.Instance.SetUseFlooding(_useFlooding.isOn);
        }

        public void PushUseEvent()
        {
            SystemData.Instance.SetUseEvent(_useEvent.isOn);
        }

        public void PushResetConfigButton()
        {
            TextAsset textAsset = null;
            string[] lines = null;

            //システムコンフィグ
            textAsset = Resources.Load("File/SystemConfigOriginal") as TextAsset;
            lines = textAsset.text.Split("\n");

            SystemData.Instance.SetMortonModelDepth((MortonModelDepth)Enum.Parse(typeof(MortonModelDepth), lines[0]));
            SystemData.Instance.SetUseFlooding(bool.Parse(lines[1]));
            SystemData.Instance.SetUseEvent(bool.Parse(lines[2]));

            //ルームコンフィグ
            textAsset = Resources.Load("File/RoomConfigOriginal") as TextAsset;
            lines = textAsset.text.Split ("\n");
            var roomDataList = new Dictionary<string, RoomData>();

            for (int i = 0; i < lines.Length; i++)
            {
                string[] textData = lines[i].Split(",");
                RoomData roomData = new RoomData();

                roomData.name = textData[1];
                roomData.state = (RoomState)Enum.Parse(typeof (RoomState), textData[2]);
                roomData.width0 = float.Parse(textData[3]);
                roomData.width1 = float.Parse(textData[4]);

                roomDataList.Add(textData[0], roomData);
            }

            SystemData.Instance.roomDataList = roomDataList;

            //BasicSettingシーンの描画更新
            LoadSetting();
        }

        public void LoadSetting()
        {
            if (SystemData.Instance.mortonModelDepth == MortonModelDepth.depth4)
                _mortonModelButton.value = 0;
            else if (SystemData.Instance.mortonModelDepth == MortonModelDepth.depth8)
                _mortonModelButton.value = 1;
            else if (SystemData.Instance.mortonModelDepth == MortonModelDepth.depth16)
                _mortonModelButton.value = 2;
            else if (SystemData.Instance.mortonModelDepth == MortonModelDepth.depth32)
                _mortonModelButton.value = 3;
            else if (SystemData.Instance.mortonModelDepth == MortonModelDepth.depth64)
                _mortonModelButton.value = 4;

            _useFlooding.isOn = SystemData.Instance.useFlooding;
            _useEvent.isOn = SystemData.Instance.useEvent;
        }
    }
}
