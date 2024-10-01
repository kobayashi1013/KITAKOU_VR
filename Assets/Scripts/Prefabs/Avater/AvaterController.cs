using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Avater
{
    public class AvaterController : MonoBehaviour
    {
        [Header("�ڒn")]
        [SerializeField] private float _rayLength = 0f;
        [SerializeField] private LayerMask _rayMask;
        [Header("���̑�")]
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private float _externalForceDamping = 0f;

        private CharacterController _controller;
        private bool _isGrounded = false; //�ڒn����
        private float _gravitySpeed = 0f; //�d�͑��xy
        private Vector3 _externalForce = Vector3.zero; //�O�͑��x
        private Vector3 _velocity = Vector3.zero; //���x

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => AvaterPhysics()); //�A�o�^�[��������
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
        /// �A�o�^�[�̕����v�Z
        /// </summary>
        private void AvaterPhysics()
        {
            Vector3 velocity = Vector3.zero;
            velocity += UseGravity();
            velocity += AttenuationForce();

            //_controller.Move(velocity * Time.deltaTime);
            Debug.Log(velocity);
            _velocity = velocity;
        }

        /// <summary>
        /// �d��
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        private Vector3 AttenuationForce()
        {
            //�O�͌v�Z
            _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _externalForceDamping * Time.deltaTime);

            return _externalForce;
        }

        /// <summary>
        /// �O�͂̉��Z
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Vector3 force)
        {
            _externalForce += force;
        }
    }
}
