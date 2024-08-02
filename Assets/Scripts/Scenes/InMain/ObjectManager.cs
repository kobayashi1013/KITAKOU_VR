using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UniRx;
using UniRx.Triggers;
using Utils;
using Constant;

namespace Scenes.InMain
{
    public class ObjectManager : MonoBehaviour
    {
        private static readonly int PLANEOBJECT_SCALERATE = 10; //�v���[���I�u�W�F�N�g�̃X�P�[���䗦

        private enum MortonModelDepth
        {
            depth2 = 2,
            depth4 = 4,
            depth8 = 8,
            depth16 = 16,
            depth32 = 32,
            depth64 = 64,
        }

        [Header("Scene Objects")]
        [SerializeField] private Transform _objectSetTransform; //�I�u�W�F�N�g�Z�b�g
        [SerializeField] private Transform _mortonModelTransform; //���[�g�����f��
        [Header("Prefabs")]
        [SerializeField] private GameObject _objectPrefab;
        [Header("Parameters")]
        [SerializeField] private RoomState _objectRole;
        [SerializeField] private MortonModelDepth _depthX = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthY = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthZ = MortonModelDepth.depth8;

        private ObjectPool<GameObject> _objectPool; //�I�u�W�F�N�g���[
        private Vector3 _mortonModelAnchor; //���[�g�����f���̒[
        private Vector3 _mortonModelScale; //���[�g�����f���X�P�[��
        private Dictionary<int, List<Vector3>> _objectPositionSet = new Dictionary<int, List<Vector3>>(); //�A�o�^�[�̍��W�W��
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>(); //�I�u�W�F�N�g�v�[���p�̎���
        private List<int> _prevNeighborSpaceNumbers = new List<int>();
        private int _prevPlayerSpaceNumber = -1; //�O�C���^�[�o���̃v���C���[���

        private void Start()
        {
            //�v���C���[��ԕύX�̊Ď�
            this.FixedUpdateAsObservable()
                .Select(_ => ConvertToMortonModelPosition(InMainManager.Instance.playerObject.transform.position))
                .Where(x => GetSpaceNumber3D(x) != _prevPlayerSpaceNumber)
                .Subscribe(x => CreateAvater(x));

            //�I�u�W�F�N�g�v�[���ݒ�
            _objectPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_objectPrefab, _objectSetTransform),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));

            //���[�g�����f����Ԓ�`�i���W�j
            _mortonModelAnchor = _mortonModelTransform.transform.position - _mortonModelTransform.transform.localScale / 2; //�[���W
            _mortonModelScale = _mortonModelTransform.transform.localScale; //���[�J���X�P�[��
            Destroy(_mortonModelTransform.gameObject);

            //���W���
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //�I�u�W�F�N�g����
                if (SystemData.Instance.roomDataList[floor.roomId].state != _objectRole)
                {
                    Destroy(floor.gameObject);
                    continue;
                }

                //SeedFloor�̃x�[�X�|�C���g
                var basePosition = new Vector3(
                    floor.transform.position.x - floor.transform.localScale.x / 2 * PLANEOBJECT_SCALERATE,
                    floor.transform.position.y,
                    floor.transform.position.z - floor.transform.localScale.z / 2 * PLANEOBJECT_SCALERATE);

                //SeedFloor�̉�]
                Quaternion rotation = Quaternion.AngleAxis(floor.transform.eulerAngles.y, Vector3.up);

                //���W�z�u
                float lengthX = SystemData.Instance.roomDataList[floor.roomId].width0;
                float lengthZ = SystemData.Instance.roomDataList[floor.roomId].width1;

                while (lengthX < floor.transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    lengthZ = SystemData.Instance.roomDataList[floor.roomId].width1;
                    while (lengthZ < floor.transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        //���W����
                        var position = new Vector3(
                            basePosition.x + lengthX - SystemData.Instance.roomDataList[floor.roomId].width0 / 2,
                            basePosition.y,
                            basePosition.z + lengthZ - SystemData.Instance.roomDataList[floor.roomId].width1 / 2);

                        var offset = position - floor.transform.position;
                        offset = rotation * offset;
                        position = offset + floor.transform.position;

                        //���W�̓o�^
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //���[�g�����f�����W�ɕϊ�
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //�X�y�[�X�ԍ��̎擾

                        if (!_objectPositionSet.ContainsKey(mortonSpaceNumber)) //�����̒ǉ�
                        {
                            _objectPositionSet.Add(mortonSpaceNumber, new List<Vector3>());
                        }

                        _objectPositionSet[mortonSpaceNumber].Add(position); //���W�̓o�^

                        lengthZ += SystemData.Instance.roomDataList[floor.roomId].width1;
                    }

                    lengthX += SystemData.Instance.roomDataList[floor.roomId].width0;
                }

                Destroy(floor.transform.gameObject);
            }
        }

        //�C�Ӎ��W���烂�[�g�����f�����W�ւ̕ϊ�
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * (int)_depthX,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * (int)_depthY,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * (int)_depthZ);

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

        private void CreateAvater(Vector3 playerPosition)
        {
            //Debug.Log("update player space");

            //�O����
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            //���[�g����Ԕԍ����擾
            var neighborSpaceNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int mortonSpaceNumber = GetSpaceNumber3D(new Vector3(playerPosition.x + dx[i], playerPosition.y + dy[i], playerPosition.z + dz[i]));
                neighborSpaceNumbers.Add(mortonSpaceNumber);
            }

            //���[�g����ԊO�����O
            neighborSpaceNumbers.RemoveAll(x => x.Equals(-1));

            //���[�g����Ԃ̕ύX
            var addSpaceNumbers = neighborSpaceNumbers.Except(_prevNeighborSpaceNumbers).ToList();
            var removeSpaceNumbers = _prevNeighborSpaceNumbers.Except(neighborSpaceNumbers).ToList();
            _prevNeighborSpaceNumbers = new List<int>(neighborSpaceNumbers);

            //�I�u�W�F�N�g�폜
            foreach (var spaceNumber in removeSpaceNumbers)
            {
                //�L�[�m�F
                if (!_objectPositionSet.ContainsKey(spaceNumber)) continue;

                //�I�u�W�F�N�g�폜
                foreach (var avater in _avaterPoolObjectSet[spaceNumber])
                {
                    _objectPool.Release(avater);
                }

                //�L�[�J��
                _avaterPoolObjectSet.Remove(spaceNumber);
            }

            //�A�o�^�[�ǉ�
            foreach (var spaceNumber in addSpaceNumbers)
            {
                //�L�[�m�F
                if (!_objectPositionSet.ContainsKey(spaceNumber)) continue;

                //�A�o�^�[�ǉ�
                foreach (var otherPosition in _objectPositionSet[spaceNumber])
                {
                    //�I�u�W�F�N�g�ǉ�
                    var avater = _objectPool.Get();
                    avater.transform.position = otherPosition;
                    avater.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

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