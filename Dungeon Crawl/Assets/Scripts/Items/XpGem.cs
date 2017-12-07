using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XpGem : MonoBehaviour {

    Vector3 Pos;

    // Use this for initialization
    void Start () {
       
	}

    // Update is called once per frame
    void Update()
    {
        Pos = GameObject.FindGameObjectWithTag("PlayerChar").transform.position;
        transform.position = Vector3.MoveTowards(transform.position, Pos, Time.deltaTime*10);
    }
}
