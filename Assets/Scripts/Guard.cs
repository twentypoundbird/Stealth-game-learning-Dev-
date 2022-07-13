using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public static System.Action OnGuardHasSpottedPlayer;

    public Transform pathHolder;

    [SerializeField]
    float moveSpeed, waitTime, turnSpeed;
    public float timeToSpotPlayer = .5f;

    public Light spotLight;
    public float viewDistance;
    public LayerMask viewMask;

    float viewAngle;
    float playerVisibleTimer;

    public float detectionTime = 1f;

    Transform player;

    Color originalSpotlightColor;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotLight.spotAngle;
        originalSpotlightColor = spotLight.color;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
        StartCoroutine(MoveThroughPoints(waypoints));
    }

    private void Update()
    {
        if (CanSeePlayer())
        {
            playerVisibleTimer += Time.deltaTime;
        }
        else
        {
            playerVisibleTimer -= Time.deltaTime;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotLight.color = Color.Lerp(originalSpotlightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if(playerVisibleTimer >= timeToSpotPlayer)
        {
            if(OnGuardHasSpottedPlayer != null)
            {
                OnGuardHasSpottedPlayer();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }


    


    IEnumerator MoveThroughPoints(Vector3[] waypoints)
    {
        Vector3 startPosition = waypoints[0];
        transform.position = startPosition;
        transform.LookAt(waypoints[1]);
        while (true)
        {
            for (int i = 1; i < waypoints.Length; i++)
            {
                yield return StartCoroutine(RotateGuardToTarget(waypoints[i], turnSpeed));
                yield return StartCoroutine(MoveGuardToTarget(waypoints[i], moveSpeed));
                yield return new WaitForSeconds(waitTime);
            }

            yield return StartCoroutine(RotateGuardToTarget(startPosition, turnSpeed));
            yield return StartCoroutine(MoveGuardToTarget(startPosition, moveSpeed));
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator RotateGuardToTarget(Vector3 targetPosition, float rotationSpeed)
    {
        Vector3 targetDir = (targetPosition - transform.position).normalized;
        float angleInDegrees = 90 - Mathf.Atan2(targetDir.z, targetDir.x) * Mathf.Rad2Deg;
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, angleInDegrees)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, angleInDegrees, rotationSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }

    }

    IEnumerator MoveGuardToTarget(Vector3 targetPosition, float speed)
    {

        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

    }


    bool CanSeePlayer()
    {

        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                if (Physics.Linecast(transform.position, player.position, out RaycastHit hit))
                {
                    if (hit.transform.tag == "Player") return true;

                }
            }
        }
        return false;
    }

    void VisibleZone()
    {


        //for (int i = 0; i <= viewAngle; i++)
        //{
        //    float angleInRadians = (viewAngle / 2 - i) * Mathf.Deg2Rad;
        //    float xDirection = transform.forward.normalized.x * Mathf.Cos(angleInRadians) - transform.forward.normalized.z * Mathf.Sin(angleInRadians);
        //    float yDirection = transform.forward.normalized.x * Mathf.Sin(angleInRadians) + transform.forward.normalized.z * Mathf.Cos(angleInRadians);
        //    Vector3 RayDirection = (new Vector3(xDirection, 0, yDirection)).normalized;

        //    Ray ray = new Ray(transform.position - transform.up * 0.5f, RayDirection);
        //    RaycastHit hit;
        //    if (Physics.Raycast(ray, out hit, viewDistance))
        //    {
        //        if(hit.collider.tag == "Player")
        //        {
        //            Debug.Log(true);
        //        }
        //        Debug.DrawLine(ray.origin, hit.point, Color.red);
        //    }
        //    else
        //    {
        //        Debug.DrawLine(ray.origin, RayDirection * viewDistance, Color.green);
        //    }

        //}

    }
}
