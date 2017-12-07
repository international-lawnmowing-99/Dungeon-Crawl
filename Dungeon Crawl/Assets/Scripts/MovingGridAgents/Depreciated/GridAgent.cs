using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAgent : MonoBehaviour {

    protected Pathfinding pathfinding;
    protected DungeonGenerator dungeon;
    public Node node;

    // Use this for initialization
    void OnEnable() {
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
        pathfinding = dungeon.pathfinder;

        UpdateNode();
    }

    public void UpdateNode()
    {
        if ((int)transform.position.x >= 0 && (int)transform.position.x < dungeon.width && (int)transform.position.z >= 0 && (int)transform.position.z < dungeon.height)
        {
            node = dungeon.grid[(int)transform.position.x, (int)transform.position.z];
        }
    }
}