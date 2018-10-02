using GLTF;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;

public class GazeGestureManager : MonoBehaviour {

    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }

    public GameObject empty;
    public GameObject surroundingsLogic;

    public GameObject status;
    public GameObject menucanvas;
    public GameObject facecanvas;
    public GameObject surroundingscanvas;
    public GameObject voicecanvas;
    public GameObject Remembercanvas;

    GestureRecognizer recognizer;

    // Use this for initialization
    void Start () {
        menucanvas.SetActive(true);
        facecanvas.SetActive(false);
        surroundingscanvas.SetActive(false);
        voicecanvas.SetActive(false);
        Remembercanvas.SetActive(false);
        status.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }

    public void face()
    {
        Instance = this;

        menucanvas.SetActive(false);
        facecanvas.SetActive(true);
        surroundingscanvas.SetActive(false);
        voicecanvas.SetActive(false);
        Remembercanvas.SetActive(false);
        status.SetActive(true);

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            empty.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
        };
        recognizer.StartCapturingGestures();
        empty.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
    }

    public void surroundings()
    {
        Instance = this;

        menucanvas.SetActive(false);
        facecanvas.SetActive(false);
        surroundingscanvas.SetActive(true);
        voicecanvas.SetActive(false);
        Remembercanvas.SetActive(false);
        status.SetActive(true);

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            surroundingsLogic.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);

        };
        recognizer.StartCapturingGestures();
        surroundingsLogic.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
    }

    public void remember()
    {
        Instance = this;
        menucanvas.SetActive(false);
        facecanvas.SetActive(false);
        surroundingscanvas.SetActive(false);
        voicecanvas.SetActive(true);
        Remembercanvas.SetActive(false);
        status.SetActive(false);

    }

    public void test()
    {
        Instance = this;
        menucanvas.SetActive(false);
        facecanvas.SetActive(false);
        surroundingscanvas.SetActive(false);
        voicecanvas.SetActive(false);
        Remembercanvas.SetActive(true);
        status.SetActive(false);

    }


    public void others()
    {
        Instance = this;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {

            //   FocusedObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
            //            empty.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
            surroundingsLogic.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);

        };
        recognizer.StartCapturingGestures();
        //  UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        //       empty.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
        surroundingsLogic.SendMessageUpwards("Identify", SendMessageOptions.DontRequireReceiver);
    }
}
