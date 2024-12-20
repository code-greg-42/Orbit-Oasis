using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public virtual void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.DropDraggedItem();
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.RemoveSlotSelection();
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.Click);
        }
    }
}
