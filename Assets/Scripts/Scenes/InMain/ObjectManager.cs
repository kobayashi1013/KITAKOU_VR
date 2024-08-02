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
        private static readonly int PLANEOBJECT_SCALERATE = 10; //プレーンオブジェクトのスケール比率

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
        [SerializeField] private Transform _objectSetTransform; //オブジェクトセット
        [SerializeField] private Transform _mortonModelTransform; //モートンモデル
        [Header("Prefabs")]
        [SerializeField] private GameObject _objectPrefab;
        [Header("Parameters")]
        [SerializeField] private RoomState _objectRole;
        [SerializeField] private MortonModelDepth _depthX = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthY = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthZ = MortonModelDepth.depth8;

        private ObjectPool<GameObject> _objectPool; //オブジェクト収納
        private Vector3 _mortonModelAnchor; //モートンモデルの端
        private Vector3 _mortonModelScale; //モートンモデルスケール
        private Dictionary<int, List<Vector3>> _objectPositionSet = new Dictionary<int, List<Vector3>>(); //アバターの座標集合
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>(); //オブジェクトプール用の辞書
        private List<int> _prevNeighborSpaceNumbers = new List<int>();
        private int _prevPlayerSpaceNumber = -1; //前インターバルのプレイヤー空間

        private void Start()
        {
            //プレイヤー空間変更の監視
            this.FixedUpdateAsObservable()
                .Select(_ => ConvertToMortonModelPosition(InMainManager.Instance.playerObject.transform.position))
                .Where(x => GetSpaceNumber3D(x) != _prevPlayerSpaceNumber)
                .Subscribe(x => CreateAvater(x));

            //オブジェクトプール設定
            _objectPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_objectPrefab, _objectSetTransform),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));

            //モートンモデル空間定義（座標）
            _mortonModelAnchor = _mortonModelTransform.transform.position - _mortonModelTransform.transform.localScale / 2; //端座標
            _mortonModelScale = _mortonModelTransform.transform.localScale; //ローカルスケール
            Destroy(_mortonModelTransform.gameObject);

            //座標回収
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //オブジェクト判定
                if (SystemData.Instance.roomDataList[floor.roomId].state != _objectRole)
                {
                    Destroy(floor.gameObject);
                    continue;
                }

                //SeedFloorのベースポイント
                var basePosition = new Vector3(
                    floor.transform.position.x - floor.transform.localScale.x / 2 * PLANEOBJECT_SCALERATE,
                    floor.transform.position.y,
                    floor.transform.position.z - floor.transform.localScale.z / 2 * PLANEOBJECT_SCALERATE);

                //SeedFloorの回転
                Quaternion rotation = Quaternion.AngleAxis(floor.transform.eulerAngles.y, Vector3.up);

                //座標配置
                float lengthX = SystemData.Instance.roomDataList[floor.roomId].width0;
                float lengthZ = SystemData.Instance.roomDataList[floor.roomId].width1;

                while (lengthX < floor.transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    lengthZ = SystemData.Instance.roomDataList[floor.roomId].width1;
                    while (lengthZ < floor.transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        //座標調節
                        var position = new Vector3(
                            basePosition.x + lengthX - SystemData.Instance.roomDataList[floor.roomId].width0 / 2,
                            basePosition.y,
                            basePosition.z + lengthZ - SystemData.Instance.roomDataList[floor.roomId].width1 / 2);

                        var offset = position - floor.transform.position;
                        offset = rotation * offset;
                        position = offset + floor.transform.position;

                        //座標の登録
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //モートンモデル座標に変換
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //スペース番号の取得

                        if (!_objectPositionSet.ContainsKey(mortonSpaceNumber)) //辞書の追加
                        {
                            _objectPositionSet.Add(mortonSpaceNumber, new List<Vector3>());
                        }

                        _objectPositionSet[mortonSpaceNumber].Add(position); //座標の登録

                        lengthZ += SystemData.Instance.roomDataList[floor.roomId].width1;
                    }

                    lengthX += SystemData.Instance.roomDataList[floor.roomId].width0;
                }

                Destroy(floor.transform.gameObject);
            }
        }

        //任意座標からモートンモデル座標への変換
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * (int)_depthX,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * (int)_depthY,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * (int)_depthZ);

            return mortonModelPosition;
        }

        //モートン空間番号の取得
        private int GetSpaceNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= (int)_depthX
                || position.y < 0 || position.y >= (int)_depthY
                || position.z < 0 || position.z >= (int)_depthZ) return -1;

            return BitSeparate3D((int)position.x)
                | BitSeparate3D((int)position.y) << 1
                | BitSeparate3D((int)position.z) << 2;
        }

        //ビット分割（3D）
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

            //前後空間
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            //モートン空間番号を取得
            var neighborSpaceNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int mortonSpaceNumber = GetSpaceNumber3D(new Vector3(playerPosition.x + dx[i], playerPosition.y + dy[i], playerPosition.z + dz[i]));
                neighborSpaceNumbers.Add(mortonSpaceNumber);
            }

            //モートン空間外を除外
            neighborSpaceNumbers.RemoveAll(x => x.Equals(-1));

            //モートン空間の変更
            var addSpaceNumbers = neighborSpaceNumbers.Except(_prevNeighborSpaceNumbers).ToList();
            var removeSpaceNumbers = _prevNeighborSpaceNumbers.Except(neighborSpaceNumbers).ToList();
            _prevNeighborSpaceNumbers = new List<int>(neighborSpaceNumbers);

            //オブジェクト削除
            foreach (var spaceNumber in removeSpaceNumbers)
            {
                //キー確認
                if (!_objectPositionSet.ContainsKey(spaceNumber)) continue;

                //オブジェクト削除
                foreach (var avater in _avaterPoolObjectSet[spaceNumber])
                {
                    _objectPool.Release(avater);
                }

                //キー開放
                _avaterPoolObjectSet.Remove(spaceNumber);
            }

            //アバター追加
            foreach (var spaceNumber in addSpaceNumbers)
            {
                //キー確認
                if (!_objectPositionSet.ContainsKey(spaceNumber)) continue;

                //アバター追加
                foreach (var otherPosition in _objectPositionSet[spaceNumber])
                {
                    //オブジェクト追加
                    var avater = _objectPool.Get();
                    avater.transform.position = otherPosition;
                    avater.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    //キー作成
                    if (!_avaterPoolObjectSet.ContainsKey(spaceNumber))
                    {
                        _avaterPoolObjectSet.Add(spaceNumber, new List<GameObject>());
                    }

                    //辞書登録
                    _avaterPoolObjectSet[spaceNumber].Add(avater);
                }
            }
        }
    }
}