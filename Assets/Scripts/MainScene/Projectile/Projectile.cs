using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const float maxLifetime = 10.0f;
    private const float captureRadius = 3.0f;
    private const float groundSequenceDelay = 1.5f;
    private Coroutine groundSequenceCoroutine;

    [SerializeField] private GameObject detonationEffect;

    private float lifetimeTimer = 0.0f;

    public GameObject DetonationEffect => detonationEffect;

    private void OnEnable()
    {
        lifetimeTimer = 0.0f;
    }

    private void OnDisable()
    {
        lifetimeTimer = 0.0f;
    }

    private void Update()
    {
        lifetimeTimer += Time.deltaTime;

        // deactivate without explosion effects if timer exceeds the set max lifetime
        if (lifetimeTimer >= maxLifetime)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Item item))
        {
            SendToInventory(item);

            // play sound effects
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.PickupItem);
            if (item is Animal animal)
            {
                MainSoundManager.Instance.PlaySoundEffect(animal.Sound);
            }

            if (item is DeadTree && QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.RemoveDeadTrees)
            {
                // update quest manager with quest completion
                QuestManager.Instance.UpdateCurrentQuest();
            }

            // deactivate projectile and return to pool
            Deactivate();
        }
        else if (collision.gameObject.TryGetComponent(out BuildableObject buildable))
        {
            BuildManager.Instance.DeleteBuild(buildable);

            // return to pool
            Deactivate();
        }
        else if (collision.gameObject.CompareTag("Ground")// || collision.gameObject.TryGetComponent(out BuildableObject _))
            && groundSequenceCoroutine == null)
        {
            if (gameObject.activeInHierarchy)
            {
                groundSequenceCoroutine = StartCoroutine(GroundSequence());
            }
        }
    }

    private IEnumerator GroundSequence()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(groundSequenceDelay);

        // check for nearby items
        Collider[] colliders = Physics.OverlapSphere(transform.position, captureRadius);

        int deadTreesPickedUp = 0;
        bool atLeastOneItemPickedUp = false;
        bool atLeastOneAnimalPickedUp = false;
        MainSoundManager.SoundEffect animalSound = MainSoundManager.SoundEffect.NoSound;

        // find any items in collider array
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Item item))
            {
                SendToInventory(item);

                atLeastOneItemPickedUp = true;

                if (item is Animal animal)
                {
                    atLeastOneAnimalPickedUp = true;

                    // okay if it overrides, this way only one animal sound is played per pickup sequence
                    animalSound = animal.Sound;
                }
                
                // increment counter if picked up item was a dead tree (added check for remove dead trees quest)
                if (item is DeadTree && QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.RemoveDeadTrees)
                {
                    deadTreesPickedUp++;
                }
            }
        }

        if (atLeastOneAnimalPickedUp && animalSound != MainSoundManager.SoundEffect.NoSound)
        {
            MainSoundManager.Instance.PlaySoundEffect(animalSound);
        }

        if (atLeastOneItemPickedUp)
        {
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.PickupItem);
        }

        if (deadTreesPickedUp > 0)
        {
            QuestManager.Instance.UpdateCurrentQuest(deadTreesPickedUp);
        }

        Deactivate();
    }

    private void Deactivate()
    {
        // Stop the ground sequence coroutine if it is running
        if (groundSequenceCoroutine != null)
        {
            StopCoroutine(groundSequenceCoroutine);
            groundSequenceCoroutine = null;
        }

        // instantiate detonation effect on position
        GameObject detonationEffectInstance = Instantiate(detonationEffect);
        detonationEffectInstance.transform.position = transform.position;

        // set movement back to zero
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Deactivate the projectile game object and return to pool
        gameObject.SetActive(false);
    }

    private void SendToInventory(Item item)
    {
        // add item to player inventory
        item.PickupItem();

        // update active animals list if the item is an animal, to reflect the removal from the scene
        if (item is Animal animal)
        {
            AnimalManager.Instance.RemoveActiveAnimal(animal);
        }
        // update navmesh with the lack of the object if item was a placeable item such as a tree
        else if (item is PlaceableItem placeableItem)
        {
            // update data manager with lack of placed item
            DataManager.Instance.RemovePlacedItem(placeableItem);

            // update navmesh with lack of placed item
            NavMeshManager.Instance.UpdateNavMesh();
        }
    }
}
