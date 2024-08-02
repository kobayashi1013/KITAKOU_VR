using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Constant;
using Utils;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InMain
{
    public class RoomObjectManager_tmp : MonoBehaviour
    {
        [Serializable]
        public struct ObjectData
        {
            public RoomState state;
            public Vector3 position;
        }

        [Serializable]
        public class ObjectPrefabs
        {
            public GameObject student;
            public GameObject people;
        }

        private static readonly int PLANEOBJECT_SCALERATE = 10; //�v���[���I�u�W�F�N�g�̃X�P�[���䗦

        [Header("SceneObject")]
        [SerializeField] private Transform _objectSetTransform; //�I�u�W�F�N�g���܂Ƃ߂�
        [SerializeField] private Transform _mortonModelTransform;
        [Header("Prefabs")]
        [SerializeField] private ObjectPrefabs _objectPrefabs;
        [Header("Component")]
        [SerializeField] private InMainManager _inMainManagerCs;

        private Vector3 _mortonModelAnchor; //���[�g�����f����Ԃ̒[���W
        private Vector3 _mortonModelScale; //���[�g�����f����Ԃ̃X�P�[��
        //private ObjectPool<GameObject> _studentPool; //�I�u�W�F�N�g���[
        private List<ObjectPool<GameObject>> _objectPool = new List<ObjectPool<GameObject>>(); //�I�u�W�F�N�g���[
        private Dictionary<int, List<ObjectData>> _objectDataDict = new Dictionary<int, List<ObjectData>>(); //�I�u�W�F�N�g�̃f�[�^�W��
        private List<Dictionary<int, List<GameObject>>> _objectDict = new List<Dictionary<int, List<GameObject>>>(); //�I�u�W�F�N�g�v�[���̃I�u�W�F�N�g���L�^����
        private int _prevPlayerSpaceNumber = -1; //�O�̃v���C���[���
        private List<int> _prevNeighborSpaceNumbers = new List<int>(); //�O�̃v���C���[�אڋ��

        private void Start()
        {
            //�I�u�W�F�N�g�v�[����`
            var studentPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_objectPrefabs.student, _objectSetTransform),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));
            _objectPool.Add(studentPool);
            _objectDict.Add(new Dictionary<int, List<GameObject>>());

            var peoplePool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_objectPrefabs.people, _objectSetTransform),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));
            _objectPool.Add(peoplePool);
            _objectDict.Add(new Dictionary<int, List<GameObject>>());

            //�`��X�V
            this.FixedUpdateAsObservable()
                .Select(_ => ConvertToMortonModelPosition(_inMainManagerCs.playerObject.transform.position))
                .Where(x => IsChangePlayerSpaceNumber(x))
                .Select(x => GetNeighborSpaceNumbers(x))
                .Subscribe(x => ObjectManage(x));

            //���[�g�����f����Ԃ��`����
            _mortonModelAnchor = _mortonModelTransform.position - _mortonModelTransform.localScale / 2;
            _mortonModelScale = _mortonModelTransform.transform.localScale;
            Destroy(_mortonModelTransform.gameObject);

            //�����I�u�W�F�N�g�̍��W�����
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //�I�u�W�F�N�g�z�u���Ȃ�����T��
                if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.Empty)
                {
                    Destroy(floor.gameObject);
                    continue;
                }

                //SeedFloor�̒[���W
                var positionAnchor = new Vector3(
                    floor.transform.position.x - floor.transform.localScale.x / 2 * PLANEOBJECT_SCALERATE,
                    floor.transform.position.y,
                    floor.transform.position.z - floor.transform.localScale.z / 2 * PLANEOBJECT_SCALERATE);

                //SeedFloor�̉�]
                Quaternion rotation = Quaternion.AngleAxis(floor.transform.eulerAngles.y, Vector3.up);

                //�X�|�[������A�o�^�[�̎��
                var avaterRole = SystemData.Instance.roomDataList[floor.roomId].state;

                //�A�o�^�[�X�y�[�X�̃X�P�[��
                float width0 = SystemData.Instance.roomDataList[floor.roomId].width0;
                float width1 = SystemData.Instance.roomDataList[floor.roomId].width1;

                //�A�o�^�[�̉��
                float lengthX = width0;
                while (lengthX < floor.transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    var lengthZ = width1;
                    while (lengthZ < floor.transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        //�A�o�^�[�X�y�[�X�̃Z���^�[���W
                        var position = new Vector3(
                            positionAnchor.x + lengthX - width0 / 2,
                            positionAnchor.y,
                            positionAnchor.z + lengthZ - width1 / 2);

                        //�A�o�^�[���W�̒���
                        var offset = position - floor.transform.position;
                        offset = rotation * offset;
                        position = offset + floor.transform.position;

                        //���W�̓o�^
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //���[�g�����f�����W�ɕϊ�
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //�X�y�[�X�ԍ��̎擾

                        //_objectDataSet�̃L�[�o�^
                        if (!_objectDataDict.ContainsKey(mortonSpaceNumber))
                        {
                            _objectDataDict.Add(mortonSpaceNumber, new List<ObjectData>());
                        }

                        //�I�u�W�F�N�g�f�[�^�̓o�^
                        var objectData = new ObjectData();
                        objectData.state = avaterRole;
                        objectData.position = position;
                        _objectDataDict[mortonSpaceNumber].Add(objectData);

                        //����Z���W�ɂ���A�o�^�[�����
                        lengthZ += width1;
                    }

                    //����X���W�ɂ���A�o�^�[�����
                    lengthX += width0;
                }

                Destroy(floor.gameObject);
            }
        }

        /// <summary>
        /// ���W�����[�g�����f����Ԃ���Ƃ������W�ɕϊ�
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * 16,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * 16,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * 16);

            return mortonModelPosition;
        }

        /// <summary>
        /// ���[�g�����W���烂�[�g����Ԕԍ����擾����
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int GetSpaceNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= 16
                || position.y < 0 || position.y >= 16
                || position.z < 0 || position.z >= 16) return -1;

            return BitSeparate3D((int)position.x)
                | BitSeparate3D((int)position.y) << 1
                | BitSeparate3D((int)position.z) << 2;
        }

        /// <summary>
        /// �v���C���[�̋�Ԕԍ��ɕω������邩���`�F�b�N����
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsChangePlayerSpaceNumber(Vector3 position)
        {
            var playerSpaceNumber = GetSpaceNumber3D(position);

            if (playerSpaceNumber != _prevPlayerSpaceNumber)
            {
                _prevPlayerSpaceNumber = playerSpaceNumber;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// �r�b�g�𕪊�����i3D�Łj
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int BitSeparate3D(int data)
        {
            data = (data | data << 8) & 0x0000f00f;
            data = (data | data << 4) & 0x000c30c3;
            data = (data | data << 2) & 0x00249249;
            return data;
        }

        /// <summary>
        /// �O���Ԕԍ����擾
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private List<int> GetNeighborSpaceNumbers(Vector3 position)
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

            return neighborSpaceNumbers;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̃X�|�[���̊Ǘ�
        /// </summary>
        /// <param name="neighborSpaceNumbers"></param>
        private void ObjectManage(List<int> neighborSpaceNumbers)
        {
            //���[�g����Ԃ̕ύX
            var addSpaceNumbers = neighborSpaceNumbers.Except(_prevNeighborSpaceNumbers).ToList();
            var removeSpaceNumbers = _prevNeighborSpaceNumbers.Except(neighborSpaceNumbers).ToList();
            _prevNeighborSpaceNumbers = new List<int>(neighborSpaceNumbers);

            //�I�u�W�F�N�g�폜
            for (int i = 0; i < _objectDict.Count; i++)
            {
                foreach (var spaceNumber in removeSpaceNumbers)
                {
                    //�L�[�m�F
                    if (!_objectDict[i].ContainsKey(spaceNumber)) continue;

                    //�I�u�W�F�N�g�폜
                    foreach (var obj in _objectDict[i][spaceNumber])
                    {
                        _objectPool[i].Release(obj);
                    }

                    //�����J��
                    _objectDict[i].Remove(spaceNumber);
                }
            }

            //�I�u�W�F�N�g�ǉ�
            foreach (var spaceNumber in addSpaceNumbers)
            {
                //�L�[�m�F
                if (!_objectDataDict.ContainsKey(spaceNumber)) continue;

                //�I�u�W�F�N�g�ǉ�
                foreach (var objectData in _objectDataDict[spaceNumber])
                {
                    //�I�u�W�F�N�g���擾
                    var obj = _objectPool[(int)objectData.state].Get();
                    obj.transform.position = objectData.position;
                    obj.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

                    //�I�u�W�F�N�g�����ւ̓o�^
                    if (!_objectDict[(int)objectData.state].ContainsKey(spaceNumber))
                        _objectDict[(int)objectData.state].Add(spaceNumber, new List<GameObject>());
                    else
                        _objectDict[(int)objectData.state][spaceNumber].Add(obj);
                }
            }
        }
    }
}
