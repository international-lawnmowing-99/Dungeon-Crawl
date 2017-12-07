using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneUi : MonoBehaviour {

    public int FoodPortionSize = 50;
    public int PotionChuggSize = 50;
    GameObject HUD;
    GameObject SkillRadial;
    DungeonGenerator dungeon;
   [HideInInspector] public GameObject  backpackExpansion, fairy;
    Text foodText, potionText;
    GameObject PauseMenu;

    void Awake () {

        SkillRadial = GameObject.Find("SkillRadial");
        HUD = GameObject.Find("HUD");
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();

        backpackExpansion = GameObject.Find("BackpackExpansion");
        fairy = GameObject.Find("Fairy");

        foodText = GameObject.Find("StoredFoodText").GetComponent<Text>();
        potionText = GameObject.Find("StoredPotionText").GetComponent<Text>();

        PauseMenu = GameObject.Find("PauseMenu");
        PauseMenu.SetActive(false);

        fairy.SetActive(false);
        //StartCoroutine(TurnOffTheSkillRadial());
        //SkillRadial.SetActive(false);
    }

    //private IEnumerator TurnOffTheSkillRadial()
    //{
    //    yield return new WaitForSeconds(.5f);
    //    SkillRadial.SetActive(false);
    //}

    public void Update()
    {
        if (dungeon.internalPlayer != null)
        {
        foodText.text = dungeon.internalPlayer.GetComponent<ICharacterBase>().storedFood.ToString(); //Throwing a Null reference
        potionText.text = dungeon.internalPlayer.GetComponent<ICharacterBase>().storedPotion.ToString(); //^

        }
    }

    public void Quit()
    {
        Application.Quit();
    }
    public void Pause()
    {

        PauseMenu.SetActive(true);
        //HUD.SetActive(false);
        //PauseMenu.SetActive(true);
    }

    public void UnPause()
    {
        PauseMenu.SetActive(false);
    }
    //{
    //    HUD.SetActive(true);
    //    PauseMenu.SetActive(false);
    //}

    //public void MainMenu()
    //{
    //    SceneManager.LoadScene("Menu Scene");
    //}


    public void CharacterSelect()
    {
        SceneManager.LoadScene("Character Select");
    }
    public void LevelUp()
    {
        //do stuff
    }
    public void GoToNextLevel()
    {
        dungeon.GenerateLevel(dungeon.dungeonLevel);
    }
    public void HideMenu()
    {
        GameObject.Find("SkillRadial").SetActive(false);
    }

    public void SetExpansionActive(GameObject chosenButton)
    {
        chosenButton.transform.GetChild(0).gameObject.SetActive(!chosenButton.transform.GetChild(0).gameObject.activeSelf);
    }

    //For food and potion, if there is surplus after the player's health has reached its maximum store the excess
    public void EatFood()
    {
        if (dungeon.internalPlayer.GetComponent<ICharacterBase>().storedFood > FoodPortionSize)
        {
            dungeon.internalPlayer.GetComponent<ICharacterBase>().SetHunger(dungeon.internalPlayer.GetComponent<ICharacterBase>().GetHunger() + FoodPortionSize);
            dungeon.internalPlayer.GetComponent<ICharacterBase>().storedFood -= FoodPortionSize;
        }
        else if (dungeon.internalPlayer.GetComponent<ICharacterBase>().storedFood > 0)
        {
            dungeon.internalPlayer.GetComponent<ICharacterBase>().SetHunger(dungeon.internalPlayer.GetComponent<ICharacterBase>().GetHunger() + dungeon.internalPlayer.GetComponent<ICharacterBase>().storedFood);
            dungeon.internalPlayer.GetComponent<ICharacterBase>().storedFood = 0;
        }
        else
        {
            Debug.Log("Out of food");
        }
    }
    public void DrinkPotion()
    {
        if (dungeon.internalPlayer.GetComponent<ICharacterBase>().storedPotion > PotionChuggSize)
        {
            dungeon.internalPlayer.GetComponent<ICharacterBase>().SetHP(dungeon.internalPlayer.GetComponent<ICharacterBase>().GetHP() + PotionChuggSize);
            dungeon.internalPlayer.GetComponent<ICharacterBase>().storedPotion -= PotionChuggSize;
        }
        else if (dungeon.internalPlayer.GetComponent<ICharacterBase>().storedPotion > 0)
        {
            dungeon.internalPlayer.GetComponent<ICharacterBase>().SetHP(dungeon.internalPlayer.GetComponent<ICharacterBase>().GetHP() + dungeon.internalPlayer.GetComponent<ICharacterBase>().storedPotion);
            dungeon.internalPlayer.GetComponent<ICharacterBase>().storedPotion = 0;
        }
        else
        {        Debug.Log("Bottle-o run");


        }
    }
}
