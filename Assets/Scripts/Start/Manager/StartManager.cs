using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Constant;

namespace Start.Manager
{
    public class StartManager : MonoBehaviour
    {
        private void Awake()
        {
            //�V�X�e���f�[�^�ݒ�
            if (SystemData.Instance == null)
            {
                SystemData.Instance = new SystemData();
            }
        }

        public void PushVrButton()
        {
            //Debug.Log("Start VR Mode");

            //VR���[�h
            SystemData.Instance.SetSceneMode(SceneMode.VR);

            //���C���V�[���ֈړ�
            SceneManager.LoadScene((int)SceneName.InScene);
        }

        public void PushPcButton()
        {
            //Debug.Log("Start PC Mode");

            //PC���[�h
            SystemData.Instance.SetSceneMode(SceneMode.PC);

            //���C���V�[���ֈړ�
            SceneManager.LoadScene((int)SceneName.InScene);
        }
    }
}