using System;
using UnityEngine;

[Serializable]
public sealed class WeaponStack
{
    [SerializeField] private WeaponData weapon;
    [Min(0)]
    [SerializeField] private int count;

    public WeaponStack(WeaponData weapon, int count)
    {
        this.weapon = weapon;
        this.count = Mathf.Max(0, count);
    }

    public WeaponData Weapon => weapon;
    public int Count => count;

    internal void Add(int amount)
    {
        count += Mathf.Max(0, amount);
    }

    internal bool TryRemove(int amount)
    {
        if (amount <= 0 || count < amount)
            return false;

        count -= amount;
        return true;
    }

    public WeaponStack Copy()
    {
        return new WeaponStack(weapon, count);
    }
}
