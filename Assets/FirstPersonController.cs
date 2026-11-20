using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRoot;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2.2f;
    [SerializeField] private float groundAcceleration = 45f;
    [SerializeField] private float airAcceleration = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -22f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Look")]
    [SerializeField] private float sensitivity = 0.12f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float eyeOffsetFromTop = 0.15f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Head Bob")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float bobVerticalAmount = 0.035f;
    [SerializeField] private float bobHorizontalAmount = 0.02f;
    [SerializeField] private float strideLength = 3.2f;
    [SerializeField] private float sprintBobMultiplier = 1.4f;
    [SerializeField] private float crouchBobMultiplier = 0.6f;
    [SerializeField] private float bobBlendSpeed = 8f;

    private CharacterController controller;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float pitch;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float currentHeight;
    private bool isCrouching;
    private bool isSprinting;

    private float bobPhase;
    private float bobBlend;
    private Vector3 bobOffset;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentHeight = standHeight;
        ApplyHeight();
        UpdateCameraPosition();
        CreateInputActions();
    }

    private void CreateInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddBinding("<Gamepad>/leftStick");

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");

        crouchAction = new InputAction("Crouch", InputActionType.Button, "<Keyboard>/leftCtrl");
        crouchAction.AddBinding("<Keyboard>/c");
        crouchAction.AddBinding("<Gamepad>/buttonEast");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        crouchAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
                 && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
            HandleLook();

        HandleCrouch();
        HandleMovement();
        HandleHeadBob();
        UpdateCameraPosition();
    }

    private void HandleLook()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>() * sensitivity;

        transform.Rotate(Vector3.up * delta.x);

        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleCrouch()
    {
        bool wantsCrouch = crouchAction.IsPressed();

        if (wantsCrouch)
            isCrouching = true;
        else if (isCrouching && CanStandUp())
            isCrouching = false;

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        if (Mathf.Abs(currentHeight - targetHeight) < 0.001f) currentHeight = targetHeight;

        ApplyHeight();
    }

    private void ApplyHeight()
    {
        controller.height = currentHeight;
        controller.center = new Vector3(0f, currentHeight * 0.5f, 0f);
    }

    private void UpdateCameraPosition()
    {
        if (cameraRoot == null) return;

        cameraRoot.localPosition = new Vector3(0f, currentHeight - eyeOffsetFromTop, 0f) + bobOffset;
    }

    private bool CanStandUp()
    {
        float radius = controller.radius * 0.95f;
        Vector3 origin = transform.position + Vector3.up * (currentHeight - radius);
        float distance = standHeight - currentHeight + 0.05f;

        return !Physics.SphereCast(origin, radius, Vector3.up, out _, distance,
                                   obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;
        float dt = Time.deltaTime;

        coyoteTimer = grounded ? coyoteTime : coyoteTimer - dt;
        jumpBufferTimer = jumpAction.WasPressedThisFrame() ? jumpBufferTime : jumpBufferTimer - dt;

        Vector2 input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        isSprinting = sprintAction.IsPressed() && !isCrouching && input.y > 0.1f;
        float targetSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
        Vector3 targetVelocity = wishDir * targetSpeed;

        float accel = grounded ? groundAcceleration : airAcceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, accel * dt);


        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !(isCrouching && !CanStandUp()))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            isCrouching = false;
        }

        verticalVelocity += gravity * dt;

        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;

        Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(motion * dt);
    }

    private void HandleHeadBob()
    {
        float dt = Time.deltaTime;
        float speed = horizontalVelocity.magnitude;

        bool active = enableHeadBob && controller.isGrounded && speed > 0.1f;

        bobBlend = Mathf.MoveTowards(bobBlend, active ? 1f : 0f, bobBlendSpeed * dt);

        if (active)
        {
            bobPhase += (speed / strideLength) * Mathf.PI * 2f * dt;
            bobPhase = Mathf.Repeat(bobPhase, Mathf.PI * 2f);
        }

        float multiplier = isCrouching ? crouchBobMultiplier : (isSprinting ? sprintBobMultiplier : 1f);

        float y = Mathf.Sin(bobPhase * 2f) * bobVerticalAmount;
        float x = Mathf.Sin(bobPhase) * bobHorizontalAmount;

        bobOffset = new Vector3(x, y, 0f) * (bobBlend * multiplier);
    }
}