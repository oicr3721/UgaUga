using UnityEngine;

public enum WeaponRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("저장 데이터에서 무기를 식별하는 변경되지 않는 ID입니다.")]
    [SerializeField] private string weaponId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private WeaponRarity rarity;

    [Header("Runtime")]
    [Tooltip("실제 공격 방식을 구현하는 BaseWeapon 프리팹입니다.")]
    [SerializeField] private BaseWeapon weaponPrefab;

    [Header("Attack")]
    [Min(0f)]
    [SerializeField] private float baseDamage;
    [Min(0f)]
    [SerializeField] private float baseRange;

    [Header("Attack Timing")]
    [Min(0f)]
    [SerializeField] private float attackWindup;
    [Min(0f)]
    [SerializeField] private float attackCooldown;

    [Header("Random Offsets")]
    [SerializeField] private Vector2 windupOffset;
    [SerializeField] private Vector2 cooldownOffset;
    [SerializeField] private Vector2 attackRangeOffset;

    public string WeaponId => weaponId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public WeaponRarity Rarity => rarity;
    public BaseWeapon WeaponPrefab => weaponPrefab;
    public float BaseDamage => baseDamage;
    public float BaseRange => baseRange;
    public float AttackWindup => attackWindup;
    public float AttackCooldown => attackCooldown;
    public Vector2 WindupOffset => windupOffset;
    public Vector2 CooldownOffset => cooldownOffset;
    public Vector2 AttackRangeOffset => attackRangeOffset;
}
