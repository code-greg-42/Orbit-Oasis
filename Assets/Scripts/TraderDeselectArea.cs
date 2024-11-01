using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TraderDeselectArea : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // unselect current selection
            TraderMenuManager.Instance.RemoveSlotSelection();
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.Click);
        }
    }
}
