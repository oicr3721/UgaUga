using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Mount")]
    [SerializeField] private Transform weaponMount;

    public BaseWeapon CurrentWeapon { get; private set; }

    public bool Equip(WeaponData weaponData, Teammate owner)
    {
        if (weaponData == null || weaponData.WeaponPrefab == null)
        {
            Debug.LogWarning("장착할 WeaponData 또는 WeaponPrefab이 없습니다.", owner);
            return false;
        }

        if (CurrentWeapon != null)
            Destroy(CurrentWeapon.gameObject);

        CurrentWeapon = null;

        Transform mount = weaponMount != null ? weaponMount : transform;
        CurrentWeapon = Instantiate(weaponData.WeaponPrefab, mount);
        CurrentWeapon.transform.localPosition = Vector3.zero;
        CurrentWeapon.transform.localRotation = Quaternion.identity;
        CurrentWeapon.Initialize(owner, weaponData);
        return true;
    }
}
