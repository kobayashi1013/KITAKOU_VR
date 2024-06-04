using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Prefabs
{
    public class Gemini : MonoBehaviour
    {
        [System.Serializable]
        public class ResponseData
        {
            //Geminiからのリスポンス
            public int answer;
        }

        [SerializeField] private string _googleAppScriptId; //Google App Script ID

        private void Start()
        {
            StartCoroutine(ApiConnection("test"));
        }

        private IEnumerator ApiConnection(string userText)
        {
            string url = "https://script.google.com/macros/s/" + _googleAppScriptId + "/exec?userText=" + UnityWebRequest.EscapeURL(userText);
            Debug.Log(url);
            var request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError //接続エラー
                || request.result == UnityWebRequest.Result.ProtocolError) //プロトコルエラー
            {
                Debug.LogError("error : " + request.result);
            }
            else //成功
            {
                var result = request.downloadHandler.text;
                Debug.Log(result);
                var response = JsonUtility.FromJson<ResponseData>(result);
            }
        }
    }
}
