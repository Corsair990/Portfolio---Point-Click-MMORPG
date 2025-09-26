using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Unity.Cinemachine;

public class CharacterController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;
    [SerializeField] private SkillObjectController skillObjectController;
    private Camera cam;

    [Header("Setup")]
    [SerializeField] private LayerMask walkableLayers;
    [SerializeField] private LayerMask interactableLayers;

    [Header("Settings")]
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 6f;
    [SerializeField] float interactionRange = 2.1f;

    float animationSpeed = 0f;
    bool isSprinting = false;

    public void Awake()
    {
        if (isServer)
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }
            if (skillObjectController == null)
            {
                skillObjectController = GetComponent<SkillObjectController>();
            }

            agent.speed = walkSpeed;
        }
    }

    public override void OnStartAuthority()
    {
        if (cam == null)
        {
            cam = Camera.main;
            CinemachineCamera followCamera = GameObject.FindFirstObjectByType<CinemachineCamera>();

            if (followCamera != null) 
            {
                followCamera.Target.TrackingTarget = this.transform;
                followCamera.Target.LookAtTarget = this.transform;
            }
        }
    }

    public void Update()
    {
        if (isOwned)
        {
            if (Input.GetMouseButtonDown(0) && cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                GetClickedPosition(ray.origin, ray.direction);
            }

            if (Input.GetMouseButtonDown(1) && cam != null) 
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                GetClickedInteractable(ray.origin, ray.direction);
            }
        }
    }

    public void LateUpdate()
    {
        if (isServer)
        {
            if (anim != null && agent.velocity.magnitude > 0.01f)
            {
                float targetSpeed = agent.velocity.magnitude;

                animationSpeed = Mathf.Lerp(animationSpeed, targetSpeed, 0.1f);

                anim.SetFloat("Velocity", animationSpeed);
                //Debug.Log($"Velocity: { animationSpeed }");
            }
        }
    }

    [Command]
    private void GetClickedPosition(Vector3 _origin, Vector3 _dir)
    {
        if (_origin == null || _dir == null) return;

        Ray ray = new Ray(_origin, _dir);

        RaycastHit[] hits = new RaycastHit[3];

        int hitCount = Physics.RaycastNonAlloc(ray, hits, 100f, walkableLayers);

        if (hitCount > 0) 
        {
            Vector3 targetPosition = hits[0].point;

            NavMeshPath path = new NavMeshPath();

            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                Debug.Log($"[Server]: Path calculated for Client: {netId} successfully!");

                MoveToClickedPosition(targetPosition);
            }
            else
            {
                Debug.Log($"[Server]: Could not calculate a complete path for Client: {netId}.");
            }
        }
    }

    [Command]
    private void GetClickedInteractable(Vector3 _origin, Vector3 _dir)
    {
        if (_origin == null || _dir == null) return;

        Ray ray = new Ray(_origin, _dir);

        RaycastHit[] hits = new RaycastHit[3];

        int hitCount = Physics.RaycastNonAlloc(ray, hits, 100f, interactableLayers);

        if (hitCount > 0)
        {
            int index = 0;

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].transform.CompareTag("SkillObject"))
                {
                    index = i;
                    continue;
                }
                else
                {
                    Debug.Log("No skill object hit.");
                    return;
                }
            }

            float distance = Vector3.Distance(transform.position, hits[index].transform.position);

            if (distance <= interactionRange)
            {
                NetworkIdentity skillObject = hits[index].transform.GetComponent<NetworkIdentity>();

                if (skillObject != null)
                {
                    transform.LookAt(Vector3.Lerp(transform.position, hits[index].transform.position, 10f));
                    skillObjectController.UseSkillObject(skillObject);
                }

                else
                {
                    Debug.Log("No Network Identity found on the Skill Object.");
                }
            }

            else
            {
                transform.LookAt(Vector3.Lerp(transform.position, hits[index].transform.position, 10f));
                agent.SetDestination(hits[0].transform.position);
                Debug.Log($"[Server]: Too far away. Walking to Skill Object.");
            }
        }
    }

    [Server]
    private void MoveToClickedPosition(Vector3 _moveToPosition) 
    {
        if (agent != null && _moveToPosition != null)
        {
            agent.speed = isSprinting ? runSpeed : walkSpeed;

            agent.SetDestination(_moveToPosition);

            Debug.Log($"[Server]: Moving Client: {netId} to position: {_moveToPosition}.");
        }
    }
}