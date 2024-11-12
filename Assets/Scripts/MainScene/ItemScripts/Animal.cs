using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.AI;

public class Animal : Item
{
    [Header("Grazing Settings")]
    [SerializeField] private float roamRadius;
    [SerializeField] private float minIdleTime = 3.0f;
    [SerializeField] private float maxIdleTime = 30.0f;

    [Header("Sound Settings")]
    [SerializeField] private MainSoundManager.SoundEffect sound;

    // manual rotation settings
    private const float rotationSpeedVertical = 10.0f;
    private const float rotationSpeedHorizontal = 4.0f;
    private Quaternion targetRotation;
    private Vector3 smoothedVelocity;

    // grazing/eating animation variables
    private const float grazingTimeMin = 12.0f; // minimum idle time to set the eating/grazing animation

    // anti stuck variables
    private const float stuckThresholdSqr = 0.01f;
    private const float stuckTime = 90.0f; // keeping this high because it's cute when they get stuck for a bit, just not forever.
    private float stuckTimer = 0.0f;
    private Vector3 lastPosition;

    private NavMeshAgent agent;
    private Animator animalAnim;

    public MainSoundManager.SoundEffect Sound => sound;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animalAnim = GetComponent<Animator>();
        agent.updateRotation = false;
    }

    private void OnEnable()
    {
        StartCoroutine(RoamCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        animalAnim.SetFloat("speed", agent.velocity.magnitude);

        // get smoothed velocity for use in rotation setting
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, agent.velocity, rotationSpeedVertical * Time.deltaTime);

        // if both actual velocity is above zero and smoothed velocity is also not zero
        if (agent.velocity.sqrMagnitude > 0.01f && smoothedVelocity != Vector3.zero)
        {
            // different target rotation logic based on whether the agent is on flat ground or not
            if (Mathf.Approximately(agent.velocity.y, 0))
            {
                targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(smoothedVelocity), rotationSpeedHorizontal * Time.deltaTime);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(smoothedVelocity);
            }

            transform.rotation = targetRotation;
        }
    }

    private IEnumerator RoamCoroutine()
    {
        bool isInitial = true;

        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // skip the wait time on the first roam after enabling
                if (!isInitial)
                {
                    // randomize idle time
                    float idleTime = Random.Range(minIdleTime, maxIdleTime);

                    // set eating animation --- only if animal is on the grass level, as opposed to on a buildable object
                    if (idleTime > grazingTimeMin)
                    {
                        Vector3 rayOrigin = transform.position;
                        rayOrigin.y += 0.4f;

                        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.0f) && hit.collider.CompareTag("Ground"))
                        {
                            animalAnim.SetBool("isEating", true);
                        }
                    }

                    yield return new WaitForSeconds(idleTime);

                    // reset eating animation after idle time concludes --- set here so animal does not start moving before eating animation stops
                    animalAnim.SetBool("isEating", false);
                }
                else
                {
                    isInitial = false;
                }

                SetRandomDestination();
                animalAnim.SetBool("isEating", false); // ensure eating animation stops on new destination
                stuckTimer = 0.0f; // reset timer when setting new destination
            }

            // check if stuck
            if ((transform.position - lastPosition).sqrMagnitude < stuckThresholdSqr)
            {
                stuckTimer += 0.5f;
                if (stuckTimer >= stuckTime)
                {
                    SetRandomDestination(); // force new destination
                    animalAnim.SetBool("isEating", false); // ensure eating animation stops on new destination
                    stuckTimer = 0.0f; // reset timer
                }
            }
            else
            {
                stuckTimer = 0.0f; // reset timer if moved
            }

            lastPosition = transform.position; // update recent position

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SetRandomDestination()
    {
        // find random direction within range
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;

        // adjust direction to be relative to current position
        randomDirection += transform.position;

        // if a valid path is found, set destination to that location
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
