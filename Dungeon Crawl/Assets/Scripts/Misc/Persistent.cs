using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    //
    // Stores the attributes of players when they are switched or deleted
    //

   
public class Persistent : MonoBehaviour {
    public static GameObject chosenPlayer;
    //public static Brave_Soldier_Sofia savedSofia;
    //public static Stitched_Man_Victor savedVictor;

    public struct PersistentStats
    {
        public int hunger;
        public int storedFood;
        public int storedPotion;
        public int level;
        public int xpGems;
        public float hp;
    }

    public static PersistentStats savedSofiaStats;
    public static PersistentStats savedVictorStats;

    public static int lastDungeonLevel = 0;
    public static int greatestVisitedDungeonLevel = 0;
    //public static int playerHunger, playerXP, playerLevel, playerStoredFood;

        public static List<GameObject> playerHeldItems = new List<GameObject>();

        public static int playerLevel()
    {
        return 0;
    }
    public static float playerHP;


}
