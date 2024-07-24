using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Constant;
using TMPro;
using Utils;
using Struct;
using System;

namespace Scenes.LocationSetting
{
    public class LocationSettingManager : MonoBehaviour
    {
        public static readonly float WIDTH_VALUE_MIN = 0.5f; //幅の限界値
        public static readonly float WIDTH_VALUE_MAX = 5.0f;

        [Header("SceneObject")]
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private TMP_Dropdown _roomState;
        [SerializeField] private TMP_Text _widthValueText0;
        [SerializeField] private TMP_Text _widthValueText1;
        [SerializeField] private Slider _widthSlider0;
        [SerializeField] private Slider _widthSlider1;
        [SerializeField] private TMP_InputField _widthInputField0;
        [SerializeField] private TMP_InputField _widthInputField1;

        public static LocationSettingManager Instance;
        private string _prevRoomId = "default";

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }

        public void PushBasicSettingButton()
        {
            SaveRoomData(_prevRoomId);
            SceneManager.LoadScene((int)SceneName.BasicSettingScene);
        }

        public void PushBackButton()
        {
            SaveRoomData(_prevRoomId);
            SceneManager.LoadScene((int)SceneName.StartScene);
        }

        public void PushRoomButtons(string id)
        {
            SaveRoomData(_prevRoomId);
            LoadRoomData(id);

            _prevRoomId = id;
        }

        public void OnChangeStateButton()
        {

        }

        public void MoveSlider0()
        {
            _widthValueText0.text = _widthSlider0.value.ToString("F1");
        }

        public void MoveSlider1()
        {
            _widthValueText1.text = _widthSlider1.value.ToString("F1");
        }

        public void OnEditInputField0()
        {
            if (_widthInputField0.text == "") return;
            var value = WidthValueLimit(float.Parse(_widthInputField0.text));

            _widthValueText0.text = value.ToString("F1");
            _widthSlider0.value = value;
            _widthInputField0.text = null;
        }

        public void OnEditInputField1()
        {
            if (_widthInputField1.text == "") return;
            var value = WidthValueLimit(float.Parse(_widthInputField1.text));

            _widthValueText1.text = value.ToString("F1");
            _widthSlider1.value = value;
            _widthInputField1.text = null;
        }

        /// <summary>
        /// 幅値の制限
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float WidthValueLimit(float value)
        {
            //制限
            if (value < WIDTH_VALUE_MIN) return WIDTH_VALUE_MIN;
            if (value > WIDTH_VALUE_MAX) return WIDTH_VALUE_MAX;

            return value;
        }

        /// <summary>
        /// 部屋データのUI表示
        /// </summary>
        /// <param name="id"></param>
        private void LoadRoomData(string id)
        {
            //部屋名
            _roomNameText.text = SystemData.Instance.roomDataList[id].name;

            //部屋状態
            _roomState.value = (int)SystemData.Instance.roomDataList[id].state;
            _widthValueText0.text = SystemData.Instance.roomDataList[id].width0.ToString("F1");
            _widthSlider0.value = SystemData.Instance.roomDataList[id].width0;

            _widthValueText1.text = SystemData.Instance.roomDataList[id].width1.ToString("F1");
            _widthSlider1.value = SystemData.Instance.roomDataList[id].width1;
        }

        /// <summary>
        /// 部屋データのセーブ
        /// </summary>
        /// <param name="id"></param>
        private void SaveRoomData(string id)
        {
            if (id == "default") return;

            var roomData = new RoomData();
            roomData.name = SystemData.Instance.roomDataList[id].name;
            roomData.state = (RoomState)Enum.ToObject(typeof(RoomState), _roomState.value);
            roomData.width0 = float.Parse(_widthValueText0.text);
            roomData.width1 = float.Parse(_widthValueText1.text);

            SystemData.Instance.roomDataList[id] = roomData;
        }
    }
}
