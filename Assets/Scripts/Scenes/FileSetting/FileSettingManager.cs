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
        /// txt�t�@�C���̃C���|�[�g
        /// </summary>
        public void ImportConfigFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //�G�N�X�v���[���[�\��

            openFileDialog.Filter = "�e�L�X�g �h�L�������g (*.txt)|*.txt"; //txt���J��
            openFileDialog.CheckFileExists = true; //���݂��Ȃ��t�@�C���Ɍx�����o��

            openFileDialog.ShowDialog(); //�G�N�X�v���[���[�\��
            string filePath = openFileDialog.FileName; //�t�@�C����
            if (filePath == "") return;

            string content = File.ReadAllText(filePath);
            string[] lines = content.Split('\n'); //�t�@�C�������s�ŕ���

            var roomDataList = new Dictionary<string, RoomData>();
            foreach (string line in lines)
            {
                string[] text = line.Split(',');

                if (SystemData.Instance.roomDataList.ContainsKey(text[0]))
                {
                    var roomData = new RoomData();
                    roomData.name = SystemData.Instance.roomDataList[text[0]].name;

                    //�f�[�^�ϊ�
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
                        return; //�R���t�B�O�C���|�[�g�𒆎~
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

            SystemData.Instance.roomDataList = roomDataList; //�f�[�^���X�g�֔��f
        }

        /// <summary>
        /// txt�t�@�C���̃G�N�X�|�[�g
        /// </summary>
        public void ExportConfigFile()
        {
            //�����f�[�^���e�L�X�g��
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

            //�_�C�A���O
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "�e�L�X�g �h�L�������g (*.txt)| *.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "config_room";

            saveFileDialog.ShowDialog();
            string filePath = saveFileDialog.FileName;
            if (filePath == "") return;

            //�t�@�C���Z�[�u
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// �e���v���[�g�̓ǂݍ���
        /// </summary>
        public void ConfigFileDownload()
        {
            //�e���v���[�g�t�@�C���ǂݍ���
            var csvFile = Resources.Load("Csv/RoomConfigOriginal") as TextAsset;
            string content = csvFile.text;

            //�_�C�A���O�\��
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "�e�L�X�g �h�L�������g (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "config_room";

            saveFileDialog.ShowDialog();
            string filePath = saveFileDialog.FileName;
            if (filePath == "") return;

            //�t�@�C���Z�[�u
            File.WriteAllText(filePath , content);
        }
    }
}
