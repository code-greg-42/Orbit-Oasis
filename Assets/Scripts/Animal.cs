using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : Item
{
    [Header("Grazing Settings")]
    [SerializeField] private float roamRadius;

    private float minIdleTime = 3.0f;
    private float maxIdleTime = 30.0f;

    // anti stuck variables
    private float stuckThresholdSqr = 0.01f;
    private float stuckTime = 120.0f; // keeping this high because it's cute when they get stuck for a bit, just not forever.
    private float stuckTimer = 0.0f;
    private Vector3 lastPosition;

    private NavMeshAgent agent;
    private Animator animalAnim;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animalAnim = GetComponent<Animator>();
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
    }

    private IEnumerator RoamCoroutine()
    {
        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // randomize idle time
                float idleTime = Random.Range(minIdleTime, maxIdleTime);

                yield return new WaitForSeconds(idleTime);

                SetRandomDestination();
                stuckTimer = 0.0f; // reset timer when setting new destination
            }

            // check if stuck
            if ((transform.position - lastPosition).sqrMagnitude < stuckThresholdSqr)
            {
                stuckTimer += 0.5f;
                if (stuckTimer >= stuckTime)
                {
                    SetRandomDestination(); // force new destination
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
