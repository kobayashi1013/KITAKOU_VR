using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Constant;

namespace Scenes.InScene.Manager
{
    public class InSceneManager : MonoBehaviour
    {
        public GameObject _vrPlayerPrefab;
        public GameObject _pcPlayerPrefab;

        void Start()
        {
            //プレイヤースポーン
            if (SystemData.Instance.sceneMode == SceneMode.VR)
            {
                Instantiate(_vrPlayerPrefab, Vector3.zero, Quaternion.identity);
            }
            else if (SystemData.Instance.sceneMode == SceneMode.PC)
            {
                Instantiate(_pcPlayerPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }
}
