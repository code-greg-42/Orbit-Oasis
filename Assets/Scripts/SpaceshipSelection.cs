using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipSelection : MonoBehaviour
{
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform playerTransform;

    private bool isSpaceshipSelectionActive;
    private Coroutine walkAwayDeactivation;

    private const float walkAwayCheckTime = 1.0f;
    private const float deactivationDistance = 5.0f;
    private float sqrDeactivationDistance;

    private void Start()
    {
        sqrDeactivationDistance = deactivationDistance * deactivationDistance;
    }

    public void ActivateSpaceshipSelection()
    {
        if (!isSpaceshipSelectionActive)
        {
            isSpaceshipSelectionActive = true;
            selectionPanel.SetActive(true);

            if (walkAwayDeactivation != null)
            {
                StopCoroutine(walkAwayDeactivation);
                walkAwayDeactivation = null;
            }

            walkAwayDeactivation = StartCoroutine(WalkAwayDeactivationCoroutine());
        }
    }

    public void DeactivateSpaceshipSelection()
    {
        if (isSpaceshipSelectionActive)
        {
            isSpaceshipSelectionActive = false;
            selectionPanel.SetActive(false);

            if (walkAwayDeactivation != null)
            {
                StopCoroutine(walkAwayDeactivation);
                walkAwayDeactivation = null;
            }
        }
    }

    private IEnumerator WalkAwayDeactivationCoroutine()
    {
        while (isSpaceshipSelectionActive)
        {
            yield return new WaitForSeconds(walkAwayCheckTime);

            float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
            if (sqrDistance > sqrDeactivationDistance)
            {
                DeactivateSpaceshipSelection();
                break;
            }
        }
    }
}
