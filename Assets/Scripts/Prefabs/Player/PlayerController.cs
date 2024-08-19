using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Prefabs.Avater;

namespace Prefabs.Player
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly float _gravity = 9.81f;
        private static readonly float _colliderRange = 0.8f; //�v���C���[�̎��͂Ԃ���͈�

        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [Header("Parameters")]
        [SerializeField] LayerMask _avaterLayer; //�A�o�^�[�̃��C���[
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
            /*this.UpdateAsObservable()
                .Select(_ => UseGravity()) //�d��
                .Select(x => new Vector3(Input.GetAxis("Horizontal"), x, Input.GetAxis("Vertical")) * _playerSpeed)
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));*/
            //�v���C���[�ړ�
            this.UpdateAsObservable()
                .Select(_ => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")))
                .Select(x => x * _playerSpeed * UseDash())
                .Select(x => transform.TransformDirection(x))
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));

            //�d��
            this.UpdateAsObservable()
                .Select(_ => UseGravity())
                .Select(x => new Vector3(0, x, 0))
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
        /// �_�b�V�����g��
        /// </summary>
        /// <returns></returns>
        private float UseDash()
        {
            if (Input.GetKey(KeyCode.LeftShift)) return 1.6f;
            else return 1.0f;
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

                controller.Move(direction.normalized, deltaTime);
            }
        }
    }
}