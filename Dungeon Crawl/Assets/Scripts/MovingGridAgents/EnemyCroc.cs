using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCroc : IEnemyBase {


	void Start() {


        CharElement = Elements.Fire;

        CharAttRange = 1;

        BaseMaxHp = 350;
        BaseAtt = 250;
        BaseDef = 250;

        //CharLevel = 1; //Have this serialised!

        DeriveStats();
        //duolizard

    }
}
