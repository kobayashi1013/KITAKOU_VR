using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Constant;
using UnityEngine.XR.Management;

namespace Scenes.Start
{
    public class StartManager : MonoBehaviour
    {
        [SerializeField] private GameObject _applicationHandler;

        private void Start()
        {
            //�A�v���P�[�V�����J�n
            if (SystemData.Instance == null)
            {
                //�A�v���P�[�V�����n���h��
                Instantiate(_applicationHandler);

                //�V�X�e���f�[�^����
                SystemData.Instance = new SystemData();

                //XR�̖�����
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }

        public void PushVrButton()
        {
            StartCoroutine(StartVR());
        }

        public IEnumerator StartVR()
        {
            //Debug.Log("Start VR Mode");

            //XR�̗L����
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();

                //VR���[�h
                SystemData.Instance.SetSceneMode(SceneMode.VR);

                //���C���V�[���ֈړ�
                if (SystemData.Instance.simulationTime == SimulationTime.Morning)
                    SceneManager.LoadScene((int)SceneName.InMainMorningScene);
                else if (SystemData.Instance.simulationTime == SimulationTime.Night)
                    SceneManager.LoadScene((int)SceneName.InMainNightScene);
                else if (SystemData.Instance.simulationTime == SimulationTime.Experiment)
                    SceneManager.LoadScene((int)SceneName.Experiment);
            }
        }

        public void PushPcButton()
        {
            //Debug.Log("Start PC Mode");

            //PC���[�h
            SystemData.Instance.SetSceneMode(SceneMode.PC);

            //���C���V�[���ֈړ�
            if (SystemData.Instance.simulationTime == SimulationTime.Morning)
                SceneManager.LoadScene((int)SceneName.InMainMorningScene);
            else if (SystemData.Instance.simulationTime == SimulationTime.Night)
                SceneManager.LoadScene((int)SceneName.InMainNightScene);
            else if (SystemData.Instance.simulationTime == SimulationTime.Experiment)
                SceneManager.LoadScene((int)SceneName.Experiment);
        }

        public void PushSettingButton()
        {
            SceneManager.LoadScene((int)SceneName.BasicSettingScene);
        }
    }
}
