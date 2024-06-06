using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Prefabs
{
    public class Gemini : MonoBehaviour
    {
        [SerializeField] private string _googleAppScriptId; //Google App Script ID

        private string _googleAppScriptUrl;

        private void Start()
        {
            //GoogleAppScriptURL
            _googleAppScriptUrl = "https://script.google.com/macros/s/" + _googleAppScriptId + "/exec";

            StartCoroutine(ApiConnection("今から災害がおこります。あなたは私を励ましてください。"));
        }

        private IEnumerator ApiConnection(string userMessage)
        {
            WWWForm form = new WWWForm();
            form.AddField("userMessage", userMessage);
            UnityWebRequest request = UnityWebRequest.Post(_googleAppScriptUrl, form);

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
