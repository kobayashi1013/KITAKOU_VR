using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Prefabs.Player;
using UnityEngine;

namespace Prefabs.Avater
{
    public class AvaterController : MonoBehaviour
    {
        [Header("時間")]
        [SerializeField] private float _rotationTime = 0f;
        [SerializeField] private float _moveTime = 0f;
        [SerializeField] private float _minWaitingTime = 0f;
        [SerializeField] private float _maxWaitingTime = 0f;

        private Rigidbody _rigidbody;

        private void Start()
        {
            //コンポーネント取得
            _rigidbody = GetComponent<Rigidbody>();

            //アクション開始
            Action();
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
        /// アバターの回転
        /// </summary>
        /// <returns></returns>
        private async UniTask Rotation()
        {
            //回転角度の決定
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //回転
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

        private async UniTask Move()
        {
            //回転角度の決定
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //回転
            float timer = 0f;
            while (timer < _rotationTime)
            {
                if (this == null) break; //エラーハンドリング
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //移動位置の決定
            Vector3 movement = randomRotation * Vector3.forward;

            //移動
            timer = 0f;
            while (timer < _rotationTime)
            {
                if (this == null) break; //エラーハンドリング
                timer += Time.deltaTime;
                _rigidbody.MovePosition(_rigidbody.position + movement * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //ランダム時間待機
            float waitingTime = UnityEngine.Random.Range(_minWaitingTime, _maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            if (!collision.gameObject.TryGetComponent<PlayerController_PC>(out var playerController)) return;

            Vector3 direction = collision.contacts[0].normal * -1;
            playerController.AddForce(direction);
        }
    }
}
