using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceAsteroid : MonoBehaviour
{
    private void OnEnable()
    {
        SpaceRaceGameManager.Instance.RegisterAsteroid(this);
    }

    private void OnDisable()
    {
        SpaceRaceGameManager.Instance.UnregisterAsteroid(this);
    }
}
