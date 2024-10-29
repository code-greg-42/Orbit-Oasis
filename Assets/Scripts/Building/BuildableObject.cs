using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    [Header("Build Settings")]
    [SerializeField] private BuildEnums.BuildType buildType;
    [SerializeField] private int buildPrefabIndex; // used for loading builds in new scene
    [SerializeField] private float buildCost; // multiple of 5
    [SerializeField] private string buildMaterialName;
    [SerializeField] private GameObject buildMaterialPrefab;

    [Header("References")]
    [SerializeField] private GameObject[] attachmentSlots;
    [SerializeField] private GameObject deleteHighlight;

    // collision check variables
    private Collider buildCollider;
    private HashSet<Collider> buildableColliders = new();
    private HashSet<Collider> otherColliders = new();
    private int buildableCollisionCount = 0;
    private int otherCollisionCount = 0;
    private const int framesPerCollisionCheck = 10;// number of frames between collision checks
    private const int colliderValidationInterval = 5;
    private Coroutine collisionCheckCoroutine;
    private Coroutine colliderValidationCoroutine;

    public Vector3 PlacementPosition { get; private set; }
    public Quaternion PlacementRotation { get; private set; }
    public bool IsPlaced { get; private set; }
    public bool IsCollidingWithBuildable { get; private set; }
    public bool IsCollidingWithOther { get; private set; }
    public BuildEnums.BuildType BuildType => buildType;
    public float BuildCost => buildCost;
    public string BuildMaterialName => buildMaterialName;
    public GameObject BuildMaterialPrefab => buildMaterialPrefab;
    public int BuildPrefabIndex => buildPrefabIndex;

    private void Awake()
    {
        buildCollider = GetComponent<Collider>();
    }

    public void PlaceObject()
    {
        // activate attachment slots
        foreach (GameObject attachmentSlot in attachmentSlots)
        {
            attachmentSlot.SetActive(true);
        }

        // remove isTrigger to allow physics interactions
        buildCollider.isTrigger = false;

        // set placement values
        PlacementPosition = transform.position;
        PlacementRotation = transform.rotation;

        // set bool
        IsPlaced = true;
    }

    public void SetIsTrigger()
    {
        buildCollider.isTrigger = true;
    }

    public void DeleteObject()
    {
        // save position to variable
        Vector3 checkPosition = gameObject.transform.position;

        // set to inactive
        gameObject.SetActive(false);

        // reactivate any freed up attachments
        BuildManager.Instance.CheckAttachmentPoints(checkPosition, true);

        // destroy object
        Destroy(gameObject);
    }

    public void EnableDeleteHighlight()
    {
        deleteHighlight.SetActive(true);
    }

    public void DisableDeleteHighlight()
    {
        deleteHighlight.SetActive(false);
    }

    public void CheckAndDisableAttachmentPoints(LayerMask buildLayer)
    {
        Collider[] overlaps = new Collider[10];

        foreach (GameObject slot in attachmentSlots)
        {
            // skip inactive slots
            if (!slot.activeSelf) continue;

            // cycle through points in slot
            foreach (Transform attachmentPoint in slot.transform)
            {
                if (attachmentPoint.TryGetComponent(out BuildAttachmentPoint buildAttachmentPoint))
                {
                    if (buildAttachmentPoint.CheckForNearbyBuild(overlaps, buildLayer))
                    {
                        slot.SetActive(false);
                        break;
                    }
                }
            }
        }
    }

    public void CheckAndEnableAttachmentPoints(LayerMask buildLayer)
    {
        Collider[] overlaps = new Collider[10];

        foreach (GameObject slot in attachmentSlots)
        {
            // skip active slots
            if (slot.activeSelf) continue;

            bool allClear = true;

            // cycle through points in slot
            foreach (Transform attachmentPoint in slot.transform)
            {
                if (attachmentPoint.TryGetComponent(out BuildAttachmentPoint buildAttachmentPoint))
                {
                    if (buildAttachmentPoint.CheckForNearbyBuild(overlaps, buildLayer))
                    {
                        allClear = false;
                        break;
                    }
                }
            }

            // if no nearby builds were found, activate slot
            if (allClear)
            {
                slot.SetActive(true);
            }
        }
    }

    // collision logic
    // done this way to handle cases of multiple collision from same type objects, as well as preventing placeability changes from single frame swaps
    // validation used for minimizing any potential bugs from trigger misses
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("BuildAttachmentPoint"))
        {
            return;
        }
        AddCollider(other);
        collisionCheckCoroutine ??= StartCoroutine(CollisionCheckCoroutine());
        colliderValidationCoroutine ??= StartCoroutine(ColliderValidationCoroutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("BuildAttachmentPoint"))
        {
            return;
        }
        RemoveCollider(other);
    }

    private void AddCollider(Collider other)
    {
        if (other.gameObject.TryGetComponent(out BuildableObject _))
        {
            if (buildableColliders.Add(other)) buildableCollisionCount++;
        }
        else
        {
            if (otherColliders.Add(other)) otherCollisionCount++;
        }
    }

    private void RemoveCollider(Collider other)
    {
        if (other.gameObject.TryGetComponent(out BuildableObject _))
        {
            if (buildableColliders.Remove(other)) buildableCollisionCount--;
        }
        else
        {
            if (otherColliders.Remove(other)) otherCollisionCount--;
        }
    }

    private IEnumerator CollisionCheckCoroutine()
    {
        while (!IsPlaced)
        {
            // wait for x frames
            for (int i = 0; i < framesPerCollisionCheck; i++)
            {
                yield return null;
            }

            // calculate each flag based on collider counts
            IsCollidingWithBuildable = buildableCollisionCount > 0;
            IsCollidingWithOther = otherCollisionCount > 0;
        }
    }

    private IEnumerator ColliderValidationCoroutine()
    {
        while (!IsPlaced)
        {
            yield return new WaitForSeconds(colliderValidationInterval);

            // remove any null or inactive colliders from the hashset
            buildableColliders.RemoveWhere(collider => collider == null || !collider.bounds.Intersects(buildCollider.bounds));
            otherColliders.RemoveWhere(collider => collider == null || !collider.bounds.Intersects(buildCollider.bounds));

            // update the collision counts to match validated sets
            buildableCollisionCount = buildableColliders.Count;
            otherCollisionCount = otherColliders.Count;
        }
    }
}
