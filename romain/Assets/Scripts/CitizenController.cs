using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Photon.Pun;

public enum CitizenState
{
    None,
    Infected,
    Vaccinated
}

public class CitizenController : MonoBehaviourPun
{
    public CitizenState state = CitizenState.None; // current citizen state
    public float minWanderingDelay = 3f; // minimum wandering delay
    public float maxWanderingDelay = 6f; // maximum wandering delay
    public float wanderingRadius = 5f; // maximum wandering radius
    public float fleeRadius = 3f; // radius in which the citizen will start to flee
    public float fleeSpeed = 2f; // flee speed
    public Image infectedCircle; // infected circle ui

    NavMeshAgent agent; // ai navmesh agent
    Animator anim; // animator controller
    VirusController virus; // virus controller in scene

    // temp values
    float wanderTimer;
    bool fleeing = false;
    Vector3 startPos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        wanderTimer = Random.Range(minWanderingDelay, maxWanderingDelay); // initialize wandering timer

        startPos = transform.position;
    }

    void Update()
    {
        // if we're not the master client then do nothing
        if (!photonView.IsMine)
            return;

        virus = FindObjectOfType<VirusController>();

        anim.SetBool("walking", agent.velocity.magnitude != 0 || fleeing ? true : false);

        if (virus != null)
        {
            // check if the virus is nearby
            if(Vector3.Distance(virus.transform.position, transform.position) < fleeRadius)
            {
                fleeing = true;

                // stop wandering if not stopped
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    wanderTimer = Random.Range(minWanderingDelay, maxWanderingDelay);
                }

                // flee direction and movement
                Vector3 fleeDir = (transform.position - virus.transform.position).normalized;
                fleeDir.y = transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(fleeDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                agent.Move(fleeDir * fleeSpeed * Time.deltaTime);
            }
            else
                fleeing = false;
        }

        // operate wandering timer
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance != Mathf.Infinity && agent.remainingDistance == 0 && !fleeing) wanderTimer -= Time.deltaTime;

        if(wanderTimer < 0)
        {
            Wander();
            wanderTimer = Random.Range(minWanderingDelay, maxWanderingDelay);
        }
    }

    // resets citizen position for rematch
    public void ResetPosition()
    {
        agent.isStopped = true;
        agent.ResetPath();
        fleeing = false;
        wanderTimer = Random.Range(minWanderingDelay, maxWanderingDelay);
        transform.position = startPos;
    }

    // function to allow rpc
    public void SetState(CitizenState newState)
    {
        photonView.RPC("SetStateRPC", RpcTarget.AllBuffered, newState);
    }

    [PunRPC]
    public void SetStateRPC(CitizenState newState)
    {
        state = newState;

        // switch between infected/vaccinated circles
        switch (state)
        {
            case CitizenState.None:
                infectedCircle.gameObject.SetActive(false);
                break;
            case CitizenState.Infected:
                infectedCircle.gameObject.SetActive(true);
                infectedCircle.color = Color.red;
                break;
            case CitizenState.Vaccinated:
                infectedCircle.gameObject.SetActive(true);
                infectedCircle.color = new Color(0.133f, 0.54f, 0.133f, 1f);
                break;
        }
    }

    // wander around
    void Wander()
    {
        Vector3 rdmPos = transform.position + Random.insideUnitSphere * wanderingRadius; // random point inside a sphere
        NavMesh.SamplePosition(rdmPos, out NavMeshHit hit, wanderingRadius, 1 << NavMesh.GetAreaFromName("Walkable")); // sample navmesh point at random point
        agent.SetDestination(hit.position); // set destination to it
    }

    // debug ranges
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderingRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
    }
}
