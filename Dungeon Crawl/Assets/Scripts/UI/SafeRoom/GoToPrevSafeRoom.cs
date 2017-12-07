using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPrevSafeRoom : MonoBehaviour {

void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerChar")
        {
            DungeonGenerator dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
            dungeon.UpdatePersistent(dungeon.internalPlayer.GetComponent<ICharacterBase>());
            SceneManager.LoadScene("Switch Safe Room");
            Debug.Log("Go to different safe room");
        }
    }
}
