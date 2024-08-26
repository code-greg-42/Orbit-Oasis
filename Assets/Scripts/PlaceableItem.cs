using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableItem : Item
{
    [Header("Placeable Item Settings")]
    [SerializeField] private float defaultY;
    public float DefaultY => defaultY;
}
