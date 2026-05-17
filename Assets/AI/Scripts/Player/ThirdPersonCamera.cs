using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour // camara para jugar en tercera persona
{
    [Header("Objetivo")]
    public Transform target; 

    [Header("Configuración")]
    public float distance = 5f;// distancia detrás del player
    public float height = 2f;
    public float mouseSensitivity = 3f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;

    private float yaw = 0f;// rotación horizontal
    private float pitch = 10f; // rotación vertical

    void LateUpdate()
    {
        // lee el raton
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // calcular pos camara
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);
        transform.position = target.position + offset;

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}