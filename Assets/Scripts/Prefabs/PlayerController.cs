using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Prefabs.Avater;

namespace Prefabs
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] LayerMask layer;
        private static readonly float _gravity = 9.81f;
        private static readonly float _colliderRange = 0.8f; //�v���C���[�̎��͂Ԃ���͈�

        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [Header("Parameters")]
        [SerializeField] private float _playerRotationSensitive = 1.0f; //���x
        [SerializeField] private float _playerSpeed = 1.0f; //�X�s�[�h

        private float _yVelocity = 0f;
        private List<AvaterController> _avaterControllerList = new List<AvaterController>();
        private float _collectionTime = 0f;

        void Start ()
        {
            //�v���C���[��]�i���E�j
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse X") * _playerRotationSensitive)
                .Subscribe(x => transform.Rotate(0, x, 0));

            //�v���C���[�ړ�
            this.UpdateAsObservable()
                .Select(_ => UseGravity()) //�d��
                .Select(x => new Vector3(Input.GetAxis("Horizontal"), x, Input.GetAxis("Vertical")) * _playerSpeed)
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));

            //���͂̃A�o�^�[�������̂���
            this.FixedUpdateAsObservable()
                .Subscribe(x => AvaterMover(Time.fixedDeltaTime));
        }

        //�d��
        private float UseGravity()
        {
            if (_characterController.isGrounded) _yVelocity = 0f; //�n��
            else _yVelocity -= _gravity * Time.deltaTime; //��

            return _yVelocity;
        }

        /// <summary>
        /// �A�o�^�[�������̂��鏈��
        /// </summary>
        /// <param name="deltaTime"></param>�@//�C���^�[�o������
        private void AvaterMover(float deltaTime)
        {
            _collectionTime += deltaTime;

            if (_collectionTime >= 0.1f)
            {
                _collectionTime = 0f;
                _avaterControllerList.Clear();

                var hits = Physics.OverlapSphere(this.transform.position, _colliderRange, layer);
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