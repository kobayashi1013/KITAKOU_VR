using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using TMPro;
using Prefabs.Avater;

namespace Prefabs.Player
{
    public class PlayerController_PC : MonoBehaviour
    {
        [Header("移動速度")]
        [SerializeField] private float _walkSpeed = 0f;
        [SerializeField] private float _dashSpeed = 0f;
        [Header("接地")]
        [SerializeField] private float _rayLength = 0f;
        [SerializeField] private LayerMask _rayMask;
        [Header("その他")]
        [SerializeField] private float _rotationSensitive = 0f;
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private float _externnalForceDamping = 0f;
        [SerializeField] private float _addForceSensitive = 0f;

        private CharacterController _controller;
        private bool _isGrounded = false; //接地判定
        private float _gravitySpeed = 0f; //重力速さy
        private Vector3 _externalForce = Vector3.zero; //外力速度
        private Vector3 _movement = Vector3.zero; //動作
        private Vector3 _prevPosition = Vector3.zero; //前フレームの位置
        private float _playerSpeed = 0f; //プレイヤーの実行速さ

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _prevPosition = transform.position;

            this.UpdateAsObservable().Subscribe(_ => PlayerRotation()); //プレイヤー回転
            this.UpdateAsObservable().Subscribe(_ => PlayerPhysics()); //プレイヤー物理挙動
            this.UpdateAsObservable().Subscribe(_ => PlayerSpeed()); //プレイヤーの速さを計測
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
        /// プレイヤー回転
        /// </summary>
        private void PlayerRotation()
        {
            //入力
            float mouseX = Input.GetAxis("Mouse X");

            //回転速度
            float rotateY = mouseX * _rotationSensitive;

            //回転
            transform.Rotate(0, rotateY, 0);
        }

        /// <summary>
        /// プレイヤー物理
        /// </summary>
        private void PlayerPhysics()
        {
            Vector3 movement = Vector3.zero;
            movement += PlayerMove();
            movement += UseGravity();
            movement += AttenuationForce();

            _controller.Move(movement * Time.deltaTime);
            _movement = movement;
        }

        /// <summary>
        /// 速さ計算
        /// </summary>
        private void PlayerSpeed()
        {
            //速度
            Vector3 velocity = transform.position - _prevPosition;
            _prevPosition = transform.position;

            //速さ
            float distance = velocity.magnitude;
            float speed = distance / Time.deltaTime;
            _playerSpeed = speed;
        }

        /// <summary>
        /// プレイヤー移動
        /// </summary>
        private Vector3 PlayerMove()
        {
            //入力
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool dashInput = Input.GetKey(KeyCode.LeftShift);

            //移動速度
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized; //入力方向
            Vector3 playerDirection = transform.TransformDirection(inputDirection); //プレイヤー方向に補正
            Vector3 movement = Vector3.zero; //スピード調整
            if (dashInput == true) movement = playerDirection * _dashSpeed;
            else movement = playerDirection * _walkSpeed;

            return movement;
        }

        /// <summary>
        /// 重力
        /// </summary>
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
        private Vector3 AttenuationForce()
        {
            //外力計算
            _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _externnalForceDamping * Time.deltaTime);

            return _externalForce;
        }

        /// <summary>
        /// 外力の加算
        /// </summary>
        /// <param name="force"></param>
        private void AddForce(Vector3 force)
        {
            _externalForce += force;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hit.gameObject.CompareTag("Avater")) return;
            if (!hit.gameObject.TryGetComponent<Rigidbody>(out var rigidbody)) return;

            rigidbody.AddForce(_movement * _addForceSensitive, ForceMode.Impulse);
        }
    }
}
