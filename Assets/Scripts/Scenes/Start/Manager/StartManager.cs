using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Constant;

namespace Scenes.Start.Manager
{
    public class StartManager : MonoBehaviour
    {
        private void Awake()
        {
            //システムデータ設定
            if (SystemData.Instance == null)
            {
                SystemData.Instance = new SystemData();
            }
        }

        public void PushVrButton()
        {
            //Debug.Log("Start VR Mode");

            //VRモード
            SystemData.Instance.SetSceneMode(SceneMode.VR);

            //メインシーンへ移動
            SceneManager.LoadScene((int)SceneName.InMainScene);
        }

        public void PushPcButton()
        {
            //Debug.Log("Start PC Mode");

            //PCモード
            SystemData.Instance.SetSceneMode(SceneMode.PC);

            //メインシーンへ移動
            SceneManager.LoadScene((int)SceneName.InMainScene);
        }
    }
}
