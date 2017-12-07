using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyOrb : IEnemyBase {


	void Start() {

        CharElement = Elements.Earth;

        CharAttRange = 1;

        BaseMaxHp = 200;
        BaseAtt = 250;
        BaseDef = 100;

        //CharLevel = 1; //Have this serialised!

        DeriveStats();
        //gordoman

    }
}
