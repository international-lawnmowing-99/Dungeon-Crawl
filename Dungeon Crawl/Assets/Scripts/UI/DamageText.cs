using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour {
    public int Damage;
    float time;
    public Font Myfont;
    Vector3 Goal;
    GUIStyle UiStyle;
    void Start()
    {
        time = Time.fixedTime + 2;
        Text myText = this.gameObject.AddComponent<Text>();
        myText.text = Damage.ToString();
        myText.font = Myfont;
        myText.fontSize = 300;
        myText.horizontalOverflow = HorizontalWrapMode.Overflow;
        myText.verticalOverflow = VerticalWrapMode.Overflow;
        myText.color = Color.white;
        this.GetComponent<Canvas>().worldCamera = Camera.main;

        Goal = transform.position + new Vector3(0, 2, 0);
    }
    
    void Update()
    {
        if (Time.fixedTime >= time)
        {
            Destroy(this.gameObject);
        }
        transform.position = Vector3.MoveTowards(transform.position,Goal, Time.deltaTime * 1);
    }

}
