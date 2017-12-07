using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
	

	

public class Fade : MonoBehaviour {
    public enum FadeDirection
    {
        IN = -1,
        OUT = 1
    }

    public Texture2D fadeOutTexture;
    public float fadeSpeed = 0.5f;

    private int drawDepth = -1000;
    private float alpha = 1.0f;
    private int fadeDir = -1;

    void OnGUI()
    {
        alpha += fadeDir * fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);

        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        GUI.depth = drawDepth;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);
    }

    public void BeginFade(FadeDirection fadeDirection, float newFadeSpeed)
    {
        fadeDir = (int)fadeDirection;
        fadeSpeed = newFadeSpeed;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        alpha = 1;
        BeginFade(FadeDirection.IN, fadeSpeed);
    }
}
