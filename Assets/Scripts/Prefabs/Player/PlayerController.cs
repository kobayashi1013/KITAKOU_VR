using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Prefabs.Avater;

namespace Prefabs.Player
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly float _gravity = 9.81f;
        private static readonly float _colliderRange = 0.8f; //プレイヤーの周囲ぶつかり範囲

        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [Header("Parameters")]
        [SerializeField] LayerMask _avaterLayer; //アバターのレイヤー
        [SerializeField] private float _playerRotationSensitive = 1.0f; //感度
        [SerializeField] private float _playerSpeed = 1.0f; //スピード

        private float _yVelocity = 0f;
        private List<AvaterController> _avaterControllerList = new List<AvaterController>();
        private float _collectionTime = 0f;

        void Start ()
        {
            //プレイヤー回転（左右）
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse X") * _playerRotationSensitive)
                .Subscribe(x => transform.Rotate(0, x, 0));

            //プレイヤー移動
            /*this.UpdateAsObservable()
                .Select(_ => UseGravity()) //重力
                .Select(x => new Vector3(Input.GetAxis("Horizontal"), x, Input.GetAxis("Vertical")) * _playerSpeed)
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));*/
            //プレイヤー移動
            this.UpdateAsObservable()
                .Select(_ => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")))
                .Select(x => x * _playerSpeed * UseDash())
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));

            //重力
            this.UpdateAsObservable()
                .Select(_ => UseGravity())
                .Select(x => new Vector3(0, x, 0))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));

            //周囲のアバターを押しのける
            this.FixedUpdateAsObservable()
                .Subscribe(x => AvaterMover(Time.fixedDeltaTime));
        }

        //重力
        private float UseGravity()
        {
            if (_characterController.isGrounded) _yVelocity = 0f; //地面
            else _yVelocity -= _gravity * Time.deltaTime; //空中

            return _yVelocity;
        }

        /// <summary>
        /// ダッシュを使う
        /// </summary>
        /// <returns></returns>
        private float UseDash()
        {
            if (Input.GetKey(KeyCode.LeftShift)) return 1.6f;
            else return 1.0f;
        }

        /// <summary>
        /// アバターを押しのける処理
        /// </summary>
        /// <param name="deltaTime"></param>　//インターバル時間
        private void AvaterMover(float deltaTime)
        {
            _collectionTime += deltaTime;

            if (_collectionTime >= 0.1f)
            {
                _collectionTime = 0f;
                _avaterControllerList.Clear();

                var hits = Physics.OverlapSphere(this.transform.position, _colliderRange, _avaterLayer);
                foreach (var hit in hits)
                {
                    _avaterControllerList.Add(hit.GetComponent<AvaterController>());
                }
            }

            foreach (var controller in _avaterControllerList)
            {
                var direction = new Vector3(
                    controller.transform.position.x - this.transform.position.x,
                    0f,
                    controller.transform.position.z - this.transform.position.z);

                controller.Move(direction.normalized, deltaTime);
            }
        }
    }
}