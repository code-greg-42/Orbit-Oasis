using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }

    [SerializeField] private GameObject inventoryMenu;

    private bool menuActivated;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleInventoryMenu()
    {
        inventoryMenu.SetActive(!menuActivated);
        menuActivated = !menuActivated;
    }
}
