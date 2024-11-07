using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FishingPlayer : MonoBehaviour
{
    private float moveSpeed = 50.0f;
    private float boostForce = 20.0f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform playerObject;

    // keybinds
    private KeyCode boostKey = KeyCode.LeftShift;
    private KeyCode boostKeyAlt = KeyCode.Space;

    private Vector2 movement;
    //private bool isFlipped = false;
    private bool setBoost = false;

    private void Update()
    {
        GetInput();
        //FlipSprite();
        //RotateVisual();

        if (Input.GetKeyDown(boostKey) || Input.GetKeyDown(boostKeyAlt))
        {
            setBoost = true;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (setBoost)
        {
            Boost();
            setBoost = false;
        }
    }

    private void CatchFish(FishingFish fish)
    {
        // get the item prefab index from the fish
        int prefabIndex = fish.ItemPrefabIndex;

        // upload the index to the data manager to update it about the catch
        DataManager.Instance.AddCaughtFish(prefabIndex);
    }

    private void GetInput()
    {
        // get inputs from WASD keys
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
    }

    private void MovePlayer()
    {
        // add force to move the player
        rb.AddForce(movement * moveSpeed);
    }

    private void Boost()
    {
        // Apply an impulse force in the direction of movement
        rb.AddForce(movement.normalized * boostForce, ForceMode2D.Impulse);
    }

    //private void FlipSprite()
    //{
    //    if (rb.velocity.x < 0 && !isFlipped)
    //    {
    //        FlipToFacingLeft();
    //    }
    //    else if (rb.velocity.x > 0 && isFlipped)
    //    {
    //        FlipToFacingRight();
    //    }
    //}

    //private void FlipToFacingLeft()
    //{
    //    Vector3 scale = playerObject.localScale;
    //    scale.x = -Mathf.Abs(scale.x);
    //    playerObject.localScale = scale;
    //    isFlipped = true;
    //}

    //private void FlipToFacingRight()
    //{
    //    Vector3 scale = playerObject.localScale;
    //    scale.x = Mathf.Abs(scale.x);
    //    playerObject.localScale = scale;
    //    isFlipped = false;
    //}

    //private void RotateVisual()
    //{
    //    Vector2 velocity = rb.velocity;

    //    if (velocity != Vector2.zero)
    //    {
    //        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

    //        if (isFlipped)
    //        {
    //            angle += 180;
    //        }

    //        // set the target rotation
    //        playerObject.rotation = Quaternion.Euler(0, 0, angle);
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FishingFish"))
        {
            if (collision.gameObject.TryGetComponent(out FishingFish fish))
            {
                if (!fish.IsExplosive)
                {
                    CatchFish(fish);
                }
                else
                {
                    Debug.Log("Ah! That's an explosive fish!");
                    DataManager.Instance.ClearCaughtFish();
                }
            }
            collision.gameObject.SetActive(false);
        }
    }
}
