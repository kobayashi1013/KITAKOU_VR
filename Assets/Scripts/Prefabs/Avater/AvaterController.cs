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
            //コンポーネント取得
            _characterController = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => AvaterPhysics());

            //アクション開始
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
            while (timer < _avaterConfig.rotationTime)
            {
                if (this == null) break; //エラーハンドリング
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _avaterConfig.rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //ランダム時間待機
            float waitingTime = UnityEngine.Random.Range(_avaterConfig.minWaitingTime, _avaterConfig.maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }

        private async UniTask Move()
        {
            //回転角度の決定
            Quaternion currentRotation = transform.rotation;
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            //回転
            float timer = 0f;
            while (timer < _avaterConfig.rotationTime)
            {
                if (this == null) break; //エラーハンドリング
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(currentRotation, randomRotation, timer / _avaterConfig.rotationTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            //移動位置の決定
            Vector3 movement = randomRotation * Vector3.forward;

            //移動
            timer = 0f;
            _isMove = 1f;
            while (timer < _avaterConfig.rotationTime)
            {
                if (this == null) break; //エラーハンドリング
                timer += Time.deltaTime;
                _characterController.Move(movement * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _isMove = 0f;

            //ランダム時間待機
            float waitingTime = UnityEngine.Random.Range(_avaterConfig.minWaitingTime, _avaterConfig.maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }
    }
}
