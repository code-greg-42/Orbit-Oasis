using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingSeaweed : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Stuck to seaweed!");
        transform.SetParent(collision.transform);
    }
}
