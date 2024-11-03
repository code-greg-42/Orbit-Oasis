using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }
    public bool BuildModeActive { get; private set; }
    public bool DeleteModeActive { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject[] buildPrefabs;
    [SerializeField] private Transform orientation;
    [SerializeField] private Material buildPreviewMaterial;
    [SerializeField] private LayerMask attachmentLayer;
    [SerializeField] private LayerMask buildLayer;

    // build settings
    private float placementDistance = 4.6f;
    private float attachmentSearchRadius = 2.5f;
    private float attachmentDisableRadius = 7.0f;
    private float deleteHighlightRadius = 6.0f;
    private float cameraVerticalOffset = 0.25f;
    private float groundSnapThreshold = 1.0f;
    private Color validPreviewColor = new(166 / 255f, 166 / 255f, 166 / 255f, 40 / 255f); // gray transparent color
    private Color invalidPreviewColor = new(255 / 255f, 0 / 255f, 0 / 255f, 65 / 255f); // red transparent color
    private float buildRefundRatio = 1.0f;
    private const float colorUpdateFrequency = 0.03f;
    private float lastColorUpdateTime;

    // keybinds
    private KeyCode placeBuildKey = KeyCode.F;
    private KeyCode undoBuildKey = KeyCode.Q;
    private KeyCode deleteModeKey = KeyCode.V;

    private GameObject currentPreview;
    private BuildableObject currentPreviewBuildable;
    private BuildableObject lastPlacedBuild;
    private BuildableObject currentDeleteModeHighlightedBuild;
    private Material originalMaterial;
    private int currentPrefabIndex = 0; // index to track build object type selection
    private bool previewIsPlaceable; // bool to track whether preview is in a placeable position
    private float lastPlacedTime = 0.0f; // used to see if build cooldown has surpassed
    private const float buildCooldown = 0.3f;

    private bool previewInAttachmentSlot; // used for allowing place build despite a collision with a buildable object

    // --- this script can be optimized ---

    // --- potential gameplay improvements :
    // --- 1. build is not placeable even when in an attachment slot if it collides directly through an already placed build
    // --- 2. placed builds hold references to other builds they are attached to
    // --- 3. upon deletion of a build, checks if one of the attached builds eventually connects to the ground, and if not, builds fall or are destroyed

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // load and instantiate buildable objects from DataManager's BuildList
        LoadBuilds();
    }

    private void Update()
    {
        if (BuildModeActive)
        {
            if (DeleteModeActive)
            {
                UpdateDeleteModeHighlight();

                if (Input.GetKeyDown(placeBuildKey) && currentDeleteModeHighlightedBuild != null && !DialogueManager.Instance.DialogueWindowActive)
                {
                    DeleteBuild(currentDeleteModeHighlightedBuild);
                }
            }
            else
            {
                // skip if build was recently placed
                if (Time.time - lastPlacedTime < buildCooldown)
                {
                    return;
                }

                // otherwise create new preview
                if (currentPreview == null)
                {
                    // instantiate new preview
                    CreatePreview();
                }

                UpdatePreviewPosition();

                UpdatePreviewIsPlaceable();

                // prevents color from updating back and forth too much
                if (Time.time - lastColorUpdateTime >= colorUpdateFrequency)
                {
                    UpdatePreviewColor(previewIsPlaceable);
                    lastColorUpdateTime = Time.time;
                }

                if (Input.GetKeyDown(placeBuildKey) && currentPreview != null && previewIsPlaceable && !DialogueManager.Instance.DialogueWindowActive)
                {
                    PlaceBuild();
                }

                if (Input.GetKeyDown(undoBuildKey) && lastPlacedBuild != null && !DialogueManager.Instance.DialogueWindowActive)
                {
                    DeleteBuild(lastPlacedBuild);
                }

                HandleUserPrefabChange();
            }
            
            if (Input.GetKeyDown(deleteModeKey) && !DialogueManager.Instance.DialogueWindowActive)
            {
                ToggleDeleteMode();
            }
        }
    }

    public void ToggleBuildMode()
    {
        // play sound effect
        MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.ToggleBuildMode);

        if (BuildModeActive)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
                currentPreviewBuildable = null;
            }
            // reset
            originalMaterial = null;
            previewIsPlaceable = false;
            previewInAttachmentSlot = false;

            // cancel delete mode if active
            if (DeleteModeActive)
            {
                ToggleDeleteMode();
            }
        }
        BuildModeActive = !BuildModeActive;

        // update UI with current state of build mode
        if (BuildModeActive)
        {
            MainUIManager.Instance.ActivateBuildModeIndicator();
        }
        else
        {
            MainUIManager.Instance.DeactivateBuildModeIndicator();
        }
    }

    public void ToggleOffBuildMode(float delay = 0.25f)
    {
        StartCoroutine(ToggleOffBuildModeCoroutine(delay));
    }

    public void ToggleOnBuildMode(float delay = 0.25f)
    {
        StartCoroutine(ToggleOnBuildModeCoroutine(delay));
    }

    private IEnumerator ToggleOnBuildModeCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!BuildModeActive)
        {
            ToggleBuildMode();
        }
    }

    private IEnumerator ToggleOffBuildModeCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (BuildModeActive)
        {
            ToggleBuildMode();
        }
    }

    public void ToggleDeleteMode()
    {
        if (BuildModeActive)
        {
            if (currentPreview != null)
            {
                // clear current preview
                Destroy(currentPreview);
                currentPreview = null;
                currentPreviewBuildable = null;
            }

            // turn off highlight and reset reference if active
            if (currentDeleteModeHighlightedBuild != null)
            {
                currentDeleteModeHighlightedBuild.DisableDeleteHighlight();
                currentDeleteModeHighlightedBuild = null;
            }
            // swap bool
            DeleteModeActive = !DeleteModeActive;

            // update UI with current state of delete mode
            if (DeleteModeActive)
            {
                // deactivate build mode indicator and activate delete mode indicator
                MainUIManager.Instance.DeactivateBuildModeIndicator();
                MainUIManager.Instance.ActivateDeleteModeIndicator();
            }
            else
            {
                // deactivate delete mode indicator and re-activate build mode indicator
                MainUIManager.Instance.DeactivateDeleteModeIndicator();
                MainUIManager.Instance.ActivateBuildModeIndicator();
            }
        }
    }

    private void CreatePreview()
    {
        if (currentPreview == null)
        {
            previewInAttachmentSlot = false;

            Vector3 targetPosition = CalcTargetPosition();
            currentPreview = Instantiate(buildPrefabs[currentPrefabIndex], targetPosition, buildPrefabs[currentPrefabIndex].transform.rotation);

            // set reference and set collider to 'isTrigger' to prevent any interferences during preview phase
            if (currentPreview.TryGetComponent(out BuildableObject buildable))
            {
                currentPreviewBuildable = buildable;
                buildable.SetIsTrigger();
            }

            // set preview material to transparent material
            SetPreviewMaterial(true);
        }
    }

    private void ChangePrefab(int index)
    {
        if (index >= 0 && index < buildPrefabs.Length)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
                currentPreviewBuildable = null;
            }
            currentPrefabIndex = index;

            // instantiate a new preview with the new prefab index
            CreatePreview();
        }
    }

    private void LoadBuilds()
    {
        if (DataManager.Instance.BuildList.ItemList.Count > 0)
        {
            // copy of list
            List<BuildableObjectData> builds = new(DataManager.Instance.BuildList.ItemList);

            foreach (BuildableObjectData buildData in builds)
            {
                // instantiate new prefab at the saved position and rotation
                GameObject newBuildObject = Instantiate(buildPrefabs[buildData.buildPrefabIndex], buildData.placementPosition, buildData.placementRotation);

                if (newBuildObject.TryGetComponent(out BuildableObject newBuildable))
                {
                    // place object in scene and activate all attachments/components
                    newBuildable.PlaceObject();

                    // deactivate any impacted attachment points
                    CheckAttachmentPoints(newBuildable.transform.position);
                }
                else
                {
                    Debug.LogError("New BuildableObject component not found.");
                }
            }
            // update navmesh with new builds
            NavMeshManager.Instance.UpdateNavMesh();
        }
    }

    private void UpdatePreviewPosition()
    {
        if (BuildModeActive)
        {
            Transform closestAttachmentPoint = FindClosestAttachmentPoint();

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (closestAttachmentPoint != null)
            {
                targetPosition = closestAttachmentPoint.position;
                targetRotation = closestAttachmentPoint.rotation;
                previewIsPlaceable = true;
                previewInAttachmentSlot = true;
            }
            else
            {
                previewInAttachmentSlot = false;

                targetPosition = CalcTargetPosition();
                targetRotation = Quaternion.LookRotation(orientation.forward) *
                                    Quaternion.Euler(buildPrefabs[currentPrefabIndex].transform.rotation.eulerAngles);

                if (targetPosition.y == buildPrefabs[currentPrefabIndex].transform.position.y)
                {
                    previewIsPlaceable = true;
                }
                else
                {
                    previewIsPlaceable = false;
                }
            }

            currentPreview.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }

    private void PlaceBuild()
    {
        if (BuildModeActive && currentPreview != null && previewIsPlaceable)
        {
            if (currentPreview.TryGetComponent<BuildableObject>(out var buildable))
            {
                // change material to solid
                SetPreviewMaterial(false);

                lastPlacedBuild = buildable;
                lastPlacedTime = Time.time;

                // add gameobject to build list
                DataManager.Instance.AddBuild(buildable);

                // place build
                buildable.PlaceObject();

                // play sound effect
                MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.PlaceBuild);

                // update quest manager if on a PlaceBuild quest
                if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.PlaceABuild ||
                    QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.PlaceMoreBuilds)
                {
                    QuestManager.Instance.UpdateCurrentQuest();
                }

                // update inventory with building cost
                InventoryManager.Instance.UseItem(buildable.BuildMaterialName, buildable.BuildCost, true);

                // deactivate any impacted attachment points
                CheckAttachmentPoints(lastPlacedBuild.transform.position);

                // update navmesh
                NavMeshManager.Instance.UpdateNavMesh();
            }
            // finalize placement
            currentPreview = null;
            currentPreviewBuildable = null;
            originalMaterial = null;
            // reset bools
            previewIsPlaceable = false;
            previewInAttachmentSlot = false;
        }
    }

    public void DeleteBuild(BuildableObject buildable)
    {
        // instantiate material object
        GameObject materialPrefabObj = Instantiate(buildable.BuildMaterialPrefab, orientation.position, Quaternion.identity, null);
        materialPrefabObj.SetActive(false);

        // get the item component
        if (materialPrefabObj.TryGetComponent(out Item item))
        {
            // set quantity to build cost * refund ratio
            item.SetQuantity((int)(buildable.BuildCost * buildRefundRatio));

            // add item to inventory by using .pickup --- if there is no space the item will be dropped accordingly
            // data manager will be updated via PickupItem - no need to do it here
            item.PickupItem();
        }

        // remove from build list
        DataManager.Instance.RemoveBuild(buildable);

        // delete the object whether refund was issued or not
        buildable.DeleteObject();

        // play sound effect
        MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.DeleteBuild);

        // update navmesh surface
        NavMeshManager.Instance.UpdateNavMesh();

        // update QuestManager if on Undo Last Build quest
        if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.UndoLastBuild)
        {
            QuestManager.Instance.UpdateCurrentQuest();
        }
    }

    private void UpdatePreviewColor(bool isPlaceable)
    {
        if (currentPreview.TryGetComponent(out Renderer renderer))
        {
            Color targetColor = isPlaceable ? validPreviewColor : invalidPreviewColor;

            if (renderer.material.color != targetColor)
            {
                renderer.material.color = targetColor;
            }
        }
    }

    private void UpdatePreviewIsPlaceable()
    {
        if (currentPreview != null && currentPreviewBuildable != null)
        {
            // if player doesn't have enough materials, set to false
            if (DataManager.Instance.PlayerBuildMaterial < currentPreviewBuildable.BuildCost)
            {
                previewIsPlaceable = false;
                return;
            }

            // if preview is colliding with object such as a tree
            if (currentPreviewBuildable.IsCollidingWithOther)
            {
                previewIsPlaceable = false;
                return;
            }

            if (currentPreviewBuildable.IsCollidingWithBuildable && !previewInAttachmentSlot)
            {
                previewIsPlaceable = false;
            }
        }
    }

    private void UpdateDeleteModeHighlight()
    {
        if (BuildModeActive && DeleteModeActive)
        {
            BuildableObject closestBuild = FindClosestBuildableObject();

            if (closestBuild != null)
            {
                if (currentDeleteModeHighlightedBuild != null)
                {
                    if (currentDeleteModeHighlightedBuild != closestBuild)
                    {
                        // disable highlight on currently highlighted build
                        currentDeleteModeHighlightedBuild.DisableDeleteHighlight();

                        // enable highlight on new closest build
                        closestBuild.EnableDeleteHighlight();

                        // set reference
                        currentDeleteModeHighlightedBuild = closestBuild;
                    }
                }
                else
                {
                    // enable highlight
                    closestBuild.EnableDeleteHighlight();

                    // set reference
                    currentDeleteModeHighlightedBuild = closestBuild;
                }
            }
            else
            {
                if (currentDeleteModeHighlightedBuild != null)
                {
                    // disable highlight
                    currentDeleteModeHighlightedBuild.DisableDeleteHighlight();

                    // set reference to null
                    currentDeleteModeHighlightedBuild = null;
                }
            }
        }
    }

    private Transform FindClosestAttachmentPoint()
    {
        Vector3 previewPosition = CalcTargetPosition();
        BuildableObject currentBuildableObject = currentPreview.GetComponent<BuildableObject>();

        Collider[] results = new Collider[16];
        int size = Physics.OverlapSphereNonAlloc(previewPosition, attachmentSearchRadius, results, attachmentLayer);

        Transform closestAttachmentPoint = null;
        Transform secondClosestAttachmentPoint = null;
        float closestSqrDistance = attachmentSearchRadius;
        float secondClosestSqrDistance = attachmentSearchRadius;

        for (int i = 0; i < size; i++)
        {
            if (results[i].TryGetComponent(out BuildAttachmentPoint attachmentPoint))
            {
                if (attachmentPoint.AttachmentType == currentBuildableObject.BuildType)
                {
                    float sqrDistance = (previewPosition - attachmentPoint.transform.position).sqrMagnitude;
                    if (sqrDistance < closestSqrDistance)
                    {
                        secondClosestAttachmentPoint = closestAttachmentPoint;
                        secondClosestSqrDistance = closestSqrDistance;

                        closestSqrDistance = sqrDistance;
                        closestAttachmentPoint = attachmentPoint.transform;
                    }
                    else if (sqrDistance < secondClosestSqrDistance)
                    {
                        secondClosestSqrDistance = sqrDistance;
                        secondClosestAttachmentPoint = attachmentPoint.transform;
                    }
                }
            }
        }

        // if the two closest points are very close to each other
        if (closestAttachmentPoint != null && secondClosestAttachmentPoint != null &&
            (closestAttachmentPoint.position - secondClosestAttachmentPoint.position).sqrMagnitude < 0.05f * 0.05f)
        {
            // Compare their Y rotations with the current preview's Y rotation
            float yRotationCurrentPreview = currentPreview.transform.eulerAngles.y;
            float yRotationClosest = closestAttachmentPoint.eulerAngles.y;
            float yRotationSecondClosest = secondClosestAttachmentPoint.eulerAngles.y;

            float firstAngleDiff = Mathf.Abs(Mathf.DeltaAngle(yRotationCurrentPreview, yRotationClosest));
            float secondAngleDiff = Mathf.Abs(Mathf.DeltaAngle(yRotationCurrentPreview, yRotationSecondClosest));

            // return point with rotation most similar to current preview
            return firstAngleDiff < secondAngleDiff ? closestAttachmentPoint : secondClosestAttachmentPoint;
        }

        return closestAttachmentPoint;
    }

    private BuildableObject FindClosestBuildableObject()
    {
        Vector3 startPosition = CalcTargetPosition();

        Collider[] results = new Collider[10];
        int numResults = Physics.OverlapSphereNonAlloc(startPosition, deleteHighlightRadius, results, buildLayer);

        BuildableObject closestBuildableObject = null;
        float closestSqrDistance = deleteHighlightRadius;

        for (int i = 0; i < numResults; i++)
        {
            if (results[i].TryGetComponent(out BuildableObject buildableObject))
            {
                if (buildableObject.IsPlaced)
                {
                    float sqrDistance = (startPosition - buildableObject.transform.position).sqrMagnitude;
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closestBuildableObject = buildableObject;
                    }
                }
            }
        }
        return closestBuildableObject;
    }

    private void SetPreviewMaterial(bool isPreview)
    {
        if (currentPreview.TryGetComponent<Renderer>(out var renderer))
        {
            if (isPreview)
            {
                originalMaterial = renderer.material;
                renderer.material = buildPreviewMaterial;
            }
            else
            {
                renderer.material = originalMaterial;
            }
        }
    }

    public void CheckAttachmentPoints(Vector3 placedPosition, bool enable = false)
    {
        Collider[] overlapResults = new Collider[12];

        int numResults = Physics.OverlapSphereNonAlloc(placedPosition, attachmentDisableRadius, overlapResults, buildLayer);

        if (!enable)
        {
            // disable any used attachment points
            for (int i = 0; i < numResults; i++)
            {
                if (overlapResults[i].TryGetComponent(out BuildableObject buildObject))
                {
                    if (buildObject.IsPlaced)
                    {
                        buildObject.CheckAndDisableAttachmentPoints(buildLayer);
                    }
                }
            }
        }
        else
        {
            // enable any unused attachment points
            for (int i = 0; i < numResults; i++)
            {
                if (overlapResults[i].TryGetComponent(out BuildableObject buildObject))
                {
                    if (buildObject.IsPlaced)
                    {
                        buildObject.CheckAndEnableAttachmentPoints(buildLayer);
                    }
                }
            }
        }
    }

    private void HandleUserPrefabChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangePrefab(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangePrefab(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangePrefab(2);
        }
    }

    private Vector3 CalcTargetPosition()
    {
        // get original position as the position out in front of the camera, adjusted for where the camera is looking left/right
        Vector3 targetPosition = orientation.position + orientation.forward * placementDistance;

        // get camera's forward direction for up/down calculation
        Vector3 cameraForward = Camera.main.transform.forward;

        // adjust slightly to make the position slightly higher than where the camera is looking (allows for building up)
        float verticalAdjustment = (cameraForward.y + cameraVerticalOffset) * placementDistance;

        // ensure target position is at minimum, above ground
        targetPosition.y = Mathf.Max(buildPrefabs[currentPrefabIndex].transform.position.y, targetPosition.y + verticalAdjustment);

        // if it's near the ground, snap it to the ground
        if (targetPosition.y < buildPrefabs[currentPrefabIndex].transform.position.y + groundSnapThreshold)
        {
            targetPosition.y = buildPrefabs[currentPrefabIndex].transform.position.y;
        }

        return targetPosition;
    }
}
