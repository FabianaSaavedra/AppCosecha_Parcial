using UnityEngine;

public class MirarAlJugador : MonoBehaviour
{
    private Transform camara;

    void Start()
    {
        camara = Camera.main.transform;
    }

    void LateUpdate()
    {
        Vector3 direccion = transform.position - camara.position;
        direccion.y = 0f;   // no se inclina hacia arriba ni hacia abajo

        if (direccion.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direccion);
    }
}
