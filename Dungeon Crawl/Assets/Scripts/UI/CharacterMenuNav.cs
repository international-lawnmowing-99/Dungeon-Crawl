using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterMenuNav : MonoBehaviour {

    public GameObject Vix, Victor, Sophia;

    GameObject currentSelection;
    public void ChooseSophia()
    {
        currentSelection = Sophia;
        MakeSelection();
    }
    public void ChooseVictor()
    {
        currentSelection = Victor;
        MakeSelection();
    }
    public void ChooseVix()
    {
        Debug.Log("New character VIX coming soon...");
        currentSelection = Vix;
        MakeSelection();
//        ICharacterBase = new Vix();//
    }
    void MakeSelection()
    {
        Persistent.chosenPlayer = currentSelection;
        StartCoroutine(LoadNextScene());
    }
    private IEnumerator LoadNextScene()
    {
        gameObject.GetComponent<Fade>().BeginFade(Fade.FadeDirection.OUT, 0.9f);
        yield return new WaitForSeconds(0.6f);
                 SceneManager.LoadScene("Game Scene");

    }
}
