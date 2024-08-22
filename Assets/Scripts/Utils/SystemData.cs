using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Constant;
using Struct;

namespace Utils
{
    public class SystemData
    {
        public static SystemData Instance;
        public SceneMode sceneMode { get; private set; }
        public MortonModelDepth mortonModelDepth { get; private set; } = MortonModelDepth.depth4;
        public bool useFlooding { get; private set; } = false;
        public int avaterTotallingNum { get; private set; }
        public Dictionary<string, RoomData> roomDataList = new Dictionary<string, RoomData>();

        public SystemData()
        {
            //RoomDataì«Ç›çûÇ›
            var csvFile = Resources.Load("Csv/RoomDataCsv") as TextAsset;
            var csvData = new List<string[]>();
            var roomData = new RoomData();
            var reader = new StringReader(csvFile.text);

            int height = 0;
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                csvData.Add(line.Split(','));
                height++;
            }

            for (int i = 0; i < height; i++)
            {
                roomData.name = csvData[i][1];
                roomData.state = RoomState.Student;
                roomData.width0 = 1.0f;
                roomData.width1 = 1.0f;

                roomDataList.Add(csvData[i][0], roomData);
            }
        }

        public void SetSceneMode(SceneMode mode)
        {
            //Debug.Log("SetSceneMode(" + mode + ")");
            sceneMode = mode;
        }

        public void SetMortonModelDepth(MortonModelDepth depth)
        {
            //Debug.Log("SetMortonModelDepth(" + depth + ")");
            mortonModelDepth = depth;
        }

        public void SetUseFlooding(bool value)
        {
            //Debug.Log("SetUseFlooding(" + value + ")");
            useFlooding = value;
        }

        public void SetAvaterTotallingNum(int num)
        {
            //Debug.Log("SetAvaterTotallingNum(" + num + ")");
            avaterTotallingNum = num;
        }
    }
}
