using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Prefabs
{
    public class Gemini : MonoBehaviour
    {
        [SerializeField] private string _googleAppsScriptId; //Google App Script ID

        private string _googleAppsScriptUrl;

        private void Start()
        {
            //GoogleAppScriptURL
            _googleAppsScriptUrl = "https://script.google.com/macros/s/" + _googleAppsScriptId + "/exec";

            StartCoroutine(ApiConnection("������ЊQ��������܂��B���Ȃ��͎����܂��Ă��������B"));
        }

        private IEnumerator ApiConnection(string userMessage)
        {
            WWWForm form = new WWWForm();
            form.AddField("userMessage", userMessage);
            UnityWebRequest request = UnityWebRequest.Post(_googleAppsScriptUrl, form);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError //�ڑ��G���[
                || request.result == UnityWebRequest.Result.ProtocolError) //�v���g�R���G���[
            {
                Debug.LogError("error : " + request.result);
            }
            else //����
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
}
