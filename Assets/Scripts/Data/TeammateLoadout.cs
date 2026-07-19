using System;
using UnityEngine;

[Serializable]
public sealed class TeammateLoadout
{
    [SerializeField] private string candidateId;
    [SerializeField] private TeammateData teammate;
    [SerializeField] private WeaponData equippedWeapon;

    public TeammateLoadout(string candidateId, TeammateData teammate, WeaponData equippedWeapon)
    {
        this.candidateId = candidateId;
        this.teammate = teammate;
        this.equippedWeapon = equippedWeapon;
    }

    public string CandidateId => candidateId;
    public TeammateData Teammate => teammate;
    public WeaponData EquippedWeapon => equippedWeapon;

    public static TeammateLoadout FromDefaultWeapon(TeammateData teammate)
    {
        return new TeammateLoadout(
            string.Empty,
            teammate,
            teammate != null ? teammate.DefaultWeapon : null);
    }
}
