using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using TMPro;
using Prefabs.Avater;

namespace Prefabs.Player
{
    public class PlayerController_PC : MonoBehaviour
    {
        [Header("�ړ����x")]
        [SerializeField] private float _walkSpeed = 0f;
        [SerializeField] private float _dashSpeed = 0f;
        [Header("�ڒn")]
        [SerializeField] private float _rayLength = 0f;
        [SerializeField] private LayerMask _rayMask;
        [Header("���̑�")]
        [SerializeField] private float _rotationSensitive = 0f;
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private float _externnalForceDamping = 0f;
        [SerializeField] private float _addForceSensitive = 0f;

        private CharacterController _controller;
        private bool _isGrounded = false; //�ڒn����
        private float _gravitySpeed = 0f; //�d�͑���y
        private Vector3 _externalForce = Vector3.zero; //�O�͑��x
        private Vector3 _movement = Vector3.zero; //����
        private Vector3 _prevPosition = Vector3.zero; //�O�t���[���̈ʒu
        private float _playerSpeed = 0f; //�v���C���[�̎��s����

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _prevPosition = transform.position;

            this.UpdateAsObservable().Subscribe(_ => PlayerRotation()); //�v���C���[��]
            this.UpdateAsObservable().Subscribe(_ => PlayerPhysics()); //�v���C���[��������
            this.UpdateAsObservable().Subscribe(_ => PlayerSpeed()); //�v���C���[�̑������v��
            this.FixedUpdateAsObservable().Subscribe(_ => IsGround()); //�ڒn����

            this.ObserveEveryValueChanged(_ => _isGrounded) //�d�͑��x���Z�b�g
                .Pairwise()
                .Where(pair => !pair.Previous && pair.Current)
                .Subscribe(_ => _gravitySpeed = 0f);
        }

        /// <summary>
        /// �ڒn�𔻒肷��
        /// </summary>
        /// <returns></returns>
        private void IsGround()
        {
            var ray = new Ray(transform.position, Vector3.down);
            _isGrounded = Physics.Raycast(ray, _rayLength, _rayMask);
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
        /// �v���C���[����
        /// </summary>
        private void PlayerPhysics()
        {
            Vector3 movement = Vector3.zero;
            movement += PlayerMove();
            movement += UseGravity();
            movement += AttenuationForce();

            _controller.Move(movement * Time.deltaTime);
            _movement = movement;
        }

        /// <summary>
        /// �����v�Z
        /// </summary>
        private void PlayerSpeed()
        {
            //���x
            Vector3 velocity = transform.position - _prevPosition;
            _prevPosition = transform.position;

            //����
            float distance = velocity.magnitude;
            float speed = distance / Time.deltaTime;
            _playerSpeed = speed;
        }

        /// <summary>
        /// �v���C���[�ړ�
        /// </summary>
        private Vector3 PlayerMove()
        {
            //����
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool dashInput = Input.GetKey(KeyCode.LeftShift);

            //�ړ����x
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized; //���͕���
            Vector3 playerDirection = transform.TransformDirection(inputDirection); //�v���C���[�����ɕ␳
            Vector3 movement = Vector3.zero; //�X�s�[�h����
            if (dashInput == true) movement = playerDirection * _dashSpeed;
            else movement = playerDirection * _walkSpeed;

            return movement;
        }

        /// <summary>
        /// �d��
        /// </summary>
        private Vector3 UseGravity()
        {
            //�d�͌v�Z
            if (_isGrounded == false) _gravitySpeed -= _gravity * Time.deltaTime;

            //�������x
            Vector3 movement = new Vector3(0, _gravitySpeed, 0);

            return movement;
        }

        /// <summary>
        /// �O�͌����̌v�Z
        /// </summary>
        private Vector3 AttenuationForce()
        {
            //�O�͌v�Z
            _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _externnalForceDamping * Time.deltaTime);

            return _externalForce;
        }

        /// <summary>
        /// �O�͂̉��Z
        /// </summary>
        /// <param name="force"></param>
        private void AddForce(Vector3 force)
        {
            _externalForce += force;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hit.gameObject.CompareTag("Avater")) return;
            if (!hit.gameObject.TryGetComponent<Rigidbody>(out var rigidbody)) return;

            rigidbody.AddForce(_movement * _addForceSensitive, ForceMode.Impulse);
        }
    }
}
