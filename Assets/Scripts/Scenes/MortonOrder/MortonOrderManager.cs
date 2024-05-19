using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

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
        [Header("Parameters")]
        [SerializeField] private MortonOrderDepth _depth = MortonOrderDepth.depth8;

        private Vector3 _downPos; //モデルの端
        private Vector3 _upPos;
        private int _prevOrderNumber = -1;
        private Dictionary<int, List<Vector3>> _otherPosSet = new Dictionary<int, List<Vector3>>(); //Otherの座標集合

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
                var otherMortonPos = ConvertPositionToMorton(other.transform.position);
                int orderNumber = GetOrderNumber3D((int)otherMortonPos.x, (int)otherMortonPos.y, (int)otherMortonPos.z);
                if (_otherPosSet.ContainsKey(orderNumber)) //キーが存在する
                {
                    _otherPosSet[orderNumber].Add(other.transform.position);
                }
                else //キーが存在しない
                {
                    _otherPosSet.Add(orderNumber, new List<Vector3>());
                    _otherPosSet[orderNumber].Add(other.transform.position);
                }

                //Otherオブジェクトの消去
                Destroy(other);
            }
        }

        void Update()
        {
            //座標変換
            var playerMortonPos = ConvertPositionToMorton(_player.transform.position);

            //空間変更の確認
            int newOrderNumber = GetOrderNumber3D((int)playerMortonPos.x, (int)playerMortonPos.y, (int)playerMortonPos.z);
            if (newOrderNumber != _prevOrderNumber)
            {
                CreateAvater(playerMortonPos);
                _prevOrderNumber = newOrderNumber;
            }
        }

        //モートン座標への変換
        private Vector3 ConvertPositionToMorton(Vector3 vec)
        {
            var mortonPos = new Vector3(
                (vec.x - _downPos.x) / transform.localScale.x * (int)_depth,
                (vec.y - _downPos.y) / transform.localScale.y * (int)_depth,
                (vec.z - _downPos.z) / transform.localScale.z * (int)_depth);

            return mortonPos;
        }

        private int GetOrderNumber3D(int x, int y, int z)
        {
            if (x < 0 || x >= (int)_depth
                || y < 0 || y >= (int)_depth
                || z < 0 || z >= (int)_depth) return -1;

            return BitSeparate3D(x)
                | BitSeparate3D(y) << 1
                | BitSeparate3D(z) << 2;
        }

        private int BitSeparate3D(int data)
        {
            data = (data | data << 8) & 0x0000f00f;
            data = (data | data << 4) & 0x000c30c3;
            data = (data | data << 2) & 0x00249249;
            return data;
        }

        private void CreateAvater(Vector3 playerMortonPos)
        {
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            var orderNumbers = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                orderNumbers.Add(GetOrderNumber3D((int)playerMortonPos.x + dx[i], (int)playerMortonPos.y + dy[i], (int)playerMortonPos.z + dz[i]));
                Debug.Log(orderNumbers[i]);
            }
        }
    }
}
