using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    private float speed;
    private float runSpeed = 50f;
    private float walkSpeed = 10f;
    private float jumpForce = 150f;
    private Rigidbody rb;

    private bool isGrounded;

    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference run;

    public Transform camera;

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        run.action.Enable();
    }
    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
        run.action.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
    }

    bool IsGrounded()
    {
        return (Physics.SphereCast(transform.position, 0.5f, -transform.up, out RaycastHit hit, 0.6f));
    }

    private void FixedUpdate()
    {
        isGrounded = IsGrounded();

        if (camera != null)
        {
            Vector3 forward = camera.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        Vector2 moveInput = move.action.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        Vector3 targetVelocity = moveDirection * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (jump.action.IsPressed() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        if (run.action.IsPressed() && !isGrounded)
        {
            speed = runSpeed;
        }
        else {
            speed = walkSpeed;
        }
    }
}
