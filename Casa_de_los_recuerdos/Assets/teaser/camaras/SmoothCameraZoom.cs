using UnityEngine;

public class SmoothCameraZoom : MonoBehaviour
{
    [Header("Configuración")]
    public float zoomSpeed = 10f;
    public float smoothTime = 0.3f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    [Header("Eje de Movimiento")]
    public bool useLocalForward = true;
    public Vector3 customAxis = Vector3.forward; 

    private float targetZoom;
    private float currentZoom;
    private float zoomVelocity;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        currentZoom = 0f;
        targetZoom = 0f;
    }

    void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            scrollInput += 0.1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            scrollInput -= 0.1f;


        if (scrollInput != 0f)
        {
            targetZoom += scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, smoothTime);

        Vector3 direction = useLocalForward ? transform.forward : customAxis.normalized;
        transform.position = startPosition + direction * currentZoom;
    }

    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void ZoomIn(float amount)
    {
        targetZoom = Mathf.Clamp(targetZoom + amount, minZoom, maxZoom);
    }

    public void ZoomOut(float amount)
    {
        targetZoom = Mathf.Clamp(targetZoom - amount, minZoom, maxZoom);
    }
}