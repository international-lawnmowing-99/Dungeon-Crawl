//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CharacterStats : GridAgent {

//    public bool justMoved = false;

//    public int health, hunger;
//    public Weapon currentWeapon;

//    private float tolerance = 0.01f;
//    private Vector3 lastPos,positionDifference;
//    private GameObject stairs, key;
//	// Use this for initialization
//	void Start()
//    {
//        lastPos = transform.position;
//        stairs = dungeon.internalStair;
//        key = dungeon.internalKey;
//    }

//    // Update is called once per frame
//    void Update () {
//        positionDifference = transform.position - lastPos;
//        if (positionDifference.magnitude > tolerance)
//        {
//            UpdateNode();

//            if (dungeon.internalKey != null)
//            {
//                if (node.x ==  key.GetComponent<Key>().node.x && node.y == key.GetComponent<Key>().node.y)
//                {
//                    key.transform.parent = transform;
//                    key.GetComponent<Key>().node = dungeon.grid[0, 0];
//                }

//            }

//            justMoved = true;
//        }

//        lastPos = transform.position;
//    }
//}