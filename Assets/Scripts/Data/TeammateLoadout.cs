using System;
using UnityEngine;

[Serializable]
public sealed class TeammateLoadout
{
    [SerializeField] private TeammateData teammate;
    [SerializeField] private WeaponData equippedWeapon;

    public TeammateLoadout(TeammateData teammate, WeaponData equippedWeapon)
    {
        this.teammate = teammate;
        this.equippedWeapon = equippedWeapon != null ? equippedWeapon : teammate?.DefaultWeapon;
    }

    public TeammateData Teammate => teammate;
    public WeaponData EquippedWeapon => equippedWeapon != null
        ? equippedWeapon
        : teammate?.DefaultWeapon;

    public static TeammateLoadout FromDefaultWeapon(TeammateData teammate)
    {
        return new TeammateLoadout(teammate, teammate != null ? teammate.DefaultWeapon : null);
    }
}
