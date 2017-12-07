using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    GameObject PlayerChar;
    public float followSpeed = 1;

    public Transform fightView, isoView, topView;
	void Start() {

        UpdatePlayer();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (PlayerChar == null)//I'd like to fix this later
        {
            UpdatePlayer();
        }
        Vector3 distance = transform.position - PlayerChar.transform.position;
        if (distance.magnitude >= 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, PlayerChar.transform.position, 2* followSpeed * Time.deltaTime);
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(transform.position, PlayerChar.transform.position, followSpeed*Time.deltaTime);


        }
    }

    public void UpdatePlayer()
    {
        PlayerChar = GameObject.FindWithTag("PlayerChar"); 
        //Debug.Log(PlayerChar);
    }

}
