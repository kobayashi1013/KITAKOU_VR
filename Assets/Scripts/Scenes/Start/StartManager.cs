using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Constant;
using UnityEngine.XR.Management;

namespace Scenes.Start
{
    public class StartManager : MonoBehaviour
    {
        private void Awake()
        {
            //システムデータ設定
            if (SystemData.Instance == null)
            {
                SystemData.Instance = new SystemData();

                //XRの無効化
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }

        public void PushVrButton()
        {
            StartCoroutine(StartVR());
        }

        public IEnumerator StartVR()
        {
            //Debug.Log("Start VR Mode");

            //XRの有効化
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();

                //VRモード
                SystemData.Instance.SetSceneMode(SceneMode.VR);

                //メインシーンへ移動
                SceneManager.LoadScene((int)SceneName.InMainScene);
            }
        }

        public void PushPcButton()
        {
            //Debug.Log("Start PC Mode");

            //PCモード
            SystemData.Instance.SetSceneMode(SceneMode.PC);

            //メインシーンへ移動
            SceneManager.LoadScene((int)SceneName.InMainScene);
        }

        public void PushSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.BasicSettingScene);
        }
    }
}
