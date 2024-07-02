using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Item
{
    [SerializeField] private float foodPerUnit;

    public float FoodPerUnit => foodPerUnit;
}
