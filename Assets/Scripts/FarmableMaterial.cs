using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmableMaterial : MonoBehaviour
{
    public void PickupMaterial()
    {
        Debug.Log("Picking up material!");
        gameObject.SetActive(false);
    }
}
