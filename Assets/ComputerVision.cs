using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA.WebCam;

public class ComputerVision : MonoBehaviour {

    PhotoCapture photoCaptureObject = null;
    Resolution cameraResolution;
    Vector3 cameraPosition;
    Quaternion cameraRotation;

    string url = "https://api.cognitive.azure.cn/vision/v1.0/describe";
    string key = "7676129de598429a868a472692246041";


    public GameObject status;
    public GameObject surroundings;


    void OnPhotoCaptureCreated(PhotoCapture captureObject)
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

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
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

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
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

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
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
        //使用微软认知服务Api进行环境物体检测
        var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(imageData);
        // www.method="POST";
        www.chunkedTransfer = false;

        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        if (!IsResponseValid(www))
            yield break;
        string responseString = www.downloadHandler.text;
        //  Debug.Log("msResponseString : " + responseString);

        //创建JSONObject对象
        JSONObject j = new JSONObject(responseString);
        Debug.Log("get msComputerVisionResponseJson : " + j);

        //清除已经存在的标签对象
        var existing = GameObject.FindGameObjectsWithTag("canvas2");
        foreach (var go in existing)
        {
            Destroy(go);
        }

        status.SetActive(false);


        var a = j.GetField("description");
        var b = a.GetField("captions");
        var text = b.list[0].GetField("text");
        var things = a.GetField("tags");

        GameObject can = Instantiate(surroundings);
        can.SendMessageUpwards("Settext", string.Format("\nDescription : \n{0}\n", text), SendMessageOptions.DontRequireReceiver);
        can.SendMessageUpwards("Setthings", string.Format("\nObjects : \n{0}\n", things), SendMessageOptions.DontRequireReceiver);
        can.tag = "canvas2";
        Debug.Log("Surroundings Recognition has done!");
  
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void Identify()
    {
        Debug.Log("tap");
        status.GetComponent<TextMesh>().text = "taking photo...";
        status.SetActive(true);

        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
