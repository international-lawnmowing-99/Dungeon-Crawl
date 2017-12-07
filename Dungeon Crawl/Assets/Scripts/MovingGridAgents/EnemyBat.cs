using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBat : IEnemyBase { 


    void Start() {


        CharElement = Elements.Earth;

        CharAttRange = 1;

        BaseMaxHp = 150;
        BaseAtt = 200;
        BaseDef = 150;

        //CharLevel = 1; //Have this serialised!

        DeriveStats();
        //rodent

    }
}
