using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSurroundings : MonoBehaviour {

    public GameObject text;
    public GameObject things;



    void Settext(string c)
    {
        text.GetComponent<TextMesh>().text = c;
    }

    void Setthings(string c)
    {
        things.GetComponent<TextMesh>().text = c;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
