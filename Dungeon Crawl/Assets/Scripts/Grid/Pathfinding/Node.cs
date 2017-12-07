using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node  {

    public int x, y;
    public DungeonGenerator.TERRAINTYPE terrain;
    public int G, H;
    public int F { get { return G + H; } }
    public bool isOccupied = false;
    public bool containsItem = false;
    public Node parentNode;
    public List<GameObject> items = new List<GameObject>();


    public int heapIndex;

    public int CompareTo(Node other)
    {
        int compare = F.CompareTo(other.F);
        if (compare == 0)
        {
            compare = H.CompareTo(other.H);
        }
        return -compare;
    }
    // Use this for initialization
    void Start () {
		
	}
}
