using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Teammate Data", menuName = "Game/Teammate Data")]
public class TeammateData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("저장 데이터에서 팀원을 식별하는 변경되지 않는 ID입니다.")]
    [SerializeField] private string teammateId;
    [SerializeField] private string displayName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private Sprite portrait;
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private Teammate prefab;

    [Header("Recruitment")]
    [FormerlySerializedAs("meatCount")]
    [Min(0)]
    [SerializeField] private int meatCost;
    [Tooltip("향후 사냥 성공 시 보상 Meat 분배 비율로 사용됩니다.")]
    [Range(1, 5)]
    [SerializeField] private int shareRate = 1;

    [Header("Stats")]
    [FormerlySerializedAs("speed")]
    [Min(0f)]
    [SerializeField] private float moveSpeed;
    [Min(0f)]
    [SerializeField] private float strength;

    [Header("Equipment")]
    [SerializeField] private WeaponData defaultWeapon;

    public string TeammateId => teammateId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Portrait => portrait;
    public RuntimeAnimatorController AnimatorController => animatorController;
    public Teammate Prefab => prefab;
    public int MeatCost => meatCost;
    public int ShareRate => shareRate;
    public float MoveSpeed => moveSpeed;
    public float Strength => strength;
    public WeaponData DefaultWeapon => defaultWeapon;
}
