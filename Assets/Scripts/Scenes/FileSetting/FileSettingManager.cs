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
        /// Csv�t�@�C���̃C���|�[�g
        /// </summary>
        public void ImportConfigFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //�G�N�X�v���[���[�\��

            openFileDialog.Filter = "txt�t�@�C��|*.txt"; //txt���J��
            openFileDialog.CheckFileExists = true; //���݂��Ȃ��t�@�C���Ɍx�����o��

            openFileDialog.ShowDialog(); //�G�N�X�v���[���[�\��
            var file = openFileDialog.FileName; //�t�@�C����

            string content = File.ReadAllText(file);
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
            }

            SystemData.Instance.roomDataList = roomDataList; //�f�[�^���X�g�֔��f
        }
    }
}
