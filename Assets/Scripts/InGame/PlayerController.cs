using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InScene
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [Header("Parameters")]
        [SerializeField] private float _playerRotationSensitive = 1.0f; //感度
        [SerializeField] private float _playerSpeed = 1.0f; //スピード

        void Start ()
        {
            //プレイヤー回転（左右）
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse X") * _playerRotationSensitive)
                .Subscribe(x => transform.Rotate(0, x, 0));

            //プレイヤー移動
            this.UpdateAsObservable()
                .Select(_ => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * _playerSpeed)
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));
        }
    }
}