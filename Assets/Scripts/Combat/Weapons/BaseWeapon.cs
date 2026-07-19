using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    protected Teammate Owner { get; private set; }
    protected WeaponData Data { get; private set; }

    public WeaponData Definition => Data;
    public virtual float Damage => Data.BaseDamage;
    public virtual float AttackRange => Data.BaseRange;

    public void Initialize(Teammate owner, WeaponData data)
    {
        Owner = owner;
        Data = data;
        OnInitialized();
    }

    public float GetAttackRange()
    {
        return AttackRange + Random.Range(Data.AttackRangeOffset.x, Data.AttackRangeOffset.y);
    }

    public float GetAttackWindup()
    {
        return Data.AttackWindup + Random.Range(Data.WindupOffset.x, Data.WindupOffset.y);
    }

    public float GetAttackCooldown()
    {
        return Data.AttackCooldown + Random.Range(Data.CooldownOffset.x, Data.CooldownOffset.y);
    }

    public virtual void BeginAttack(Transform target)
    {
    }

    public virtual void CancelAttack()
    {
    }

    public abstract void PerformAttack(Transform target);

    public virtual void EndAttack()
    {
    }

    protected virtual void OnInitialized()
    {
    }
}
