using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityHacks : ToggleItem
{
    protected override void OnToggle()
    {
        // call gravity hacks method from main game manager
        MainGameManager.Instance.ToggleGravityHacks();
    }
}
