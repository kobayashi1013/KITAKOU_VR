using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Prefabs.Avater
{
    public class AvaterController : CharacterControllerPhysics
    {
        [SerializeField] private AvaterConfig _avaterConfig;

        private CharacterController _characterController;
        private Vector3 _externalForceVelocity = Vector3.zero;
        private float _isMove = 0f;

        private void Start()
        {
            //�R���|�[�l���g�擾
            _characterController = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => AvaterPhysics());

            //�A�N�V�����J�n
            Action();
        }

        private void AvaterPhysics()
        {
            Vector3 movement = Vector3.zero;
            movement += ExternalForce();

            _characterController.Move(movement * Time.deltaTime);
        }

        private async void Action()
        {
            while (true)
            {
                float probability = UnityEngine.Random.Range(0f, 1f);
                if (probability < 0.5) await Rotation();
                else await Move();
            }
        }

        /// <summary>
        /// �A�o�^�[�̉�]
        /// </summary>
        /// <returns></returns>
        private async UniTask Rotation()
        {
            //��]�p�x�̌���
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //��]
            float timer = 0f;
            while (timer < _avaterConfig.rotationTime)
            {
                if (this == null) break; //�G���[�n���h�����O
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _avaterConfig.rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //�����_�����ԑҋ@
            float waitingTime = UnityEngine.Random.Range(_avaterConfig.minWaitingTime, _avaterConfig.maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }

        private async UniTask Move()
        {
            //��]�p�x�̌���
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //��]
            float timer = 0f;
            while (timer < _avaterConfig.rotationTime)
            {
                if (this == null) break; //�G���[�n���h�����O
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _avaterConfig.rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //�ړ��ʒu�̌���
            Vector3 movement = randomRotation * Vector3.forward;

            //�ړ�
            timer = 0f;
            _isMove = 1f;
            while (timer < _avaterConfig.rotationTime)
            {
                if (this == null) break; //�G���[�n���h�����O
                timer += Time.deltaTime;
                _characterController.Move(movement * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _isMove = 0f;

            //�����_�����ԑҋ@
            float waitingTime = UnityEngine.Random.Range(_avaterConfig.minWaitingTime, _avaterConfig.maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }
    }
}
