using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : Item
{
    [Header("Grazing Settings")]
    [SerializeField] private float roamRadius;
    [SerializeField] private float minIdleTime = 3.0f;
    [SerializeField] private float maxIdleTime = 30.0f;

    [Header("Sound Settings")]
    [SerializeField] private MainSoundManager.SoundEffect spawnSound;

    private const float grazingTimeMin = 12.0f; // minimum idle time to set the eating/grazing animation

    // anti stuck variables
    private const float stuckThresholdSqr = 0.01f;
    private const float stuckTime = 90.0f; // keeping this high because it's cute when they get stuck for a bit, just not forever.
    private float stuckTimer = 0.0f;
    private Vector3 lastPosition;

    private NavMeshAgent agent;
    private Animator animalAnim;

    public MainSoundManager.SoundEffect SpawnSound => spawnSound;

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

                    // set eating animation --- potential to set this only if animal is on the grass level, as opposed to on a buildable object
                    if (idleTime > grazingTimeMin)
                    {
                        animalAnim.SetBool("isEating", true);
                    }

                    yield return new WaitForSeconds(idleTime);

                    // reset eating animation after idle time concludes
                    animalAnim.SetBool("isEating", false);
                }
                else
                {
                    isInitial = false;
                }

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
