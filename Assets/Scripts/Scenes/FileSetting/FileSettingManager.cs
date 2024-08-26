using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

namespace Scenes.FileSetting
{
    public class FileSettingManager : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ImportFile();
            }
        }

        private void ImportFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "csvƒtƒ@ƒCƒ‹|*.csv";
            openFileDialog.CheckFileExists = false;

            openFileDialog.ShowDialog();
            var importFileName = openFileDialog.FileName;
            Debug.Log(importFileName);
        }
    }
}
