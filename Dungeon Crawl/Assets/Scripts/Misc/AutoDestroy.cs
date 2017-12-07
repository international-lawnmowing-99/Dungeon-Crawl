using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {

    public float deathTime = 5;

	// Use this for initialization
	void Start () {
        StartCoroutine(waitForDeath());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    IEnumerator waitForDeath()
    {
        yield return new WaitForSeconds(deathTime);
        Destroy(gameObject);
    }
}
