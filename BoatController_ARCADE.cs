using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class BoatController_Final : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController firstPersonController;
    public Rigidbody rb;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float turnSpeed = 80f;
    public float acceleration = 8f;
    public float deceleration = 10f;

    [Header("Exit Settings")]
    [Tooltip("Transform point where player will be placed after exiting")]
    public Transform exitPoint;

    [Tooltip("UI Text to show prompt 'Press Space to Exit'")]
    public TextMeshProUGUI exitPrompt;

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float turnInput = 0f;
    private bool canExit = false;
    private Rigidbody playerRb;
    private bool hasExited = false; // Ngăn gọi ExitBoat() nhiều lần

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.drag = 1.5f;
        rb.angularDrag = 4f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (firstPersonController != null)
        {
            firstPersonController.playerCanMove = false;
            firstPersonController.enableJump = false;
            firstPersonController.enableCrouch = false;
            firstPersonController.cameraCanMove = true;

            firstPersonController.transform.SetParent(transform);

            playerRb = firstPersonController.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = true;
            }
        }
        else
        {
            Debug.LogError("BoatController_Final: FirstPersonController not assigned!");
        }
    }

    void Update()
    {
        float v = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        targetSpeed = v * moveSpeed;

        if (canExit && !hasExited && Input.GetKeyDown(KeyCode.Space))
        {
            ExitBoat();
        }
    }

    void FixedUpdate()
    {
        float rate = Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);

        Vector3 forwardMove = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRot);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger Enter: {other.gameObject.name}, Tag: {other.tag}");

        if (other.CompareTag("HouseTrigger"))
        {
            canExit = true;
            Debug.Log("House trigger detected!");

            if (exitPrompt != null)
            {
                exitPrompt.text = "Press Space to Exit Boat";
                exitPrompt.gameObject.SetActive(true);
            }

            if (exitPoint == null)
            {
                exitPoint = other.transform.Find("ExitPoint");
                if (exitPoint == null)
                {
                    Debug.LogWarning("ExitPoint child not found under HouseTrigger. Searching parent...");
                    exitPoint = FindExitPointRecursive(other.transform.root);
                }
                if (exitPoint != null)
                {
                    Debug.Log("ExitPoint found: " + exitPoint.name);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HouseTrigger"))
        {
            canExit = false;
            if (exitPrompt != null)
            {
                exitPrompt.gameObject.SetActive(false);
            }
        }
    }

    public void ExitBoat()
    {
        if (hasExited) return;
        hasExited = true;

        if (firstPersonController == null)
        {
            Debug.LogError("FirstPersonController is null!");
            return;
        }

        firstPersonController.playerCanMove = true;
        firstPersonController.enableJump = true;
        firstPersonController.enableCrouch = true;
        firstPersonController.cameraCanMove = true;

        firstPersonController.transform.SetParent(null);

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        if (exitPoint != null)
        {
            firstPersonController.transform.position = exitPoint.position;
            firstPersonController.transform.rotation = exitPoint.rotation;
            Debug.Log("Player moved to exitPoint: " + exitPoint.name);
        }
        else
        {
            Debug.LogError("No exitPoint found - player placed at trigger location!");
            firstPersonController.transform.position = transform.position + Vector3.up * 2f;
        }

        if (exitPrompt != null)
        {
            exitPrompt.gameObject.SetActive(false);
        }

        BoatCamera_FreeLook cameraScript = Camera.main.GetComponent<BoatCamera_FreeLook>();
        if (cameraScript != null)
        {
            cameraScript.SwitchToPlayer(firstPersonController.transform);
            Debug.Log("Camera switched to player");
        }
        else
        {
            Debug.LogWarning("BoatCamera_FreeLook not found on Main Camera!");
        }

        this.enabled = false;
    }

    private Transform FindExitPointRecursive(Transform parent)
    {
        if (parent.name == "ExitPoint") return parent;
        foreach (Transform child in parent)
        {
            var result = FindExitPointRecursive(child);
            if (result != null) return result;
        }
        return null;
    }
}