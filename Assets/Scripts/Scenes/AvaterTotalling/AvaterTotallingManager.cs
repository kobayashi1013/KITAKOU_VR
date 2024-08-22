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
        private static readonly int PLANEOBJECT_SCALERATE = 10; //�v���[���I�u�W�F�N�g�̃X�P�[���䗦

        private void Start()
        {
            //�A�o�^�[�̐l��
            var avaterNum = 0;

            //���e�l���v��
            var seedFloorList = FindObjectsOfType<RoomId>();
            foreach (var floor in seedFloorList)
            {
                //�������z�u�Ȃ��̎�
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
