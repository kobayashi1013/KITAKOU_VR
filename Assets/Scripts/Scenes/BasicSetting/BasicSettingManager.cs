using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Constant;

namespace Scenes.BasicSetting
{
    public class BasicSettingManager : MonoBehaviour
    {
        public void PushLocationSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.LocationSettingScene);
        }

        public void PushBackButton()
        {
            SceneManager.LoadScene((int)SceneName.StartScene);
        }
    }
}
