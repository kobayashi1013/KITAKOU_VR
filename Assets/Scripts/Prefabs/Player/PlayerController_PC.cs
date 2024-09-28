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

        [Header("�ړ�")]
        [SerializeField] private float acceleration = 5.0f;
        [SerializeField] private float maxVelocity = 10.0f;
        [Header("��]")]
        [SerializeField] private float _rotationSensitive = 1.0f;

        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            //�v���C���[��]
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse X") * _rotationSensitive)
                .Subscribe(x => transform.Rotate(0, x, 0));

            //�v���C���[�ړ�
            this.FixedUpdateAsObservable()
                .Subscribe(_ => PlayerMove());
        }

        private void PlayerMove()
        {
            //����
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal == 0 && vertical == 0) //���͂Ȃ�
            {
                //�ړ�����
                Vector3 movement = _rigidbody.velocity.normalized * -1;

                //�ړ������ɗ͂�������
                _rigidbody.AddForce(movement * acceleration);

                //��~
                if (_rigidbody.velocity.magnitude < MIN_VELOCITY)
                {
                    //_rigidbody.velocity = new Vector3()
                }
            }
            else
            {
                //�ړ�����
                Vector3 direction = new Vector3(horizontal, 0.0f, vertical);
                Vector3 movement = transform.TransformDirection(direction);

                //�ړ������ɗ͂�������
                _rigidbody.AddForce(movement * acceleration);

                //���x����
                if (_rigidbody.velocity.magnitude > maxVelocity)
                {
                    _rigidbody.velocity = _rigidbody.velocity.normalized * maxVelocity;
                }
            }
        }
    }
}
