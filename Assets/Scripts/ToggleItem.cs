using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleItem : Item
{
    [Header("Toggle Item Settings")]
    [SerializeField] protected string alertMessage;

    public override bool IsDroppable { get; } = false;

    public virtual void ToggleAbility()
    {
        // show alert message
        MainUIManager.Instance.ShowAlertText(alertMessage, 2.5f);
    }
}
