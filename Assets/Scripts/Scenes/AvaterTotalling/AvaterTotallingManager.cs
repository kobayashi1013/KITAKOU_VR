using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Constant;
using Utils;

namespace Scenes.AvaterTotalling
{
    public class AvaterTotallingManager : MonoBehaviour
    {
        private static readonly int PLANEOBJECT_SCALERATE = 10; //プレーンオブジェクトのスケール比率

        private void Start()
        {
            //アバターの人数
            var avaterNum = 0;

            //収容人数計測
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //部屋が配置なしの時
                if (SystemData.Instance.roomDataList[floor.roomId].state == RoomState.Empty) continue;

                float lengthX = SystemData.Instance.roomDataList[floor.roomId].width0;
                float lengthZ = SystemData.Instance.roomDataList[floor.roomId].width1;

                while (lengthX < floor.transform.localScale.x * PLANEOBJECT_SCALERATE)
                {
                    lengthZ = SystemData.Instance.roomDataList[floor.roomId].width1;
                    while (lengthZ < floor.transform.localScale.z * PLANEOBJECT_SCALERATE)
                    {
                        avaterNum++;
                        lengthZ += SystemData.Instance.roomDataList[floor.roomId].width1;
                    }

                    lengthX += SystemData.Instance.roomDataList[floor.roomId].width0;
                }
            }

            SystemData.Instance.SetAvaterTotallingNum(avaterNum);
            SceneManager.LoadScene((int)SceneName.LocationSettingScene);
        }
    }
}
