using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Prefabs.Character.Avater
{
    public class AvaterController : CharacterPhysics
    {
        [SerializeField] private AvaterConfig _avaterConfig;

        private Rigidbody _rigidbody;
        private float _isMove = 0f;

        private void Start()
        {
            //コンポーネント取得
            _rigidbody = GetComponent<Rigidbody>();

            this.FixedUpdateAsObservable().Subscribe(_ => AvaterPhysics());

            //アクション開始
            //Action();
        }

        private void AvaterPhysics()
        {
            /*Vector3 movement = Vector3.zero;

            movement += ExternalForce(Time.fixedDeltaTime);

            _rigidbody.MovePosition(_rigidbody.position + movement);*/
            // 現在の位置と目標位置を補間する
            Vector3 newPosition = Vector3.Lerp(_rigidbody.position, _rigidbody.position + new Vector3(5, 0, 0), Time.fixedDeltaTime);

            // MovePositionを使って移動
            _rigidbody.MovePosition(newPosition);
            //Debug.Log(movement);
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
                timer += Time.fixedDeltaTime;
                _rigidbody.MovePosition(_rigidbody.position + movement * Time.fixedDeltaTime);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
            _isMove = 0f;

            //ランダム時間待機
            float waitingTime = UnityEngine.Random.Range(_avaterConfig.minWaitingTime, _avaterConfig.maxWaitingTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent<CharacterPhysics>(out var characterPhysics) == false) return;

            Vector3 direction = collision.contacts[0].normal * -1;
            characterPhysics.AddForce(direction * _isMove * characterPhysicsConfig.addForceSensitive);
        }
    }
}
