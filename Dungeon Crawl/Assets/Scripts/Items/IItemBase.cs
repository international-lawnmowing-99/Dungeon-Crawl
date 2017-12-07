using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IItemBase : MonoBehaviour {

    // Use this for initialization
    public Node node;

	public int Value;
	public float HPrestore;
	public float HungerRestore;
	public float rarity;
	public bool fairy = false;

	public enum ITEMTYPE
    {
        smallMeat, largeMeat, redPotion, bigRedPotion, panacea, Fairy, GoldenSword, GoldenSheild, GoldenApple, CrystalSword, CrystalSheild, CrystalApple, onestarScroll, twostarScroll, threestarScroll, fourstarScroll, fivestarScroll, sixstarScroll, Gladius, OldBone, HeartCharm, KnightsSidearm, HawkStone, MermaidScale, PetHamster, Wok, StarBow, KingsCrown, ScolarsPin, CrowsEye, CrimsonFang, MedusasEaring, GluttonsNapkin, DiamondShard, Caledfwlch, CurseAmulet, RainbowShell
	}
    public IItemBase()
    { }
    public IItemBase(ITEMTYPE type)
    {
        switch (type)
        {
			      case ITEMTYPE.smallMeat:
				HungerRestore = 0.2f;
				HPrestore = 0.1f;
				break;
				  case ITEMTYPE.largeMeat:
				HungerRestore = 0.4f;
				HPrestore = 0.3f;
				break;
				  case ITEMTYPE.redPotion:
				HPrestore = 0.5f;
				break;
				  case ITEMTYPE.bigRedPotion:
				HPrestore = 0.75f;
				break;
				  case ITEMTYPE.panacea:
				break;
				  case ITEMTYPE.Fairy:
				fairy = true;
					  break;
				  case ITEMTYPE.GoldenSword:
					  break;
				  case ITEMTYPE.GoldenSheild:
					  break;
				  case ITEMTYPE.GoldenApple:
					  break;
				  case ITEMTYPE.CrystalSword:
					  break;
				  case ITEMTYPE.CrystalSheild:
					  break;
				  case ITEMTYPE.CrystalApple:
					  break;
		  
			case ITEMTYPE.onestarScroll:
				Value = Random.Range(25,70); 
                break;
            case ITEMTYPE.twostarScroll:
				Value = Random.Range(100, 200);
				break;
			case ITEMTYPE.threestarScroll:
				Value = Random.Range(300, 450);
				break;
			case ITEMTYPE.fourstarScroll:
				Value = Random.Range(500, 800);
				break;
			case ITEMTYPE.fivestarScroll:
				Value = Random.Range(900, 1250);
				break;
			case ITEMTYPE.sixstarScroll:
				Value = Random.Range(2000, 3000);
				break;
			case ITEMTYPE.Gladius:
				Value = 150;
				break;
			case ITEMTYPE.OldBone:
				Value = 250;
				break;
			case ITEMTYPE.HeartCharm:
				Value = 300;
				break;
			case ITEMTYPE.HawkStone:
				Value = 350;
				break;
			case ITEMTYPE.MermaidScale:
				Value = 400;
				break;
			case ITEMTYPE.PetHamster:
				Value = 450;
				break;
			case ITEMTYPE.Wok:
				Value = 500;
				break;
			case ITEMTYPE.StarBow:
				Value = 550;
				break;
			case ITEMTYPE.KingsCrown:
				Value = 650;
				break;
			case ITEMTYPE.ScolarsPin:
				Value = 750;
				break;
			case ITEMTYPE.CrowsEye:
				Value = 900;
				break;
			case ITEMTYPE.CrimsonFang:
				Value = 1000;
				break;
			case ITEMTYPE.MedusasEaring:
				Value = 1250;
				break;
			case ITEMTYPE.GluttonsNapkin:
				Value = 1500;
				break;
			case ITEMTYPE.DiamondShard:
				Value = 1750;
				break;
			case ITEMTYPE.Caledfwlch:
				Value = 2000;
				break;
			case ITEMTYPE.CurseAmulet:
				Value = 3000;
				break;
			case ITEMTYPE.RainbowShell:
				Value = 6000;
				break;
			default:
                break;
        }
    }
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IItemBase GetItem()
    {
        return this;
    }
}
