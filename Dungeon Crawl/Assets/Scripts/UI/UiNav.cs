using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiNav : MonoBehaviour {



    //GameObject OptionMenu;
    //GameObject MainMenu; //Never used?



    void Start()
    {



        //MainMenu = GameObject.Find("Main Menu");

        //OptionMenu = GameObject.Find("Options");
        //OptionMenu.SetActive(false);
    }


	
    public void Play() {
        StartCoroutine(loadNextScene());
    }

private IEnumerator loadNextScene()
    {
        
            gameObject.GetComponent<Fade>().BeginFade(Fade.FadeDirection.OUT, 0.9f);
            yield return new WaitForSeconds(.6f);
            SceneManager.LoadScene("Character Select");
        }
    //public void Options()
    //{
    //    MainMenu.SetActive(false);
    //    OptionMenu.SetActive(true);
    //}
    //public void Back()
    //{
    //    MainMenu.SetActive(true);
    //    OptionMenu.SetActive(false);
    //}

    //public void Quality()
    //{
    //
    //}
}
