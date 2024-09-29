using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using TMPro;

namespace Prefabs.Player
{
    public class PlayerController_PC : MonoBehaviour
    {
        [Header("移動速度")]
        [SerializeField] private float _walkSpeed = 1.0f;
        [SerializeField] private float _dashSpeed = 1.0f;
        [Header("接地")]
        [SerializeField] private float _rayLength = 1.0f;
        [SerializeField] private LayerMask _rayMask;
        [Header("その他")]
        [SerializeField] private float _rotationSensitive = 1.0f;
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private float _externnalForceDamping = 1.0f;

        private CharacterController _controller;
        private bool _isGrounded = false;
        private float _velocityY = 0f;
        private Vector3 _externalForce = Vector3.zero;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => PlayerRotation());
            this.UpdateAsObservable().Subscribe(_ => PlayerMove());
            this.UpdateAsObservable().Subscribe(_ => UseGravity());
            this.UpdateAsObservable().Subscribe(_ => AttenuationForce());

            this.FixedUpdateAsObservable().Subscribe(_ => IsGround());

            this.ObserveEveryValueChanged(x => x._isGrounded)
                .Where(x => x)
                .Subscribe(x => _velocityY = 0f);
        }

        /// <summary>
        /// 接地を判定する
        /// </summary>
        /// <returns></returns>
        private void IsGround()
        {
            var ray = new Ray(transform.position, Vector3.down);
            _isGrounded =  Physics.Raycast(ray, _rayLength, _rayMask);
        }

        /// <summary>
        /// プレイヤー移動
        /// </summary>
        private void PlayerMove()
        {
            //入力
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool dashInput = Input.GetKey(KeyCode.LeftShift);

            //移動速度
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
            Vector3 playerDirection = transform.TransformDirection(inputDirection);
            Vector3 movement = Vector3.zero;
            if (dashInput == true) movement = playerDirection * _dashSpeed;
            else movement = playerDirection * _walkSpeed;

            //移動
            _controller.Move(movement * Time.deltaTime);
        }

        /// <summary>
        /// プレイヤー回転
        /// </summary>
        private void PlayerRotation()
        {
            //入力
            float mouseX = Input.GetAxis("Mouse X");

            //回転速度
            float rotateY = mouseX * _rotationSensitive;

            //回転
            transform.Rotate(0, rotateY, 0);
        }

        /// <summary>
        /// 重力
        /// </summary>
        private void UseGravity()
        {
            //重力計算
            if (_isGrounded == false) _velocityY -= _gravity * Time.deltaTime;

            //落下速度
            Vector3 movement = new Vector3(0, _velocityY, 0);

            //落下
            _controller.Move(movement * Time.deltaTime);
        }

        /// <summary>
        /// 外力減衰の計算
        /// </summary>
        private void AttenuationForce()
        {
            //外力計算
            _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _externnalForceDamping * Time.deltaTime);

            //移動
            _controller.Move(_externalForce * Time.deltaTime);
        }

        /// <summary>
        /// 外力の加算
        /// </summary>
        /// <param name="force"></param>
        private void AddForce(Vector3 force)
        {
            _externalForce += new Vector3(force.x, 0f, force.z);
            if (_isGrounded) _velocityY += force.y;
        }
    }
}
