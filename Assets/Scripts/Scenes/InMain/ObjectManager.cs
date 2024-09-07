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
        private static readonly int Y_DEPTH = 3; //�O�K����

        [Header("SceneObjects")]
        [SerializeField] private Transform _objectSetTransform; //�I�u�W�F�N�g�Z�b�g
        [SerializeField] private Transform _mortonModelTransform; //���[�g�����f��
        [Header("PrefabObjects")]
        [SerializeField] private PrefabTable _prefabTable;
        [SerializeField] private float _additionPrefabSpawnRate = 0.5f; //�ǉ��v���n�u�̃X�|�[���m��

        private Vector3 _mortonModelAnchor; //���[�g�����f���̒[
        private Vector3 _mortonModelScale; //���[�g�����f���X�P�[��
        private List<Manager> _managers = new List<Manager>();
        private List<int> _prevNeighbor = new List<int>();
        private int _prevSpace = -1; //�O�C���^�[�o���̃v���C���[���

        private class Manager
        {
            private ObjectPool<GameObject> _pool;
            private Dictionary<int, List<GameObject>> _releases = new Dictionary<int, List<GameObject>>();
            private Dictionary<int, List<Vector3>> _positions = new Dictionary<int, List<Vector3>>();

            public Manager(GameObject prefab, Transform objectSetTransform)
            {
                _pool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab, objectSetTransform),
                    actionOnGet: target => target.SetActive(true),
                    actionOnRelease: target => target.SetActive(false),
                    actionOnDestroy: target => Destroy(target));
            }

            public void SetPosition(int space, Vector3 position)
            {
                if (!_positions.ContainsKey(space)) //�����̒ǉ�
                {
                    _positions.Add(space, new List<Vector3>());
                }

                _positions[space].Add(position); //���W�̓o�^
            }

            public void Remover(List<int> neighbors)
            {
                //�I�u�W�F�N�g�폜
                foreach (var neighbor in neighbors)
                {
                    //�L�[�m�F
                    if (!_positions.ContainsKey(neighbor)) continue;

                    //�I�u�W�F�N�g�폜
                    foreach (var avater in _releases[neighbor])
                    {
                        _pool.Release(avater);
                    }

                    //�L�[�J��
                    _releases.Remove(neighbor);
                }
            }

            public void Adder(List<int> neighbors)
            {
                //�I�u�W�F�N�g�ǉ�
                foreach (var neighbor in neighbors)
                {
                    //�L�[�m�F
                    if (!_positions.ContainsKey(neighbor)) continue;

                    //�L�[�쐬
                    if (!_releases.ContainsKey(neighbor))
                    {
                        _releases.Add(neighbor, new List<GameObject>());
                    }

                    //�A�o�^�[�ǉ�
                    foreach (var position in _positions[neighbor])
                    {
                        //�I�u�W�F�N�g�ǉ�
                        var avater = _pool.Get();
                        avater.transform.position = position;
                        avater.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                        //�����o�^
                        _releases[neighbor].Add(avater);
                    }
                }
            }
        }

        private void Start()
        {
            //���[�g�����f����Ԓ�`�i���W�j
            _mortonModelAnchor = _mortonModelTransform.transform.position - _mortonModelTransform.transform.localScale / 2; //�[���W
            _mortonModelScale = _mortonModelTransform.transform.localScale; //���[�J���X�P�[��
            Destroy(_mortonModelTransform.gameObject);

            //�}�l�[�W���[�쐬
            foreach (var prefab in _prefabTable.prefabs)
            {
                _managers.Add(new Manager(prefab, _objectSetTransform));
            }
            
            //���W���
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //�z�u�Ȃ�
                if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.Empty)
                {
                    Destroy(floor.transform.gameObject);
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
                        var circleMoveNum = (int)Random.Range(0, 360);
                        var circleMove = new Vector3(0.2f * Mathf.Cos(circleMoveNum), 0, 0.2f * Mathf.Sin(circleMoveNum));
                        position = offset + floor.transform.position + circleMove;

                        //���W�̓o�^
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //���[�g�����f�����W�ɕϊ�
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //�X�y�[�X�ԍ��̎擾

                        //�z�u
                        if (Random.Range(0f, 100f) < _additionPrefabSpawnRate)
                        {
                            var rand = (int)Random.Range(0, _prefabTable.addition.Count);
                            var num = _prefabTable.addition[rand];
                            _managers[num].SetPosition(mortonSpaceNumber, position);
                        }
                        else if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.Student)
                        {
                            var rand = (int)Random.Range(0, _prefabTable.student.Count);
                            var num = _prefabTable.student[rand];
                            _managers[num].SetPosition(mortonSpaceNumber, position);
                        }
                        else if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.People)
                        {
                            var rand = (int)Random.Range(0, _prefabTable.people.Count);
                            var num = _prefabTable.people[rand];
                            _managers[num].SetPosition(mortonSpaceNumber, position);
                        }

                        lengthZ += SystemData.Instance.roomDataList[floor.roomId].width1;
                    }

                    lengthX += SystemData.Instance.roomDataList[floor.roomId].width0;
                }

                Destroy(floor.transform.gameObject);
            }
        }

        private void Update()
        {
            var position = ConvertToMortonModelPosition(InMainManager.Instance.playerObject.transform.position);
            var space = GetSpaceNumber3D(position);

            if (space != _prevSpace)
            {
                //Debug.Log("Space Update");
                _prevSpace = space;
                _prevNeighbor = Updater(position, _prevNeighbor);
            }
        }

        //�C�Ӎ��W���烂�[�g�����f�����W�ւ̕ϊ�
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * (int)SystemData.Instance.mortonModelDepth,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * (int)Y_DEPTH,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * (int)SystemData.Instance.mortonModelDepth);

            return mortonModelPosition;
        }

        //���[�g����Ԕԍ��̎擾
        private int GetSpaceNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= (int)SystemData.Instance.mortonModelDepth
                || position.y < 0 || position.y >= (int)Y_DEPTH
                || position.z < 0 || position.z >= (int)SystemData.Instance.mortonModelDepth) return -1;

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

        private List<int> Updater(Vector3 playerPosition, List<int> prevNeighbor)
        {
            //�O����
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            //�אڋ�Ԃ��擾
            var neighbor = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int number = GetSpaceNumber3D(new Vector3(playerPosition.x + dx[i], playerPosition.y + dy[i], playerPosition.z + dz[i]));
                neighbor.Add(number);
            }

            //�ǉ��E�폜��Ԃ̎擾
            neighbor.RemoveAll(x => x.Equals(-1));
            var addNeighbor = neighbor.Except(prevNeighbor).ToList();
            var removeNeighbor = prevNeighbor.Except(neighbor).ToList();

            foreach (var manager in _managers)
            {
                manager.Adder(addNeighbor);
                manager.Remover(removeNeighbor);
            }

            return neighbor;
        }
    }
}