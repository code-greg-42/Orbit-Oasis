using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ToggleItem : Item
{
    protected float recentToggleTime;
    protected float toggleCooldown = 1.0f;
    protected bool isActive;

    public override bool IsDroppable { get; } = false;

    public abstract void ToggleAbility();
}
