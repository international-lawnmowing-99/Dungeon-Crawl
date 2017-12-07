using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brave_Soldier_Sofia : ICharacterBase {
    bool Skill1InQue;
    bool Skill2InQue;
    void Start () {

        //AttackOver = true;
        Skill1InQue = false;
        Skill2InQue = false;

        CharAttRange = 1;

        CharLevel = 1;
        CharRank = 1; //Save and load
                      
        //XpGems = 0;

        L1MaxHp = 300;
        L50MaxHp = 600;

        L1Att = 500;
        L50Att = 1600;

        L1Def = 340;
        L50Def = 1000;

        AttGrowth = 22;
        HpGrowth = 8; 
        DefGrowth = 12;

        DeriveBaseStats();
        DeriveStats();

        SkillRadial.SetActive(false);
    }

    IEnumerator SkillAttackWithDelayX4(float time)
    {
        
        Debug.Log("Bam");
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        Debug.Log("Bam");
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        Debug.Log("Bam");
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        Debug.Log("Bam");
        SkillAttack(RaycastTarget, 0.5f);
        Skill1InQue = false;
        RaycastTarget = null;
        PlayerHasControl = true;
        TurnTickover();
    }

    IEnumerator SkillAttackWithDelayX6(float time)
    {
        
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        SkillAttack(RaycastTarget, 0.5f);
        yield return new WaitForSecondsRealtime(time);
        SkillAttack(RaycastTarget, 0.5f);
        Skill2InQue = false;
        RaycastTarget = null;
        PlayerHasControl = true;
        CurEffect = Effects.AttBuff;
        StatusDUR = 5;
        TurnTickover();
    }
    
    protected override void Skill1()
    {
        if (Skill1CD == 0 && Skill2InQue == false)
        {
            
            SkillRadial.SetActive(false);
            SkillTargeting = true;
            PlayerHasControl = false;
            Skill1InQue = true;
            
        }
    }
    protected override void Skill2()
    {
        if (Skill2CD == 0 && Skill1InQue == false)
        {
            
            SkillRadial.SetActive(false);
            SkillTargeting = true;
            PlayerHasControl = false;
            Skill2InQue = true;
        }
    }
    void FixedUpdate()
    {

        if (RaycastTarget != null && SkillTargeting == true)
        {
            float distance = Vector3.Distance(RaycastTarget.transform.position, this.transform.position);
            Debug.Log(distance);
            if (distance <=1.3)
            {
                if (Skill1InQue == true)
                {
                    SkillTargeting = false;
                    Skill1CD = 10;
                    
                    //Anims.SetTrigger("Skill1");

                    CharTap();
                    SkillRadial.SetActive(false);
                    StartCoroutine(SkillAttackWithDelayX4(0.2f));
                }
                else if (Skill2InQue == true)
                {
                    SkillTargeting = false;
                    Skill2CD = 10;
                    //Anims.SetTrigger("Skill2");

                    CharTap();
                    SkillRadial.SetActive(false);
                    StartCoroutine(SkillAttackWithDelayX6(0.2f));
                }
            }
        }
    }
}
