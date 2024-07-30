using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constant;
using Utils;

namespace Scenes.InMain
{
    public class RoomObjectManager : MonoBehaviour
    {
        [Serializable]
        public struct ObjectData
        {
            public RoomState state;
            public Vector3 position;
        }

        private static readonly int PLANEOBJECT_SCALERATE = 10; //�v���[���I�u�W�F�N�g�̃X�P�[���䗦

        [Header("SceneObject")]
        [SerializeField] private GameObject _mortonModelObject;

        private Vector3 _mortonModelAnchor; //���[�g�����f����Ԃ̒[���W
        private Vector3 _mortonModelScale; //���[�g�����f����Ԃ̃X�P�[��
        private Dictionary<int, List<ObjectData>> _objectDataSet = new Dictionary<int, List<ObjectData>>(); //�I�u�W�F�N�g�̃f�[�^�W��

        private void Start()
        {
            //���[�g�����f����Ԃ��`����
            _mortonModelAnchor = _mortonModelObject.transform.position - _mortonModelObject.transform.localScale / 2;
            _mortonModelScale = _mortonModelObject.transform.localScale;
            Destroy(_mortonModelObject);

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
                        if (!_objectDataSet.ContainsKey(mortonSpaceNumber))
                        {
                            _objectDataSet.Add(mortonSpaceNumber, new List<ObjectData>());
                        }

                        //�I�u�W�F�N�g�f�[�^�̓o�^
                        var objectData = new ObjectData();
                        objectData.state = avaterRole;
                        objectData.position = position;
                        _objectDataSet[mortonSpaceNumber].Add(objectData);

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
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * 4,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * 4,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * 4);

            return mortonModelPosition;
        }

        /// <summary>
        /// ���[�g�����W���烂�[�g����Ԕԍ����擾����
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int GetSpaceNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= 4
                || position.y < 0 || position.y >= 4
                || position.z < 0 || position.z >= 4) return -1;

            return BitSeparate3D((int)position.x)
                | BitSeparate3D((int)position.y) << 1
                | BitSeparate3D((int)position.z) << 2;
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
    }
}
