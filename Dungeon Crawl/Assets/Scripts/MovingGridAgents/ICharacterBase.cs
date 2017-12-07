using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ICharacterBase : IEntityBase
{
    public bool startedAttack, finishedAttack, hasFairy;
    int xp;

    public List<Potion> heldPotions;
    public ParticleSystem levelUP, revive;
    public List<Scroll> heldScrolls;
    public int storedFood = 0;
    public int storedPotion = 0;


    //Movement Variables
    float speed = 2.0f;
    float minSwipeFactor = 1;
    public Vector3 targetPos;
    Transform tr;
    Vector2 startPos;
    Vector2 direction;
    GameObject Cam;
    Animation Camclip;
    

    Text HungerText;
    Text HpText;
    Text LevelCountText;   
    GameObject XpBar;

    Animator CharAnims;

    protected GameObject SkillRadial;
    protected Button SkillButton1;
    protected Button SkillButton2;
    protected RawImage Skill1ButtonTex;
    protected RawImage Skill2ButtonTex;
    public GameObject Skill1Eff;
    public GameObject Skill2Eff;
    public Texture2D Skill1Tex;
    public Texture2D Skill2Tex;
    public Texture2D CDtex;

    protected bool SkillTargeting;

    protected int L1MaxHp;
    protected int L1Att;
    protected int L1Def;
    protected int L50MaxHp;
    protected int L50Att;
    protected int L50Def;
    bool IsoCam;

    protected bool PlayerHasControl;
    protected int HpGrowth;
    protected int AttGrowth;
    protected int DefGrowth;

    protected int CharRank;
    protected int WeaponBonus;
    protected int SkillBonus;

    protected GameObject treasureHolder;

    List<Rect> Screengrid = new List<Rect>();

    int cols;
    int rows;

    CompassDir dir;

    Vector3 lastTarget;
    bool waiting = false;

    static int idleStateHash = Animator.StringToHash("Idle");

    protected virtual void Skill1()
    {; }
    protected virtual void Skill2()
    {; }

    protected override void PlayerSkillCD()
    {
        if (Skill1CD > 0) //skill 1 cooldown
        { 
        Skill1ButtonTex.texture = CDtex;
            
            Skill1CD--;


            if (Skill1CD == 0) //skill 1 cooldown 
            {
                Skill1ButtonTex.texture = Skill1Tex;
            }
        }
        if (Skill2CD > 0)//skill 2 cooldown 
        {
            Skill2ButtonTex.texture = CDtex;
            Skill2CD--;

            if (Skill2CD == 0)//skill 2 cooldown 
            {
                Skill2ButtonTex.texture = Skill2Tex;
            }
        }

    }

    protected void Awake()
    {
        PlayerHasControl = true;
        finishedAttack = false;
        treasureHolder = GameObject.Find("TreasureHolder");
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();

        FindSkillRadial();

        SkillRadial.SetActive(true);
        SkillButton1 = GameObject.Find("Skill1").GetComponent<Button>();
        SkillButton2 = GameObject.Find("Skill2").GetComponent<Button>();

        Skill1ButtonTex = GameObject.Find("Skill1").GetComponent<RawImage>();
        Skill2ButtonTex = GameObject.Find("Skill2").GetComponent<RawImage>();

        Skill1ButtonTex.texture = Skill1Tex;
        Skill2ButtonTex.texture = Skill2Tex;

        SkillButton1.onClick.AddListener(Skill1);
        SkillButton2.onClick.AddListener(Skill2);

        RaycastTarget = null;
        SkillTargeting = false;


        if (Persistent.playerLevel() > 0)
        {
            CharLevel = Persistent.playerLevel();
        }
        DeriveStats();
        UpdateNode();
        BuildGrid();
        IsPlayer = true;

        tr = this.transform;
        targetPos = this.transform.position;
        lastTarget = targetPos;

        Cam = GameObject.Find("Main Camera");
        CharAnims = GetComponentInChildren<Animator>();
        Anims = CharAnims;
        Camclip = Cam.GetComponent<Animation>();

        //HpHolder = GameObject.FindGameObjectWithTag("HpText");
        HungerText = GameObject.Find("HungerText").GetComponentInChildren<Text>();
        LevelCountText = GameObject.Find("LevelCountText").GetComponentInChildren<Text>();
        HpText = GameObject.Find("HealthText").GetComponentInChildren<Text>();

        XpBar = GameObject.Find("XPBarEnergy");
    }

    private void FindSkillRadial()
    {
        if (dungeon == null)
        {
            SkillRadial = GameObject.Find("SkillRadial");
        }
        else
        {
            SkillRadial = dungeon.SkillRadial;
        }
    }

    protected void Update()
    {
        
        if (SkillTargeting)
        {
            if (Input.GetMouseButtonDown(1))
            {
                SkillTargeting = false;
                CharTap();
                SkillRadial.SetActive(false);
                PlayerHasControl = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                GridTarget();
            }
        }
        LevelCheck();

        HpText.text = Mathf.Round(CharCurrHp).ToString(); //update Hp
        LevelCountText.text = "Level: " + CharLevel.ToString(); //update xp gems
        HungerText.text = hunger.ToString(); //update hunger
        HpCheck();

        HandleInput();
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);// Movement transition between an inital point and a designated destination

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            CharTap(); //DEBUG
        }


        if (startedAttack && CharAnims.GetCurrentAnimatorStateInfo(0).shortNameHash == idleStateHash)
        {
            startedAttack = false;
            finishedAttack = true;
        }

    }
    public float GetHP()
    {
        return CharCurrHp;
    }
    public void SetHP(float newHP)
    {
        CharCurrHp = newHP;
    }
    public int GetXP()
    {
        return XpGems;
    }
    public void SetXP(int newXPGems)
    {
        XpGems = newXPGems;
    }
    public int GetHunger()
    {
        return hunger;
    }
    public void SetHunger(int newHunger)
    {
        hunger = newHunger;
    } 
    public int GetLevel()
    {
        return CharLevel;
    }
    public void SetLevel(int newLevel)
    {
        CharLevel = newLevel;
    }
    protected void LevelCheck()
    {
        if ((Math.Pow(XpGems, (double)1/3)) >= CharLevel)//Wrong math TODO
        {
            CharLevel++;
            DeriveStats();
            Instantiate(levelUP, transform.position, Quaternion.identity);
        }
    }


    protected void DeriveBaseStats()
    {
        BaseMaxHp = L1MaxHp + (HpGrowth * (CharRank - 1));
        BaseAtt = L1Att + (AttGrowth * (CharRank - 1));
        BaseDef = L1Def + (DefGrowth * (CharRank - 1));

        DeriveStats();
    }
    void HandleInput()
    {
        if (PlayerHasControl == true)
        {

            if (Input.GetAxis("Vertical") == 1 && tr.position == targetPos && TestPositionTerrain(targetPos + Vector3.forward))
            {
                dungeon.grid[(int)Math.Round((targetPos + Vector3.forward).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.forward).z, MidpointRounding.AwayFromZero)].isOccupied = true;
                node.isOccupied = false;
                node = dungeon.grid[(int)Math.Round((targetPos + Vector3.forward).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.forward).z, MidpointRounding.AwayFromZero)];

                TurnToFace(CompassDir.N);
                targetPos += Vector3.forward;
                TurnTickover();

                CharAnims.SetTrigger("Walking");
            }
            if (Input.GetAxis("Horizontal") == -1 && tr.position == targetPos && TestPositionTerrain(targetPos + Vector3.left))
            {
                dungeon.grid[(int)Math.Round((targetPos + Vector3.left).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.left).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                node.isOccupied = false;
                node = dungeon.grid[(int)Math.Round((targetPos + Vector3.left).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.left).z, MidpointRounding.AwayFromZero)];
                TurnToFace(CompassDir.W);
                targetPos += Vector3.left;
                TurnTickover();
                CharAnims.SetTrigger("Walking");

            }
            if (Input.GetAxis("Vertical") == -1 && tr.position == targetPos && TestPositionTerrain(targetPos + Vector3.back))
            {
                dungeon.grid[(int)Math.Round((targetPos + Vector3.back).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.back).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                node.isOccupied = false;
                node = dungeon.grid[(int)Math.Round((targetPos + Vector3.back).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.back).z, MidpointRounding.AwayFromZero)];
                TurnToFace(CompassDir.S);
                targetPos += Vector3.back;
                TurnTickover();
                CharAnims.SetTrigger("Walking");

            }
            if (Input.GetAxis("Horizontal") == 1 && tr.position == targetPos && TestPositionTerrain(targetPos + Vector3.right))
            {
                dungeon.grid[(int)Math.Round((targetPos + Vector3.right).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.right).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                node.isOccupied = false;
                node = dungeon.grid[(int)Math.Round((targetPos + Vector3.right).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.right).z, MidpointRounding.AwayFromZero)];
                TurnToFace(CompassDir.E);
                targetPos += Vector3.right;
                TurnTickover();
                CharAnims.SetTrigger("Walking");

            }
            if (Input.GetAxis("Horizontal2") == 1 && tr.position == targetPos)
            {
                HighLevelAttack(CompassDir.E);
            }
            if (Input.GetAxis("Horizontal2") == -1 && tr.position == targetPos)
            {
                HighLevelAttack(CompassDir.W);
            }
            if (Input.GetAxis("Vertical2") == 1 && tr.position == targetPos)
            {
                HighLevelAttack(CompassDir.N);
            }
            if (Input.GetAxis("Vertical2") == -1 && tr.position == targetPos)
            {
                HighLevelAttack(CompassDir.S);
            }
        }

        //Debug.Log(tr.position);
        //Debug.Log("target =" + targetPos);

        if (!waiting && ! justMoved && tr.position == targetPos)// && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            StartCoroutine(waitABit());
        }

        // Track a single touch as a directional vector
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Handle finger movements based on touch phase
            switch (touch.phase)
            {
                //Record initial touch position
                case TouchPhase.Began:
                    startPos = touch.position;

                    break;

                // Determine direction by comparing the current touch position with the initial one
                case TouchPhase.Moved:
                    direction = touch.position - startPos;
                    break;

                // Report that an input has been chosen when the finger is lifted
                case TouchPhase.Ended:

                    if (Mathf.Approximately(startPos.x, touch.position.x) && Mathf.Approximately(startPos.y, touch.position.y) && Screengrid[4].Contains(startPos))
                    { CharTap(); }

                    else if (Mathf.Approximately(startPos.x, touch.position.x) && Mathf.Approximately(startPos.y, touch.position.y))
                    { DetectTouchGrid(); }

                    else if (direction.magnitude > minSwipeFactor)
                    { Move(); }

                    break;
            }
        }
        XpCheck();
        
        PickUpItem();
    }

    private void HighLevelAttack(CompassDir direciton)
    {
        startedAttack = true;
        CharAnims.SetTrigger("Basic Attack");
        //Camclip.clip = Camclip.GetClip("Camera Isometric to FightView");
        //Camclip.Play();

        BasicAttack(direciton);
        Input.ResetInputAxes();
    }

    private void XpCheck()
    {
        // do the calculations once
        XpBar.transform.localScale = new Vector3((float)(( XpGems - Math.Pow(CharLevel - 1, 3) )/ (Math.Pow( CharLevel, 3) - Math.Pow(CharLevel - 1, 3))), 1, 1);
    }

    private void PickUpItem()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (node.items.Count > 0)
            {
                bool itemMarkedForDestroy = true;
                if (node.items[0].GetComponentInChildren<IItemBase>().Value > 0)
                {
                    //node.items[0].transform.parent = null;
                    node.items[0].transform.parent = treasureHolder.transform;
                    node.items[0].transform.localPosition = Vector3.zero;
                    itemMarkedForDestroy = false;
                }
                else
                {
                    //Debug.Log("Use value, but no exchange value!");
                }

                if (node.items[0].GetComponent<Food>() != null)
                {
                    int maxFood = 210;//set max food level 500?

                    if (hunger + node.items[0].GetComponent<Food>().kilojules <= maxFood)
                    {
                        hunger += node.items[0].GetComponent<Food>().kilojules;
                    }
                    else if (hunger < maxFood)
                    {
                        storedFood += node.items[0].GetComponent<Food>().kilojules - maxFood + hunger;
                        hunger = maxFood;
                    }
                    else
                    {
                        storedFood += node.items[0].GetComponent<Food>().kilojules;
                    }
                }
                else if (node.items[0].GetComponent<Potion>() != null)
                {
                    if (CharCurrHp + node.items[0].GetComponent<Potion>().healthRestore <= CharMaxHp)
                    {
                        CharCurrHp += node.items[0].GetComponent<Potion>().healthRestore;
                    }
                    else if (CharCurrHp < CharMaxHp)
                    {
                        storedPotion += node.items[0].GetComponent<Potion>().healthRestore - CharMaxHp + (int)CharCurrHp;
                        CharCurrHp = CharMaxHp;
                    }
                    else
                    {
                        storedPotion += node.items[0].GetComponent<Potion>().healthRestore;
                    }
                }
                else if (node.items[0].GetComponent<Scroll>() != null)
                {
                    heldScrolls.Add((Scroll)node.items[0].GetComponent<Scroll>().GetItem());
                }

                else if (node.items[0].GetComponent<IItemBase>().fairy)
                {
                    hasFairy = true;
                    //GameObject fairy = GameObject.Find("Fairy");
                    //fairy.SetActive(true);
                }

                GameObject doomedObject = node.items[0];
                node.items.Remove(doomedObject);
                dungeon.itemList.Remove(doomedObject);

                if (itemMarkedForDestroy)
                {
                    Destroy(doomedObject);

                }
                if (node.items.Count <= 0)
                {
                    node.containsItem = false;
                }

            }
        }
    }
    protected void DetectTouchGrid()
    {
        if (Screengrid[7].Contains(startPos))
        {
            BasicAttack(CompassDir.S);
            CharAnims.SetTrigger("Basic Att");
        }
        else if (Screengrid[5].Contains(startPos))
        {
            BasicAttack(CompassDir.E);
            CharAnims.SetTrigger("Basic Att");
        }
        else if (Screengrid[1].Contains(startPos))

        {
            BasicAttack(CompassDir.N);
            CharAnims.SetTrigger("Basic Att");
        }
        else if (Screengrid[3].Contains(startPos))
        {
            BasicAttack(CompassDir.W);
            CharAnims.SetTrigger("Basic Att");
        }
    }


    protected void Move()
    {
        dungeon.grid[(int)targetPos.x, (int)targetPos.z].isOccupied = true;
        float angle = Mathf.Atan2(direction.y, direction.x);
        float octant = Mathf.Round(8 * angle / (2 * Mathf.PI) + 8) % 8;
        dir = (CompassDir)octant;  // Typecast to enum: 0 -> E etc

        CharAnims.SetTrigger("Walking");

        {
            if (tr.position == targetPos)
            { 

                if (dir == CompassDir.N && TestPositionTerrain(targetPos + Vector3.forward))
                {
                    dungeon.grid[(int)Math.Round((targetPos + Vector3.forward).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.forward).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                    TurnToFace(CompassDir.N);
                    targetPos += Vector3.forward;
                    CharAnims.SetTrigger("Walking");
                    TurnTickover();
                }
                if (dir == CompassDir.E && TestPositionTerrain(targetPos + Vector3.right))
                {
                    dungeon.grid[(int)Math.Round((targetPos + Vector3.right).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.right).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                    TurnToFace(CompassDir.E);
                    targetPos += Vector3.right;
                    CharAnims.SetTrigger("Walking");
                    TurnTickover();
                }
                if (dir == CompassDir.S && TestPositionTerrain(targetPos + Vector3.back))
                {
                    dungeon.grid[(int)Math.Round((targetPos + Vector3.back).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.back).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                    TurnToFace(CompassDir.S);
                    targetPos += Vector3.back;
                    CharAnims.SetTrigger("Walking");
                    TurnTickover();
                }
                if (dir == CompassDir.W && TestPositionTerrain(targetPos + Vector3.left))
                {
                    dungeon.grid[(int)Math.Round((targetPos + Vector3.left).x, MidpointRounding.AwayFromZero), (int)Math.Round((targetPos + Vector3.left).z, MidpointRounding.AwayFromZero)].isOccupied = true;

                    TurnToFace(CompassDir.W);
                    targetPos += Vector3.left;
                    CharAnims.SetTrigger("Walking");
                    TurnTickover();
                }
            }
        }
    }

    protected void GridTarget()
    {
        
        Ray ray = Cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
			Debug.Log(hit.transform.gameObject);
            RaycastTarget = hit.transform.gameObject; //had issues with this in past
            if(RaycastTarget.tag != "Enemy")
            {
                RaycastTarget = null;
				Debug.Log("Code reached");
            }
			Debug.Log("RaycastTarget" + RaycastTarget);
        }
    }

    public void CharTap()
    {
        if (IsoCam == false)
        {

            Camclip.clip = Camclip.GetClip("Camera Isometric to TopDown");
            Camclip.Play();
            IsoCam = true;
            SkillRadial.SetActive(true);
        }

        else if (IsoCam == true)
        {

            Camclip.clip = Camclip.GetClip("Camera TopDown to Isometric");
            Camclip.Play();
            IsoCam = false;
            SkillRadial.SetActive(false);
        }
    }
    
    private bool TestPositionTerrain(Vector3 testPosition)
    {
        if (!dungeon.grid[(int)testPosition.x,(int)testPosition.z].isOccupied)
        {
            return true;
        }        
        return false;
    }

    public void OnGUI()
    {
        //if (node!= null && node.containsItem && node.items.Count > 0 && node.items[0].GetComponent<IItemBase>()!=null)
        //{
        //    GUI.color = Color.blue;

        //    GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 100, 100), "Press T to pick up item");

        //}
        ////GUI.color = Color.white;


        //GUI.Label(new Rect(0, Screen.height - 60, 300, 100), "Stored Potion = " + storedPotion.ToString());


        //var style = new GUIStyle();
        //style.normal.textColor = Color.black;
        //style.fontSize = 50;
        //int i = 0;
        //foreach (Rect R in Screengrid)
        //{
        //    //GUI.Label(R, i.ToString(), style);
        //    GUI.Box(R, i.ToString());
        //    i++;
        //
        //}
    }

    void BuildGrid()
    {
        cols = 3;
        rows = 3;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Screengrid.Add(new Rect(j * (Screen.width / cols), i * (Screen.height / rows), Screen.width / cols, Screen.height / rows));
            }
        }
    }
    IEnumerator waitABit()
    {
        waiting = true;
        yield return new WaitForSeconds(.2f);


        if (tr.position == targetPos)
        {
            CharAnims.ResetTrigger("Idle"); //Missing Component Exception being thrown


            lastTarget = targetPos;

            CharAnims.SetTrigger("Idle");
        }
        waiting = false;
    }
}