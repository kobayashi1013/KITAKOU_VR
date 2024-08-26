using Constant;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.SceneManagement;

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
