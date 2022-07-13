using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event System.Action OnReachedEndOfLevel;

    public float speed = 7;
    public float smoothMoveTime = .1f;
    public float turnSpeed = 8;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rb;
    bool disabled;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        angle = rb.rotation.eulerAngles.y;
        Guard.OnGuardHasSpottedPlayer += Disable;
    }

    private void Update()
    {
        MovePLayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Finish")
        {
            Disable();
            if(OnReachedEndOfLevel != null)
            {
                OnReachedEndOfLevel();
            }
        }
    }
    

    void MovePLayer()
    {
        Vector3 directionToMove = Vector3.zero;
        if (!disabled)
        {
            directionToMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }
        float moveMagnitude = directionToMove.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, moveMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        float angleToDirection = Mathf.Atan2(directionToMove.x, directionToMove.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, angleToDirection, Time.deltaTime * turnSpeed * moveMagnitude);

        velocity = transform.forward * speed * smoothInputMagnitude;
    }

    void Disable()
    {
        disabled = true;
    }

    private void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
    }

    private void OnDestroy()
    {
        Guard.OnGuardHasSpottedPlayer -= Disable;
    }
}
