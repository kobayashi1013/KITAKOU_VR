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
        private static readonly int Y_DEPTH = 3; //三階建て

        [Header("SceneObjects")]
        [SerializeField] private Transform _objectSetTransform; //オブジェクトセット
        [SerializeField] private Transform _mortonModelTransform; //モートンモデル
        [Header("PrefabObjects")]
        [SerializeField] private PrefabTable _prefabTable;
        [SerializeField] private float _additionPrefabSpawnRate = 0.5f; //追加プレハブのスポーン確率

        private Vector3 _mortonModelAnchor; //モートンモデルの端
        private Vector3 _mortonModelScale; //モートンモデルスケール
        private List<Manager> _managers = new List<Manager>();
        private List<int> _prevNeighbor = new List<int>();
        private int _prevSpace = -1; //前インターバルのプレイヤー空間

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
                if (!_positions.ContainsKey(space)) //辞書の追加
                {
                    _positions.Add(space, new List<Vector3>());
                }

                _positions[space].Add(position); //座標の登録
            }

            public void Remover(List<int> neighbors)
            {
                //オブジェクト削除
                foreach (var neighbor in neighbors)
                {
                    //キー確認
                    if (!_positions.ContainsKey(neighbor)) continue;

                    //オブジェクト削除
                    foreach (var avater in _releases[neighbor])
                    {
                        _pool.Release(avater);
                    }

                    //キー開放
                    _releases.Remove(neighbor);
                }
            }

            public void Adder(List<int> neighbors)
            {
                //オブジェクト追加
                foreach (var neighbor in neighbors)
                {
                    //キー確認
                    if (!_positions.ContainsKey(neighbor)) continue;

                    //キー作成
                    if (!_releases.ContainsKey(neighbor))
                    {
                        _releases.Add(neighbor, new List<GameObject>());
                    }

                    //アバター追加
                    foreach (var position in _positions[neighbor])
                    {
                        //オブジェクト追加
                        var avater = _pool.Get();
                        avater.transform.position = position;
                        avater.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                        //辞書登録
                        _releases[neighbor].Add(avater);
                    }
                }
            }
        }

        private void Start()
        {
            //モートンモデル空間定義（座標）
            _mortonModelAnchor = _mortonModelTransform.transform.position - _mortonModelTransform.transform.localScale / 2; //端座標
            _mortonModelScale = _mortonModelTransform.transform.localScale; //ローカルスケール
            Destroy(_mortonModelTransform.gameObject);

            //マネージャー作成
            foreach (var prefab in _prefabTable.prefabs)
            {
                _managers.Add(new Manager(prefab, _objectSetTransform));
            }
            
            //座標回収
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //配置なし
                if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.Empty)
                {
                    Destroy(floor.transform.gameObject);
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
                        var circleMoveNum = (int)Random.Range(0, 360);
                        var circleMove = new Vector3(0.2f * Mathf.Cos(circleMoveNum), 0, 0.2f * Mathf.Sin(circleMoveNum));
                        position = offset + floor.transform.position + circleMove;

                        //座標の登録
                        var mortonModelPosition = ConvertToMortonModelPosition(position); //モートンモデル座標に変換
                        var mortonSpaceNumber = GetSpaceNumber3D(mortonModelPosition); //スペース番号の取得

                        //配置
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

        //任意座標からモートンモデル座標への変換
        private Vector3 ConvertToMortonModelPosition(Vector3 position)
        {
            var mortonModelPosition = new Vector3(
                (position.x - _mortonModelAnchor.x) / _mortonModelScale.x * (int)SystemData.Instance.mortonModelDepth,
                (position.y - _mortonModelAnchor.y) / _mortonModelScale.y * (int)Y_DEPTH,
                (position.z - _mortonModelAnchor.z) / _mortonModelScale.z * (int)SystemData.Instance.mortonModelDepth);

            return mortonModelPosition;
        }

        //モートン空間番号の取得
        private int GetSpaceNumber3D(Vector3 position)
        {
            if (position.x < 0 || position.x >= (int)SystemData.Instance.mortonModelDepth
                || position.y < 0 || position.y >= (int)Y_DEPTH
                || position.z < 0 || position.z >= (int)SystemData.Instance.mortonModelDepth) return -1;

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

        private List<int> Updater(Vector3 playerPosition, List<int> prevNeighbor)
        {
            //前後空間
            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1, -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int[] dz = { -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            //隣接空間を取得
            var neighbor = new List<int>();
            for (int i = 0; i < 27; i++)
            {
                int number = GetSpaceNumber3D(new Vector3(playerPosition.x + dx[i], playerPosition.y + dy[i], playerPosition.z + dz[i]));
                neighbor.Add(number);
            }

            //追加・削除空間の取得
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