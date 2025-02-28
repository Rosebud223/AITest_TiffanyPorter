using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    // Public Variables
    public float pickupRange = 5f;
    public float baseThrowForce = 10f; 
    public float maxThrowMultiplier = 2f; 
    public float shrinkFactor = 0.5f; // Shrink object by 50%
    public Camera playerCamera; // Reference to the player's camera
    public Transform holdPosition;

    // Private Variables
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHolding = false;
    private Vector3 originalScale;
    private CharacterController playerController;

    void Start()
    {
        // Find the CharacterController in the Player hierarchy
        playerController = GetComponent<CharacterController>();

        if (playerController == null)
        {
            Debug.LogError("CharacterController not found! Make sure this script is attached to the player.");
        }

        // Find the camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // Automatically assigns the main camera
        }
    }

    void Update()
    {
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * pickupRange, Color.red);

        // Toggle pickup and throw with the "E" key
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                ThrowObject();
            }
        }

        if (isHolding)
        {
            // Shrink the object while being held
            if (heldObject != null)
            {
                heldObject.transform.localScale = Vector3.Lerp(heldObject.transform.localScale, originalScale * shrinkFactor, Time.deltaTime * 5f);
            }
        }
    }

    void TryPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name); // Debugging raycast hit

            if (hit.collider.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;
                heldObjectRb = heldObject.GetComponent<Rigidbody>();

                if (heldObjectRb != null)
                {
                    // Save the original size of the object
                    originalScale = heldObject.transform.localScale;

                    heldObjectRb.isKinematic = true; // Disable physics while holding
                    heldObject.transform.SetParent(holdPosition);
                    heldObject.transform.localPosition = Vector3.zero;
                    isHolding = true;
                    Debug.Log("Picked up: " + heldObject.name); // Debugging successful pickup
                }
                else
                {
                    Debug.LogWarning("Object does not have a Rigidbody to pick up.");
                }
            }
            else
            {
                Debug.LogWarning("Object hit but does not have 'Pickup' tag.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything.");
        }
    }

    void ThrowObject()
    {
        if (heldObject != null)
        {
            // Return the object to its original size when thrown
            heldObject.transform.localScale = originalScale;

            heldObject.transform.SetParent(null);
            heldObjectRb.isKinematic = false;

            // Use playerController.velocity.magnitude to calculate speed
            float playerSpeed = playerController.velocity.magnitude;
            float throwStrength = baseThrowForce + (playerSpeed * maxThrowMultiplier);

            // Apply force in the direction the player is looking
            heldObjectRb.AddForce(playerCamera.transform.forward * throwStrength, ForceMode.Impulse);

            heldObject = null;
            isHolding = false;
            Debug.Log("Object thrown.");
        }
    }
}
