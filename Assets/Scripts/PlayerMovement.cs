using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;

    private Vector2 movement;
    private bool isDashing = false;
    private bool canDash = true;

    private Interactable currentInteractable;

    // Called by the player input
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    public void OnDash(InputValue value)
    {
        // Only trigger the dash if button is pressed and cooldown is over
        if (value.isPressed && canDash && !isDashing && movement != Vector2.zero)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalSpeed = moveSpeed;

        // Dash duration
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // Wait for cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        Debug.Log("Dash Ready");
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // Apply high velocity in the direction the player is currently moving
            rb.MovePosition(rb.position + movement.normalized * dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Normal movement
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // Detects when player walks into counter area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Interactable interactable))
        {
            currentInteractable = interactable;
            Debug.Log(currentInteractable.interactMessage);
        }
    }

    // Clears the interaction when walking away
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Interactable interactable))
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
            }
        }
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}
