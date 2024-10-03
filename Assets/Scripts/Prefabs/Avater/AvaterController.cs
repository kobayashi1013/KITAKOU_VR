using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Prefabs.Avater
{
    public class AvaterController : MonoBehaviour
    {
        [Header("��]")]
        [SerializeField] private float _rotationTime = 0f;
        [SerializeField] private float _minWaitingTime = 0f;
        [SerializeField] private float _maxWaitingTime = 0f;

        private void Start()
        {
            Action(); //�A�N�V�����J�n
        }

        private async void Action()
        {
            while (true)
            {
                await Rotation();
            }
        }

        private async UniTask Rotation()
        {
            //��]�p�x�̌���
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //�v���C���[�̉�]
            float timer = 0f;
            while (timer < _rotationTime)
            {
                if (this == null) break; //�G���[�n���h�����O
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //�����_�����ԑҋ@
            float waitingTime = UnityEngine.Random.Range(_minWaitingTime, _maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }
    }
}
