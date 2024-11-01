using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDeselectArea : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // unselect current selection
            InventoryManager.Instance.RemoveSlotSelection();
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.Click);
        }
    }
}
