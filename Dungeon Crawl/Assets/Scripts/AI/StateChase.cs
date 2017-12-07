using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChase : IEnemyState {
    public bool secondPath = false;


    ICharacterBase character;
    DungeonGenerator dungeon;
    Node lastPlayerNode;
    int pathIndex = 0;

    public List<Node> currentPath = new List<Node>();

    IEnemyBase self;
    public StateChase (ICharacterBase newCharacter, IEnemyBase newEnemy) {
        
        character = newCharacter;
        self = newEnemy;
        dungeon = GameObject.FindGameObjectWithTag("GameController").GetComponent<DungeonGenerator>();
	}

    void IEnemyState.Update()
    {
        if (character.justMoved)// debug || Input.GetKeyDown(KeyCode.B))
        {

            if (currentPath.Count > 0)
            {
                currentPath.Clear();
            }

            UpdateStrategy();

            AttackPlayer();

            MoveTowardsPlayer();

        }
    }

    private void UpdateStrategy()
    {
        /*
         
        track the number of active enemies
        if it's the only one just charge in as normal

        if there's a second or a third and this one is the last one maybe block an exit

        
         
         
         
         */

        //List of possible moves for player

        //If there's one, just charge


        //if (self.distToPlayer < 4)
        //{

        //}
    }

    private void AttackPlayer()
    {
        if (Mathf.Abs(character.node.x - self.node.x) < 2 && Mathf.Abs(character.node.y - self.node.y) < 2)
        {
            // Only attack in non-diagonal directions

            //character.CharCurrHp-=20;
            //if (character.node.x > self.node.x && character.node.y > self.node.y)
            //    self.BasicAttack(IEntityBase.CompassDir.NE);
            if (character.node.x > self.node.x && character.node.y == self.node.y)
            {
                self.BasicAttack(IEntityBase.CompassDir.E);
                self.Anims.SetTrigger("Attack");
            }

            //else if (character.node.x > self.node.x && character.node.y < self.node.y)
            //    self.BasicAttack(IEntityBase.CompassDir.SE);

            else if (character.node.x == self.node.x && character.node.y > self.node.y)
            {
                self.BasicAttack(IEntityBase.CompassDir.N);
                self.Anims.SetTrigger("Attack");
            }

            else if (character.node.x == self.node.x && character.node.y < self.node.y)
            {
                self.BasicAttack(IEntityBase.CompassDir.S);
                self.Anims.SetTrigger("Attack");
            }

            //else if (character.node.x < self.node.x && character.node.y > self.node.y)
            //    self.BasicAttack(IEntityBase.CompassDir.NW);
            else if (character.node.x < self.node.x && character.node.y == self.node.y)
            {
                self.BasicAttack(IEntityBase.CompassDir.W);
                self.Anims.SetTrigger("Attack");
            }
            //else if (character.node.x < self.node.x && character.node.y < self.node.y)
            //    self.BasicAttack(IEntityBase.CompassDir.SW);
            //else if (character.node.x == self.node.x && character.node.y == self.node.y)
            //{
            //    //int i = 0;
            //}
            //else
            //Debug.Log("If you're reading this something has gone wrong :`(");
        }
    }

    private void MoveTowardsPlayer()
    {
        /*
         get the player position
         get your position

        do a move

        get the straight line distance

        get updated positions

        if the player has moved some distance
            new path
         
         */

        // TODO: Optimise this!!!!
        // Find the room the player is in and if it hasn't changed use existing path if available?

        //if (lastPlayerNode != character.node && currentPath != null  && currentPath.Count > pathIndex && !currentPath[pathIndex].isOccupied)
        //{
        //    self.node.isOccupied = false;

        //    self.Move(currentPath[pathIndex]);
        //    currentPath[pathIndex].isOccupied = true;
        //    self.node = currentPath[pathIndex];
        //    pathIndex++;
        //}

         if (character.node.terrain == DungeonGenerator.TERRAINTYPE.Grass)// && self.targetPos == self.transform.position)
        {

            PathManager.RequestPath(self.node, dungeon.grid[(int)Math.Round(character.targetPos.x, MidpointRounding.AwayFromZero), (int)Math.Round(character.targetPos.z, MidpointRounding.AwayFromZero)], OnPathFound, false, self);

        }
        lastPlayerNode = character.node;
    }

    //}
    
    

    private void OnPathFound(List<Node> path, bool pathSuccess)
    {
        if (pathSuccess && path.Count > 0)// && )
        {                currentPath = path;

            if (!path[0].isOccupied)// && path[0] != dungeon.grid[(int)Math.Round( character.targetPos.x, MidpointRounding.AwayFromZero),(int)Math.Round( character.targetPos.y, MidpointRounding.AwayFromZero)])
            {
                //Look for a second path if another enemy blocks the way
                secondPath = false;

                self.node.isOccupied = false;

                self.Move(currentPath[0]);

                currentPath[0].isOccupied = true;
                self.node = currentPath[0];
                pathIndex = 1;
            }

            else
            {
                secondPath = true;
                if (Mathf.Abs( character.targetPos.x - self.node.x) <= 1 || Mathf.Abs(character.targetPos.z - self.node.y) <= 1)
                {
                    PathManager.RequestPath(self.node, dungeon.grid[(int)Math.Round(character.targetPos.x, MidpointRounding.AwayFromZero), (int)Math.Round(character.targetPos.z, MidpointRounding.AwayFromZero)], OnSecondPathFound, true, self);
                }
            }
        }
    }

    private void OnSecondPathFound(List<Node> path, bool pathSuccess)
    {
        if (pathSuccess && path.Count > 0)
        {
            if (!path[0].isOccupied)
            {
                secondPath = false;
                currentPath = path;

                self.node.isOccupied = false;
                self.Move(currentPath[0]);
                currentPath[0].isOccupied = true;
                self.node = currentPath[0];
                pathIndex = 1;
            }
            else
            {
                //Debug.Log("No second path found");
            }
        }
    }
}