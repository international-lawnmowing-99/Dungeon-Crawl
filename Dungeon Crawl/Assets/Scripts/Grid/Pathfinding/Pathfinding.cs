using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {
    private DungeonGenerator dungeon;
    private int dungeonNodeCount;

    public List<Node> foundPath;
    public HashSet<Node> debugClosedSet;
    bool isInitialised = false;

    void Awake()
    {
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
        dungeonNodeCount = dungeon.width * dungeon.height;
    }
    public void StartFindPath(Node startNode, Node targetNode, bool occupiedImpassable, IEnemyBase self)
    {
        StartCoroutine(FindPath(startNode, targetNode, occupiedImpassable, self));
    }

    //public void StartFindPath(Node startNode, Node targetNode)
    //{
    //    StartCoroutine(FindPath(startNode, targetNode));
    //}
    private IEnumerator FindPath(Node startNode, Node targetNode, bool occupiedImpassable, IEnemyBase self)
    {
        if (!isInitialised)
        {
            Awake();
            isInitialised = true;
        }
        Heap<Node> openSet = new Heap<Node>(dungeonNodeCount);
        HashSet<Node> closedSet = new HashSet<Node>();

        bool successfulPath = false;

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode.x == targetNode.x &&currentNode.y == targetNode.y)
            {
                RetracePath(startNode, targetNode);
                successfulPath = true;
                debugClosedSet = closedSet;
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (!(neighbour.terrain == DungeonGenerator.TERRAINTYPE.Grass) || closedSet.Contains(neighbour))
                {
                    continue;
                }

                if (occupiedImpassable)
                {
                    if (neighbour != self.character.node && neighbour != self.node && neighbour.isOccupied)
                    {
                        continue;
                    }
                }

                int newMovementCostToNeighbour = currentNode.G + GetNodeDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.G || !openSet.Contains(neighbour))
                {
                    neighbour.G = newMovementCostToNeighbour;
                    neighbour.H = GetNodeDistance(neighbour, targetNode);

                    neighbour.parentNode = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
        yield return null;
        dungeon.pathManager.FinishedProcessingPath(foundPath, successfulPath);
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }

        path.Reverse();
        foundPath = path;
    }

    int GetNodeDistance(Node a, Node b)
    {
        int xDist = Mathf.Abs(a.x - b.x);
        int yDist = Mathf.Abs(a.y - b.y);

        if (xDist > yDist)
        {
            return 14 * yDist + 10 * (xDist - yDist);
        }
        else
        {
            return 14 * xDist + 10 * (yDist - xDist);
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x==0 && y==0)
                {
                    continue;
                }
                if (x!=0 && y!=0)
                {
                    continue;
                }

                int checkX = node.x + x;
                int checkY = node.y + y;

                if (checkX >= 0 && checkX < dungeon.width && checkY >= 0 && checkY < dungeon.height)
                {
                    neighbours.Add(dungeon.grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
}