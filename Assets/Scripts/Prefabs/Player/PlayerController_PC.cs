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
        [Header("�ړ����x")]
        [SerializeField] private float _walkSpeed = 1.0f;
        [SerializeField] private float _dashSpeed = 1.0f;
        [Header("�ڒn")]
        [SerializeField] private float _rayLength = 1.0f;
        [SerializeField] private LayerMask _rayMask;
        [Header("���̑�")]
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
        /// �ڒn�𔻒肷��
        /// </summary>
        /// <returns></returns>
        private void IsGround()
        {
            var ray = new Ray(transform.position, Vector3.down);
            _isGrounded =  Physics.Raycast(ray, _rayLength, _rayMask);
        }

        /// <summary>
        /// �v���C���[�ړ�
        /// </summary>
        private void PlayerMove()
        {
            //����
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool dashInput = Input.GetKey(KeyCode.LeftShift);

            //�ړ����x
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
            Vector3 playerDirection = transform.TransformDirection(inputDirection);
            Vector3 movement = Vector3.zero;
            if (dashInput == true) movement = playerDirection * _dashSpeed;
            else movement = playerDirection * _walkSpeed;

            //�ړ�
            _controller.Move(movement * Time.deltaTime);
        }

        /// <summary>
        /// �v���C���[��]
        /// </summary>
        private void PlayerRotation()
        {
            //����
            float mouseX = Input.GetAxis("Mouse X");

            //��]���x
            float rotateY = mouseX * _rotationSensitive;

            //��]
            transform.Rotate(0, rotateY, 0);
        }

        /// <summary>
        /// �d��
        /// </summary>
        private void UseGravity()
        {
            //�d�͌v�Z
            if (_isGrounded == false) _velocityY -= _gravity * Time.deltaTime;

            //�������x
            Vector3 movement = new Vector3(0, _velocityY, 0);

            //����
            _controller.Move(movement * Time.deltaTime);
        }

        /// <summary>
        /// �O�͌����̌v�Z
        /// </summary>
        private void AttenuationForce()
        {
            //�O�͌v�Z
            _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _externnalForceDamping * Time.deltaTime);

            //�ړ�
            _controller.Move(_externalForce * Time.deltaTime);
        }

        /// <summary>
        /// �O�͂̉��Z
        /// </summary>
        /// <param name="force"></param>
        private void AddForce(Vector3 force)
        {
            _externalForce += new Vector3(force.x, 0f, force.z);
            if (_isGrounded) _velocityY += force.y;
        }
    }
}
