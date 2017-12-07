using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor  {

    public Node tL, bR;

    public Corridor(Node topOrLeft, Node bottomORRight)
    {
        tL = topOrLeft;
        bR = bottomORRight;
    }
    public Corridor() { }
}