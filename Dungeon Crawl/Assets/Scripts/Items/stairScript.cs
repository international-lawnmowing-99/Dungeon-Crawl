using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stairScript : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tile") 
        {
            //Debug.Log("yolo");
            //Debug.Log(other.gameObject.name);
            Destroy(other.gameObject);
            Destroy(this);
        }
    }
}
