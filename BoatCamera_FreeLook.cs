using UnityEngine;

public class BoatCamera_FreeLook : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Boat or Player

    [Header("Offset (relative to target)")]
    public Vector3 offset = new Vector3(0f, 8f, -8f);

    [Header("Follow Settings")]
    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;

    private Rigidbody targetRb;

    void Start()
    {
        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!target) return;

        // Desired position relative to target orientation
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Smooth position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmooth * Time.fixedDeltaTime
        );

        // Desired rotation: look in target's forward direction
        Quaternion desiredRotation = Quaternion.LookRotation(target.forward, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmooth * Time.fixedDeltaTime
        );
    }

    // ===================== ADDED METHODS =====================

    public void SwitchToPlayer(Transform playerTarget)
    {
        target = playerTarget;
        targetRb = target.GetComponent<Rigidbody>();
    }

    public void SwitchToBoat(Transform boatTarget)
    {
        target = boatTarget;
        targetRb = target.GetComponent<Rigidbody>();
    }
}
