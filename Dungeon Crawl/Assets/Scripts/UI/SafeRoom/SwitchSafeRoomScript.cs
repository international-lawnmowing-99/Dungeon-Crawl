using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwitchSafeRoomScript : MonoBehaviour {

    public GameObject button;


    const int width = 5;
    const int height = 4;

    private Button[] buttonList = new Button[width*height];

    private int level;
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                level = 5 * (i + (5 * j));
                if (level <= Persistent.greatestVisitedDungeonLevel)
                {
                    GameObject newButton = Instantiate(button, new Vector3(100 + (i * button.GetComponent<RectTransform>().rect.width + 10), 100 + (j * button.GetComponent<RectTransform>().rect.height + 10), 0), Quaternion.identity, GameObject.Find("Canvas").transform);
                    newButton.GetComponentInChildren<Text>().text = level.ToString();
                    newButton.GetComponent<Button>().onClick.AddListener(() => GoToSafeRoom());
                }
            }
        }
    }
	
    private void GoToSafeRoom()
    {
        Persistent.lastDungeonLevel = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text);
        SceneManager.LoadScene("Game Scene");

    }
}
