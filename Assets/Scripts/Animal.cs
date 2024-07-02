using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : Item
{
    [SerializeField] private float foodPerUnit;
    [SerializeField] private float roamRadius;

    public float FoodPerUnit => foodPerUnit;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        StartCoroutine(RoamCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator RoamCoroutine()
    {
        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                SetRandomDestination();
            }
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
