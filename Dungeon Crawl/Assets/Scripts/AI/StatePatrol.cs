using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePatrol : IEnemyState {
    ICharacterBase character;
    IEnemyBase self;
    DungeonGenerator dungeon;

    public float detectionRadius = 3.3f;
    public StatePatrol(ICharacterBase newCharacter, IEnemyBase newEnemy, DungeonGenerator newDungeon)
    {
        character = newCharacter;
        self = newEnemy;
        dungeon = newDungeon;
        self.UpdateNode();
    }
    void IEnemyState.Update()
    {
        Vector3 distToPlayer = character.transform.position - self.transform.position;
        if (distToPlayer.magnitude < detectionRadius)
        {
            self.UpdateNode();
            //self.node.isOccupied = false;
            self.currentState = self.chaseState;
            self.currentState.Update();
            goto SKIPUPDATE;
        }

        Vector3 pos = self.gameObject.transform.position;

        List<Node> walkableNeighbours = new List<Node>();
        foreach (Node n in dungeon.pathfinder.GetNeighbours(dungeon.grid[(int)self.transform.position.x, (int)self.transform.position.z]))
        {
            if (!n.isOccupied && n.terrain == DungeonGenerator.TERRAINTYPE.Grass)
            {
                walkableNeighbours.Add(n);
            }
        }

        if (Input.GetKey(KeyCode.A) || character.justMoved || Input.GetKeyDown(KeyCode.B))
        {
            if (walkableNeighbours.Count > 0)
            {
                int direction = UnityEngine.Random.Range(0, walkableNeighbours.Count);

                Node newPositionNode = walkableNeighbours[direction];

                self.targetPos = new Vector3(newPositionNode.x, .6f, newPositionNode.y);
                
                self.node.isOccupied = false;
                newPositionNode.isOccupied = true;
                self.node = newPositionNode;
                //character.gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z);
            }
            //character.node
            // Move in a random direction
        }
        //int direction = UnityEngine.Random.Range(0, 4);
        //switch (direction)
        //{
        //    case 0:
        //        character.gameObject.transform.position = new Vector3(pos.x + 1, pos.y, pos.z);
        //        break;
        //    case 1:
        //        character.gameObject.transform.position = new Vector3(pos.x - 1, pos.y, pos.z);
        //        break;
        //    case 2:
        //        character.gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z + 1);
        //        break;
        //    case 3:
        //        character.gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z - 1);
        //        break;
        //    default:
        //        break;
        //}
        //character.gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z);
        SKIPUPDATE:;  
    }
}