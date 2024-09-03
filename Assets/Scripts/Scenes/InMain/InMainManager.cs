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
        [SerializeField] private GameObject _playerPosition;
        [SerializeField] private GameObject _vrPlayerPrefab;
        [SerializeField] private GameObject _pcPlayerPrefab;
        [Header("SceneObjects")]
        [SerializeField] private GameObject _floodingSystem;
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private GameObject _doorObjects;

        public static InMainManager Instance;
        public GameObject playerObject { get; private set; }

        private void Start()
        {
            //インスタンス
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            //洪水の有無
            _floodingSystem.SetActive(SystemData.Instance.useFlooding);
            _eventSystem.SetActive(SystemData.Instance.useEvent);
            _doorObjects.SetActive(SystemData.Instance.useRestrictedArea);

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
