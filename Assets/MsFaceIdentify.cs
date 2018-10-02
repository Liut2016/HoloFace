using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.XR.WSA.WebCam;

public class MsFaceIdentify : MonoBehaviour {

    UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = null;

    Resolution cameraResolution;

    Vector3 cameraPosition;
    Quaternion cameraRotation;

    public GameObject status;
    public GameObject IdPrefab;
    public Image imagePrefab;
    public GameObject img;
    public GameObject canvas;

    public GameObject RememberLogic;
    public GameObject RememberCanvas;

    //微软认知服务Api网址与密钥
    string MsDetectUrl = "https://api.cognitive.azure.cn/face/v1.0/detect?returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses";
    string MsIdentifyUrl = "https://api.cognitive.azure.cn/face/v1.0/identify";
    string MsApiKey = "d3e1656a556e41f9bd7db8f3d18a5321";
    static string personGroupId = "test0";

    string url = "http://api.bosonnlp.com/keywords/analysis";
    string key = "MxxAfy-A.25878.HhhMBGEYzyne";
    private DictationRecognizer dictationRecognizer2;
    string first;
 //   public GameObject text0;
 //   public GameObject keyword;
 //   public GameObject VoiceCanvas;

    void OnPhotoCaptureCreated(UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        cameraResolution = UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        UnityEngine.XR.WSA.WebCam.CameraParameters c = new UnityEngine.XR.WSA.WebCam.CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        // c.pixelFormat = CapturePixelFormat.PNG;
        c.pixelFormat = CapturePixelFormat.JPEG;
        // c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Camera ready");


            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            Debug.Log(Application.persistentDataPath);
            //      photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.PNG, onCapturedPhotoToDiskCallback);

            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        Texture2D a;



        if (result.success)
        {
            Debug.Log("photo captured");
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            var cameraToWorldMatrix = new Matrix4x4();
            photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);

            cameraPosition = cameraToWorldMatrix.MultiplyPoint3x4(new Vector3(0, 0, -1));
            cameraRotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

            Matrix4x4 projectionMatrix;
            photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out projectionMatrix);
            Matrix4x4 pixelToCameraMatrix = projectionMatrix.inverse;

            status.GetComponent<TextMesh>().text = "photo captured, processing...";
            status.transform.position = cameraPosition;
            status.transform.rotation = cameraRotation;

            StartCoroutine(PostToFaceAPI(imageBufferList.ToArray(), cameraToWorldMatrix, pixelToCameraMatrix));
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void onCapturedPhotoToDiskCallback(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }

    }

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
            Debug.Log("Picture upload complete!");
            return true;
        }
    }

   

    IEnumerator<object> PostToFaceAPI(byte[] imageData, Matrix4x4 cameraToWorldMatrix, Matrix4x4 pixelToCameraMatrix)
    {

        //人脸识别字典
        Dictionary<string, string> msName = new Dictionary<string, string>();
        msName.Add("\"1806c11f-7243-4049-a5b3-f85760838919\"", "Liu Tao");
        msName.Add("\"91a13973-15c2-4f28-b123-751b33832908\"", "Ding Yiming");
        msName.Add("\"9e0abb73-dd48-4cec-a4f0-b2f38bbc8065\"", "Sun Yi");
        msName.Add("\"e95aa99b-d95b-4084-b0b6-3971efddbe17\"", "Zhang Zhenhua");



        //使用微软认知服务Api进行人脸检测
        var www = new UnityWebRequest(MsDetectUrl, "POST");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(imageData);
        // www.method="POST";
        www.chunkedTransfer = false;

        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", MsApiKey);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        if (!IsResponseValid(www))
            yield break;
        string responseString = www.downloadHandler.text;
      //  Debug.Log("msResponseString : " + responseString);

        //创建JSONObject对象
        JSONObject j = new JSONObject(responseString);
        Debug.Log("get msFaceDetectionResponseJson : "+j);

        
        //清除已经存在的标签对象
        var existing = GameObject.FindGameObjectsWithTag("canvas");
        foreach (var go in existing)
        {
            Destroy(go);
        }
        

        //判断是否有人脸
        if (j.list.Count == 0)
        {
            status.GetComponent<TextMesh>().text = "no faces found";
            yield break;
        }
        else
        {
            status.SetActive(false);
        //    canvas.SetActive(false);
        }
        

        var faceRectangles = "";
    //    Dictionary<string, TextMesh> textmeshes = new Dictionary<string, TextMesh>();

        foreach(var result in j.list)
        {
         //   GameObject Id = (GameObject)Instantiate(IdPrefab);
         //   TextMesh txtMesh = Id.GetComponent<TextMesh>();


            //定位人脸框坐标
            var p = result.GetField("faceRectangle");
            float top = -(p.GetField("top").f / cameraResolution.height - .5f);
            float left = p.GetField("left").f / cameraResolution.width - .5f;
            float width = p.GetField("width").f / cameraResolution.width;
            float height = p.GetField("height").f / cameraResolution.height;
            string id = string.Format("{0},{1},{2},{3}", p.GetField("left"), p.GetField("top"), p.GetField("width"), p.GetField("height"));
         //   textmeshes[id] = txtMesh;

            //裁剪人脸照片
            //     try
            //     {
            var source = new Texture2D(0, 0);
                source.LoadImage(imageData);
                var dest = new Texture2D((int)p["width"].i, (int)p["height"].i);
                Texture2D texture = new Texture2D((int)p["width"].i, (int)p["height"].i);
                dest.SetPixels(source.GetPixels((int)p["left"].i, cameraResolution.height - (int)p["top"].i - (int)p["height"].i, (int)p["width"].i, (int)p["height"].i));
                byte[] justThisFace = dest.EncodeToPNG();
                string filepath = Path.Combine(Application.persistentDataPath, "cropped.png");
                File.WriteAllBytes(filepath, justThisFace);
                Debug.Log("saved " + filepath);
       //     }
       //     catch(Exception e)
      //      {
      //          Debug.LogError(e);
      //      }

            if (faceRectangles == "")
            {
                faceRectangles = id;
            }
            else
            {
                faceRectangles += ";" + id;
            }

            GameObject can = (GameObject)Instantiate(canvas);
            can.transform.position = cameraToWorldMatrix.MultiplyPoint3x4(pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(left + width / 2, top, 0)));
            can.transform.rotation = cameraRotation;
            Vector3 scale = pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(width, height, 0));
            scale.z = .1f;
            can.transform.localScale = scale;
            can.SendMessageUpwards("SetPhoto", SendMessageOptions.DontRequireReceiver);
            can.tag = "canvas";



            //定位picture放置位置
            /*
        //    RawImage picture = transform.GetComponent<RawImage>();
        //    GameObject picture = (GameObject)Instantiate(picturePrefab);
            RawImage picture = (RawImage)Instantiate(picturePrefab);
            picture.transform.position = cameraToWorldMatrix.MultiplyPoint3x4(pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(left + width / 2, top, 0)));
            picture.transform.rotation = cameraRotation;
            Vector3 scale = pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(width, height, 0));
            scale.z = .1f;
            picture.transform.localScale = scale;
            picture.texture = Resources.Load(filepath) as Texture2D;

            //   picture.tag = "picture";
            */

            Image image = (Image)Instantiate(imagePrefab);
            
            //    imagePrefab = this.transform.Find("Image").GetComponent<Image>();
            //image = transform.Find("image").GetComponent<Image>();
            image.transform.position = cameraToWorldMatrix.MultiplyPoint3x4(pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(left + width / 2, top, 0)));
            image.transform.rotation = cameraRotation;
            Vector3 scale2 = pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(width, height, 0));
            scale2.z = .1f;
            image.transform.localScale = scale2;
            // image.sprite = Resources.Load(filepath) as Sprite;
            //    image.texture = Resources.Load(filepath) as Texture2D;

            //创建Sprite
            //   Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //image.sprite = sprite;
//            img.SendMessageUpwards("GetPhoto", SendMessageOptions.DontRequireReceiver);

            /*
            //定位ID放置位置
            //   Text Id = transform.GetComponent<Text>();
            Vector3 origin = cameraToWorldMatrix.MultiplyPoint3x4(pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(left + width + .1f, top, 0)));
            Id.transform.position = origin;
            Id.transform.rotation = cameraRotation;
        //    Id.tag = "ID";
            if (j.list.Count > 1)
            {
                Id.transform.localScale /= 2;
            }
            */
            Debug.Log("Face Detection has done!");


            //找出faceId
            string faceId = result.GetField("faceId").ToString();
            var msr = "{\"personGroupId\":\"" + personGroupId + "\", \"faceIds\":[" + faceId + "],\"maxNumOfCandidatesReturned\":1}";
            //  Debug.Log("msr : " + msr);
            
            //开始识别
            UnityWebRequest msIdentify = UnityWebRequest.Put(MsIdentifyUrl, msr);
            msIdentify.chunkedTransfer = false;
            msIdentify.method = "POST";
            msIdentify.SetRequestHeader("Ocp-Apim-Subscription-Key", MsApiKey);
            msIdentify.SetRequestHeader("Content-Type", "application/json");
            msIdentify.downloadHandler = new DownloadHandlerBuffer();
            yield return msIdentify.SendWebRequest();
            if (!IsResponseValid(msIdentify))
                   yield break;
            string identifyResponse = msIdentify.downloadHandler.text;
         //   Debug.Log("msIdentifyResponse : " + identifyResponse);
            JSONObject msIdentifyResponseJson = new JSONObject(identifyResponse);
            Debug.Log("msIdentifyResponseJson : " + msIdentifyResponseJson);

            foreach (var res in msIdentifyResponseJson.list)
            {
                var aa = res.GetField("candidates");
                //   var aa = msIdentifyResponseJson.GetField("faceId").ToString();
                Debug.Log("aa : " + aa);
                if (aa.ToString() == "[]")
                {
                    Debug.Log("No candidates!");
                    can.SetActive(false);
                    RememberCanvas.SetActive(true);
                    RememberLogic.SendMessageUpwards("SetTip", "emmmm.....may be a stranger?", SendMessageOptions.DontRequireReceiver);
                    RememberLogic.SendMessageUpwards("SetPhoto", SendMessageOptions.DontRequireReceiver);
                    break;
                }
                foreach (var res2 in aa.list)
                {
                    var b = res2.GetField("personId").ToString();
                //   Debug.Log("b : " + b);
                //    Debug.Log("b : " + "\"ced1d7bf-a84d-4ed4-ac61-1af09500aa44\"");

                //    var c = msName[b];
                //    Debug.Log("c : " + c);

                    char trimChar = '\"';
                    string personid = b.Trim(trimChar);
                //    Debug.Log("after trim : " + personid);
                    string GetNameUrl = "https://api.cognitive.azure.cn/face/v1.0/persongroups/test0/persons/" + personid;

                    var Getname = new UnityWebRequest(GetNameUrl, "GET");
                    Getname.chunkedTransfer = false;
                    Getname.SetRequestHeader("Ocp-Apim-Subscription-Key", MsApiKey);
                    Getname.downloadHandler = new DownloadHandlerBuffer();
                    yield return Getname.SendWebRequest();
                    if (!IsResponseValid(Getname))
                        yield break;
                    string Nameresponse = Getname.downloadHandler.text;
                //    Debug.Log("Nameresponse : " + Nameresponse);
                    JSONObject NameJson = new JSONObject(Nameresponse);
                    string c = NameJson.GetField("name").ToString().Trim(trimChar);
                //    Debug.Log("cc : " + cc);

                    //	txtMesh.text = string.Format("Gender: {0}\nAge: {1}\nMoustache: {2}\nBeard: {3}\nSideburns: {4}\nGlasses: {5}\nSmile: {6}", a.GetField("gender").str, a.GetField("age"), f.GetField("moustache"), f.GetField("beard"), f.GetField("sideburns"), a.GetField("glasses").str, a.GetField("smile"));
                    //    txtMesh.text = string.Format("Name: {0}\n", c);

                    //                   IdPrefab.GetComponent<TextMesh>().text = string.Format("\nID : {0}\n", c);
                    can.SendMessageUpwards("SetId", string.Format("\nID : {0}\n", c), SendMessageOptions.DontRequireReceiver);
                    Debug.Log("Recognition has been done!");
                }
            }
        }



    }


    // Called by GazeGestureManager when the user performs a Select gesture
    void Identify()
    {
        Debug.Log("tap");
        status.GetComponent<TextMesh>().text = "taking photo...";
        status.SetActive(true);
        RememberCanvas.SetActive(false);
        
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    /*
    // Use this for initialization  
    void Start()
    {
        dictationRecognizer2 = new DictationRecognizer();
        //订阅事件  
        dictationRecognizer2.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dictationRecognizer2.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer2.DictationComplete += DictationRecognizer_DictationComplete;
        dictationRecognizer2.DictationError += DictationRecognizer_DictationError;

        PhraseRecognitionSystem.Shutdown();
        dictationRecognizer2.Start();
    }

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        //    DictationDisplay.text = "error";

        RememberLogic.SendMessageUpwards("SetName", "Text : \nerror", SendMessageOptions.DontRequireReceiver);
      //  text0.GetComponent<TextMesh>().text = "Text : \nerror";
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        //   DictationDisplay.text = "complete:";
     //   text0.GetComponent<TextMesh>().text = "Text : \ncomplete:";
        RememberLogic.SendMessageUpwards("SetName", "Text : \ncomplete", SendMessageOptions.DontRequireReceiver);
        //如果在听写开始后第一个5秒内没听到任何声音，将会超时  
        //如果识别到了一个结果但是之后20秒没听到任何声音，也会超时  
        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
            //超时后本例重新启动听写识别器  
            //    DictationDisplay.text += "Dictation has timed out.";
            Debug.Log("TimeoutExceeded!");
         //   text0.GetComponent<TextMesh>().text = "Text : \nDictation has timed out.";
            RememberLogic.SendMessageUpwards("SetName", "Text : \nDictation has timed out.", SendMessageOptions.DontRequireReceiver);
            dictationRecognizer2.Stop();
            PhraseRecognitionSystem.Restart();
            //    DictationDisplay.text += "Dictation restart.";
        //    text0.GetComponent<TextMesh>().text = "Text : \nDictation restart.";
            RememberLogic.SendMessageUpwards("SetName", "Text : \nDictation restart.", SendMessageOptions.DontRequireReceiver);
            dictationRecognizer2.Start();
            PhraseRecognitionSystem.Shutdown();
        }
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        //    DictationDisplay.text = "result:";
        //    DictationDisplay.text += text;
        StartCoroutine(PostToBosonNLPAPI(text));
        //     text0.GetComponent<TextMesh>().text = "Result : \n";
        //     text0.GetComponent<TextMesh>().text += text;
        RememberLogic.SendMessageUpwards("SetName", "Result : \n" + text, SendMessageOptions.DontRequireReceiver);
    }

    private void DictationRecognizer_DictationHypothesis(string text)
    {
        //    DictationDisplay.text = "Hypothesis:";
        //    DictationDisplay.text += text;

    //    text0.GetComponent<TextMesh>().text = "Hypothesis : \n";
    //    text0.GetComponent<TextMesh>().text += text;
        RememberLogic.SendMessageUpwards("SetName", "Hypothesis : \n" + text, SendMessageOptions.DontRequireReceiver);
    }

    IEnumerator<object> PostToBosonNLPAPI(string text)
    {
        //使用BosonNLP HTTP API进行关键词提取
        string s = "";
        int i = 0;
        string body = "\"" + text + "\"";
        bool isIdentify = false, isName = false;
        char trimChar = '\"';

        UnityWebRequest www = UnityWebRequest.Put(url, body);
        www.chunkedTransfer = false;
        www.method = "POST";
        www.SetRequestHeader("X-Token", key);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        if (!IsResponseValid(www))
            yield break;
        string response = www.downloadHandler.text;
        Debug.Log("BosonAPI response : " + response);
        JSONObject j = new JSONObject(response);
        foreach (var res in j.list)
        {
            //    Debug.Log(res);
            foreach (var ress in res.list)
            {
                
                if (i == 1) first = ress.ToString().Trim(trimChar);
                if (i % 2 != 0) s += ress.ToString() + " ";
                i++;
                string ss = ress.ToString().Trim(trimChar);
                if (ss == "what" || ss == "what\'s" || ss == "who" || ss == "who\'s") isIdentify = true;
                else isName = true;
            }
        }

        if(isIdentify)
        {
            RememberCanvas.SetActive(false);
            Identify();
        }
        if(isName)
        {
            RememberLogic.SendMessageUpwards("SetName", "Name : " + first, SendMessageOptions.DontRequireReceiver);
        }
    //    keyword.GetComponent<TextMesh>().text = "Keyword : \n";
    //    keyword.GetComponent<TextMesh>().text += s;
    }

    void OnDestroy()
    {
        dictationRecognizer2.Stop();
        dictationRecognizer2.DictationHypothesis -= DictationRecognizer_DictationHypothesis;
        dictationRecognizer2.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer2.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer2.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer2.Dispose();
        PhraseRecognitionSystem.Restart();
    }
    */
    
}
