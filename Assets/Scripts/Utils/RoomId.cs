using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.LocationSetting.Manager;

namespace Utils
{
    public class RoomId : MonoBehaviour
    {
        public string roomId = "default";

        public void PushRoomButton()
        {
            LocationSettingManager.Instance.PushRoomButtons(roomId);
        }
    }
}
