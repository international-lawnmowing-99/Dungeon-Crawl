//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Enemy : GridAgent
//{
//    public IEnemyState currentState;
//    //bool foundPath = false;
//    public CharacterStats character;
//    //List<Node> myPath;
//    public StatePatrol patrolState;
//    public StateChase chaseState; 

//    void Start()
//    {
//        FindCharacter();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        currentState.Update();
//    }
//    public void FindCharacter()
//    {
//        patrolState = new StatePatrol(character, this);
//        chaseState = new StateChase(dungeon.internalPlayer.GetComponent<CharacterStats>(), this, pathfinding);
//        currentState = patrolState;
//    }

//    void OnDrawGizmos()
//    {
//        Gizmos.DrawWireSphere(transform.position,patrolState.detectionRadius);
//    }
//    //    Gizmos.color = Color.green;

//    //    if (foundPath)
//    //    {
//    //        foreach (Node n in myPath)
//    //        {
//    //            Gizmos.DrawSphere(new Vector3(n.x, 0.5f, n.y), 0.1f);
//    //        }
//    //    }
//    //}
//}