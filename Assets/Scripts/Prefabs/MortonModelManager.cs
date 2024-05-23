using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Prefabs
{
    public class MortonModelManager : MonoBehaviour
    {
        private enum MortonModelDepth
        {
            depth2 = 2,
            depth4 = 4,
            depth8 = 8,
            depth16 = 16,
            depth32 = 32,
            depth64 = 64,
        }

        [SerializeField] private GameObject _test;
        [Header("SceneObjects")]
        [SerializeField] private GameObject _playerObject;
        [Header("Prefabs")]
        [SerializeField] private GameObject _avaterPrefab;
        [Header("Parameters")]
        [SerializeField] private MortonModelDepth _depthX = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthY = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthZ = MortonModelDepth.depth8;

        private Vector3 _mortonModelAnchor0; //���[�g�����f���̒[
        private Vector3 _mortonModelAnchor1;
        private int _prevPlayerSpaceNumber = -1; //�O�̃v���C���[�̃��[�g����Ԕԍ�
        private List<int> _prevNeighborSpaceNumbers = new List<int>(); //�O�̃v���C���[���ӂ̃��[�g����Ԕԍ�
        private Dictionary<int, List<Vector3>> _otherPositionSet = new Dictionary<int, List<Vector3>>(); //OtherObject�̍��W�W��
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>(); //�I�u�W�F�N�g�v�[���p�̎���
        private ObjectPool<GameObject> _avaterPool; //�A�o�^�[�̎��[

        void Awake ()
        {
            //�I�u�W�F�N�g�v�[���ݒ�
            _avaterPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_avaterPrefab),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));
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

            //���[�g�����f����Ԃ̒�`�i���W�j
            _mortonModelAnchor0 = transform.position - transform.localScale / 2; //����
            _mortonModelAnchor1 = transform.position + transform.localScale / 2; //���

            //Other�I�u�W�F�N�g���
            var otherObjectList = GameObject.FindGameObjectsWithTag("Other");
            foreach (var otherObject in otherObjectList)
            {
                //���[�g����Ԕԍ��̎擾
                var mortonModelPosition = ConvertToMortonModelPosition(otherObject.transform.position);
                int mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition);

                //���[�g����Ԕԍ��o�^
                if (!_otherPositionSet.ContainsKey(mortonSpaceNumber))
                {
                    _otherPositionSet.Add(mortonSpaceNumber, new List<Vector3>());
                }

                //OtherObject���W�o�^
                _otherPositionSet[mortonSpaceNumber].Add(otherObject.transform.position);

                //OtherObject�폜
                Destroy(otherObject);
            }
        }

        void Update()
        {
            //���[�g����Ԕԍ��̎擾
            var mortonModelPosition = ConvertToMortonModelPosition(_playerObject.transform.position);
            var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition);

            //��ԕύX�̊m�F
            if (mortonSpaceNumber != _prevPlayerSpaceNumber)
            {
                CreateAvater(mortonModelPosition);
                _prevPlayerSpaceNumber = mortonSpaceNumber;
            }
        }

        //���[���h���W���烂�[�g�����f�����W�ւ̕ϊ�
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor0.x) / transform.localScale.x * (int)_depthX,
                (position.y - _mortonModelAnchor0.y) / transform.localScale.y * (int)_depthY,
                (position.z - _mortonModelAnchor0.z) / transform.localScale.z * (int)_depthZ);

            return mortonModelPosition;
        }

        //���[�g����Ԕԍ��̎擾
        private int GetSpaceNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= (int)_depthX
                || position.y < 0 || position.y >= (int)_depthY
                || position.z < 0 || position.z >= (int)_depthZ) return -1;

            return BitSeparate3D((int)position.x)
                | BitSeparate3D((int)position.y) << 1
                | BitSeparate3D((int)position.z) << 2;
        }

        //�r�b�g�����i3D�j
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
            var neighborSpaceNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int mortonSpaceNumber = GetSpaceNumber3D(new Vector3(position.x + dx[i], position.y + dy[i], position.z + dz[i]));
                neighborSpaceNumbers.Add(mortonSpaceNumber);
            }

            //���[�g����ԊO�����O
            neighborSpaceNumbers.RemoveAll(x => x.Equals(-1));

            //���[�g����Ԃ̕ύX
            var addSpaceNumbers = neighborSpaceNumbers.Except(_prevNeighborSpaceNumbers).ToList();
            var removeSpaceNumbers = _prevNeighborSpaceNumbers.Except(neighborSpaceNumbers).ToList();
            _prevNeighborSpaceNumbers = new List<int>(neighborSpaceNumbers);

            //�A�o�^�[�폜
            foreach (var spaceNumber in removeSpaceNumbers)
            {
                //�L�[�m�F
                if (!_avaterPoolObjectSet.ContainsKey(spaceNumber)) continue;

                //�I�u�W�F�N�g�폜
                foreach (var avater in _avaterPoolObjectSet[spaceNumber])
                {
                    _avaterPool.Release(avater);
                }

                //�L�[�J��
                _avaterPoolObjectSet.Remove(spaceNumber);
            }

            //�A�o�^�[�ǉ�
            foreach (var spaceNumber in addSpaceNumbers)
            {
                //�L�[�m�F
                if (!_otherPositionSet.ContainsKey(spaceNumber)) continue;

                //�A�o�^�[�ǉ�
                foreach (var otherPosition in _otherPositionSet[spaceNumber])
                {
                    //�I�u�W�F�N�g�ǉ�
                    var avater = _avaterPool.Get();
                    avater.transform.position = otherPosition;

                    //�L�[�쐬
                    if (!_avaterPoolObjectSet.ContainsKey(spaceNumber))
                    {
                        _avaterPoolObjectSet.Add(spaceNumber, new List<GameObject>());
                    }

                    //�����o�^
                    _avaterPoolObjectSet[spaceNumber].Add(avater);
                }
            }
        }
    }
}
