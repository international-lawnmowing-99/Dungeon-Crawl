using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //THIS NEEDS TO BE IN ANYTHING USING UI! SO ALL CHAR SCRIPTS
public class Stitched_Man_Victor : ICharacterBase {

    

	//private bool usedLightning = false;
    void Start () {

        CharAttRange = 1;

        CharLevel = 1;
        CharRank = 1; //Save and load

        L1MaxHp = 900;
        L50MaxHp = 1700;

        L1Att = 850;
        L50Att = 850;

        L1Def = 250;
        L50Def = 850;

        AttGrowth = 22;
        HpGrowth = 8;
        DefGrowth = 12;

        DeriveBaseStats();
        DeriveStats();
    }


    protected override void Skill1()
    {
        if (Skill1CD == 0)
        {
            //Anims.SetTrigger("Skill1");
            CharTap();
            CurEffect = Effects.AttBuff;
            Skill1CD = 10;
            StatusDUR = 5;
            TurnTickover();
        }
    }
    protected override void Skill2()
    {
        if (Skill2CD == 0)
        {
            Skill2CD = 10;
            SkillRadial.SetActive(false);
            SkillTargeting = true;
            PlayerHasControl = false;
        }
    }
    void FixedUpdate()
    {
        
        if (RaycastTarget != null && SkillTargeting == true)
        {
            SkillTargeting = false;
			IEnemyBase[] List = FindObjectsOfType<IEnemyBase>();
			foreach (IEnemyBase target in List)
			{
				Instantiate(Skill2Eff, target.transform.position, new Quaternion(0, 0, 0, 0));
				SkillAttack(target.gameObject, 0.75f);
			}
            //Anims.SetTrigger("Skill2");
            CharTap();
            SkillRadial.SetActive(false);
            SkillAttack(RaycastTarget, 1.5f);
			RaycastTarget = null;
            PlayerHasControl = true;
            TurnTickover();

        }
    }
}
