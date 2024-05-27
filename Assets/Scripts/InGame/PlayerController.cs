using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InGame
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [Header("Parameters")]
        [SerializeField] private float _playerSpeed = 1.0f;

        void Start ()
        {
            this.UpdateAsObservable()
                .Select(_ => new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")))
                .Select(x => x * _playerSpeed)
                .Subscribe(x => _characterController.Move(x * Time.deltaTime));
        }
    }
}