using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/* 
 
     Generates and holds the current level and everything inside
     
     */

public class DungeonGenerator : MonoBehaviour
{
    // Enum holds different terrains
    public enum TERRAINTYPE
    {
        NONE, Grass, Wall, Door
    }

    // Enum for different biomes. Each biome will have its own set of floor and wall tiles
    public enum BIOME
    {
        Cave, Forest, Glacier, Volcano, Void
    }
    private BIOME currentBiome;


    public GameObject[] particles = new GameObject[Enum.GetValues(typeof(BIOME)).Cast<int>().Max()+1];// caveParticles, forestParticles, glacierParticles, volcanoParticles, voidParticles;

    //Necessary for pathfinding system
    [HideInInspector]    public PathManager pathManager;
    [HideInInspector]    public Pathfinding pathfinder;

    // HUD element
    [HideInInspector]
    public GameObject SkillRadial;


    //When a void room is generated how big will it be? Affects length of corridor
    public int voidRoomSize;

    //UI info for when 
    public Font roomUIFont;
    private GUIStyle roomUIStyle;
    private String roomUIInfo;
    private Color textColour;

    //Allow editing of the number of enemies at different depths and the writing displayed on entering a level in inspector
    public List<int> enemyNumberProgression = new List<int>(4);

    public List<string> voidNames;
    public List<string> forestNames;
    public List<string> caveNames;
    public List<string> iceNames;
    public List<string> volcanoNames;

    //Prefabs that will be instantiated

        
    public GameObject goToPreviousSafeRoom, changeCharacter, rankUp,                                                            //Interactables in safe room
        floorTileVoid, floorTileCave, floorTileFactory, floorTileForest, floorTileVolcano,                                      //floor tiles
        wallTileVoid, wallTileCave, wallTileFactory, wallTileForest,  wallTileVolcano,                                          //wall tiles
        stairTile, spikeTile, batTile, player, keyTile, doorTile, item1, food, potion, equipment, scroll, treasure, trapdoor;   //enemies and items

    [HideInInspector]
        public GameObject internalPlayer, internalStair, internalKey, internalTrapdoor;                                         // for checking progression through keyed levels
       bool isKeyedLevel = false;

    //holds captured treasure
    private GameObject treasureHolder;

    //size of dungeon
    [HideInInspector]
    public int width, height;

    private List<Room> roomList = new List<Room>();
    [HideInInspector]
    public List<IEnemyBase> enemyList = new List<IEnemyBase>();
    [HideInInspector]
    public List<GameObject> itemList = new List<GameObject>();

    //This is the map of the level
    public Node[,] grid;

    CameraManager cameraScript;

    private bool descendingLevel = false;

    [HideInInspector]
    public int dungeonLevel = 0;

    //  Used to calculate the distribution of treasure
    int[] cumulativeDungeonTreasureRarities;
    int cumulativeRarity = 0;




    void Awake()
    {   
        //seed random
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        //Setup transperant fading text
        textColour = GUI.color;
        textColour.a = 1;
        roomUIStyle = new GUIStyle();
        roomUIStyle.normal.textColor = Color.white;
        roomUIStyle.font = roomUIFont;
        roomUIStyle.fontSize = 80;
        UpdateRoomUIInfo();

        //Locate references
        treasureHolder = GameObject.Find("TreasureHolder");
        cameraScript = GameObject.Find("CameraPivot").GetComponent<CameraManager>();
        SkillRadial = GameObject.Find("SkillHolder");
        
        //Add pathfinding
        pathfinder = gameObject.AddComponent<Pathfinding>();
        pathManager = gameObject.AddComponent<PathManager>();
 
        currentBiome = (BIOME)UnityEngine.Random.Range(0, Enum.GetValues(typeof(BIOME)).Cast<int>().Max());

        //Based off values in inspector set up the system for randomly choosing treasures from boxes according to rarity
        int numberOfTreasures = treasureHolder.GetComponent<TreasureHolder>().treasures.Count;
        cumulativeDungeonTreasureRarities = new int[numberOfTreasures];
        
        for (int i = 0; i < numberOfTreasures; i++)
        {
            cumulativeRarity += (int)treasureHolder.GetComponent<TreasureHolder>().treasures[i].GetComponentInChildren<IItemBase>().rarity;
            cumulativeDungeonTreasureRarities[i] = cumulativeRarity;
        }

        //Store dungeon level and generate
        dungeonLevel = Persistent.lastDungeonLevel;
        GenerateLevel(dungeonLevel);      
    }


    void Update()
    {
        //Fade text
        if (textColour.a > 0)
        {
            textColour.a = Mathf.MoveTowards(textColour.a, 0, 0.7f * Time.deltaTime);
        }

        // Display skills

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (SkillRadial.activeSelf)
            {
                SkillRadial.SetActive(false);

            }
            else
            {
                SkillRadial.SetActive(true);

                if (internalPlayer.GetComponent<ICharacterBase>().hasFairy)
                {
                    SkillRadial.transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    if (SkillRadial.transform.childCount > 4)
                    {
                        SkillRadial.transform.GetChild(1).gameObject.SetActive(false);

                    }
                }
            }
        }


        // Cheat to debug
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            UpdatePersistent(internalPlayer.GetComponent<ICharacterBase>());
            StartCoroutine(DescendLevel());
        }


        //Check if you have the key to progress if there is a trapdoor on the stairs
        if (isKeyedLevel)
        {
            float tolerance = 0.4f;

            if (Mathf.Abs(internalPlayer.transform.position.x - internalKey.transform.position.x) < tolerance
                && Mathf.Abs(internalPlayer.transform.position.z - internalKey.transform.position.z) < tolerance 
                && internalKey.transform.parent.transform.childCount > 10)// == gameObject)
            {
                internalKey.transform.parent = internalPlayer.transform;
                internalTrapdoor.transform.localRotation = Quaternion.Euler(-60, 0, 0);
                internalTrapdoor.transform.localPosition = new Vector3(internalTrapdoor.transform.position.x, internalTrapdoor.transform.position.y + 0.5f, internalTrapdoor.transform.position.z - 0.3f);
            }

            if (internalPlayer.GetComponentInChildren<Key>() != null)
            {
                if (Mathf.Abs(internalPlayer.transform.position.x - internalStair.transform.position.x) < tolerance &&
                    Mathf.Abs(internalPlayer.transform.position.z - internalStair.transform.position.z) < tolerance &&
                    !descendingLevel)
                {
                    UpdatePersistent(internalPlayer.GetComponent<ICharacterBase>());
                    StartCoroutine(DescendLevel());
                }
            }
        }

        else
        {
            float tolerance = 0.4f;

            if (Mathf.Abs(internalPlayer.transform.position.x - internalStair.transform.position.x) < tolerance &&
                Mathf.Abs(internalPlayer.transform.position.z - internalStair.transform.position.z) < tolerance &&
                !descendingLevel)
            {
                StartCoroutine(DescendLevel());

                UpdatePersistent(internalPlayer.GetComponent<ICharacterBase>());

            }
        }

        // Split up turns based on player movements
        if (internalPlayer.GetComponent<ICharacterBase>().finishedAttack)
        {
            foreach (IEnemyBase enemy in enemyList)
            {

                enemy.EnemyUpdate();
            }

            internalPlayer.GetComponent<ICharacterBase>().finishedAttack = false;
        }

        else if (internalPlayer.GetComponent<ICharacterBase>().justMoved)
        {
            //if (pathfinder.debugClosedSet != null)
            //{
            //    pathfinder.debugClosedSet.Clear();

            //}

            //reorder enemies

            //foreach (IEnemyBase enemy in enemyList)
            //{
            //    enemy.GetDistanceToPlayer();
            //}

            //enemyList = enemyList.OrderBy(o => o.distToPlayer).ToList();

            foreach (IEnemyBase enemy in enemyList)
            {

                enemy.EnemyUpdate();
            }
            internalPlayer.GetComponent<ICharacterBase>().justMoved = false;
        }
    }


    //restore saved stats
    public void UpdatePersistent(ICharacterBase activePlayer)
    {
        if (activePlayer is Brave_Soldier_Sofia)
        {
            Persistent.savedSofiaStats.hp = activePlayer.GetHP();
            Persistent.savedSofiaStats.hunger = activePlayer.GetHunger();
            Persistent.savedSofiaStats.level = activePlayer.GetLevel();
            Persistent.savedSofiaStats.xpGems = activePlayer.GetXP();

            Persistent.savedSofiaStats.storedFood = activePlayer.storedFood;
            Persistent.savedSofiaStats.storedPotion = activePlayer.storedPotion;
        }
        else if (activePlayer is Stitched_Man_Victor)
        {
            Persistent.savedVictorStats.hp = activePlayer.GetHP();
            Persistent.savedVictorStats.hunger = activePlayer.GetHunger();
            Persistent.savedVictorStats.level = activePlayer.GetLevel();
            Persistent.savedVictorStats.xpGems = activePlayer.GetXP();

            Persistent.savedVictorStats.storedFood = activePlayer.storedFood;
            Persistent.savedVictorStats.storedPotion = activePlayer.storedPotion;
        }
    }


    //generate by level
    public void GenerateLevel(int currentDungeonLevel)
    {
        foreach (GameObject particleSystem in particles)
        {
            particleSystem.SetActive(false);
        }
        if (particles[(int)currentBiome] != null)
        {
            particles[(int)currentBiome].SetActive(true);
        }

        if (SkillRadial != null)
        {
            SkillRadial.SetActive(true);

        }

        bool keyedLevelTrue = false;

        //50/50 chance of needing to find key
        if (UnityEngine.Random.Range(0, 2) > 0)
        {
            keyedLevelTrue = true;
        }
   
        // It will be uncommon to have a void room
        bool voidLevelTrue = false;
        int voidRoomRoll = UnityEngine.Random.Range(0, 100); 


        //Safe room every 5 levels
        //Change Biome every safe room
        
        if (currentDungeonLevel % 5 == 0)// && currentDungeonLevel < 100)
        {

            GenerateSafeRoom(currentBiome);

            BIOME newBiome = (BIOME)UnityEngine.Random.Range(0, Enum.GetValues(typeof(BIOME)).Cast<int>().Max());

            if (newBiome == currentBiome)
            {
                int revisedNewBiome = (int)newBiome;
                revisedNewBiome += UnityEngine.Random.Range(1, Enum.GetValues(typeof(BIOME)).Cast<int>().Max());
                if (revisedNewBiome > Enum.GetValues(typeof(BIOME)).Cast<int>().Max())
                {
                    revisedNewBiome -= Enum.GetValues(typeof(BIOME)).Cast<int>().Max();
                }
                currentBiome = (BIOME)revisedNewBiome;
            }
            else
            {
                currentBiome = newBiome;
            }
        }

        // Progressively increase scale of rooms and difficulty based on level descended

        else if (currentDungeonLevel < 10)
        {
            if (voidRoomRoll < 5)
            {
                voidLevelTrue = true;
            }

            GenerateLevel(keyedLevelTrue, voidLevelTrue, currentBiome, 7, 5, 5, 4, 3, 2, enemyNumberProgression[0], false);

        }

        else if (currentDungeonLevel < 30)
        {
            if (voidRoomRoll < 10)
            {
                voidLevelTrue = true;
            }

            GenerateLevel(keyedLevelTrue, voidLevelTrue, currentBiome, 7, 6, 7, 5, 3, 3, enemyNumberProgression[1], false);
        }

        else if (currentDungeonLevel < 45)
        {
            if (voidRoomRoll < 15)
            {
                voidLevelTrue = true;
            }

            GenerateLevel(keyedLevelTrue, voidLevelTrue, currentBiome, 7, 6, 7, 5, UnityEngine.Random.Range(2, 4), 5, enemyNumberProgression[2], false);
        }

        else
        {
            if (voidRoomRoll < 15)
            {
                voidLevelTrue = true;
            }

            GenerateLevel(keyedLevelTrue, voidLevelTrue, currentBiome, 7, 5, 7, 5, UnityEngine.Random.Range(2, 5), UnityEngine.Random.Range(2, 5), enemyNumberProgression[3], false);
        }


    }
   

    //Generate based on parameters
    public void GenerateLevel(bool isKeyed, bool containsVoidRoom, BIOME roomBiome, int maxRoomWidth, int minRoomWidth, int maxRoomHeight, int minRoomHeight, int rowsRooms, int columnsRooms, int numberOnEnemies, bool isSafeRoom)
    {

        //Restore text to opaque
        textColour.a = 1;

        //Record level reached
        dungeonLevel++;

        if (dungeonLevel > Persistent.greatestVisitedDungeonLevel)
        {
            Persistent.greatestVisitedDungeonLevel = dungeonLevel;
        }

        isKeyedLevel = isKeyed;


        //Set size of node grid
        width = columnsRooms * maxRoomWidth + 2 * voidRoomSize;
        height = rowsRooms * maxRoomHeight + 2 * voidRoomSize;

        grid = new Node[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Node n = new Node();
                n.x = i;
                n.y = j;
                grid[i, j] = n;
            }
        }

        //Destroy previous level
        if (gameObject.transform.childCount > 0)
        {
            foreach (Transform child in gameObject.transform)
            {
                Destroy(child.gameObject);
            }
        }


        if (roomList.Count> 0)
        {
            foreach (Room room in roomList)
            {
                Destroy(room);
            }
        }


        if (enemyList.Count > 0)
        {
            foreach (IEnemyBase lad in enemyList)
            {
                Destroy(lad);
            }
        }


        if (internalPlayer != null)
        {
            Destroy(internalPlayer);
        }




        // Make room geometry

        roomList = new List<Room>();
        enemyList = new List<IEnemyBase>();

        GenerateRooms(rowsRooms, columnsRooms, maxRoomWidth, minRoomWidth, maxRoomHeight, minRoomHeight);
        GenerateCorridors(rowsRooms, columnsRooms);
        if (containsVoidRoom)
        {
            GenerateVoidRoom(rowsRooms, columnsRooms);
        }

        if (isKeyed)
        {
            PlaceKey(keyTile, rowsRooms, columnsRooms);
        }

        //First level use specified default player prefab
        if (Persistent.chosenPlayer == null)
        {
            Persistent.chosenPlayer = player;
        }

        
        if (!isSafeRoom)
        {
            RoughUpRooms();
            PlaceStairs(stairTile, rowsRooms, columnsRooms);
            MakeStairsWalkableAgain_MSWA();
            PlaceCharacter(Persistent.chosenPlayer, rowsRooms, columnsRooms);
            PlaceInGameItems(rowsRooms, columnsRooms);

            PlaceMonsters(numberOnEnemies, rowsRooms, columnsRooms);

        //Make sure enemies are connected to character

            foreach (IEnemyBase enemy in enemyList)
            {
                enemy.FindCharacter();

                enemy.targetPos = enemy.transform.position;

                enemy.CharLevel = Mathf.RoundToInt(UnityEngine.Random.Range(Math.Max(dungeonLevel - 3, 0), dungeonLevel + 3));
                if (enemy.CharLevel < 1)
                {
                    enemy.CharLevel = 1;
                }
                if (enemy.CharLevel >= (dungeonLevel + 1)) //Foreign code; appologies if i broke something!
                {
                    enemy.transform.localScale = enemy.transform.localScale + (enemy.transform.localScale * 0.5f);
                }
            }


        }





        InstantiateTiles(roomBiome);
        UpdateRoomUIInfo();

        if (SkillRadial != null)
        {
            SkillRadial.SetActive(false);

        }
    }

    private void Start()
    {
        if (SkillRadial != null)
        {
            SkillRadial.SetActive(false);

        }
    }


    private void PlaceInGameItems(int rowsRooms, int columnsRooms)
    {
        for (int i = 0; i < 3; i++)
        {
            PlaceItem(treasure, rowsRooms, columnsRooms, 0.01f);
        }

        PlaceItem(potion, rowsRooms, columnsRooms, 0.01f);
        PlaceItem(potion, rowsRooms, columnsRooms, 0.01f);
        PlaceItem(food, rowsRooms, columnsRooms, 0.01f);
        PlaceItem(food, rowsRooms, columnsRooms, 0.01f);
    }

    // Allow the player to walk on the stair tile
    private void MakeStairsWalkableAgain_MSWA()
    {
        grid[(int)internalStair.transform.position.x, (int)internalStair.transform.position.z].isOccupied = false;
        grid[(int)internalStair.transform.position.x, (int)internalStair.transform.position.z].containsItem = true;
        if (isKeyedLevel)
        {
            grid[(int)internalKey.transform.position.x, (int)internalKey.transform.position.z].isOccupied = false;
            grid[(int)internalKey.transform.position.x, (int)internalKey.transform.position.z].containsItem = true;

        }
    }

    //Stop the rooms from all being rectangular by choosing random corners to erase
    //
    //Same method, just repeated 4x with each different corner of the rectangle
    private void RoughUpRooms()
    {
        foreach (Room room in roomList)
        {
            if (!room.isAttachedToVoidRom)
            {
                //Number of corners to take out
                int numberOfCornersToDisfigue = UnityEngine.Random.Range(1, 5);
                List<int> usedCorners = new List<int>();

                //Choose which particular corners
                for (int i = 0; i < numberOfCornersToDisfigue; i++)
                {
                    int chosenCornerTopLeftClockwise = UnityEngine.Random.Range(0, 4);

                    while (room.exits[chosenCornerTopLeftClockwise].isRoughed)
                    {
                        if (chosenCornerTopLeftClockwise < 3)
                        {
                            chosenCornerTopLeftClockwise++;
                        }
                        else
                        {
                            chosenCornerTopLeftClockwise = 0;
                        }
                    }

                    int randomX, randomY;


                    switch (chosenCornerTopLeftClockwise)
                    {
                        /*
                         room.exits[x]:
                                0 = n
                                1 = e
                                2 = s
                                3 = w
                         */
                        
                        
                        //the number of x and y tiles to remove from the room based on corner location
                        
                        case 0:
                            if (room.northExit)
                            {
                                randomX = UnityEngine.Random.Range(0, room.exits[0].xPos);
                            }
                            else
                            {
                                randomX = UnityEngine.Random.Range(0, room.width / 2);
                            }


                            if (room.westExit)
                            {
                                randomY = UnityEngine.Random.Range(room.exits[3].yPos + 1, room.height);
                            }
                            else
                            {
                                randomY = UnityEngine.Random.Range(room.height / 2, room.height);
                            }


                            for (int x = 0; x < randomX; x++)
                            {
                                for (int y = randomY; y < room.height; y++)
                                {
                                    if (x == randomX - 1 || y == randomY)
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.Wall;
                                    }
                                    else
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.NONE;
                                    }
                                }
                            }

                            room.exits[0].isRoughed = true;
                            break;
                        case 1:
                            if (room.northExit)
                            {
                                randomX = UnityEngine.Random.Range(room.exits[0].xPos + 1, room.width);
                            }
                            else
                            {
                                randomX = UnityEngine.Random.Range(room.width / 2, room.width);
                            }
                            if (room.eastExit)
                            {
                                randomY = UnityEngine.Random.Range(room.exits[1].yPos + 1, room.height);
                            }
                            else
                            {
                                randomY = UnityEngine.Random.Range(room.height / 2, room.height);
                            }

                            for (int x = randomX; x < room.width; x++)
                            {
                                for (int y = randomY; y < room.height; y++)
                                {
                                    if (x == randomX || y == randomY)
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.Wall;
                                    }
                                    else
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.NONE;
                                    }
                                }
                            }
                            room.exits[1].isRoughed = true;
                            break;
                        case 2:
                            if (room.southExit)
                            {
                                randomX = UnityEngine.Random.Range(room.exits[2].xPos + 1, room.width);
                            }
                            else
                            {
                                randomX = UnityEngine.Random.Range(room.width / 2, room.width);
                            }

                            if (room.eastExit)
                            {
                                randomY = UnityEngine.Random.Range(0, room.exits[1].yPos);
                            }
                            else
                            {
                                randomY = UnityEngine.Random.Range(0, room.height / 2);
                            }
                            for (int x = randomX; x < room.width; x++)
                            {
                                for (int y = 0; y < randomY; y++)
                                {
                                    if (x == randomX || y == randomY - 1)
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.Wall;
                                    }
                                    else
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.NONE;
                                    }
                                }
                            }
                            room.exits[2].isRoughed = true;
                            break;
                        case 3:

                            if (room.southExit)
                            {
                                randomX = UnityEngine.Random.Range(0, room.exits[2].xPos);
                            }
                            else
                            {
                                randomX = UnityEngine.Random.Range(0, room.width / 2);
                            }
                            if (room.westExit)
                            {
                                randomY = UnityEngine.Random.Range(0, room.exits[3].yPos);
                            }
                            else
                            {
                                randomY = UnityEngine.Random.Range(0, room.height / 2);
                            }
                            for (int x = 0; x < randomX; x++)
                            {
                                for (int y = 0; y < randomY; y++)
                                {
                                    if (x == randomX - 1 || y == randomY - 1)
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.Wall;
                                    }
                                    else
                                    {
                                        grid[room.xPos + x, room.yPos + y].terrain = TERRAINTYPE.NONE;
                                    }
                                }
                            }
                            room.exits[3].isRoughed = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }


    //Place the tiles based on biome
    private void InstantiateTiles(BIOME chosenBiome)
    {
        switch (chosenBiome)
        {
            case BIOME.Cave:
                InstantiateTiles(width, height, floorTileCave, wallTileCave);
                break;
            case BIOME.Forest:
                InstantiateTiles(width, height, floorTileFactory, wallTileFactory);
                break;
            case BIOME.Glacier:
                InstantiateTiles(width, height, floorTileForest, wallTileForest);
                break;
            case BIOME.Volcano:
                InstantiateTiles(width, height, floorTileVolcano, wallTileVolcano);
                break;
            case BIOME.Void:
                InstantiateTiles(width, height, floorTileVoid, wallTileVoid);
                break;
            default:
                break;
        }
    }

    //Fade in and out of levels
    private IEnumerator DescendLevel()
    {
        descendingLevel = true;
        gameObject.GetComponent<Fade>().BeginFade(Fade.FadeDirection.OUT, .9f);
        yield return new WaitForSeconds(0.6f);
        GenerateLevel(dungeonLevel);
        gameObject.GetComponent<Fade>().BeginFade(Fade.FadeDirection.IN, .5f);
        yield return new WaitForSeconds(0.3f);
        descendingLevel = false;

    }

    //The void room was intended to contain quests, but it currently exists as just an empty room with a long corridor
    private void GenerateVoidRoom(int rowsOfRooms, int columnsOfRooms)
    {
        int side = UnityEngine.Random.Range(0, 1);
        if (side == 0)
        {
            int chosenRoomX = UnityEngine.Random.Range(2, columnsOfRooms);
            int index = (chosenRoomX) * rowsOfRooms - 1;

            //Debug.Log("Index = " + index);
            //Debug.Log(rowsOfRooms);

            Room chosenRoom = roomList[index];


            int roomOffset = UnityEngine.Random.Range(1, chosenRoom.width - 1);
            int corridorLength = voidRoomSize;
            for (int x = -voidRoomSize / 2; x < voidRoomSize / 2; x++)
            {
                for (int y = 0; y < voidRoomSize; y++)
                {
                    if (x == -voidRoomSize / 2 || y == 0 || x == voidRoomSize / 2 - 1 || y == voidRoomSize - 1)
                    {
                        grid[chosenRoom.xPos + roomOffset + x, chosenRoom.yPos + chosenRoom.height + voidRoomSize + y].terrain = TERRAINTYPE.Wall;
                    }
                    else
                    {
                        grid[chosenRoom.xPos + roomOffset + x, chosenRoom.yPos + chosenRoom.height + voidRoomSize + y].terrain = TERRAINTYPE.Grass;
                    }
                }
            }

            for (int i = 0; i <= corridorLength; i++)
            {
                grid[chosenRoom.xPos + roomOffset - 1, chosenRoom.yPos + chosenRoom.height + i].terrain = TERRAINTYPE.Wall;
                grid[chosenRoom.xPos + roomOffset, chosenRoom.yPos + chosenRoom.height + i].terrain = TERRAINTYPE.Grass;
                grid[chosenRoom.xPos + roomOffset + 1, chosenRoom.yPos + chosenRoom.height + i].terrain = TERRAINTYPE.Wall;
            }

            grid[chosenRoom.xPos + roomOffset, chosenRoom.yPos + chosenRoom.height - 1].terrain = TERRAINTYPE.Grass; //This used to be the door

            chosenRoom.exits[0] = new Room.CorridorEnding(Room.Direction.North, roomOffset, chosenRoom.height - 1);
            chosenRoom.northExit = true;
            chosenRoom.isAttachedToVoidRom = true;
            //debugRoom = chosenRoom;
        }
        else
        {
            int chosenRoomY = UnityEngine.Random.Range(1, columnsOfRooms);

            Room chosenRoom = roomList[roomList.Count - 1];

            int roomOffset = UnityEngine.Random.Range(0, chosenRoom.height);
        }
    }


    //Non-random room with interactable items
    private void GenerateSafeRoom(BIOME currentBiome)
    {

        GenerateLevel(false, false, currentBiome, 7, 7, 7, 7, 1, 1, 0, true);

        //Interactables
        GameObject rankUP = PlaceItem(rankUp, new Vector3(1, 0, 5), 0);
        GameObject characterChanger = PlaceItem(changeCharacter, new Vector3(1, 0, 1), 0);
        GameObject goToPrev = PlaceItem(goToPreviousSafeRoom, new Vector3(5, 0, 1), 0);
        if (Persistent.chosenPlayer == null)
        {
            Persistent.chosenPlayer = player;
        }

        internalPlayer = PlaceItem(Persistent.chosenPlayer, new Vector3(3, 0, 3), 0);
    
        //Snap to position
        GameObject.Find("CameraPivot").transform.position = internalPlayer.transform.position;

        //Saved stats for different characters
        RestorePersistentStats();

        internalStair = PlaceItem(stairTile, new Vector3(5, 0, 5), 0.01f);


        grid[(int)rankUp.transform.position.x, (int)rankUp.transform.position.z].isOccupied = false;
        grid[(int)characterChanger.transform.position.x, (int)characterChanger.transform.position.z].isOccupied = false;
        MakeStairsWalkableAgain_MSWA();


        //Fly treasure at camera
        StartCoroutine(displayCapturedLoot());
        if (SkillRadial != null)
        {
            SkillRadial.SetActive(false);

        }

    }


    //Saved stats for different characters
    private void RestorePersistentStats()
    {
        ICharacterBase characterScript = internalPlayer.GetComponent<ICharacterBase>();

        if (characterScript is Brave_Soldier_Sofia)
        {
            if (Persistent.savedSofiaStats.level != 0)
            {
                characterScript.SetHP(Persistent.savedSofiaStats.hp);
                characterScript.SetHunger(Persistent.savedSofiaStats.hunger);
                characterScript.SetLevel(Persistent.savedSofiaStats.level);
                characterScript.SetXP(Persistent.savedSofiaStats.xpGems);

                characterScript.storedFood = Persistent.savedSofiaStats.storedFood;
                characterScript.storedPotion = Persistent.savedSofiaStats.storedPotion;

            }
        }
        else if (characterScript is Stitched_Man_Victor)
        {
            if (Persistent.savedVictorStats.level != 0)
            {
                characterScript.SetHP(Persistent.savedVictorStats.hp);
                characterScript.SetHunger(Persistent.savedVictorStats.hunger);
                characterScript.SetLevel(Persistent.savedVictorStats.level);
                characterScript.SetXP(Persistent.savedVictorStats.xpGems);

                characterScript.storedFood = Persistent.savedVictorStats.storedFood;
                characterScript.storedPotion = Persistent.savedVictorStats.storedPotion;
            }
        }
        characterScript.UpdateNode();

    }

    private void PlaceKey(GameObject keyTile, int rowsOfRooms, int columnsOfRooms)
    {
        internalKey = PlaceItem(keyTile, rowsOfRooms, columnsOfRooms, 0.01f);
    }

    private void PlaceCharacter(GameObject player, int rowsOfRooms, int columnsOfRooms)
    {
        cameraScript.gameObject.transform.position = internalPlayer.transform.position;
        internalPlayer = PlaceItem(player, rowsOfRooms, columnsOfRooms, 0.01f);

        RestorePersistentStats();
    }

    //Actually place the tiles, based on the 2D grid
    private void InstantiateTiles(int width, int height, GameObject floorTile, GameObject wallTile)
    {
        GameObject tileHolder = new GameObject();
        tileHolder.transform.parent = gameObject.transform;

        tileHolder.name = "Tiles";
        tileHolder.tag = "Tiles";


        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                if (grid[i, j].terrain == TERRAINTYPE.Grass)
                {
                    Instantiate(floorTile, new Vector3(i, 0, j), Quaternion.identity, tileHolder.transform);
                }
                else if (grid[i, j].terrain == TERRAINTYPE.Wall)
                {
                    Instantiate(wallTile, new Vector3(i, 0, j), Quaternion.identity, tileHolder.transform);
                    grid[i, j].isOccupied = true;
                }
                else if (grid[i, j].terrain == TERRAINTYPE.Door)
                {
                    Instantiate(doorTile, new Vector3(i, 0, j), Quaternion.identity, tileHolder.transform);
                }
            }
        }

    }

    private void PlaceMonsters(int numberOfMonsters, int rowsRooms, int columnsRooms)
    {
        for (int i = 0; i < numberOfMonsters; i++)
        {
            if (i % 2 == 0)
            {
                enemyList.Add(PlaceItem(spikeTile, rowsRooms, columnsRooms, .6f).GetComponent<IEnemyBase>());

            }
            else
            {
                enemyList.Add(PlaceItem(batTile, rowsRooms, columnsRooms, .6f).GetComponent<IEnemyBase>());

            }
        }

    }

    private void PlaceStairs(GameObject stairTile, int rowsOfRooms, int columnsOfRooms)
    {
        internalStair = PlaceItem(stairTile, rowsOfRooms, columnsOfRooms, 0.01f);
        internalStair.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        if (isKeyedLevel)
        {
            internalTrapdoor = PlaceItem(trapdoor, internalStair.transform.position, 0f);
            internalTrapdoor.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
    }


    //Deals with all types of game objects, appropriately adding them to the correct lists and adjusting their nodes
    private GameObject PlaceItem(GameObject newObject, int rows, int columns, float height)
    {


        GameObject placedObject;
        int roomIndex = UnityEngine.Random.Range(0, rows * columns);
        Room luckyRoom = roomList[roomIndex];

        int xPos = UnityEngine.Random.Range(1, luckyRoom.width - 1);
        int yPos = UnityEngine.Random.Range(1, luckyRoom.height - 1);



        if (newObject.GetComponentInChildren<ICharacterBase>() != null && Mathf.Abs(luckyRoom.yPos + yPos - internalStair.transform.position.y) > 4 && Mathf.Abs(luckyRoom.xPos + xPos - internalStair.transform.position.x) > 4 && grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].terrain == TERRAINTYPE.Grass && !grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].isOccupied && !grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].containsItem)
        {
            placedObject = Instantiate(newObject, new Vector3(luckyRoom.xPos + xPos, height, luckyRoom.yPos + yPos), Quaternion.identity, gameObject.transform);
            grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].isOccupied = true;
        }

        else if (grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].terrain == TERRAINTYPE.Grass && !grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].isOccupied && !grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].containsItem)
        {
            placedObject = Instantiate(newObject, new Vector3(luckyRoom.xPos + xPos, height, luckyRoom.yPos + yPos), Quaternion.identity, gameObject.transform);
            if (placedObject.GetComponentInChildren<IItemBase>() == null)
            {
                grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].isOccupied = true;
            }
            else
            {
                grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].containsItem = true;
                grid[luckyRoom.xPos + xPos, luckyRoom.yPos + yPos].items.Add(placedObject);
                itemList.Add(placedObject);
            }
        }
        else
        {
            placedObject = PlaceItem(newObject, rows, columns, height);
            //WARNING!! This will cause a stack overflow if all the tiles are full of items!
            //Maybe count the number of floortiles?
        }
        placedObject.name = newObject.ToString();

        return placedObject;
    }

    //place item at position
    public GameObject PlaceItem(GameObject tile, Vector3 position, float height)
    {
        GameObject item = Instantiate(tile, new Vector3(position.x, height, position.z), Quaternion.identity);

        if (item.tag != "PlayerChar")
        {
            grid[(int)position.x, (int)position.z].containsItem = true;
            grid[(int)position.x, (int)position.z].items.Add(item);

        }
        
        item.transform.parent = gameObject.transform;
        return item;
    }

    //Link rooms via corridors using recursive backtracking algorithm
    private void GenerateCorridors(int rows, int columns)
    {
        Stack<Room> roomStack = new Stack<Room>();

        //first room     
        Room activeRoom = roomList[0];
        activeRoom.isVisisted = true;

        int roomsVisited = 0;

        //while there are unvisited rooms

        while (roomsVisited < roomList.Count || Input.GetKey(KeyCode.Escape))
        {
            roomsVisited = 0;
            foreach (Room room in roomList)
            {
                if (room.isVisisted)
                {
                    roomsVisited++;
                }
            }


            List<Room> neighbouringUnvisitedRooms = new List<Room>();

            // above
            if (activeRoom.index + 1 < roomList.Count && (activeRoom.index + 1) % rows != 0 && roomList[activeRoom.index + 1] != null)
            {
                if (!roomList[activeRoom.index + 1].isVisisted)
                {
                    roomList[activeRoom.index + 1].directionToPreviousRoom = Room.Direction.South;
                    neighbouringUnvisitedRooms.Add(roomList[activeRoom.index + 1]);
                }
            }

            //below
            if (activeRoom.index - 1 > 0 && activeRoom.index % rows != 0 && roomList[activeRoom.index - 1] != null)
            {
                if (!roomList[activeRoom.index - 1].isVisisted)
                {
                    roomList[activeRoom.index - 1].directionToPreviousRoom = Room.Direction.North;
                    neighbouringUnvisitedRooms.Add(roomList[activeRoom.index - 1]);
                }
            }

            // left                                           
            if (activeRoom.index + rows < roomList.Count && roomList[activeRoom.index + rows] != null)
            {
                if (!roomList[activeRoom.index + rows].isVisisted)
                {
                    roomList[activeRoom.index + rows].directionToPreviousRoom = Room.Direction.West;
                    neighbouringUnvisitedRooms.Add(roomList[activeRoom.index + rows]);
                }
            }

            // right
            if (activeRoom.index - rows > 0 && roomList[activeRoom.index - rows] != null)
            {
                if (!roomList[activeRoom.index - rows].isVisisted)
                {
                    roomList[activeRoom.index - rows].directionToPreviousRoom = Room.Direction.East;
                    neighbouringUnvisitedRooms.Add(roomList[activeRoom.index - rows]);
                }
            }

            if (neighbouringUnvisitedRooms.Count > 0)
            {
                int randomRoom = UnityEngine.Random.Range(0, neighbouringUnvisitedRooms.Count);

                Room chosenNeighbour = neighbouringUnvisitedRooms[randomRoom];

                roomStack.Push(chosenNeighbour);
                GenerateACorridor(activeRoom, chosenNeighbour);
                activeRoom = chosenNeighbour;
                activeRoom.isVisisted = true;
            }
            else if (roomStack.Count > 0)
            {
                activeRoom = roomStack.Pop();
            }
            else
            {
                break;
            }
        }

    }

    private void GenerateACorridor(Room activeRoom, Room chosenNeighbour)
    {
        activeRoom.exitCount++;
        chosenNeighbour.exitCount++;

        Room nearestRoom, furthestRoom;

        switch (chosenNeighbour.directionToPreviousRoom)
        {
            case Room.Direction.North:

                if (activeRoom.yPos > chosenNeighbour.yPos)
                {
                    nearestRoom = chosenNeighbour;
                    furthestRoom = activeRoom;
                }
                else
                {
                    nearestRoom = activeRoom;
                    furthestRoom = chosenNeighbour;
                }

                for (int m = -1; m < activeRoom.yPos - chosenNeighbour.yPos - chosenNeighbour.height + 1; m++)
                {
                    grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - 1, chosenNeighbour.yPos + chosenNeighbour.height + m].terrain = TERRAINTYPE.Wall;
                    grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2, chosenNeighbour.yPos + chosenNeighbour.height + m].terrain = TERRAINTYPE.Grass;
                    grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 + 1, chosenNeighbour.yPos + chosenNeighbour.height + m].terrain = TERRAINTYPE.Wall;
                }

                activeRoom.exits[2] = (new Room.CorridorEnding(Room.Direction.South, furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - activeRoom.xPos, 0));
                activeRoom.southExit = true;

                chosenNeighbour.exits[0] = (new Room.CorridorEnding(Room.Direction.North, furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - chosenNeighbour.xPos, chosenNeighbour.height - 1));
                chosenNeighbour.northExit = true;

                break;


            case Room.Direction.South:
                if (activeRoom.yPos > chosenNeighbour.yPos)
                {
                    nearestRoom = chosenNeighbour;
                    furthestRoom = activeRoom;
                }
                else
                {
                    nearestRoom = activeRoom;
                    furthestRoom = chosenNeighbour;
                }

                for (int m = -1; m < chosenNeighbour.yPos - activeRoom.yPos - activeRoom.height + 1; m++)
                {
                    grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - 1, activeRoom.yPos + activeRoom.height + m].terrain = TERRAINTYPE.Wall;
                    grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2, activeRoom.yPos + activeRoom.height + m].terrain = TERRAINTYPE.Grass;
                    grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 + 1, activeRoom.yPos + activeRoom.height + m].terrain = TERRAINTYPE.Wall;
                }

                activeRoom.exits[0] = (new Room.CorridorEnding(Room.Direction.North, furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - activeRoom.xPos, activeRoom.height - 1));
                activeRoom.northExit = true;

                chosenNeighbour.exits[2] = (new Room.CorridorEnding(Room.Direction.South, furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - chosenNeighbour.xPos, 0));
                chosenNeighbour.southExit = true;

                break;



            case Room.Direction.East:
                if (activeRoom.yPos > chosenNeighbour.yPos)
                {
                    nearestRoom = chosenNeighbour;
                    furthestRoom = activeRoom;
                }
                else
                {
                    nearestRoom = activeRoom;
                    furthestRoom = chosenNeighbour;
                }

                for (int m = -1; m < activeRoom.xPos - chosenNeighbour.xPos - chosenNeighbour.width + 1; m++)
                {
                    grid[chosenNeighbour.xPos + chosenNeighbour.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - 1].terrain = TERRAINTYPE.Wall;
                    grid[chosenNeighbour.xPos + chosenNeighbour.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 + 1].terrain = TERRAINTYPE.Wall;
                    grid[chosenNeighbour.xPos + chosenNeighbour.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2].terrain = TERRAINTYPE.Grass;
                }

                activeRoom.exits[3] = (new Room.CorridorEnding(Room.Direction.West, 0, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - activeRoom.yPos));
                activeRoom.westExit = true;
                chosenNeighbour.exits[1] = (new Room.CorridorEnding(Room.Direction.East, chosenNeighbour.width - 1, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - chosenNeighbour.yPos));
                chosenNeighbour.eastExit = true;

                break;

            case Room.Direction.West:
                if (activeRoom.yPos > chosenNeighbour.yPos)
                {
                    nearestRoom = chosenNeighbour;
                    furthestRoom = activeRoom;
                }
                else
                {
                    nearestRoom = activeRoom;
                    furthestRoom = chosenNeighbour;
                }

                for (int m = -1; m < chosenNeighbour.xPos - activeRoom.xPos - activeRoom.width + 1; m++)
                {
                    grid[activeRoom.xPos + activeRoom.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - 1].terrain = TERRAINTYPE.Wall;
                    grid[activeRoom.xPos + activeRoom.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 + 1].terrain = TERRAINTYPE.Wall;
                    grid[activeRoom.xPos + activeRoom.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2].terrain = TERRAINTYPE.Grass;

                }

                activeRoom.exits[1] = (new Room.CorridorEnding(Room.Direction.East, activeRoom.width - 1, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - activeRoom.yPos));
                activeRoom.eastExit = true;
                chosenNeighbour.exits[3] = (new Room.CorridorEnding(Room.Direction.West, 0, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - chosenNeighbour.yPos));
                chosenNeighbour.westExit = true;

                break;
            default:
                break;
        }
        //Debug.Log(chosenNeighbour.directionToPreviousRoom);


    }

    //Add rooms based on inspector variables
    private void GenerateRooms(int rows, int columns, int maxRoomWidth, int minRoomWidth, int maxRoomHeight, int minRoomHeight)
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                int roomWidth = UnityEngine.Random.Range(minRoomWidth, maxRoomWidth);
                int roomHeight = UnityEngine.Random.Range(minRoomHeight, maxRoomHeight);

                int roomXOffset = UnityEngine.Random.Range(0, maxRoomWidth - roomWidth);
                int roomYOffset = UnityEngine.Random.Range(0, maxRoomHeight - roomHeight);

                int xPos = i * maxRoomWidth + roomXOffset;
                int yPos = j * maxRoomHeight + roomYOffset;

                GameObject roomHolder = new GameObject();
                roomHolder.transform.position = new Vector3(xPos, 0, yPos);
                roomHolder.transform.parent = gameObject.transform;
                roomHolder.name = "Room";
                roomHolder.AddComponent<Room>();
                roomHolder.GetComponent<Room>().Initialise(xPos, yPos, roomWidth, roomHeight);
                roomHolder.GetComponent<Room>().index = roomList.Count;
                roomList.Add(roomHolder.GetComponent<Room>());
            }
        }

        foreach (Room room in roomList)
        {
            for (int i = 0; i < room.width; i++)
            {
                for (int j = 0; j < room.height; j++)
                {
                    if (i == 0 || i == room.width - 1 || j == 0 || j == room.height - 1)
                    {
                        grid[room.xPos + i, room.yPos + j].terrain = TERRAINTYPE.Wall;
                    }

                    else //if ((i % floorScale == 0 && j % floorScale == 0))
                    {
                        grid[room.xPos + i, room.yPos + j].terrain = TERRAINTYPE.Grass;
                    }
                }
            }
        }
    }

    private void OldGenerateCorridors()
    {
        //if there are neighbouring rooms that aren't visited
        //choose a neighbour
        //push current room to stack
        //make a corridor between current room and neighbour
        //make neighbour current room and mark visited
        //else if stack not empty
        //pop first node
        //make it the current node


        ////vertical
        //for (int i = 0; i < columns; i++)
        //{
        //    for (int j = 0; j < rows; j++)
        //    {
        //        Room currentRoom = roomList[i * rows + j];

        //        Room nextRoom = currentRoom;

        //        if (i * rows + j + 1 < columns * rows)
        //        {
        //            nextRoom = roomList[i * rows + j + 1];
        //        }
        //        else
        //        {
        //            break;
        //        }

        //        if (currentRoom.xPos < nextRoom.xPos + nextRoom.width && currentRoom.xPos + currentRoom.width > nextRoom.xPos)
        //        {
        //            GameObject corridorHolder = new GameObject();
        //            corridorHolder.transform.parent = gameObject.transform;
        //            corridorHolder.name = "Corridor";


        //            Room nearestRoom, furthestRoom;
        //            if (currentRoom.xPos > nextRoom.xPos)
        //            {
        //                nearestRoom = nextRoom;
        //                furthestRoom = currentRoom;
        //            }
        //            else
        //            {
        //                nearestRoom = currentRoom;
        //                furthestRoom = nextRoom;
        //            }

        //            //Corridor corridor = new Corridor(grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2, currentRoom.yPos + nextRoom.yPos - currentRoom.yPos], grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2, currentRoom.yPos + currentRoom.height]);
        //            //nearestRoom.connectedCorridors.Add(corridor);
        //            //furthestRoom.connectedCorridors.Add(corridor);
        //            int rand = UnityEngine.Random.Range(0, 2);

        //            for (int m = -1; m < nextRoom.yPos - currentRoom.yPos - currentRoom.height + 1; m++)
        //            {

        //                grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 - 1, currentRoom.yPos + currentRoom.height + m].terrain = TERRAINTYPE.Wall;

        //                grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2, currentRoom.yPos + currentRoom.height + m].terrain = TERRAINTYPE.Grass;

        //                grid[furthestRoom.xPos + (nearestRoom.xPos + nearestRoom.width - furthestRoom.xPos) / 2 + 1, currentRoom.yPos + currentRoom.height + m].terrain = TERRAINTYPE.Wall;
        //            }

        //        }
        //    }
        //}

        // horizontal
        //for (int i = 0; i < columns; i++)
        //{
        //    for (int j = 0; j < rows; j++)
        //    {
        //        Room currentRoom = roomList[i * rows + j];

        //        Room nextRoom = currentRoom;

        //        if ((i + 1) * rows + j < columns * rows)
        //        {
        //            nextRoom = roomList[(i + 1) * rows + j];
        //        }
        //        else
        //        {
        //            break;
        //        }


        //        if (currentRoom.yPos < nextRoom.yPos + nextRoom.height && currentRoom.yPos + currentRoom.height > nextRoom.yPos)
        //        {
        //            GameObject corridorHolder = new GameObject();
        //            corridorHolder.transform.parent = gameObject.transform;
        //            corridorHolder.name = "Corridor";

        //            Room nearestRoom, furthestRoom;
        //            if (currentRoom.yPos > nextRoom.yPos)
        //            {
        //                nearestRoom = nextRoom;
        //                furthestRoom = currentRoom;
        //            }
        //            else
        //            {
        //                nearestRoom = currentRoom;
        //                furthestRoom = nextRoom;
        //            }

        //            //Corridor corridor = new Corridor(grid[currentRoom.xPos + currentRoom.width, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2]
        //            //                                , grid[currentRoom.xPos + nextRoom.xPos - currentRoom.xPos, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2]);

        //            for (int m = -1; m < nextRoom.xPos - currentRoom.xPos - currentRoom.width + 1; m++)
        //            {
        //                grid[currentRoom.xPos + currentRoom.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 - 1].terrain = TERRAINTYPE.Wall;

        //                grid[currentRoom.xPos + currentRoom.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2 + 1].terrain = TERRAINTYPE.Wall;

        //                grid[currentRoom.xPos + currentRoom.width + m, furthestRoom.yPos + (nearestRoom.yPos + nearestRoom.height - furthestRoom.yPos) / 2].terrain = TERRAINTYPE.Grass;

        //            }
        //        }
        //    }
        //}
    }

    //Every level display text inputted in the inspector
    private void UpdateRoomUIInfo()
    {
        roomUIInfo = Enum.GetName(typeof(BIOME), currentBiome);

        switch (currentBiome)
        {
            case BIOME.Cave:
                if (caveNames.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, caveNames.Count);
                    roomUIInfo = ("" + caveNames[index]);
                }
                break;
            case BIOME.Forest:
                if (forestNames.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, forestNames.Count);
                    roomUIInfo = ("" + forestNames[index]);

                }
                break;
            case BIOME.Glacier:
                if (iceNames.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, iceNames.Count);
                    roomUIInfo = ("" + iceNames[index]);

                }

                break;
            case BIOME.Volcano:
                if (volcanoNames.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, volcanoNames.Count);
                    roomUIInfo = ("" + volcanoNames[index]);

                }

                break;
            case BIOME.Void:
                if (voidNames.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, voidNames.Count);
                    roomUIInfo = ("" + voidNames[index]);

                }

                break;
            default:
                break;
        }
        //Debug.Log(roomUIInfo);
    }

    private void OnGUI()
    {
        //Fade text
        float alpha =  textColour.a;
        textColour = Color.black;
        textColour.a = alpha/2;
        roomUIStyle.normal.textColor = textColour;

        int textWidthFactor = 23;
        GUI.Label(new Rect(Screen.width / 2 - roomUIInfo.Length * textWidthFactor+5, Screen.height / 2+5 , 100, 50), roomUIInfo , roomUIStyle);

        string dungeonLevelString = "\ndungeon level: " + dungeonLevel;

        GUI.Label(new Rect(Screen.width / 2 - dungeonLevelString.Length * (textWidthFactor -5) +3, Screen.height / 2+60+3, 100, 50), dungeonLevelString, roomUIStyle);



        textColour = Color.white;
        textColour.a = alpha;
        roomUIStyle.normal.textColor = textColour;

        GUI.Label(new Rect(Screen.width / 2 - roomUIInfo.Length * textWidthFactor, Screen.height / 2, 100, 50), roomUIInfo, roomUIStyle);
        GUI.Label(new Rect(Screen.width / 2 - dungeonLevelString.Length * (textWidthFactor - 5), Screen.height / 2 + 60, 100, 50), dungeonLevelString, roomUIStyle);


        //GUI.Label(new Rect(0, Screen.height - 40, 300, 100), internalPlayer.GetComponent<ICharacterBase>().heldPotions.Count.ToString());
    }
    //Debug
    //void OnDrawGizmos()
    //{
    //    if (grid != null)
    //    {
    //        Gizmos.color = Color.white;
    //        //if (internalPlayer != null)
    //        //{
    //        //    Gizmos.DrawWireSphere(new Vector3(internalPlayer.GetComponent<ICharacterBase>().node.x, .8f, internalPlayer.GetComponent<ICharacterBase>().node.y), 0.21f);
    //        //}

    //        foreach (Node n in grid)
    //        {
    //            if (pathfinder.debugClosedSet != null)
    //            {
    //                if (pathfinder.debugClosedSet.Contains(n))
    //                {
    //                    Gizmos.color = Color.grey;
    //                    Gizmos.DrawCube(new Vector3(n.x, 1.1f, n.y), new Vector3(0.25f, 0.25f, 0.25f));
    //                }

    //            }
    //            if (n.isOccupied)
    //            {
    //                if (n.terrain == TERRAINTYPE.Wall)
    //                {
    //                    Gizmos.color = Color.red;
    //                    Gizmos.DrawCube(new Vector3(n.x, 1.1f, n.y), new Vector3(0.25f, 0.25f, 0.25f));
    //                }
    //                else
    //                {
    //                    Gizmos.color = Color.green;
    //                    Gizmos.DrawCube(new Vector3(n.x, 1.1f, n.y), new Vector3(0.25f, 0.25f, 0.25f));
    //                }
    //            }
    //            else if (n.containsItem)
    //            {
    //                Gizmos.color = Color.blue;
    //                Gizmos.DrawCube(new Vector3(n.x, 1.1f, n.y), new Vector3(0.25f, 0.25f, 0.25f));
    //            }
    //        }
    //    }
    //    foreach (IEnemyBase madLad in enemyList)
    //    {
    //        if (madLad.chaseState.currentPath != null)
    //        {


    //                //if (enemyList[0] == madLad)
    //                //{
    //                //    Gizmos.color = Color.red;
    //                //    Gizmos.DrawSphere(new Vector3(madLad.targetPos.x, 0, madLad.targetPos.z), 0.3f);
    //                //    foreach (Node n in madLad.chaseState.currentPath)
    //                //    {
    //                //        Gizmos.DrawSphere(new Vector3(n.x, 1.1f, n.y), 0.1f);
    //                //    }

    //                //}
    //                //if (enemyList[1] == madLad)
    //                //{
    //                //    Gizmos.color = Color.white;
    //                //    Gizmos.DrawSphere(new Vector3(madLad.targetPos.x, 0, madLad.targetPos.z), 0.3f);
    //                //    foreach (Node n in madLad.chaseState.currentPath)
    //                //    {
    //                //        Gizmos.DrawSphere(new Vector3(n.x, 1.2f, n.y), 0.1f);
    //                //    }

    //                //}
    //                //if (enemyList[2] == madLad)
    //                //{
    //                //    Gizmos.color = Color.green;
    //                //    Gizmos.DrawSphere(new Vector3(madLad.targetPos.x, 0, madLad.targetPos.z), 0.3f);
    //                //    foreach (Node n in madLad.chaseState.currentPath)
    //                //    {
    //                //        Gizmos.DrawSphere(new Vector3(n.x, 1.3f, n.y), 0.1f);
    //                //    }

    //                //}

    //            if (madLad.chaseState.currentPath.Count > 0)// && madLad.chaseState.secondPath)
    //            {
    //                Gizmos.color = Color.cyan;
    //                Gizmos.DrawCube(new Vector3(madLad.chaseState.currentPath[0].x, .5f, madLad.chaseState.currentPath[0].y), Vector3.one / 2);
    //            }
    //            //    Gizmos.color = Color.yellow;


    //            //    foreach (Node n in madLad.chaseState.currentPath)
    //            //    {
    //            //        Gizmos.DrawSphere(new Vector3(n.x, 1.5f, n.y), 0.1f);
    //            //    }

    //            //}
    //            //else if (madLad.chaseState.currentPath.Count > 0 && enemyList[0] == madLad)
    //            //{
    //            //    Gizmos.color = Color.cyan;


    //            //    foreach (Node n in madLad.chaseState.currentPath)
    //            //    {
    //            //        Gizmos.DrawSphere(new Vector3(n.x, 1.3f, n.y), 0.1f);
    //            //    }


    //            //}
    //        }

    //        //    }
    //        //    //    foreach (Room r in roomList)
    //        //    //    {
    //        //    //        foreach (Room.CorridorEnding exit in r.exits)
    //        //    //        {
    //        //    //            Gizmos.DrawWireSphere(new Vector3(r.xPos + exit.xPos, 1, r.yPos + exit.yPos), 0.1f);
    //        //    //        }
    //        //    }
    //    }
    //}




        //When the saferoom is reached make the treasure fly at you based on rarity
    IEnumerator displayCapturedLoot()
    {
        while (treasureHolder.transform.childCount>0)
        {
            int randomNumber = UnityEngine.Random.Range(0, cumulativeRarity);

            int index = 0;
            for (int i = 1; i < cumulativeDungeonTreasureRarities.Length; i++)
            {
                if (randomNumber >= cumulativeDungeonTreasureRarities[i-1] && randomNumber <= cumulativeDungeonTreasureRarities[i])
                {
                    index = i -1;
                    //Debug.Log("Chosen Index::::::::::::::::::");
                    //Debug.Log(index);

                    break;
                }
            }


            GameObject treasureObject =  Instantiate(treasureHolder.GetComponent<TreasureHolder>().treasures[index], treasureHolder.transform.position, treasureHolder.transform.rotation);
            treasureObject.AddComponent<DisplayAnim>();
            GameObject deletThis = treasureHolder.transform.GetChild(0).gameObject;
            deletThis.transform.parent = null;
            Destroy(deletThis);
            yield return new WaitForSeconds(0.9f);
        }
    }
}