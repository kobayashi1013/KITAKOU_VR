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

        private static readonly int PLANEOBJECT_SCALERATE = 10; //プレーンオブジェクトのスケール比率

        [Header("SceneObject")]
        [SerializeField] private Transform _objectSetTransform; //オブジェクトをまとめる
        [SerializeField] private Transform _mortonModelTransform;
        [Header("Prefabs")]
        [SerializeField] private ObjectPrefabs _objectPrefabs;
        [Header("Component")]
        [SerializeField] private InMainManager _inMainManagerCs;

        private Vector3 _mortonModelAnchor; //モートンモデル空間の端座標
        private Vector3 _mortonModelScale; //モートンモデル空間のスケール
        //private ObjectPool<GameObject> _studentPool; //オブジェクト収納
        private List<ObjectPool<GameObject>> _objectPool = new List<ObjectPool<GameObject>>(); //オブジェクト収納
        private Dictionary<int, List<ObjectData>> _objectDataDict = new Dictionary<int, List<ObjectData>>(); //オブジェクトのデータ集合
        private List<Dictionary<int, List<GameObject>>> _objectDict = new List<Dictionary<int, List<GameObject>>>(); //オブジェクトプールのオブジェクトを記録する
        private int _prevPlayerSpaceNumber = -1; //前のプレイヤー空間
        private List<int> _prevNeighborSpaceNumbers = new List<int>(); //前のプレイヤー隣接空間

        private void Start()
        {
            //オブジェクトプール定義
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

            //描画更新
            this.FixedUpdateAsObservable()
                .Select(_ => ConvertToMortonModelPosition(_inMainManagerCs.playerObject.transform.position))
                .Where(x => IsChangePlayerSpaceNumber(x))
                .Select(x => GetNeighborSpaceNumbers(x))
                .Subscribe(x => ObjectManage(x));

            //モートンモデル空間を定義する
            _mortonModelAnchor = _mortonModelTransform.position - _mortonModelTransform.localScale / 2;
            _mortonModelScale = _mortonModelTransform.transform.localScale;
            Destroy(_mortonModelTransform.gameObject);

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
                        if (!_objectDataDict.ContainsKey(mortonSpaceNumber))
                        {
                            _objectDataDict.Add(mortonSpaceNumber, new List<ObjectData>());
                        }

                        //オブジェクトデータの登録
                        var objectData = new ObjectData();
                        objectData.state = avaterRole;
                        objectData.position = position;
                        _objectDataDict[mortonSpaceNumber].Add(objectData);

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
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * 16,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * 16,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * 16);

            return mortonModelPosition;
        }

        /// <summary>
        /// モートン座標からモートン空間番号を取得する
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
        /// プレイヤーの空間番号に変化があるかをチェックする
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

        /// <summary>
        /// 前後空間番号を取得
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private List<int> GetNeighborSpaceNumbers(Vector3 position)
        {
            //前後空間
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            //モートン空間番号を取得
            var neighborSpaceNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int mortonSpaceNumber = GetSpaceNumber3D(new Vector3(position.x + dx[i], position.y + dy[i], position.z + dz[i]));
                neighborSpaceNumbers.Add(mortonSpaceNumber);
            }

            //モートン空間外を除外
            neighborSpaceNumbers.RemoveAll(x => x.Equals(-1));

            return neighborSpaceNumbers;
        }

        /// <summary>
        /// オブジェクトのスポーンの管理
        /// </summary>
        /// <param name="neighborSpaceNumbers"></param>
        private void ObjectManage(List<int> neighborSpaceNumbers)
        {
            //モートン空間の変更
            var addSpaceNumbers = neighborSpaceNumbers.Except(_prevNeighborSpaceNumbers).ToList();
            var removeSpaceNumbers = _prevNeighborSpaceNumbers.Except(neighborSpaceNumbers).ToList();
            _prevNeighborSpaceNumbers = new List<int>(neighborSpaceNumbers);

            //オブジェクト削除
            for (int i = 0; i < _objectDict.Count; i++)
            {
                foreach (var spaceNumber in removeSpaceNumbers)
                {
                    //キー確認
                    if (!_objectDict[i].ContainsKey(spaceNumber)) continue;

                    //オブジェクト削除
                    foreach (var obj in _objectDict[i][spaceNumber])
                    {
                        _objectPool[i].Release(obj);
                    }

                    //辞書開放
                    _objectDict[i].Remove(spaceNumber);
                }
            }

            //オブジェクト追加
            foreach (var spaceNumber in addSpaceNumbers)
            {
                //キー確認
                if (!_objectDataDict.ContainsKey(spaceNumber)) continue;

                //オブジェクト追加
                foreach (var objectData in _objectDataDict[spaceNumber])
                {
                    //オブジェクトを取得
                    var obj = _objectPool[(int)objectData.state].Get();
                    obj.transform.position = objectData.position;
                    obj.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

                    //オブジェクト辞書への登録
                    if (!_objectDict[(int)objectData.state].ContainsKey(spaceNumber))
                        _objectDict[(int)objectData.state].Add(spaceNumber, new List<GameObject>());
                    else
                        _objectDict[(int)objectData.state][spaceNumber].Add(obj);
                }
            }
        }
    }
}
