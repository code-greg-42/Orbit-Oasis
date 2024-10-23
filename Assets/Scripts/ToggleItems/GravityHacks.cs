using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityHacks : ToggleItem
{
    public override void ToggleAbility()
    {
        base.ToggleAbility();

        // call gravity hacks method from main game manager
        MainGameManager.Instance.ToggleGravityHacks();
    }
}
