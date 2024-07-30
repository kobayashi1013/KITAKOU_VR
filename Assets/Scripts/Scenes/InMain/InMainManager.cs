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
            //�C���X�^���X
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            //�v���C���[�X�|�[��
            if (SystemData.Instance.sceneMode == SceneMode.VR)
            {
                playerObject = Instantiate(_vrPlayerPrefab, _playerPosition.transform.position, Quaternion.identity);
            }
            else if (SystemData.Instance.sceneMode == SceneMode.PC)
            {
                playerObject = Instantiate(_pcPlayerPrefab, _playerPosition.transform.position, Quaternion.identity);
            }

            //�X�|�i�[�폜
            Destroy(_playerPosition);
        }
    }
}