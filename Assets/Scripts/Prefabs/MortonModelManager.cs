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

        private Vector3 _mortonModelAnchor0; //モートンモデルの端
        private Vector3 _mortonModelAnchor1;
        private int _prevPlayerSpaceNumber = -1; //前のプレイヤーのモートン空間番号
        private List<int> _prevNeighborSpaceNumbers = new List<int>(); //前のプレイヤー周辺のモートン空間番号
        private Dictionary<int, List<Vector3>> _otherPositionSet = new Dictionary<int, List<Vector3>>(); //OtherObjectの座標集合
        private Dictionary<int, List<GameObject>> _avaterPoolObjectSet = new Dictionary<int, List<GameObject>>(); //オブジェクトプール用の辞書
        private ObjectPool<GameObject> _avaterPool; //アバターの収納

        void Awake ()
        {
            //オブジェクトプール設定
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

            //モートンモデル空間の定義（座標）
            _mortonModelAnchor0 = transform.position - transform.localScale / 2; //下限
            _mortonModelAnchor1 = transform.position + transform.localScale / 2; //上限

            //Otherオブジェクト回収
            var otherObjectList = GameObject.FindGameObjectsWithTag("Other");
            foreach (var otherObject in otherObjectList)
            {
                //モートン空間番号の取得
                var mortonModelPosition = ConvertToMortonModelPosition(otherObject.transform.position);
                int mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition);

                //モートン空間番号登録
                if (!_otherPositionSet.ContainsKey(mortonSpaceNumber))
                {
                    _otherPositionSet.Add(mortonSpaceNumber, new List<Vector3>());
                }

                //OtherObject座標登録
                _otherPositionSet[mortonSpaceNumber].Add(otherObject.transform.position);

                //OtherObject削除
                Destroy(otherObject);
            }
        }

        void Update()
        {
            //モートン空間番号の取得
            var mortonModelPosition = ConvertToMortonModelPosition(_playerObject.transform.position);
            var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition);

            //空間変更の確認
            if (mortonSpaceNumber != _prevPlayerSpaceNumber)
            {
                CreateAvater(mortonModelPosition);
                _prevPlayerSpaceNumber = mortonSpaceNumber;
            }
        }

        //ワールド座標からモートンモデル座標への変換
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor0.x) / transform.localScale.x * (int)_depthX,
                (position.y - _mortonModelAnchor0.y) / transform.localScale.y * (int)_depthY,
                (position.z - _mortonModelAnchor0.z) / transform.localScale.z * (int)_depthZ);

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

        //アバター生成
        private void CreateAvater(Vector3 position)
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

            //モートン空間の変更
            var addSpaceNumbers = neighborSpaceNumbers.Except(_prevNeighborSpaceNumbers).ToList();
            var removeSpaceNumbers = _prevNeighborSpaceNumbers.Except(neighborSpaceNumbers).ToList();
            _prevNeighborSpaceNumbers = new List<int>(neighborSpaceNumbers);

            //アバター削除
            foreach (var spaceNumber in removeSpaceNumbers)
            {
                //キー確認
                if (!_avaterPoolObjectSet.ContainsKey(spaceNumber)) continue;

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
                if (!_otherPositionSet.ContainsKey(spaceNumber)) continue;

                //アバター追加
                foreach (var otherPosition in _otherPositionSet[spaceNumber])
                {
                    //オブジェクト追加
                    var avater = _avaterPool.Get();
                    avater.transform.position = otherPosition;

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
