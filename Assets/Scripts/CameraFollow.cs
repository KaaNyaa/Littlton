using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Drag Player here
    public float smoothing = 5f; // How "floaty" the camera feels

    [Header("Boundary Settings")]
    public Vector2 minBounds; // Bottom-Left corner of the map
    public Vector2 maxBounds; // Top-Right corner of the map

    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        // Find the player automatically by Tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        // Calculate how much the camera "sees" so we can stop it BEFORE it shows the void
        Camera cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Determine where the camera wants to be
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            // Clamp that position so it stays inside the grid
            // Subtract half the camera size so the EDGE of the camera hits the wall
            float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
            float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

            Vector3 clampedPosition = new Vector3(clampedX, clampedY, transform.position.z);

            // Smoothly move to that clamped position
            transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothing * Time.deltaTime);
        }
    }
}