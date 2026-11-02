using UnityEngine;
using UnityEngine.EventSystems;

public class RotateInteraction : MonoBehaviour
{
    public enum RotationAxis
    {
        LocalY,       // Sumbu tegak tubuh (kepala - kaki)
        LocalZ,       // Sumbu Z lokal
        LocalX,       // Sumbu X lokal
        ParentUp,     // Tegak lurus kartu (normal kartu)
        ParentForward // Menghadap ke atas kartu
    }

    [Header("Pengaturan Sumbu Putar")]
    [Tooltip("Pilih sumbu putar. Gunakan LocalY untuk putaran 360 badan seperti patung/turntable.")]
    public RotationAxis rotationAxis = RotationAxis.LocalY;

    [Tooltip("Centang jika arah putaran ingin dibalik")]
    public bool invertDirection = false;

    [Header("Pengaturan Putar Otomatis")]
    [Tooltip("Centang jika ingin model berputar pelan saat sedang tidak disentuh")]
    public bool enableAutoRotate = true;
    [Tooltip("Kecepatan putar otomatis")]
    public float autoRotateSpeed = 30f;

    [Header("Sensitivitas Geser (Drag)")]
    [Tooltip("Sensitivitas saat drag mouse di Unity Editor / PC")]
    public float mouseSensitivity = 0.4f;
    [Tooltip("Sensitivitas saat geser jari di layar HP")]
    public float touchSensitivity = 0.2f;

    private Vector3 lastMousePos;
    private bool isInteracting = false;
    private float idleTimer = 0f;

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();

        // Putar otomatis secara halus jika sedang tidak digeser/disentuh
        if (enableAutoRotate && !isInteracting)
        {
            idleTimer += Time.deltaTime;
            // Beri jeda 0.4 detik setelah dilepas sebelum mulai berputar otomatis lagi
            if (idleTimer > 0.4f)
            {
                ApplyRotation(autoRotateSpeed * Time.deltaTime);
            }
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Abaikan input sentuh jika sedang menyentuh elemen UI (tombol/slider)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                isInteracting = true;
                idleTimer = 0f;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                isInteracting = true;
                idleTimer = 0f;
                float rotationAmount = -touch.deltaPosition.x * touchSensitivity;
                ApplyRotation(rotationAmount);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isInteracting = false;
            }
        }
        else if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            isInteracting = false;
        }
    }

    void HandleMouseInput()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
        // Abaikan klik mouse jika kursor sedang di atas UI (tombol/slider)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonUp(0)) isInteracting = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isInteracting = true;
            idleTimer = 0f;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            if (Mathf.Abs(mouseDelta.x) > 0.1f)
            {
                isInteracting = true;
                idleTimer = 0f;
                float rotationAmount = -mouseDelta.x * mouseSensitivity;
                ApplyRotation(rotationAmount);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isInteracting = false;
        }
        #endif
    }

    /// <summary>
    /// Memutar model pada sumbu yang dipilih (default: sumbu tegak tubuh model sendiri / LocalY).
    /// </summary>
    void ApplyRotation(float angle)
    {
        float dir = invertDirection ? -1f : 1f;
        float finalAngle = angle * dir;

        switch (rotationAxis)
        {
            case RotationAxis.LocalY:
                transform.Rotate(Vector3.up, finalAngle, Space.Self);
                break;
            case RotationAxis.LocalZ:
                transform.Rotate(Vector3.forward, finalAngle, Space.Self);
                break;
            case RotationAxis.LocalX:
                transform.Rotate(Vector3.right, finalAngle, Space.Self);
                break;
            case RotationAxis.ParentUp:
                Vector3 parentUp = (transform.parent != null) ? transform.parent.up : transform.up;
                transform.Rotate(parentUp, finalAngle, Space.World);
                break;
            case RotationAxis.ParentForward:
                Vector3 parentFwd = (transform.parent != null) ? transform.parent.forward : transform.forward;
                transform.Rotate(parentFwd, finalAngle, Space.World);
                break;
        }
    }
}
