using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnemyBase : IEntityBase {

    public IEnemyState currentState;
    public ICharacterBase character;
    public StatePatrol patrolState;
    public StateChase chaseState;

    public float distToPlayer;
    public Vector3 currentPos, targetPos;
    void Start()
    {
        FindCharacter();
        currentPos = transform.position;
        targetPos = transform.position;
        IsPlayer = false;
        if (Anims == null)
        {
            Anims = GetComponent<Animator>();
        }
        if (Anims == null)
        {
            Anims = GetComponentInChildren<Animator>();
        }
    }
    void Update() 
    {

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 2.2f);

        HpCheck();

    }

    public void EnemyUpdate()
    {
        //change this so that it only happens once the player commits a turn
        currentState.Update();
    }
    public void FindCharacter()
    {
        dungeon = GameObject.FindObjectOfType<DungeonGenerator>();
        character = GameObject.FindObjectOfType<ICharacterBase>();
        patrolState = new StatePatrol(character, this, dungeon);
        chaseState = new StateChase(dungeon.internalPlayer.GetComponent<ICharacterBase>(), this);
        currentState = patrolState;
    }

    void OnDrawGizmos()
    {
        if (patrolState == null)
        {
            FindCharacter();
        }
        if (currentState == patrolState)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, patrolState.detectionRadius);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 2);
        }
    }

    public void Move(Node newNode)
    {
        //Anims.SetTrigger("Walking");

        targetPos = new Vector3(newNode.x, 0.6f, newNode.y);

        Vector3 direction = new Vector3(newNode.x, 0, newNode.y) - transform.position;

        float angle = Mathf.Atan2(direction.z, direction.x);
        float octant = Mathf.Round(8 * angle / (2 * Mathf.PI) + 8) % 8;

        CompassDir dir = (CompassDir)octant;  // Typecast to enum: 0 -> E etc

        TurnToFace(dir);

        if (currentPos == targetPos)
        {
            targetPos += Vector3.forward;

        }
    }

    public void GetDistanceToPlayer()
    {
        distToPlayer = Mathf.Sqrt((character.targetPos.x - targetPos.x) * (character.targetPos.x - targetPos.x) + (character.targetPos.z - targetPos.z) * (character.targetPos.z - targetPos.z));
    }
}