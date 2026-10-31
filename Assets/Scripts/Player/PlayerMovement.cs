using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Скорости движения")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Прыжок и гравитация")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Приседание")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float crouchAmount = 0.6f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Покачивание головы (Head Bob)")]
    [SerializeField] private bool enableBob = true;
    [SerializeField] private float bobSpeedWalk = 10f;
    [SerializeField] private float bobSpeedSprint = 14f;
    [SerializeField] private float bobSpeedCrouch = 7f;
    [SerializeField] private float bobAmountWalk = 0.035f;
    [SerializeField] private float bobAmountSprint = 0.07f;
    [SerializeField] private float bobAmountCrouch = 0.015f;

    [Header("Дыхание на месте (Idle)")]
    [SerializeField] private float bobSpeedIdle = 1.8f;
    [SerializeField] private float bobAmountIdle = 0.008f;

    [Header("Звук шагов (Луп)")]
    [SerializeField] private AudioClip footstepLoopClip;
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.4f;
    [SerializeField, Range(0f, 1f)] private float sprintVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float crouchVolume = 0.2f;

    private CharacterController controller;
    private PlayerStamina stamina;
    private AudioSource footstepSource;

    private float verticalVelocity;
    private bool isCrouching;

    private float standingHeight;
    private float crouchingHeight;
    private float currentHeight;
    private float bottomY;

    private float cameraStandingX;
    private float cameraStandingY;
    private float currentCameraY;

    private float bobTimer;
    private Vector3 currentBobOffset;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = GetComponent<PlayerStamina>();

        standingHeight = controller.height;
        crouchingHeight = standingHeight * 0.5f;
        currentHeight = standingHeight;
        bottomY = controller.center.y - (standingHeight / 2f);

        if (cameraHolder != null)
        {
            cameraStandingX = cameraHolder.localPosition.x;
            cameraStandingY = cameraHolder.localPosition.y;
            currentCameraY = cameraStandingY;
        }

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepLoopClip;
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            if (footstepSource.isPlaying) footstepSource.Pause();
            return;
        }

        HandleCrouch();

        Vector2 input = GameInput.Instance.Player.Move.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);

        bool wantsSprint = !isCrouching && input.sqrMagnitude > 0.01f && GameInput.Instance.Player.Sprint.IsPressed();
        // Спринт и расход стамины
        bool isSprinting = wantsSprint && (stamina == null || stamina.Consume(Time.deltaTime));

        float speed = walkSpeed;
        if (isCrouching) speed = crouchSpeed;
        else if (isSprinting) speed = sprintSpeed;

        Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;

        // Проверка приземления
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded && !isCrouching && GameInput.Instance.Player.Jump.WasPressedThisFrame())
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        UpdateHeadBob(input, isSprinting);
        HandleContinuousFootsteps(input, isSprinting);
    }

    private void HandleContinuousFootsteps(Vector2 input, bool isSprinting)
    {
        if (footstepSource == null || footstepLoopClip == null) return;

        bool isMovingOnGround = controller.isGrounded && input.sqrMagnitude > 0.01f;

        if (isMovingOnGround)
        {
            if (!footstepSource.isPlaying) footstepSource.Play();

            float targetVol = walkVolume;
            float targetPitch = 1.0f;

            if (isCrouching)
            {
                targetVol = crouchVolume;
                targetPitch = 0.95f;
            }
            else if (isSprinting)
            {
                targetVol = sprintVolume;
                targetPitch = 1.10f;
            }

            float finalVol = targetVol * SettingsData.GameVolume;

            if (Mathf.Abs(footstepSource.volume - finalVol) > 0.01f)
                footstepSource.volume = Mathf.MoveTowards(footstepSource.volume, finalVol, Time.deltaTime * 6f);

            if (Mathf.Abs(footstepSource.pitch - targetPitch) > 0.01f)
                footstepSource.pitch = Mathf.MoveTowards(footstepSource.pitch, targetPitch, Time.deltaTime * 4f);
        }
        else if (footstepSource.isPlaying)
        {
            footstepSource.volume = Mathf.MoveTowards(footstepSource.volume, 0f, Time.deltaTime * 8f);
            if (footstepSource.volume <= 0.01f)
                footstepSource.Pause();
        }
    }

    private void HandleCrouch()
    {
        bool wantsCrouch = GameInput.Instance.Player.Crouch.IsPressed();
        if (!wantsCrouch && isCrouching && HasCeilingAbove())
            wantsCrouch = true;

        isCrouching = wantsCrouch;

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        if (Mathf.Abs(currentHeight - targetHeight) > 0.001f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            controller.height = currentHeight;
            controller.center = new Vector3(controller.center.x, bottomY + (currentHeight / 2f), controller.center.z);
        }

        if (cameraHolder != null)
        {
            float targetCamY = isCrouching ? (cameraStandingY - crouchAmount) : cameraStandingY;
            if (Mathf.Abs(currentCameraY - targetCamY) > 0.001f)
            {
                currentCameraY = Mathf.MoveTowards(currentCameraY, targetCamY, crouchTransitionSpeed * Time.deltaTime);
            }
        }
    }

    private void UpdateHeadBob(Vector2 input, bool isSprinting)
    {
        if (cameraHolder == null || !enableBob) return;

        Vector3 targetBob;
        bool isMoving = controller.isGrounded && input.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            float frequency = isCrouching ? bobSpeedCrouch : (isSprinting ? bobSpeedSprint : bobSpeedWalk);
            float amount = isCrouching ? bobAmountCrouch : (isSprinting ? bobAmountSprint : bobAmountWalk);

            bobTimer += Time.deltaTime * frequency;
            targetBob.x = Mathf.Cos(bobTimer) * (amount * 0.6f);
            targetBob.y = Mathf.Sin(bobTimer * 2f) * amount;
            targetBob.z = 0f;
        }
        else
        {
            targetBob.x = Mathf.Cos(Time.time * (bobSpeedIdle * 0.5f)) * (bobAmountIdle * 0.5f);
            targetBob.y = Mathf.Sin(Time.time * bobSpeedIdle) * bobAmountIdle;
            targetBob.z = 0f;
        }

        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBob, Time.deltaTime * 6f);

        Vector3 camPos = cameraHolder.localPosition;
        camPos.x = cameraStandingX + currentBobOffset.x;
        camPos.y = currentCameraY + currentBobOffset.y;
        cameraHolder.localPosition = camPos;
    }

    private bool HasCeilingAbove()
    {
        Vector3 origin = transform.position + Vector3.up * (bottomY + crouchingHeight);
        float radius = controller.radius * 0.85f;
        float dist = standingHeight - crouchingHeight;
        return Physics.SphereCast(origin, radius, Vector3.up, out _, dist);
    }
}