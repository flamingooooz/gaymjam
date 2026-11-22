using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private float speed;
    private float runSpeed = 30f;
    private float walkSpeed = 7f;
    private float jumpForce = 100f;
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
    public InputActionReference pause;

    public static bool isPaused = false;

    public Transform camera;
    public ParticleSystem dashParticle;
    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        run.action.Enable();
        crouch.action.Enable();
        drink.action.Enable();
        pause.action.Enable();
    }
    public void Update()
    {
        if (cooldownTimer >= dashCooldownTime) 
        {
            GameManager.instance.dashNote.SetActive(true);
        }
        else {
            GameManager.instance.dashNote.SetActive(false);
        }
            //Pausing
            if (pause.action.WasPressedThisFrame() && isPaused == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPaused = true;
            Debug.Log("pause");
            GameManager.instance.pause();
            Time.timeScale = 0;
        }
        else if (pause.action.WasPressedThisFrame() && isPaused == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            isPaused = false;
            Debug.Log("not pause");
            GameManager.instance.pause();
        }
    }
    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
        run.action.Disable();
        crouch.action.Disable();
        drink.action.Disable();
        pause.action.Disable();
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
        if (moveInput!= Vector2.zero)
        {
            Audiomanager.StartContinuousSound(SoundType.Walk);
        }
        else
        {
            Audiomanager.StopContinuousSound();
        }
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
                dashParticle.Play();
                speed = runSpeed;
                Invoke("ResetTimer", 0.5f);
            }
            else {
            for (float f = 0f; f <= 1; f += Time.deltaTime)
            {
                speed = Mathf.Lerp(speed, walkSpeed, f);
            }
            dashParticle.Stop();
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
                if (drink.action.IsPressed())
                {
                Audiomanager.PlaySound(SoundType.Drink);
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

