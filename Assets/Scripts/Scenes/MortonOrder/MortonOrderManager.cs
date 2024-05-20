using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Scenes.MortonOrder
{
    public class MortonOrderManager : MonoBehaviour
    {
        private enum MortonOrderDepth
        {
            depth2 = 2,
            depth4 = 4,
            depth8 = 8,
            depth16 = 16,
            depth32 = 32,
            depth64 = 64,
        }
 
        [Header("SceneObjects")]
        [SerializeField] private GameObject _player;
        [Header("Prefabs")]
        [SerializeField] private GameObject _avaterPrefab;
        [SerializeField] private GameObject _test;
        [Header("Parameters")]
        [SerializeField] private MortonOrderDepth _depth = MortonOrderDepth.depth8;

        private Vector3 _downPos; //���f���̒[
        private Vector3 _upPos;
        private int _prevOrderNumber = -1;
        private List<int> _prevOrderNumbers = new List<int>();
        private Dictionary<int, List<Vector3>> _otherPositionSet = new Dictionary<int, List<Vector3>>(); //Other�̍��W�W��
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>();
        private ObjectPool<GameObject> _avaterPool;

        void Awake()
        {
            //�I�u�W�F�N�g�v�[���ݒ�
            _avaterPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_avaterPrefab),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));

            //Other���W�W���̃L�[�ݒ�
            for (int i = 0; i < Mathf.Pow((int)_depth, 3); i++)
            {
                _otherPositionSet.Add(i, new List<Vector3>());
            }
        }

        void Start()
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    Instantiate(_test, new Vector3(2 * i, 0, 2 * j), Quaternion.identity);
                }
            }
            //��Ԓ�`
            _downPos = transform.position - transform.localScale / 2;
            _upPos = transform.position + transform.localScale / 2;

            //Other�I�u�W�F�N�g���
            var otherList = GameObject.FindGameObjectsWithTag("Other");
            foreach (var other in otherList)
            {
                //Other�I�u�W�F�N�g�̓o�^
                var otherMortonPosition = ConvertToMortonPosition(other.transform.position);
                int otherOrderNumber = GetOrderNumber3D(otherMortonPosition);
                _otherPositionSet[otherOrderNumber].Add(other.transform.position);

                //Other�I�u�W�F�N�g�̏���
                Destroy(other);
            }
        }

        void Update()
        {
            //���[�g�����f�����W�ɕϊ�
            var playerMortonPosition = ConvertToMortonPosition(_player.transform.position);

            //��ԕύX�̊m�F
            int newOrderNumber = GetOrderNumber3D(playerMortonPosition);
            if (newOrderNumber != _prevOrderNumber)
            {
                CreateAvater(playerMortonPosition);
                _prevOrderNumber = newOrderNumber;
            }
        }

        //���[�g�����W�ւ̕ϊ�
        private Vector3 ConvertToMortonPosition(Vector3 position)
        {
            var mortonPosition = new Vector3(
                (position.x - _downPos.x) / transform.localScale.x * (int)_depth,
                (position.y - _downPos.y) / transform.localScale.y * (int)_depth,
                (position.z - _downPos.z) / transform.localScale.z * (int)_depth);

            return mortonPosition;
        }

        //���[�g����Ԕԍ����擾
        private int GetOrderNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= (int)_depth
                || position.y < 0 || position.y >= (int)_depth
                || position.z < 0 || position.z >= (int)_depth) return -1;

            return BitSeparate3D((int)position.x)
                | BitSeparate3D((int)position.y) << 1
                | BitSeparate3D((int)position.z) << 2;
        }

        //�r�b�g�����i3D)
        private int BitSeparate3D(int data)
        {
            data = (data | data << 8) & 0x0000f00f;
            data = (data | data << 4) & 0x000c30c3;
            data = (data | data << 2) & 0x00249249;
            return data;
        }

        //�A�o�^�[����
        private void CreateAvater(Vector3 position)
        {
            //�O����
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            //���[�g����Ԕԍ����擾
            var newOrderNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int orderNumber = GetOrderNumber3D(new Vector3(position.x + dx[i], position.y + dy[i], position.z + dz[i]));
                newOrderNumbers.Add(orderNumber);
            }

            //���[�g����ԊO�����O
            newOrderNumbers.RemoveAll(x => x.Equals(-1));

            //���[�g����Ԃ̕ύX
            var addOrderNumbers = newOrderNumbers.Except(_prevOrderNumbers).ToList();
            var removeOrderNumbers = _prevOrderNumbers.Except(newOrderNumbers).ToList();
            _prevOrderNumbers = new List<int>(newOrderNumbers);

            //�A�o�^�[�폜
            foreach (var orderNumber in removeOrderNumbers)
            {
                //�I�u�W�F�N�g�폜
                foreach (var avater in _avaterPoolObjectSet[orderNumber])
                {
                    _avaterPool.Release(avater);
                }

                //�L�[�J��
                _avaterPoolObjectSet.Remove(orderNumber);
            }

            //�A�o�^�[�ǉ�
            foreach (var orderNumber in addOrderNumbers)
            {
                //�L�[�쐬
                if (!_avaterPoolObjectSet.ContainsKey(orderNumber))
                {
                    _avaterPoolObjectSet.Add(orderNumber, new List<GameObject>());
                }

                //�A�o�^�[�ǉ�
                foreach (var otherPosition in _otherPositionSet[orderNumber])
                {
                    //�I�u�W�F�N�g�ǉ�
                    var avater = _avaterPool.Get();
                    avater.transform.position = otherPosition;

                    //�����o�^
                    _avaterPoolObjectSet[orderNumber].Add(avater);
                }
            }
        }
    }
}
