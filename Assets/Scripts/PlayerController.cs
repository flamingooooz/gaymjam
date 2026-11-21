using System.IO.Compression;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private float speed;
    private float runSpeed = 50f;
    private float walkSpeed = 10f;
    private float jumpForce = 150f;
    private float dashCooldownTime = 1f;
    private float cooldownTimer;
    private Rigidbody rb;
    private Vector3 playerScale;
    private float reach = 5f;

    private bool isGrounded;

    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference run;
    public InputActionReference crouch;
    public InputActionReference drink;

    public Transform camera;

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        run.action.Enable();
        crouch.action.Enable();
        drink.action.Enable();
    }
    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
        run.action.Disable();
        crouch.action.Disable();
        drink.action.Disable();
    }

    private void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
    }

    bool IsGrounded()
    {
        return Physics.SphereCast(transform.position, 0.5f, -transform.up, out RaycastHit hit, 0.6f);
    }

    private void FixedUpdate()
    {
        isGrounded = IsGrounded();

        cooldownTimer += Time.fixedDeltaTime;

        //player rotation
        if (camera != null)
        {
            Vector3 forward = camera.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        //movement
        Vector2 moveInput = move.action.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        Vector3 targetVelocity = moveDirection * speed * (Mathf.Sqrt(GameManager.instance.thirst) / 10);
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        //jump
        if (jump.action.IsPressed() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce * (Mathf.Sqrt(GameManager.instance.thirst) / 10), ForceMode.Impulse);
        }
        //dash
        if (run.action.IsPressed() && cooldownTimer >= dashCooldownTime)
        {
            speed = runSpeed;
            Invoke("ResetTimer", 0.5f);
        }
        else {
            speed = walkSpeed;
        }
        
        if (crouch.action.IsPressed())
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 3, transform.localScale.z);
        }
        else
        {
            transform.localScale = playerScale;
        }
        Debug.DrawRay(transform.position, transform.forward);
        if (Physics.BoxCast(transform.position, transform.localScale, transform.forward, transform.rotation, reach, LayerMask.GetMask("bottle")))
        {
            GameManager.instance.drinkHint.SetActive(true);
            if(drink.action.IsPressed())
            {
                GameManager.instance.Drink(15f);
                SpawnManager.instance.RespawnBottle();
            }
        }
        else
        {
            GameManager.instance.drinkHint.SetActive(false);
        }
    }

    private void ResetTimer()
    {
        cooldownTimer = 0f;
    }
}
