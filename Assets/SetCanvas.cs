using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCanvas : MonoBehaviour {

    public GameObject Id;
    public GameObject photo;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void SetPhoto () {
        
        photo.SendMessageUpwards("GetPhoto", SendMessageOptions.DontRequireReceiver);

    }

    void SetId(string c)
    {
        Id.GetComponent<TextMesh>().text = c;
    }
}
