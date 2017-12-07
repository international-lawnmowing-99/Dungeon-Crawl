using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatonAnimationScript : MonoBehaviour {
    Animator batonAnimator;
    Animator sophieAnimator;
    static int idleStateHash = Animator.StringToHash("Idle");
    static int walkStateHash = Animator.StringToHash("Walk");
    static int basicAttackStateHash = Animator.StringToHash("Basic Attack");

    bool spinning = false;

    const double batonStart = 35f / 129f;
    const double batonEnd = 40f/129f;
    //Transform releaseTransform;

    // Use this for initialization
    void Start () {
        batonAnimator = GetComponent<Animator>();
        sophieAnimator = transform.parent.parent.parent.parent.GetComponent<Animator>();
        //Debug.Log(idleStateHash);


	}
	
	// Update is called once per frame
	void Update () {
        if (sophieAnimator == null)
        {
            Destroy(gameObject);
        }

        if (sophieAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == basicAttackStateHash || sophieAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == walkStateHash)
        {
            transform.parent = GameObject.FindGameObjectWithTag("SophiaBatonHolder").transform;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.identity;
        }

         else if (sophieAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == idleStateHash)

        {
            if (spinning)
            {
                transform.position = new Vector3(GameObject.FindGameObjectWithTag("SophiaBatonHolder").transform.position.x, transform.position.y, GameObject.FindGameObjectWithTag("SophiaBatonHolder").transform.position.z);
            }
            if (!spinning 
                && sophieAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - (int)sophieAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > batonStart 
                && sophieAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - (int)sophieAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < batonEnd)
//                && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
            {
                batonAnimator.SetTrigger("StartSpin");
                batonAnimator.ResetTrigger("EndSpin");

                Quaternion sophiaRotation = transform.parent.parent.parent.parent.transform.rotation;
                transform.parent = null;
                transform.rotation = sophiaRotation;// Quaternion.identity;//rotate 90 when character is rotated 90!

                spinning = true;
                transform.Translate(new Vector3(-0.04f, 0, 0));
                //transform.position = new Vector3(0,height.y,0);
            }
            //else if (spinning)
            //{

            //}
            
            else
            {
                batonAnimator.SetTrigger("EndSpin");
                batonAnimator.ResetTrigger("StartSpin");

                if (sophieAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - (int)sophieAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 90f/129f)
                {

                transform.parent = GameObject.FindGameObjectWithTag("SophiaBatonHolder").transform;
                    transform.localPosition = new Vector3(0, 0, 0);
                    transform.localRotation = Quaternion.identity;

                    spinning = false;

                }
            }


        }
        else
        {
            if (transform.parent == null)
            {
                transform.parent = GameObject.FindGameObjectWithTag("SophiaBatonHolder").transform;
                transform.localPosition = new Vector3(0, 0, 0);
                transform.localRotation = Quaternion.identity;

            }
            //batonAnimator.GetCurrentAnimatorStateInfo(0) = 10;
            batonAnimator.SetTrigger("EndSpin");
            batonAnimator.ResetTrigger("StartSpin");
            //transform.parent = GameObject.FindGameObjectWithTag("SophiaBatonHolder").transform;


        }
    }
}
