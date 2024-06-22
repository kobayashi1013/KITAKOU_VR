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
        private static readonly int PLANEOBJECT_SCALERATE = 10; //プレーンオブジェクトのスケール比率
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
        [SerializeField] private GameObject _floorSet; //スポーン床集合
        [SerializeField] private GameObject _mortonModelObject; //モートンモデル
        [Header("Prefabs")]
        [SerializeField] private GameObject _avaterPrefab;
        [Header("Parameters")]
        [SerializeField] private MortonModelDepth _depthX = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthY = MortonModelDepth.depth8;
        [SerializeField] private MortonModelDepth _depthZ = MortonModelDepth.depth8;

        private ObjectPool<GameObject> _avaterPool; //アバター収納
        private Vector3 _mortonModelAnchor; //モートンモデルの端
        private Vector3 _mortonModelScale; //モートンモデルスケール
        private Dictionary<int, List<Vector3>> _avaterPositionSet = new Dictionary<int, List<Vector3>>(); //アバターの座標集合
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>(); //オブジェクトプール用の辞書
        private List<int> _prevNeighborSpaceNumbers = new List<int>();
        private int _prevPlayerSpaceNumber = -1; //前インターバルのプレイヤー空間
        private float _matWidthX = 1f;
        private float _matWidthZ = 1f;

        private void Start()
        {
            //プレイヤー空間変更の監視
            this.FixedUpdateAsObservable()
                .Select(_ => ConvertToMortonModelPosition(InSceneManager.Instance.playerObject.transform.position))
                .Where(x => GetSpaceNumber3D(x) != _prevPlayerSpaceNumber)
                .Subscribe(x => CreateAvater(x));

            //オブジェクトプール設定
            _avaterPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_avaterPrefab),
                actionOnGet: target => target.SetActive(true),
                actionOnRelease: target => target.SetActive(false),
                actionOnDestroy: target => Destroy(target));

            //モートンモデル空間定義（座標）
            _mortonModelAnchor = _mortonModelObject.transform.position - _mortonModelObject.transform.localScale / 2; //端座標
            _mortonModelScale = _mortonModelObject.transform.localScale; //ローカルスケール
            Destroy(_mortonModelObject);

            //座標回収
            var seedFloorTransformList = new List<Transform>(_floorSet.transform.Cast<Transform>());
            foreach (var transform in seedFloorTransformList)
            {
                //SeedFloorのベースポイント
                var basePosition = new Vector3(
                    transform.position.x - transform.localScale.x / 2 * PLANEOBJECT_SCALERATE,
                    transform.position.y,
                    transform.position.z - transform.localScale.z / 2 * PLANEOBJECT_SCALERATE);

                //SeedFloorの回転
                Quaternion rotation = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);

                //座標配置
                float lengthX = _matWidthX;
                float lengthZ = _matWidthZ;

                while (lengthX < transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    lengthZ = _matWidthZ;
                    while (lengthZ < transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        //座標の回転
                        var position = new Vector3(
                            basePosition.x + lengthX - _matWidthX / 2,
                            basePosition.y,
                            basePosition.z + lengthZ - _matWidthZ / 2);

                        var offset = position - transform.position;
                        offset = rotation * offset;
                        position = offset + transform.position;

                        //座標の登録
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //モートンモデル座標に変換
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //スペース番号の取得

                        if (!_avaterPositionSet.ContainsKey(mortonSpaceNumber)) //辞書の追加
                        {
                            _avaterPositionSet.Add(mortonSpaceNumber, new List<Vector3>());
                        }

                        _avaterPositionSet[mortonSpaceNumber].Add(position); //座標の登録

                        lengthZ += _matWidthZ;
                    }

                    lengthX += _matWidthX;
                }

                Destroy(transform.gameObject);
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

            //アバター削除
            foreach (var spaceNumber in removeSpaceNumbers)
            {
                //キー確認
                if (!_avaterPositionSet.ContainsKey(spaceNumber)) continue;

                //オブジェクト削除
                foreach (var avater in _avaterPoolObjectSet[spaceNumber])
                {
                    _avaterPool.Release(avater);
                }

                //キー開放
                _avaterPoolObjectSet.Remove(spaceNumber);
            }

            //アバター追加
            foreach (var spaceNumber in addSpaceNumbers)
            {
                //キー確認
                if (!_avaterPositionSet.ContainsKey(spaceNumber)) continue;

                //アバター追加
                foreach (var otherPosition in _avaterPositionSet[spaceNumber])
                {
                    //オブジェクト追加
                    var avater = _avaterPool.Get();
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