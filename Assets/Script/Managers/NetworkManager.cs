using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager> {
    //public string baseUrl = "http://52.78.149.126/";
    public string baseUrl = "http://ajwapi-dev.fbl.kr/";
    protected NetworkManager() {
#if DEVELOPMENT_BUILD
        baseUrl = "http://ajwapi-dev.fbl.kr/";
        Debug.Log("Development Build");
#endif
    }
    public delegate void Callback(HttpResponse response);

    //public string baseUrl = "http://ec2-52-78-149-126.ap-northeast-2.compute.amazonaws.com:8000/";

    public void request(string method, string url, WWWForm data, Callback callback, bool neeAuthor = true) {
        StartCoroutine(_request(method, url, data, callback, neeAuthor));
    }
    public void request(string method, string url, Callback callback, bool neeAuthor = true) {
        request(method, url, null, callback, neeAuthor);
    }

    IEnumerator _request(string method, string url, WWWForm data, Callback callback, bool needAuthor = true){
        UnityWebRequest _www;
        switch(method){
            case "POST":
                _www = UnityWebRequest.Post(url, data);
                break;
            case "PUT":
                _www = UnityWebRequest.Put(url,data.data);
                _www.SetRequestHeader("Content-Type","application/x-www-form-urlencoded");
                break;
            case "DELETE":
                _www = UnityWebRequest.Delete(url);
                break;
            case "GET":
            default:
                _www = UnityWebRequest.Get(url);
                break;
        }

        if (needAuthor) {
            //_www.SetRequestHeader("Authorization", "Token " + GameManager.Instance.userStore.userTokenId);
            //_www.downloadHandler = new DownloadHandlerBuffer();
        }

        using(UnityWebRequest www = _www){
            yield return www.Send();
            callback(new HttpResponse(www));
        }
    }
}

public class HttpResponse {
    public bool isError;
    public string errorMessage;
    public string data;
    public long responseCode;
    public UnityWebRequest request;
    public string header;
    public HttpResponse(UnityWebRequest _request){
        request = _request;
        responseCode = _request.responseCode;
        isError = _request.isNetworkError;
        errorMessage = _request.error;
        data = _request.downloadHandler.text;
        header = _request.GetResponseHeader("Link");
    }
}