using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class DictonManager : MonoBehaviour {

    [Tooltip("A text area for the recognizer to display the recognized strings.")]
    public GameObject text0;
    public GameObject keyword;
    private DictationRecognizer dictationRecognizer;
    public GameObject RememberCanvas;
    public GameObject empty;
    public GameObject RememberLogic;

    string url = "http://api.bosonnlp.com/keywords/analysis";
    string key = "MxxAfy-A.25878.HhhMBGEYzyne";
    string first;

    // Use this for initialization  
    void Start()
    {
        dictationRecognizer = new DictationRecognizer();
        //订阅事件  
        dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;

        PhraseRecognitionSystem.Shutdown();
        dictationRecognizer.Start();
    }

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        //    DictationDisplay.text = "error";


        text0.GetComponent<TextMesh>().text = "Text : \nerror";
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
     //   DictationDisplay.text = "complete:";
        text0.GetComponent<TextMesh>().text = "Text : \ncomplete:";
        //如果在听写开始后第一个5秒内没听到任何声音，将会超时  
        //如果识别到了一个结果但是之后20秒没听到任何声音，也会超时  
        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
            //超时后本例重新启动听写识别器  
            //    DictationDisplay.text += "Dictation has timed out.";
            Debug.Log("TimeoutExceeded!");
            text0.GetComponent<TextMesh>().text = "Text : \nDictation has timed out.";
            dictationRecognizer.Stop();
            PhraseRecognitionSystem.Restart();
            //    DictationDisplay.text += "Dictation restart.";
            text0.GetComponent<TextMesh>().text = "Text : \nDictation restart.";
            PhraseRecognitionSystem.Shutdown();
            dictationRecognizer.Start();
            
        }
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
    //    DictationDisplay.text = "result:";
    //    DictationDisplay.text += text;
        StartCoroutine(PostToBosonNLPAPI(text));
        text0.GetComponent<TextMesh>().text = "Your sentences : ";
        text0.GetComponent<TextMesh>().text += text;
    }

    private void DictationRecognizer_DictationHypothesis(string text)
    {
    //    DictationDisplay.text = "Hypothesis:";
    //    DictationDisplay.text += text;

        text0.GetComponent<TextMesh>().text = "Hypothesis : ";
        text0.GetComponent<TextMesh>().text += text;
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
            Debug.Log("Keyword upload complete!");
            return true;
        }
    }

    IEnumerator<object> PostToBosonNLPAPI(string text)
    {
        //使用BosonNLP HTTP API进行关键词提取
        string s="";
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
        foreach(var res in j.list)
        {
        //    Debug.Log(res);
            foreach(var ress in res.list)
            {
                if (i == 1) first = ress.ToString().Trim(trimChar);
                if (i % 2 != 0) s += ress.ToString()+" ";
                i++;
                string ss = ress.ToString().Trim(trimChar);
                if (ss == "what" || ss == "what\'s" || ss == "who" || ss == "who\'s") isIdentify = true;
                else isName = true;
                //       Debug.Log("ress : " + ress);
            }
        }
        if (isIdentify)
        {
            RememberCanvas.SetActive(false);
            empty.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
        //    Identify();
        }
        if (isName)
        {
            text0.GetComponent<TextMesh>().text = "Name : ";
            text0.GetComponent<TextMesh>().text += first;
        //    RememberLogic.SendMessageUpwards("SetName", "Name : " + first, SendMessageOptions.DontRequireReceiver);
        }

        keyword.GetComponent<TextMesh>().text = "Keyword : \n";
        keyword.GetComponent<TextMesh>().text += s;
    }


    public void StartRemember()
    {

        RememberLogic.SendMessageUpwards("Remember", first, SendMessageOptions.DontRequireReceiver);
    }
    // Update is called once per frame  
    void Update()
    {

    }

    void OnDestroy()
    {
        dictationRecognizer.Stop();
        dictationRecognizer.DictationHypothesis -= DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer.Dispose();
        PhraseRecognitionSystem.Restart();
    }
}
