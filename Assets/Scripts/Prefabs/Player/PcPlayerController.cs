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
        private Vector2 _moveInput = Vector2.zero; //�ړ�����
        private Vector2 _rotateInput = Vector2.zero; //��]����
        private float _gravitySpeed = 0f; //���݂̏d�͑��x
        private bool _dashInput = false; //�_�b�V������
        private bool _isGrounded = false; //�ڒn����

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
        /// �v���C���[��]
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
        /// �v���C���[�ړ�
        /// </summary>
        /// <returns></returns>
        private Vector3 PlayerMove()
        {
            Vector3 movement = Vector3.zero;
            Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y); //���͕���
            Vector3 localDirection = transform.TransformDirection(inputDirection); //������␳

            if (_dashInput == true) movement = localDirection * _playerConfig.dashSpeed; //�_�b�V��
            else movement = localDirection * _playerConfig.walkSpeed;

            return movement;
        }

        /// <summary>
        /// �d��
        /// </summary>
        /// <returns></returns>
        private Vector3 GravityForce()
        {
            if (_isGrounded == false) _gravitySpeed -= _playerConfig.gravitySensitive * Time.deltaTime;
            Vector3 movement = new Vector3(0f, _gravitySpeed, 0f);
            return movement;
        }

        /// <summary>
        /// �ڒn����
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
