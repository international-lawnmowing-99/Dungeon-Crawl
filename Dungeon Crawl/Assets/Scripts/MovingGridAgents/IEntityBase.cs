using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class IEntityBase : MonoBehaviour {
    //[Tooltip("UseForEnemiesOnly")]
    //public GameObject emptyScroll, emptyPotion, emptyWeapon, emptyFood;
    public GameObject fairy;
	public AudioClip HitFX;
	private AudioSource source;
	private float volLowRange = .5f;
	private float volHighRange = 1.0f;

	bool droppedLoot = false;
    public enum Elements
    {
        none = 0,
        Fire = 1,
        Water = 2,
        Earth = 3,
        Thunder = 4,
        Dark = 5,
        Light = 6,
        Mechanical = 7
    };
    public enum Effects
    {
        none = 0,
        AttBuff = 1,
        DefBuff = 2,
        AttDeBuff = 3,
        DefDeBuff = 4,
        Poison = 5,
        Stun = 6,
        Mini = 7,
        Dizzy = 8,
        Float = 9,
        Fear = 10,
        Burn = 11
    };

    public enum CompassDir
    {
        E = 0, NE = 1,
        N = 2, NW = 3,
        W = 4, SW = 5,
        S = 6, SE = 7
    };
    protected int hunger = 200;
    protected int XpGems, BaseMaxHp, BaseAtt, BaseDef;
    public int CharLevel;
    public int CharMaxHp;
    public float CharCurrHp;
    protected float CharAtt;
    protected int CharDef;
    protected int CharAttRange;

    protected int Skill1CD, Skill2CD, StatusDUR;

    public float ATKBuffMulti, ATKWepMulti, ATKSkillMulti, ATKAtt;
    public int ATKLvl;
    public Elements ATKElement;
    public Effects ATKEffects;
    public bool ATKInQue = false;

    bool debugAttack = false;

    //public GUIText GUIDamage;
    public GameObject DamageText;
    
    protected RaycastHit hit;
    protected GameObject RaycastTarget;

    protected Elements CharElement;
    protected Effects CurEffect;

    protected Pathfinding pathfinding;
    protected DungeonGenerator dungeon;
    public Node node;

    public GameObject XpCrystal;

    public bool IsPlayer, justMoved;

    public Animator Anims;
    protected float ElementCheck(Elements Att, Elements Def)
    {

        switch (Att)
        {
            case Elements.Fire:
                switch (Def)
                {
                    case Elements.Water:
                        return 0.5f;


                    case Elements.Earth:
                        return 2;


                }
                break;

            case Elements.Water:
                switch (Def)
                {
                    case Elements.Fire:
                        return 2;

                    case Elements.Thunder:
                        return 0.5f;

                }
                break;

            case Elements.Earth:
                switch (Def)
                {
                    case Elements.Fire:
                        return 0.5f;

                    case Elements.Thunder:
                        return 2;

                }
                break;

            case Elements.Thunder:
                switch (Def)
                {
                    case Elements.Water:
                        return 2;

                    case Elements.Earth:
                        return 0.5f;

                }
                break;

            case Elements.Dark:
                switch (Def)
                {

                    case Elements.Light:
                        return 2;


                    case Elements.Mechanical:
                        return 0.5f;

                }
                break;

            case Elements.Light:
                switch (Def)
                {

                    case Elements.Dark:
                        return 2;

                    case Elements.Mechanical:
                        return 0.5f;

                }
                break;

            case Elements.Mechanical:

                if (Def == Elements.Mechanical) { return 2; }

                break;
        }

        return 1;
    }
	private void HungerCheck()
    {
        if (hunger <= 0)
        {
            CharCurrHp = CharCurrHp - (0.1f * CharMaxHp);
        }
    }
    protected void TurnTickover()
    {
        HungerCheck();
        hunger--;
        justMoved = true;
        PlayerSkillCD();
        if (StatusDUR > 0) 
        {
            StatusDUR--;
            if (StatusDUR == 0)
            {
                CurEffect = Effects.none;
            }
        }

    }

    protected virtual void PlayerSkillCD()
    { }
	void Awake()
	{

		source = GetComponent<AudioSource>();

	}
	public void BasicAttack(CompassDir Dir)
    {
        TurnToFace(Dir);

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, CharAttRange)) //raycasting for characters originates from floortile level!
        {
			//source.PlayOneShot(HitFX);
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.green, CharAttRange);
            RaycastTarget = hit.transform.gameObject; //had issues with this in past
            //Debug.Log(RaycastTarget);
            if (RaycastTarget.tag == "PlayerChar" || RaycastTarget.tag == "Enemy")
            {
                RaycastTarget.GetComponent<IEntityBase>().ATKBuffMulti = 1;
                RaycastTarget.GetComponent<IEntityBase>().ATKLvl = CharLevel;
                RaycastTarget.GetComponent<IEntityBase>().ATKAtt = CharAtt;
                RaycastTarget.GetComponent<IEntityBase>().ATKWepMulti = 1;
                RaycastTarget.GetComponent<IEntityBase>().ATKSkillMulti = 1;
                RaycastTarget.GetComponent<IEntityBase>().ATKElement = CharElement;
                RaycastTarget.GetComponent<IEntityBase>().ATKEffects = Effects.none; //effects unimplemented
                RaycastTarget.GetComponent<IEntityBase>().ATKInQue = true;
				//only one attack processed per turn! reset on process and add each attack to the struct, or make an actual struct and have each attack be one added to the character.


			}
        }
        TurnTickover();

    }

    public void Multiattack(CompassDir Dir)
    {
        TurnToFace(Dir);

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, CharAttRange)) //raycasting for characters originates from floortile level!
        {
            //Coroutine this to make damage be applied in several swings rather than one, then end turn
            RaycastTarget = hit.transform.gameObject;

            if (RaycastTarget.tag == "Enemy")
            {
                RaycastTarget.GetComponent<IEntityBase>().ATKBuffMulti = 1;
                RaycastTarget.GetComponent<IEntityBase>().ATKLvl = CharLevel;
                RaycastTarget.GetComponent<IEntityBase>().ATKAtt = CharAtt;
                RaycastTarget.GetComponent<IEntityBase>().ATKWepMulti = 1;
                RaycastTarget.GetComponent<IEntityBase>().ATKSkillMulti = 1;
                RaycastTarget.GetComponent<IEntityBase>().ATKElement = CharElement;
                RaycastTarget.GetComponent<IEntityBase>().ATKEffects = Effects.none; //effects unimplemented
                RaycastTarget.GetComponent<IEntityBase>().ATKInQue = true;
                //only one attack processed per turn! reset on process and add each attack to the struct, or make an actual struct and have each attack be one added to the character.
            }
        }
        TurnTickover();

    }

    public void SkillAttack(GameObject Target, float SkillMulti)
    {


        Target.GetComponent<IEntityBase>().ATKBuffMulti = 1;
        Target.GetComponent<IEntityBase>().ATKLvl = CharLevel;
        Target.GetComponent<IEntityBase>().ATKAtt = CharAtt; //something else here!
        Target.GetComponent<IEntityBase>().ATKWepMulti = 1;
        Target.GetComponent<IEntityBase>().ATKSkillMulti = SkillMulti;
        Target.GetComponent<IEntityBase>().ATKElement = CharElement;
        Target.GetComponent<IEntityBase>().ATKEffects = Effects.none; //effects unimplemented
        Target.GetComponent<IEntityBase>().ATKInQue = true;
        //only one attack processed per turn! reset on process and add each attack to the struct, or make an actual struct and have each attack be one added to the character.
    }
        

    
    protected void TurnToFace(CompassDir Dir)
    {

        if (Dir == CompassDir.N)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

        if (Dir == CompassDir.E)
        {
            transform.eulerAngles = new Vector3(0, 90, 0);
        }

        if (Dir == CompassDir.S)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }

        if (Dir == CompassDir.W)
        {
            transform.eulerAngles = new Vector3(0, 270, 0);
        }
    }
    protected void DeriveStats()
    {
        CharMaxHp = BaseMaxHp + (CharLevel - 1) * (((2 * BaseMaxHp * CharLevel) / 100) + CharLevel + 10);
        CharAtt = BaseAtt + (CharLevel - 1) * (((2 * BaseAtt * CharLevel) / 100) + CharLevel + 10);
        CharDef = BaseDef + (CharLevel - 1) * (((2 * BaseDef * CharLevel) / 100) + CharLevel + 10);

        if (CharCurrHp <= 0)
        {
            CharCurrHp = CharMaxHp;
        }
    }
    protected float EffectCheck()
    {
        //same thing as element check, sort of.
        return 1;
    }

    protected void OnTriggerEnter (Collider Other)
    {
        if (Other.tag == "Xp" && IsPlayer == true)
        {
            Destroy(Other.gameObject);
            XpGems++;
        }
    }

    
    protected void ApplyAttack(Elements AttElement, float buffMulti, float lvl, float att, float wepMulti, float skillMulti, Effects ApplyEff)
    {
        //float DefenderDeubuffMulti = this.
        

        float def = CharDef;
        float EffMulti = EffectCheck();
        float EleMulti = ElementCheck(CharElement, AttElement);

        float Hplost = ((2 * lvl) * (att / def) * skillMulti * wepMulti * EleMulti * EffMulti) * 10;

        GameObject Go = Instantiate(DamageText, transform.position, new Quaternion(0,0,0,0));
        DamageText Dt = Go.GetComponent<DamageText>();
        Dt.Damage = Mathf.RoundToInt(Hplost);
        Dt.Damage = Dt.Damage - (Dt.Damage * 2);

        CharCurrHp -= Hplost;
        //this.ApplyEffect(other.ApplyEff)
        StartCoroutine(DelayToSynchroniseAnimations());

    }

    private IEnumerator DelayToSynchroniseAnimations()
    {
        yield return new WaitForSeconds(1.5f);
        Anims.SetTrigger("Damaged");
    }

    protected void HpCheck()
    {

        if (ATKInQue == true)
        {
            ApplyAttack(ATKElement, ATKBuffMulti, ATKLvl, ATKAtt, ATKWepMulti, ATKSkillMulti, ATKEffects); //probably should be elsewhere TODO
            ATKInQue = false;
        }

        if (CharCurrHp <= 0)
        {
            CharCurrHp = 0;





                if (this.tag == "Enemy")
            {

                if (dungeon.internalPlayer.GetComponent<ICharacterBase>().GetHP() > 0)
                {
                    StartCoroutine(EnemyDeathTransition());

                }
            }

            if (this.tag == "PlayerChar")
            {
                ICharacterBase character = (ICharacterBase)this;
                if (character.hasFairy)
                {
                    DoFairyStuff(character);
                }
                else
                {


                    StartCoroutine(PlayerDeathTransition());
                    //this.transform.position = Vector3.zero;

                    Persistent.savedSofiaStats = new Persistent.PersistentStats();
                    Persistent.savedVictorStats = new Persistent.PersistentStats();

                }
            }
        }
    }

    private void DoFairyStuff(ICharacterBase theCharacter)
    {
        Debug.Log("Visual Feedback for fairy");
        theCharacter.SetHP(theCharacter.CharMaxHp);
        theCharacter.hasFairy = false;
    }

    IEnumerator EnemyDeathTransition()
    {

            yield return new WaitForSeconds(2);



            Anims.SetTrigger("Dead");

            yield return new WaitForSeconds(1);
            node.isOccupied = false;
        if (!droppedLoot)
        {
            DropLoot();

        }


        int i = 0;
            while (i < CharLevel)
            {
                Instantiate(XpCrystal, this.transform.position, this.transform.rotation);
                i++;
            }

            this.transform.position = Vector3.zero;
        gameObject.SetActive(false);
            dungeon.enemyList.Remove((IEnemyBase)this);
            Destroy(this);
        
    }
    private IEnumerator PlayerDeathTransition()
    {
        Anims.SetTrigger("Dead");


        yield return new WaitForSeconds(3);

        float fadeTime = 3f;
        float fadeSpeed = .5f / fadeTime;

        dungeon.gameObject.GetComponent<Fade>().BeginFade(Fade.FadeDirection.OUT, fadeSpeed);

        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene("Game Over");
    }
    private void DropLoot()
    {
        droppedLoot = true;
        if (GetRandom(13))
        {
            dungeon.PlaceItem(fairy, transform.position, 0);
        }
        else if (GetRandom(2))
        {
            dungeon.PlaceItem(dungeon.treasure, transform.position, 0);
        }
        //Weapons
        //if (GetRandom(70))
        //{
        //    //5 star
        //    DropItem(emptyWeapon, IItemBase.ITEMTYPE.fiveStarWeapon);
        //}
        //else if (GetRandom(50))
        //{
        //    DropItem(emptyWeapon, IItemBase.ITEMTYPE.fourStarWeapon);

        //}
        //else if (GetRandom(50))
        //{
        //    DropItem(emptyWeapon, IItemBase.ITEMTYPE.threeStarWeapon);

        //}
        //else if (GetRandom(45))
        //{
        //    DropItem(emptyWeapon, IItemBase.ITEMTYPE.twoStarWeapon);
        //}
        //else if (GetRandom(35))
        //{
        //    DropItem(emptyWeapon, IItemBase.ITEMTYPE.oneStarWeapon);
        //}

        ////Scrolls
        //if (GetRandom(55))
        //{
        //    DropItem(emptyScroll, IItemBase.ITEMTYPE.warpToTown);
        //}

        //else if (GetRandom(35))
        //{
        //    DropItem(emptyScroll, IItemBase.ITEMTYPE.statusScroll);
        //}

        //else if (GetRandom(25))
        //{
        //    DropItem(emptyScroll, IItemBase.ITEMTYPE.damageScroll);
        //}

        //Potions
        //if (GetRandom(40))
        //{
        //    DropItem(emptyPotion, IItemBase.ITEMTYPE.panacea);
        //}

        //else if (GetRandom(35))
        //{
        //    DropItem(emptyPotion, IItemBase.ITEMTYPE.bigRedPotion);
        //}

        //else if (GetRandom(20))
        //{
        //    DropItem(emptyPotion, IItemBase.ITEMTYPE.redPotion);
        //}


        ////Food
        //if (GetRandom(25))
        //{
        //    DropItem(emptyFood, IItemBase.ITEMTYPE.largeMeat);

        //}
        //else if (GetRandom(15))
        //{
        //    DropItem(emptyFood, IItemBase.ITEMTYPE.smallMeat);

        //}

    }
    

    private void DropItem(GameObject newItem, IItemBase.ITEMTYPE type)
    {
        GameObject gamObj = dungeon.PlaceItem(newItem, transform.position, 0);
        //IItemBase item = new IItemBase(type);//Never used?
        //gamObj.AddComponent<IItemBase>();
    }

    void OnEnable()
    {
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
        pathfinding = dungeon.pathfinder;

        UpdateNode();

        Anims = GetComponentInChildren<Animator>();



    }

    public void UpdateNode()
    {
        if (dungeon == null)
        {
            dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
        }

        
        if ((int)transform.position.x >= 0 && (int)transform.position.x < dungeon.width && (int)transform.position.z >= 0 && (int)transform.position.z < dungeon.height)
        {
            Node prevNode;

            if (node != null)
            {
                prevNode = node;
                if (prevNode.terrain == DungeonGenerator.TERRAINTYPE.Grass)
                {
                    prevNode.isOccupied = false;
                }
            }

            node = dungeon.grid[(int)Math.Round(transform.position.x, MidpointRounding.AwayFromZero), (int)Math.Round(transform.position.z, MidpointRounding.AwayFromZero)];
            node.isOccupied = true;

        }
        else
        {
            Debug.Log("Didn't update node!!");
        }
    }
    bool GetRandom(int denominator)
    {
        int rand = UnityEngine.Random.Range(0, denominator);
        if (rand == 0)
        {
            return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (ATKInQue)
        {
            debugAttack = true;
        }

        if (debugAttack)
        {
            Gizmos.DrawLine(transform.position, transform.TransformDirection(Vector3.forward));
        }
    }
}

