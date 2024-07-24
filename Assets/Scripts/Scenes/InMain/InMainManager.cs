using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Constant;

namespace Scenes.InMain
{
    public class InMainManager : MonoBehaviour
    {
        [Header("Players")]
        public GameObject _playerPosition;
        public GameObject _vrPlayerPrefab;
        public GameObject _pcPlayerPrefab;

        public static InMainManager Instance;
        public GameObject playerObject { get; private set; }

        private void Start()
        {
            //インスタンス
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            //プレイヤースポーン
            if (SystemData.Instance.sceneMode == SceneMode.VR)
            {
                playerObject = Instantiate(_vrPlayerPrefab, _playerPosition.transform.position, Quaternion.identity);
            }
            else if (SystemData.Instance.sceneMode == SceneMode.PC)
            {
                playerObject = Instantiate(_pcPlayerPrefab, _playerPosition.transform.position, Quaternion.identity);
            }

            //スポナー削除
            Destroy(_playerPosition);
        }
    }
}
