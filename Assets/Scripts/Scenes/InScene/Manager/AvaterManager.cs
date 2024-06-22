using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InScene.Manager
{
    public class AvaterManager : MonoBehaviour
    {
        private static readonly int PLANEOBJECT_SCALERATE = 10; //�v���[���I�u�W�F�N�g�̃X�P�[���䗦
        private static readonly Vector3 _restrictY = new Vector3(1, 0, 1);

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
        [SerializeField] private GameObject _floorSet; //�X�|�[�����W��
        [SerializeField] private GameObject _mortonModelObject; //���[�g�����f��
        [Header("Prefabs")]
        [SerializeField] private GameObject _avaterPrefab;
        [Header("Parameters")]
        [SerializeField] private MortonModelDepth _depthX = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthY = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthZ = MortonModelDepth.depth8;

        private ObjectPool<GameObject> _avaterPool; //�A�o�^�[���[
        private Vector3 _mortonModelAnchor; //���[�g�����f���̒[
        private Vector3 _mortonModelScale; //���[�g�����f���X�P�[��
        private Dictionary<int, List<Vector3>> _avaterPositionSet = new Dictionary<int, List<Vector3>>(); //�A�o�^�[�̍��W�W��
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>(); //�I�u�W�F�N�g�v�[���p�̎���
        private List<int> _prevNeighborSpaceNumbers = new List<int>();
        private int _prevPlayerSpaceNumber = -1; //�O�C���^�[�o���̃v���C���[���
        private float _matWidthX = 1f;
        private float _matWidthZ = 1f;

        private void Start()
        {
            //�v���C���[��ԕύX�̊Ď�
            this.FixedUpdateAsObservable()
                .Select(_ => ConvertToMortonModelPosition(InSceneManager.Instance.playerObject.transform.position))
                .Where(x => GetSpaceNumber3D(x) != _prevPlayerSpaceNumber)
                .Subscribe(x => CreateAvater(x));

            //�I�u�W�F�N�g�v�[���ݒ�
            _avaterPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_avaterPrefab),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));

            //���[�g�����f����Ԓ�`�i���W�j
            _mortonModelAnchor = _mortonModelObject.transform.position - _mortonModelObject.transform.localScale / 2; //�[���W
            _mortonModelScale = _mortonModelObject.transform.localScale; //���[�J���X�P�[��
            Destroy(_mortonModelObject);

            //���W���
            var seedFloorTransformList = new List<Transform>(_floorSet.transform.Cast<Transform>());
            foreach (var transform in seedFloorTransformList)
            {
                //SeedFloor�̃x�[�X�|�C���g
                var basePosition = new Vector3(
                    transform.position.x - transform.localScale.x / 2 * PLANEOBJECT_SCALERATE,
                    transform.position.y,
                    transform.position.z - transform.localScale.z / 2 * PLANEOBJECT_SCALERATE);

                //SeedFloor�̉�]
                Quaternion rotation = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);

                //���W�z�u
                float lengthX = _matWidthX;
                float lengthZ = _matWidthZ;

                while (lengthX < transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    lengthZ = _matWidthZ;
                    while (lengthZ < transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        //���W�̉�]
                        var position = new Vector3(
                            basePosition.x + lengthX - _matWidthX / 2,
                            basePosition.y,
                            basePosition.z + lengthZ - _matWidthZ / 2);

                        var offset = position - transform.position;
                        offset = rotation * offset;
                        position = offset + transform.position;

                        //���W�̓o�^
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //���[�g�����f�����W�ɕϊ�
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //�X�y�[�X�ԍ��̎擾

                        if (!_avaterPositionSet.ContainsKey(mortonSpaceNumber)) //�����̒ǉ�
                        {
                            _avaterPositionSet.Add(mortonSpaceNumber, new List<Vector3>());
                        }

                        _avaterPositionSet[mortonSpaceNumber].Add(position); //���W�̓o�^

                        lengthZ += _matWidthZ;
                    }

                    lengthX += _matWidthX;
                }

                Destroy(transform.gameObject);
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

            //�A�o�^�[�폜
            foreach (var spaceNumber in removeSpaceNumbers)
            {
                //�L�[�m�F
                if (!_avaterPositionSet.ContainsKey(spaceNumber)) continue;

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
                if (!_avaterPositionSet.ContainsKey(spaceNumber)) continue;

                //�A�o�^�[�ǉ�
                foreach (var otherPosition in _avaterPositionSet[spaceNumber])
                {
                    //�I�u�W�F�N�g�ǉ�
                    var avater = _avaterPool.Get();
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