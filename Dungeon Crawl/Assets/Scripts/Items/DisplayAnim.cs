using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayAnim : MonoBehaviour
{
    GameObject internalCamera;
    GameObject treasureHolder;
    DungeonGenerator dungeon;
    // Use this for initialization
    void Start()
    {
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();

        internalCamera = GameObject.Find("Main Camera");
        treasureHolder = GameObject.Find("TreasureHolder");
        transform.rotation = Quaternion.LookRotation(internalCamera.transform.position - treasureHolder.transform.position);
        
        //Looks ok with the current camera settings
        transform.rotation = Quaternion.Euler(transform.rotation.x - 13, transform.rotation.y - 70, transform.rotation.z + 17);

    }


    //Move towards the camera then destroy self
    void FixedUpdate()
    {
        if (transform.position != internalCamera.transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, internalCamera.transform.position, 8 * Time.deltaTime);

        }
        else
        {
            dungeon.internalPlayer.GetComponent<ICharacterBase>().SetXP(dungeon.internalPlayer.GetComponent<ICharacterBase>().GetXP() + GetComponentInChildren<IItemBase>().Value);
            Destroy(gameObject);
        }
    }
}