using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 6f;
    public float jumpForce = 2.5f;
    public float gravity = -9.81f;

    [Header("Câmera")]
    public float mouseSensitivity = 120f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float cameraVerticalRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * x + transform.forward * z;
        controller.Move(movement * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void RotateCamera()
    {
        if (playerCamera == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -85f, 85f);

        playerCamera.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
