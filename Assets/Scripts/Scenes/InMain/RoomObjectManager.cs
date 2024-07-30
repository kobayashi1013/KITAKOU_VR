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

        private static readonly int PLANEOBJECT_SCALERATE = 10; //プレーンオブジェクトのスケール比率

        [Header("SceneObject")]
        [SerializeField] private GameObject _mortonModelObject;

        private Vector3 _mortonModelAnchor; //モートンモデル空間の端座標
        private Vector3 _mortonModelScale; //モートンモデル空間のスケール
        private Dictionary<int, List<ObjectData>> _objectDataSet = new Dictionary<int, List<ObjectData>>(); //オブジェクトのデータ集合

        private void Start()
        {
            //モートンモデル空間を定義する
            _mortonModelAnchor = _mortonModelObject.transform.position - _mortonModelObject.transform.localScale / 2;
            _mortonModelScale = _mortonModelObject.transform.localScale;
            Destroy(_mortonModelObject);

            //部屋オブジェクトの座標を回収
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //オブジェクト配置がない床を探索
                if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.Empty)
                {
                    Destroy(floor.gameObject);
                    continue;
                }

                //SeedFloorの端座標
                var positionAnchor = new Vector3(
                    floor.transform.position.x - floor.transform.localScale.x / 2 * PLANEOBJECT_SCALERATE,
                    floor.transform.position.y,
                    floor.transform.position.z - floor.transform.localScale.z / 2 * PLANEOBJECT_SCALERATE);

                //SeedFloorの回転
                Quaternion rotation = Quaternion.AngleAxis(floor.transform.eulerAngles.y, Vector3.up);

                //スポーンするアバターの種類
                var avaterRole = SystemData.Instance.roomDataList[floor.roomId].state;

                //アバタースペースのスケール
                float width0 = SystemData.Instance.roomDataList[floor.roomId].width0;
                float width1 = SystemData.Instance.roomDataList[floor.roomId].width1;

                //アバターの回収
                float lengthX = width0;
                while (lengthX < floor.transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    var lengthZ = width1;
                    while (lengthZ < floor.transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        //アバタースペースのセンター座標
                        var position = new Vector3(
                            positionAnchor.x + lengthX - width0 / 2,
                            positionAnchor.y,
                            positionAnchor.z + lengthZ - width1 / 2);

                        //アバター座標の調整
                        var offset = position - floor.transform.position;
                        offset = rotation * offset;
                        position = offset + floor.transform.position;

                        //座標の登録
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //モートンモデル座標に変換
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //スペース番号の取得

                        //_objectDataSetのキー登録
                        if (!_objectDataSet.ContainsKey(mortonSpaceNumber))
                        {
                            _objectDataSet.Add(mortonSpaceNumber, new List<ObjectData>());
                        }

                        //オブジェクトデータの登録
                        var objectData = new ObjectData();
                        objectData.state = avaterRole;
                        objectData.position = position;
                        _objectDataSet[mortonSpaceNumber].Add(objectData);

                        //次のZ座標にあるアバターを回収
                        lengthZ += width1;
                    }

                    //次のX座標にあるアバターを回収
                    lengthX += width0;
                }

                Destroy(floor.gameObject);
            }
        }

        /// <summary>
        /// 座標をモートンモデル空間を基準とした座標に変換
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
        /// モートン座標からモートン空間番号を取得する
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
        /// ビットを分割する（3D版）
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
