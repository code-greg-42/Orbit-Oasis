using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TraderBuyDropArea : DropArea
{
    public override void OnDrop(PointerEventData eventData)
    {
        Debug.Log("HEY! YOU DROPPED HERE!");
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicked");
    }
}
