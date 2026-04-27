using UnityEngine;

public class Flag : MonoBehaviour
{
    [Header("Configuración de Equipo")]
    public int teamID; // Ejemplo: 1 para Equipo Azul, 2 para Equipo Rojo

    [Header("Estado")]
    public bool isCaptured = false;
    public Transform carrier; // Quién lleva la bandera actualmente

    public void Capture(Transform newCarrier)
    {
        isCaptured = true;
        carrier = newCarrier;
        // Aquí podrías añadir lógica para que la bandera se "pegue" al portador
        transform.SetParent(newCarrier);
        transform.localPosition = new Vector3(0, 2, 0); // Posición sobre la cabeza
    }

    public void Drop()
    {
        isCaptured = false;
        carrier = null;
        transform.SetParent(null);
    }
}

