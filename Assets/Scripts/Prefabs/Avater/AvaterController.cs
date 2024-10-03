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
        [Header("回転")]
        [SerializeField] private float _rotationTime = 0f;
        [SerializeField] private float _minWaitingTime = 0f;
        [SerializeField] private float _maxWaitingTime = 0f;

        private void Start()
        {
            Action(); //アクション開始
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
            //回転角度の決定
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //プレイヤーの回転
            float timer = 0f;
            while (timer < _rotationTime)
            {
                if (this == null) break; //エラーハンドリング
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //ランダム時間待機
            float waitingTime = UnityEngine.Random.Range(_minWaitingTime, _maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }
    }
}
