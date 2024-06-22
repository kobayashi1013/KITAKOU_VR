using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs
{
    public class SetAvater : MonoBehaviour
    {
        private static readonly int PLANE_SCALE = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject _avaterPrefab;
        [Header("Parameters")]
        [SerializeField] private float _xWidth = 1f;
        [SerializeField] private float _zWidth = 1f;

        private Vector3 _basePoint;
        private float _xLength = 0f;
        private float _zLength = 0f;

        private void Start()
        {
            _basePoint = new Vector3(
                transform.position.x - transform.localScale.x / 2 * PLANE_SCALE,
                transform.position.y,
                transform.position.z - transform.localScale.z / 2 * PLANE_SCALE);

            _xLength = _xWidth / 2;
            _zLength = _zWidth / 2;

            while (_xLength < transform.localScale.x * PLANE_SCALE)
            {
                _zLength = _zWidth / 2;
                while (_zLength < transform.localScale.z * PLANE_SCALE)
                {
                    var position = new Vector3(_basePoint.x + _xLength, _basePoint.y, _basePoint.z + _zLength);
                    Instantiate(_avaterPrefab, position, Quaternion.identity, transform.parent);

                    _zLength += _zWidth;
                }

                _xLength += _xWidth;

                Destroy(this.gameObject);
            }
        }
    }
}
