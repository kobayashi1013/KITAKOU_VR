using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Avater
{
    public class AvaterController : MonoBehaviour
    {
        [Header("接地")]
        [SerializeField] private float _rayLength = 0f;
        [SerializeField] private LayerMask _rayMask;
        [Header("その他")]
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private float _externalForceDamping = 0f;

        private CharacterController _controller;
        private bool _isGrounded = false; //接地判定
        private float _gravitySpeed = 0f; //重力速度y
        private Vector3 _externalForce = Vector3.zero; //外力速度
        private Vector3 _velocity = Vector3.zero; //速度

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => AvaterPhysics()); //アバター物理挙動
            this.FixedUpdateAsObservable().Subscribe(_ => IsGround()); //接地判定

            this.ObserveEveryValueChanged(_ => _isGrounded) //重力速度リセット
                .Pairwise()
                .Where(pair => !pair.Previous && pair.Current)
                .Subscribe(_ => _gravitySpeed = 0f);
        }

        /// <summary>
        /// 接地を判定する
        /// </summary>
        /// <returns></returns>
        private void IsGround()
        {
            var ray = new Ray(transform.position, Vector3.down);
            _isGrounded = Physics.Raycast(ray, _rayLength, _rayMask);
        }

        /// <summary>
        /// アバターの物理計算
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
        /// 重力
        /// </summary>
        /// <returns></returns>
        private Vector3 UseGravity()
        {
            //重力計算
            if (_isGrounded == false) _gravitySpeed -= _gravity * Time.deltaTime;

            //落下速度
            Vector3 movement = new Vector3(0, _gravitySpeed, 0);

            return movement;
        }

        /// <summary>
        /// 外力減衰の計算
        /// </summary>
        /// <returns></returns>
        private Vector3 AttenuationForce()
        {
            //外力計算
            _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _externalForceDamping * Time.deltaTime);

            return _externalForce;
        }

        /// <summary>
        /// 外力の加算
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Vector3 force)
        {
            _externalForce += force;
        }
    }
}
