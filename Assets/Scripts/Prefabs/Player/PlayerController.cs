using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Constant;
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
            this.UpdateAsObservable()
                .Select(_ => PlayerMover())
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));

            //重力
            this.UpdateAsObservable()
                .Select(_ => UseGravity())
                .Select(x => new Vector3(0, x, 0))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));

            //周囲のアバターを押しのける
            this.UpdateAsObservable()
                .Subscribe(x => AvaterMove());
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
        private bool UseDash()
        {
            if (Input.GetKey(KeyCode.LeftShift)) return true;
            else return false;
        }

        private Vector3 PlayerMover()
        {
            //方向入力
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (direction.magnitude < 0.01f)
            {
                return Vector3.zero;
            }

            if (UseDash())
            {
                return direction * _playerSpeed * 1.8f;
            }

            return direction * _playerSpeed;
        }

        /// <summary>
        /// アバターを押しのける処理
        /// </summary>
        private void AvaterMove()
        {
            if (_collectionTime > 0.5f)
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

                controller.Move(direction.normalized * Time.deltaTime);
            }

            _collectionTime += Time.deltaTime;
        }
    }
}