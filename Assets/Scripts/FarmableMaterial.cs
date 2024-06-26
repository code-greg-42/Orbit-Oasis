using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmableMaterial : MonoBehaviour
{
    [SerializeField] private Sprite materialImage;
    [SerializeField] private string materialName;
    [SerializeField] private string materialDescription;

    //public void PickupMaterial()
    //{
    //    Debug.Log("Picked up material: " + materialName);
    //    InventoryManager.Instance.AddItem(materialName, materialImage, materialDescription, this.gameObject);
    //    gameObject.SetActive(false);
    //}
}
