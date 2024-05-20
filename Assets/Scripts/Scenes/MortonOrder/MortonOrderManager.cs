using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Pool;

namespace Scenes.MortonOrder
{
    public class MortonOrderManager : MonoBehaviour
    {
        private enum MortonOrderDepth
        {
            depth2 = 2,
            depth4 = 4,
            depth8 = 8,
        }
 
        [Header("SceneObjects")]
        [SerializeField] private GameObject _player;
        [Header("Prefabs")]
        [SerializeField] private GameObject _avaterPrefab;
        [Header("Parameters")]
        [SerializeField] private MortonOrderDepth _depth = MortonOrderDepth.depth8;

        private Vector3 _downPos; //モデルの端
        private Vector3 _upPos;
        private int _prevOrderNumber = -1;
        private Dictionary<int, List<Vector3>> _otherPositionSet = new Dictionary<int, List<Vector3>>(); //Otherの座標集合
        private ObjectPool<GameObject> _avaterPool;

        void Awake()
        {
            //オブジェクトプール設定
            new ObjectPool<GameObject>(CreatePoolObject, GetPoolObject, ReleasePoolObject, DestroyPoolObject);

            //Other座標集合のキー設定
            for (int i = 0; i < Mathf.Pow((int)_depth, 3); i++)
            {
                _otherPositionSet.Add(i, new List<Vector3>());
            }
        }

        void Start()
        {
            //空間定義
            _downPos = transform.position - transform.localScale / 2;
            _upPos = transform.position + transform.localScale / 2;

            //Otherオブジェクト回収
            var otherList = GameObject.FindGameObjectsWithTag("Other");
            foreach (var other in otherList)
            {
                //Otherオブジェクトの登録
                var otherMortonPosition = ConvertToMortonPosition(other.transform.position);
                int otherOrderNumber = GetOrderNumber3D(otherMortonPosition);
                _otherPositionSet[otherOrderNumber].Add(other.transform.position);

                //Otherオブジェクトの消去
                Destroy(other);
            }
        }

        void Update()
        {
            //モートンモデル座標に変換
            var playerMortonPosition = ConvertToMortonPosition(_player.transform.position);

            //空間変更の確認
            int newOrderNumber = GetOrderNumber3D(playerMortonPosition);
            if (newOrderNumber != _prevOrderNumber)
            {
                CreateAvater(playerMortonPosition);
                _prevOrderNumber = newOrderNumber;
            }
        }

        //オブジェクトプール
        public GameObject CreatePoolObject()
        {
            return Instantiate(_avaterPrefab);
        }

        public void GetPoolObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        public void ReleasePoolObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        public void DestroyPoolObject(GameObject obj)
        {
            Destroy(obj);
        }

        //モートン座標への変換
        private Vector3 ConvertToMortonPosition(Vector3 position)
        {
            var mortonPosition = new Vector3(
                (position.x - _downPos.x) / transform.localScale.x * (int)_depth,
                (position.y - _downPos.y) / transform.localScale.y * (int)_depth,
                (position.z - _downPos.z) / transform.localScale.z * (int)_depth);

            return mortonPosition;
        }

        //モートン空間番号を取得
        private int GetOrderNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= (int)_depth
                || position.y < 0 || position.y >= (int)_depth
                || position.z < 0 || position.z >= (int)_depth) return -1;

            return BitSeparate3D((int)position.x)
                | BitSeparate3D((int)position.y) << 1
                | BitSeparate3D((int)position.z) << 2;
        }

        //ビット分割（3D)
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
            var orderNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int orderNumber = GetOrderNumber3D(new Vector3(position.x + dx[i], position.y + dy[i], position.z + dz[i]));
                orderNumbers.Add(orderNumber);
            }

            //モートン空間外を除外
            orderNumbers.RemoveAll(x => x.Equals(-1));

            //アバターを配置
            foreach (var orderNumber in orderNumbers)
            {
                foreach (var otherPosition in _otherPositionSet[orderNumber])
                {
                    var avater = _avaterPool.Get();
                    avater.transform.position = otherPosition;
                }
            }
        }
    }
}
