using System;
using System.Collections.Generic;

public sealed class WeaponInventory
{
    private readonly List<WeaponStack> stacks = new();

    public event Action Changed;

    public IReadOnlyList<WeaponStack> Stacks => stacks;

    public void Initialize(IEnumerable<WeaponStack> initialStacks)
    {
        stacks.Clear();

        if (initialStacks != null)
        {
            foreach (WeaponStack stack in initialStacks)
            {
                if (stack?.Weapon != null && stack.Count > 0)
                    AddInternal(stack.Weapon, stack.Count);
            }
        }

        Changed?.Invoke();
    }

    public int GetCount(WeaponData weapon)
    {
        WeaponStack stack = FindStack(weapon);
        return stack != null ? stack.Count : 0;
    }

    public void Add(WeaponData weapon, int amount = 1)
    {
        if (weapon == null || amount <= 0)
            return;

        AddInternal(weapon, amount);
        Changed?.Invoke();
    }

    public bool TryRemove(WeaponData weapon, int amount = 1)
    {
        WeaponStack stack = FindStack(weapon);
        if (stack == null || !stack.TryRemove(amount))
            return false;

        if (stack.Count == 0)
            stacks.Remove(stack);

        Changed?.Invoke();
        return true;
    }

    public bool TryExchange(WeaponData requestedWeapon, WeaponData returnedWeapon)
    {
        WeaponStack requestedStack = FindStack(requestedWeapon);
        if (requestedStack == null || !requestedStack.TryRemove(1))
            return false;

        if (requestedStack.Count == 0)
            stacks.Remove(requestedStack);

        if (returnedWeapon != null)
            AddInternal(returnedWeapon, 1);

        Changed?.Invoke();
        return true;
    }

    private void AddInternal(WeaponData weapon, int amount)
    {
        WeaponStack stack = FindStack(weapon);
        if (stack == null)
            stacks.Add(new WeaponStack(weapon, amount));
        else
            stack.Add(amount);
    }

    private WeaponStack FindStack(WeaponData weapon)
    {
        return stacks.Find(stack => stack.Weapon == weapon);
    }
}
