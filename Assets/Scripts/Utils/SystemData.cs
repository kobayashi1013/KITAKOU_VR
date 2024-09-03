using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Constant;
using Struct;
using System.Runtime.CompilerServices;

namespace Utils
{
    public class SystemData
    {
        public static SystemData Instance;
        public SceneMode sceneMode { get; private set; }
        public MortonModelDepth mortonModelDepth { get; private set; } = MortonModelDepth.depth4;
        public bool useFlooding { get; private set; } = false;
        public bool useEvent { get; private set; } = false;
        public bool useRestrictedArea { get; private set; } = false;
        public int avaterTotallingNum { get; private set; }
        public Dictionary<string, RoomData> roomDataList = new Dictionary<string, RoomData>();

        public SystemData()
        {
            LoadSystemConfig();
            LoadRoomConfig();
         }

        /// <summary>
        /// �V�X�e���R���t�B�O�̓ǂݏo��
        /// </summary>
        private void LoadSystemConfig()
        {
            //�R���t�B�O�t�@�C�������݂��Ȃ��ꍇ
            string filePath = Path.Combine(Application.persistentDataPath, "SystemConfig.csv");
            if (File.Exists(filePath) == false)
            {
                //�I���W�i���t�@�C�����R�s�[
                var textAsset = Resources.Load("File/SystemConfigOriginal") as TextAsset;
                File.WriteAllText(filePath, textAsset.text);
            }

            //�t�@�C���̓��e��ǂݏo��
            string content = File.ReadAllText(filePath);
            string[] textData = content.Split("\n");

            //�R���t�B�O�ݒ�
            mortonModelDepth = (MortonModelDepth)Enum.Parse(typeof(MortonModelDepth), textData[0]);
            useFlooding = bool.Parse(textData[1]);
            useEvent = bool.Parse(textData[2]);
            useRestrictedArea = bool.Parse(textData[3]);
        }

        /// <summary>
        /// �V�X�e���R���t�B�O�̏�������
        /// </summary>
        public void SaveSystemConfig()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "SystemConfig.csv");
            string content = null;

            content += mortonModelDepth.ToString() + "\n"
                + useFlooding.ToString() + "\n"
                + useEvent.ToString() + "\n"
                + useRestrictedArea.ToString();

            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// ���[���R���t�B�O�̓ǂݏo��
        /// </summary>
        private void LoadRoomConfig()
        {
            //�R���t�B�O�t�@�C�������݂��Ȃ��ꍇ
            string filePath = Path.Combine(Application.persistentDataPath, "RoomConfig.csv");
            if (File.Exists(filePath) == false)
            {
                //�I���W�i���t�@�C�����R�s�[
                var textAsset = Resources.Load("File/RoomConfigOriginal") as TextAsset;
                File.WriteAllText(filePath, textAsset.text);
            }

            //�t�@�C���̓��e��ǂݏo��
            string content = File.ReadAllText(filePath);
            var roomDataList = new Dictionary<string, RoomData>();

            string[] lines = content.Split("\n");
            foreach (string line in lines)
            {
                string[] textData = line.Split(",");
                RoomData roomData = new RoomData();

                roomData.name = textData[1];
                roomData.state = (RoomState)Enum.Parse(typeof(RoomState), textData[2]);
                roomData.width0 = float.Parse(textData[3]);
                roomData.width1 = float.Parse(textData[4]);

                roomDataList.Add(textData[0], roomData);
            }

            this.roomDataList = roomDataList;
        }

        /// <summary>
        /// ���[���R���t�B�O�̏����o��
        /// </summary>
        public void SaveRoomConfig()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "RoomConfig.csv");

            string content = null;
            foreach (var key in roomDataList.Keys)
            {
                content += key + ","
                    + roomDataList[key].name + ","
                    + roomDataList[key].state.ToString() + ","
                    + roomDataList[key].width0.ToString("F1") + ","
                    + roomDataList[key].width1.ToString("F1") + "\n";
            }
            content = content.Remove(content.Length - 1);

            File.WriteAllText(filePath, content);
        }

        public void SetSceneMode(SceneMode mode)
        {
            //Debug.Log("SetSceneMode(" + mode + ")");
            sceneMode = mode;
        }

        public void SetAvaterTotallingNum(int num)
        {
            //Debug.Log("SetAvaterTotallingNum(" + num + ")");
            avaterTotallingNum = num;
        }

        public void SetMortonModelDepth(MortonModelDepth depth)
        {
            //Debug.Log("SetMortonModelDepth(" + depth + ")");
            mortonModelDepth = depth;
        }

        public void SetUseFlooding(bool state)
        {
            //Debug.Log("SetUseFlooding(" + state + ")");
            useFlooding = state;
        }

        public void SetUseEvent(bool state)
        {
            //Debug.Log("SetUseEvent(" + state + ")");
            useEvent = state;
        }

        public void SetUseRestrictedArea(bool state)
        {
            //Debug.Log("SetUseRestrictedArea(" + state + ")");
            useRestrictedArea = state;
        }
    }
}
