using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RopeData
{
    public float ChargeSpeed;
    public float MinLockRadius;
    public float MaxLockRadius;

    public float BreakDamagePerExcess;
    public float RecoveryPerMargin;
    public float Durability;
    public float RopeDamagePerPull;

    public float BreakLength;
    public float TensionForce;

    public Sprite StartCapSprite;
    public Sprite EndCapSprite;
    public Sprite RopeSprite;
}
