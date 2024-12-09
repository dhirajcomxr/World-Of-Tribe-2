using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimHash 
{
    public const string None = "None";

    public const string Velocity = "Velocity";
    public const string VelocityZ = "Velocity Z";
    public const string isMove = "Moving";

    public const string jump = "Jump";
    public const string land = "land";

    public const string takeDamage = "TakeDamage";

    public const string Interacting = "isInteracting";
    public const string CanDoCombo = "canDoCombo";

    public const string BackStab = "BackStab";

    public const string death = "death";
    public const string block = "Block";

    public const string AttackSpeedModification = "AttackSpeedMod";

    public const string Attack = "Attack";
    public const string AttackNumber = "AttackNumber";
    
    public const string Hit = "Hit";
    public const string HitNumber = "HitNumber";

    public const string RotationStop = "Rotation";


    public AnimHash()
    {        
       

    }
}
public class TagHash
{
    public const string PLAYER = "Player";
    public const string ENEMY = "Enemy";
    public const string GROUND = "ground";
    public const string SWORD = "Sword";

    public TagHash()
    {
        Animator.StringToHash(PLAYER);
        Animator.StringToHash(ENEMY);
        Animator.StringToHash(GROUND);
    }
}
