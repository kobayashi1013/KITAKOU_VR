using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Constant;
using Struct;
using Utils;

namespace Scenes.FileSetting
{
    public class FileSettingManager : MonoBehaviour
    {
        [SerializeField] private GameObject _errorMessage;

        public void PushBackButton()
        {
            SceneManager.LoadScene((int)SceneName.StartScene);
        }

        public void PushBasicSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.BasicSettingScene);
        }

        public void PushLocationSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.LocationSettingScene);
        }

        /// <summary>
        /// Csvファイルのインポート
        /// </summary>
        public void ImportConfigFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //エクスプローラー表示

            openFileDialog.Filter = "txtファイル|*.txt"; //txtを開く
            openFileDialog.CheckFileExists = true; //存在しないファイルに警告を出す

            openFileDialog.ShowDialog(); //エクスプローラー表示
            var file = openFileDialog.FileName; //ファイル名

            string content = File.ReadAllText(file);
            string[] lines = content.Split('\n'); //ファイルを改行で分割

            var roomDataList = new Dictionary<string, RoomData>();
            foreach (string line in lines)
            {
                string[] text = line.Split(',');

                if (SystemData.Instance.roomDataList.ContainsKey(text[0]))
                {
                    var roomData = new RoomData();
                    roomData.name = SystemData.Instance.roomDataList[text[0]].name;

                    //データ変換
                    try
                    {
                        roomData.state = (RoomState)Enum.Parse(typeof(RoomState), text[2]);
                        roomData.width0 = float.Parse(text[3]);
                        roomData.width1 = float.Parse(text[4]);
                    }
                    catch
                    {
                        Debug.LogError("Error : change data type");
                        _errorMessage.SetActive(true);
                        return; //コンフィグインポートを中止
                    }

                    roomDataList.Add(text[0], roomData);
                    _errorMessage.SetActive(false);
                }
            }

            SystemData.Instance.roomDataList = roomDataList; //データリストへ反映
        }
    }
}
