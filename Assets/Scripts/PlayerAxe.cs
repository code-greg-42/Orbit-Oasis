using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAxe : MonoBehaviour
{
    [Header("Speed Setting")]
    public float swingSpeed = 100.0f;

    [Header("References")]
    [SerializeField] private Transform axe;
    [SerializeField] private Transform pivot;

    private bool isSwinging = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        originalPosition = axe.transform.localPosition;
        originalRotation = axe.transform.localRotation;
    }

    public void SwingAxe()
    {
        if (!isSwinging)
        {
            StartCoroutine(SwingAxeCoroutine());
        }
    }

    private IEnumerator SwingAxeCoroutine()
    {
        isSwinging = true;
        axe.transform.SetLocalPositionAndRotation(originalPosition, originalRotation);
        axe.gameObject.SetActive(true);

        float rotationAmount = 0f;
        float targetRotation = 180f;

        while (rotationAmount < targetRotation)
        {
            float step = swingSpeed * Time.deltaTime;
            rotationAmount += step;
            axe.RotateAround(pivot.position, Vector3.up, -step);

            yield return null;
        }

        axe.gameObject.SetActive(false);
        isSwinging = false;
    }

    public void HandleCollision(Collider other)
    {
        if (other.CompareTag("FarmableObject"))
        {
            if (other.TryGetComponent<FarmableObject>(out var farmableObject))
            {
                farmableObject.FarmObject();
            }
        }

        axe.gameObject.SetActive(false);
        isSwinging = false;
    }
}
