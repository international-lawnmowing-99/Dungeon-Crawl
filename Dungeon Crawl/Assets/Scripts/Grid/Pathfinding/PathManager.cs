using System;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour {

    //DungeonGenerator dungeon;

    bool isProcessingPath = false;
    Queue<PathRequest> requestQueue;
    Pathfinding pathfinding;
    PathRequest currentPathRequest;
    //bool isInitialised = false;

    public static PathManager instance;


    struct PathRequest
    {
        public Node start;
        public Node end;
        public Action<List<Node>, bool> callback;
        public bool occupiedNodesImpassable;
        public IEnemyBase requester;
        public PathRequest(Node startNode, Node endNode, Action<List<Node>, bool> newCallback, bool newOccupiedImpassableBool, IEnemyBase newRequester)
        {
            start = startNode;
            end = endNode;
            callback = newCallback;
            requester = newRequester;
            occupiedNodesImpassable = newOccupiedImpassableBool;
        }
    }

	// Use this for initialization
	void Awake () {
        requestQueue = new Queue<PathRequest>();
        pathfinding = gameObject.GetComponent<Pathfinding>();
        instance = this;
	}

    //public static void RequestPath(Node startNode, Node endNode, Action<List<Node>, bool> callback)
    //{
    //    PathRequest newRequest = new PathRequest(startNode, endNode, callback);
    //    instance.requestQueue.Enqueue(newRequest);
    //    instance.TryNextPath();
    //}

    public static void RequestPath(Node startNode, Node endNode, Action<List<Node>, bool> callback, bool occupiedNodesImpassable, IEnemyBase requester)
    {
        PathRequest newRequest = new PathRequest(startNode, endNode, callback, occupiedNodesImpassable, requester);
        instance.requestQueue.Enqueue(newRequest);
        instance.TryNextPath();
    }

    void TryNextPath()
    {
        if (!isProcessingPath && requestQueue.Count > 0)
        {
            currentPathRequest = requestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.start, currentPathRequest.end, currentPathRequest.occupiedNodesImpassable, currentPathRequest.requester);
        }
    }
	
    public void FinishedProcessingPath(List<Node> path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryNextPath();
    }
}