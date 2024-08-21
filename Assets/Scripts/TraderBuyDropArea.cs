using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TraderBuyDropArea : DropArea
{
    public override void OnDrop(PointerEventData eventData)
    {
        if (TraderMenuManager.Instance.IsDragging)
        {
            TraderMenuManager.Instance.BuyDraggedItem();
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicked");
    }
}
