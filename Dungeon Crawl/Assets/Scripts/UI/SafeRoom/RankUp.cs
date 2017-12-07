using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankUp : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerChar")
        {
            Debug.Log("Doing rank up stuff");
        }
    }
}
