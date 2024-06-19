using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmableMaterial : MonoBehaviour
{
    public Sprite materialImage;
    public string materialName;
    public string materialDescription;

    public void PickupMaterial()
    {
        Debug.Log("Picked up material: " + materialName);
        InventoryManager.Instance.AddItem(materialName, materialImage, materialDescription);
        gameObject.SetActive(false);
    }
}
