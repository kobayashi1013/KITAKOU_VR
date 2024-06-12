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

            StartCoroutine(ApiConnection("今から災害がおこります。あなたは私を励ましてください。"));
        }

        private IEnumerator ApiConnection(string userMessage)
        {
            WWWForm form = new WWWForm();
            form.AddField("userMessage", userMessage);
            UnityWebRequest request = UnityWebRequest.Post(_googleAppsScriptUrl, form);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError //接続エラー
                || request.result == UnityWebRequest.Result.ProtocolError) //プロトコルエラー
            {
                Debug.LogError("error : " + request.result);
            }
            else //成功
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
}
