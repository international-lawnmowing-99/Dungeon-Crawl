using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strobe : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Random.Range(0,100)*Time.deltaTime,200*Time.deltaTime, 30*Time.deltaTime);
        GetComponent<Light>().color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        //if (GetComponent<Light>().intensity < 1)
        //{
        //    GetComponent<Light>().intensity -= 0.01f;

        //}
	}
}
