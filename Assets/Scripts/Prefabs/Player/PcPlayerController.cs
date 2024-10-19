using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Player
{
    public class PcPlayerController : PlayerControllerBase
    {
        private CharacterController _characterController;
        private Vector2 _moveInput = Vector2.zero; //移動入力
        private Vector2 _rotateInput = Vector2.zero; //回転入力
        private float _gravitySpeed = 0f; //現在の重力速度
        private bool _dashInput = false; //ダッシュ入力
        private bool _isGrounded = false; //接地判定

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => PlayerRotate());
            this.UpdateAsObservable().Subscribe(_ => PlayerPhysics());

            this.FixedUpdateAsObservable().Subscribe(_ => IsGrounded());
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            _rotateInput = context.ReadValue<Vector2>();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed) _dashInput = true;
            if (context.canceled) _dashInput = false;
        }

        /// <summary>
        /// プレイヤー回転
        /// </summary>
        private void PlayerRotate()
        {
            float rotateY = _rotateInput.x * _playerConfig.rotationSensitive;
            transform.Rotate(0f, rotateY, 0f);
        }

        private void PlayerPhysics()
        {
            Vector3 movement = Vector3.zero;
            movement += PlayerMove();
            movement += GravityForce();
            movement += ExternalForce();

            _characterController.Move(movement * Time.deltaTime);
        }

        /// <summary>
        /// プレイヤー移動
        /// </summary>
        /// <returns></returns>
        private Vector3 PlayerMove()
        {
            Vector3 movement = Vector3.zero;
            Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y); //入力方向
            Vector3 localDirection = transform.TransformDirection(inputDirection); //方向を補正

            if (_dashInput == true) movement = localDirection * _playerConfig.dashSpeed; //ダッシュ
            else movement = localDirection * _playerConfig.walkSpeed;

            return movement;
        }

        /// <summary>
        /// 重力
        /// </summary>
        /// <returns></returns>
        private Vector3 GravityForce()
        {
            if (_isGrounded == false) _gravitySpeed -= _playerConfig.gravitySensitive * Time.deltaTime;
            Vector3 movement = new Vector3(0f, _gravitySpeed, 0f);
            return movement;
        }

        /// <summary>
        /// 接地判定
        /// </summary>
        private void IsGrounded()
        {
            var ray = new Ray(transform.position, Vector3.down);
            var result = Physics.Raycast(ray, 0.001f, _playerConfig.isGroundedMask);

            if (_isGrounded == false && result == true) _gravitySpeed = 0f;
            _isGrounded = result;
        }
    }
}
