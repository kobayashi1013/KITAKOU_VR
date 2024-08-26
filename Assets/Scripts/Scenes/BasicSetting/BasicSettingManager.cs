using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Constant;
using Utils;

namespace Scenes.BasicSetting
{
    public class BasicSettingManager : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _mortonModelButton;
        [SerializeField] private Toggle _useFlooding;

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
        }
    }
}
