using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public enum Direction
    {
        North, South, East, West
    }

    public bool northExit, southExit, eastExit, westExit;

    public struct CorridorEnding
    {

        public Direction side;
        public int xPos;
        public int yPos;
        public bool isRoughed;
        public CorridorEnding(Direction newSide, int newXPos, int newYPos)
        {
            side = newSide;
            xPos = newXPos;
            yPos = newYPos;
            isRoughed = false;
        }
    }
    public int xPos, yPos, width, height;
    public float scale;
    public List<Corridor> connectedCorridors;
    public int exitCount;
    public bool isAttachedToVoidRom;
    public bool isVisisted = false;
    public int index;
    public Direction directionToPreviousRoom;


    public CorridorEnding[] exits;

    public Room(int x, int y, int Width, int Height)
    {
        Initialise(x, y, Width, Height);
    }

    public void Initialise(int x, int y, int Width, int Height)
    {
        xPos = x;
        yPos = y;
        width = Width;
        height = Height;
        exits = new CorridorEnding[4];
        northExit = false;
        southExit = false;
        eastExit = false;
        westExit = false;

    }
    ~Room() { }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
