using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Player
{
    public class PlayerController_PC : MonoBehaviour
    {
        private static readonly float MIN_VELOCITY = 0.5f;

        [Header("移動")]
        [SerializeField] private float acceleration = 5.0f;
        [SerializeField] private float maxVelocity = 10.0f;
        [Header("回転")]
        [SerializeField] private float _rotationSensitive = 1.0f;

        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            //プレイヤー回転
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse X") * _rotationSensitive)
                .Subscribe(x => transform.Rotate(0, x, 0));

            //プレイヤー移動
            this.FixedUpdateAsObservable()
                .Subscribe(_ => PlayerMove());
        }

        private void PlayerMove()
        {
            //入力
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal == 0 && vertical == 0) //入力なし
            {
                //移動方向
                Vector3 movement = _rigidbody.velocity.normalized * -1;

                //移動方向に力を加える
                _rigidbody.AddForce(movement * acceleration);

                //停止
                if (_rigidbody.velocity.magnitude < MIN_VELOCITY)
                {
                    //_rigidbody.velocity = new Vector3()
                }
            }
            else
            {
                //移動方向
                Vector3 direction = new Vector3(horizontal, 0.0f, vertical);
                Vector3 movement = transform.TransformDirection(direction);

                //移動方向に力を加える
                _rigidbody.AddForce(movement * acceleration);

                //速度制限
                if (_rigidbody.velocity.magnitude > maxVelocity)
                {
                    _rigidbody.velocity = _rigidbody.velocity.normalized * maxVelocity;
                }
            }
        }
    }
}
