using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectCharacter
    : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerChar")
        {
            DungeonGenerator dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
            dungeon.UpdatePersistent(dungeon.internalPlayer.GetComponent<ICharacterBase>());

            SceneManager.LoadScene("Character Select");
        }
    }
}
