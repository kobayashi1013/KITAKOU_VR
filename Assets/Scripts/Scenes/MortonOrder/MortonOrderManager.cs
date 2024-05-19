using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.MortonOrder
{
    public class MortonOrderManager : MonoBehaviour
    {
        private enum MortonOrderDepth
        {
            depth2 = 2,
            depth4 = 4,
            depth8 = 8,
        }

        [Header("SceneObjects")]
        [SerializeField] private GameObject _player;
        [Header("Parameters")]
        [SerializeField] private MortonOrderDepth _depth = MortonOrderDepth.depth8;

        private Vector3 _downPos;
        private Vector3 _upPos;

        void Start()
        {
            _downPos = transform.position - transform.localScale / 2;
            _upPos = transform.position + transform.localScale / 2;
        }

        void Update()
        {
            var playerPos = new Vector3(
                (_player.transform.position.x - _downPos.x) / transform.localScale.x * (int)_depth,
                (_player.transform.position.y - _downPos.y) / transform.localScale.y * (int)_depth,
                (_player.transform.position.z - _downPos.z) / transform.localScale.z * (int)_depth);

            Debug.Log(playerPos);
        }

        private void OrderNumber(Vector3 playerPos)
        {

        }
    }
}
