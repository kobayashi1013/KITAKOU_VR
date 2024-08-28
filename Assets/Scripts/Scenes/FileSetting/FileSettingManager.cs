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
        /// txtファイルのインポート
        /// </summary>
        public void ImportConfigFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //エクスプローラー表示

            openFileDialog.Filter = "テキスト ドキュメント (*.txt)|*.txt"; //txtを開く
            openFileDialog.CheckFileExists = true; //存在しないファイルに警告を出す

            openFileDialog.ShowDialog(); //エクスプローラー表示
            string filePath = openFileDialog.FileName; //ファイル名
            if (filePath == "") return;

            string content = File.ReadAllText(filePath);
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
                else
                {
                    Debug.LogError("Error : change data type");
                    _errorMessage.SetActive(true);
                    return;
                }
            }

            SystemData.Instance.roomDataList = roomDataList; //データリストへ反映
        }

        /// <summary>
        /// txtファイルのエクスポート
        /// </summary>
        public void ExportConfigFile()
        {
            //部屋データをテキスト化
            string content = null;
            foreach (var key in SystemData.Instance.roomDataList.Keys)
            {
                content += key + ","
                    + SystemData.Instance.roomDataList[key].name + ","
                    + SystemData.Instance.roomDataList[key].state.ToString() + ","
                    + SystemData.Instance.roomDataList[key].width0.ToString("F1") + ","
                    + SystemData.Instance.roomDataList[key].width1.ToString("F1") + "\n";
            }
            content = content.Remove(content.Length - 1);

            //ダイアログ
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "テキスト ドキュメント (*.txt)| *.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "config_room";

            saveFileDialog.ShowDialog();
            string filePath = saveFileDialog.FileName;
            if (filePath == "") return;

            //ファイルセーブ
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// テンプレートの読み込み
        /// </summary>
        public void ConfigFileDownload()
        {
            //テンプレートファイル読み込み
            var csvFile = Resources.Load("Csv/RoomConfigOriginal") as TextAsset;
            string content = csvFile.text;

            //ダイアログ表示
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "テキスト ドキュメント (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "config_room";

            saveFileDialog.ShowDialog();
            string filePath = saveFileDialog.FileName;
            if (filePath == "") return;

            //ファイルセーブ
            File.WriteAllText(filePath , content);
        }
    }
}
