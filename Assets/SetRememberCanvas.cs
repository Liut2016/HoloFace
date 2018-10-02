using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRememberCanvas : MonoBehaviour {

    public GameObject photo;
    public GameObject tip;
    public GameObject name;

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
        name.GetComponent<TextMesh>().text = "His/her name is " + s;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
