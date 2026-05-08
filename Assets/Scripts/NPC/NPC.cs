using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPC : MonoBehaviour
{

    private Coroutine currentMovementRoutine;
    protected Transform playerTransform;
    protected Animator animator;
    protected UnityEngine.AI.NavMeshAgent agent;

    [Header("Movement Type")]
    public MovementType movementType;

    public enum MovementType
    {
        Path,
        Static,
        RandomMovement,

    }

    [Header("Skins")]
    public bool useSkinSystem = true;
    public RuntimeAnimatorController[] animatorControllers;
    public NPCSkin selectedSkin;
    public enum NPCSkin
    {
        Black, Blue, Purple, Red
    }

    [Header("Path Movement")]
    public Transform[] pathPoints;
    public float waitTimePoint = 3;
    private int indexPath = 0;

    [Header("Random Movement")]
    public float movementRadius = 5f;
    public float waitTimeRandom = 4f;
    protected Vector3 patrolCenter;

    protected virtual void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        patrolCenter = transform.position;
        if (useSkinSystem)
        {
            ApplySkin();
        }
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        ResumeOriginalBehavior();

    }

    protected virtual void Update()
    {
        AdjustAnimationsAndRotations();

    }


    public void AdjustAnimationsAndRotations()
    {
        bool isMoving = agent.velocity.sqrMagnitude > 0.01f;

        if (animator.runtimeAnimatorController != null)
        {
            animator.SetBool("isRunning", isMoving);
        }

        if (agent.desiredVelocity.x > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (agent.desiredVelocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }


    public void ApplySkin()
    {
        int skinIndex = (int)selectedSkin;
        if (animatorControllers == null || skinIndex >= animatorControllers.Length)
        {
            Debug.LogWarning($"No Animator Controller assigned for NPC skin {selectedSkin}. Check the Animator Controllers array.", this);
            return;
        }

        RuntimeAnimatorController controller = animatorControllers[skinIndex];
        if (controller == null)
        {
            Debug.LogWarning($"Animator Controller slot {skinIndex} for NPC skin {selectedSkin} is empty.", this);
            return;
        }

        animator.runtimeAnimatorController = controller;
    }

    void ResumeOriginalBehavior()
    {
        ResumeMovementBehavior(movementType);
    }
    private void ResumeMovementBehavior(MovementType type)
    {
        StopCurrentRoutine();
        switch (type)
        {
            case MovementType.Static:
                agent.ResetPath();
                break;

            case MovementType.Path:
                agent.ResetPath();
                StartPathRoutine();
                break;

            case MovementType.RandomMovement:
                agent.ResetPath();
                StartRandomRoutine();
                break;

            default:
                break;
        }
    }

    protected void StartPathRoutine()
    {
        StopCurrentRoutine();
        currentMovementRoutine = StartCoroutine(FollowPath());
    }

    protected void StartRandomRoutine()
    {
        StopCurrentRoutine();
        currentMovementRoutine = StartCoroutine(RandomMovement());
    }

    protected void StopCurrentRoutine()
    {
        if (currentMovementRoutine != null)
        {
            StopCoroutine(currentMovementRoutine);
            currentMovementRoutine = null;
        }
    }

    protected IEnumerator FollowPath()
    {
        while (true)
        {
            if (pathPoints.Length > 0)
            {
                agent.SetDestination(pathPoints[indexPath].position);
               
                yield return WaitUntiDestinationReached();

                yield return new WaitForSeconds(waitTimePoint);
                indexPath = (indexPath + 1) % pathPoints.Length;
            }
            yield return null;
        }
    }

    protected IEnumerator RandomMovement()
    {
        while (true)
        {
            Vector3 randomPos = GetRandomNavMeshPosition();
            agent.SetDestination(randomPos);
            yield return WaitUntilDestinationReached();
            yield return new WaitForSeconds(waitTimeRandom);
        }
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * movementRadius + patrolCenter;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, movementRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    private IEnumerator WaitUntilDestinationReached()
    {
        while (agent.pathPending || agent.remainingDistance > 0.05f)
        {
            yield return null;
        }
    }

    private IEnumerator WaitUntiDestinationReached()
    {
        yield return WaitUntilDestinationReached();
    }
}
