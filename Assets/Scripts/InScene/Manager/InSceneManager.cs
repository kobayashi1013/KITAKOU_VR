using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Constant;

namespace Scenes.InScene.Manager
{
    public class InSceneManager : MonoBehaviour
    {
        [Header("Players")]
        public GameObject _playerPosition;
        public GameObject _vrPlayerPrefab;
        public GameObject _pcPlayerPrefab;

        void Start()
        {
            //プレイヤースポーン
            if (SystemData.Instance.sceneMode == SceneMode.VR)
            {
                Instantiate(_vrPlayerPrefab, _playerPosition.transform.position, Quaternion.identity);
            }
            else if (SystemData.Instance.sceneMode == SceneMode.PC)
            {
                Instantiate(_pcPlayerPrefab, _playerPosition.transform.position, Quaternion.identity);
            }
        }
    }
}
