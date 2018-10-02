using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FaceRememberLogic : MonoBehaviour {

    string msCreatPersonUrl = "https://api.cognitive.azure.cn/face/v1.0/persongroups/test0/persons";
    string msAddFaceUrl = "https://api.cognitive.azure.cn/face/v1.0/persongroups/test0/persons/";
    string msTrainUrl = "https://api.cognitive.azure.cn/face/v1.0/persongroups/test0/train";
    string msGetTrainStatusUrl = "https://api.cognitive.azure.cn/face/v1.0/persongroups/test0/training";
    string key = "d3e1656a556e41f9bd7db8f3d18a5321";
    string id;

    public GameObject photo;
    public GameObject tip;
    public GameObject guessname;
    public GameObject RememberCanvas;

    private bool IsResponseValid(UnityWebRequest www)
    {
        if ((www.isNetworkError || www.isHttpError) && www.responseCode != 200)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);
            return false;
        }
        else
        {
         //   Debug.Log("Picture upload complete!");
            return true;
        }
    }

    private bool IsResponseValid2(UnityWebRequest www)
    {
        if ((www.isNetworkError || www.isHttpError) && www.responseCode != 202)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);
            return false;
        }
        else
        {
            return true;
        }
    }

    void Remember(string name)
    {
        RememberCanvas.SetActive(true);
        StartCoroutine(CreatePerson(name));
    }

    void SetPhoto()
    {
        photo.SendMessageUpwards("GetPhoto", SendMessageOptions.DontRequireReceiver);
    }

    void SetTip(string s)
    {
        tip.GetComponent<TextMesh>().text = s;
    }

    void SetName(string s)
    {
        guessname.GetComponent<TextMesh>().text = s;
    }

    IEnumerator<object> CreatePerson(string name)
    {
        string body = "{\"name\":\"" + name + "\"}";
        UnityWebRequest createperson = UnityWebRequest.Put(msCreatPersonUrl, body);
        createperson.chunkedTransfer = false;
        createperson.method = "POST";
        createperson.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
        createperson.SetRequestHeader("Content-Type", "application/json");
        createperson.downloadHandler = new DownloadHandlerBuffer();
        yield return createperson.SendWebRequest();
        if (!IsResponseValid(createperson))
            yield break;
        string createpersonresponse = createperson.downloadHandler.text;

        JSONObject createpersonjson = new JSONObject(createpersonresponse);
        char trimChar = '\"';
        id = createpersonjson.GetField("personId").ToString().Trim(trimChar);
        Debug.Log("CreatePerson Success!id : " + id);
        SetTip("CreatePerson status : Success");

        if(createperson.responseCode==200) StartCoroutine(AddFace(id));
    }

    IEnumerator<object> AddFace(string id)
    {
        string filepath = Path.Combine(Application.persistentDataPath, "cropped.png");
        double startTime = (double)Time.time;
        //创建文件流
        FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度的缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取liu
        //   fileStream.Close();
        fileStream.Dispose();
        fileStream = null;

        //使用微软认知服务Api进行人脸检测
        string url = msAddFaceUrl + id + "/persistedFaces";
        var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(bytes);
        // www.method="POST";
        www.chunkedTransfer = false;

        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        if (!IsResponseValid(www))
            yield break;
        string responseString = www.downloadHandler.text;
        Debug.Log("Add Face Success!");
        SetTip("Add Face status : Success");
        if (www.responseCode == 200) StartCoroutine(Train());
    }

    IEnumerator<object> Train()
    {
        Debug.Log("Enter Train()");
        
        var train = new UnityWebRequest(msTrainUrl, "POST");
        train.chunkedTransfer = false;
        train.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
        train.downloadHandler = new DownloadHandlerBuffer();
        yield return train.SendWebRequest();
        if (!IsResponseValid2(train))
            yield break;
        string trainresponse = train.downloadHandler.text;
        
        /*
        UnityWebRequest train = UnityWebRequest.Put(msTrainUrl, "");
        train.chunkedTransfer = false;
        train.method = "POST";
        train.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
        train.downloadHandler = new DownloadHandlerBuffer();
        yield return train.SendWebRequest();
    //    if (!IsResponseValid(train))
    //        yield break;
        string trainresponse = train.downloadHandler.text;
        */

        Debug.Log("trainresponse : " + trainresponse);
        if (train.responseCode == 202) StartCoroutine(GetTrainStatus());
    }

    IEnumerator<object> GetTrainStatus()
    {
        var trainstatus = new UnityWebRequest(msGetTrainStatusUrl, "GET");
        trainstatus.chunkedTransfer = false;
        trainstatus.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
        trainstatus.downloadHandler = new DownloadHandlerBuffer();
        yield return trainstatus.SendWebRequest();
        if (!IsResponseValid(trainstatus))
            yield break;
        string trainresponse = trainstatus.downloadHandler.text;
        JSONObject trainstatusjson = new JSONObject(trainresponse);
        string s = trainstatusjson.GetField("status").ToString();
        Debug.Log("Train status : " + s);
        SetTip("Train model status : " + s);
    }




    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
