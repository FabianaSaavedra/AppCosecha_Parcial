using UnityEngine;
using UnityEngine.Events;

// Cuenta cuantas partes de una labor se han hecho (por ejemplo, surcos sembrados)
// y avisa cuando se completan todas.
public class ContadorLabor : MonoBehaviour
{
    [SerializeField] private int total = 4;

    [Tooltip("Se ejecuta una sola vez cuando se llega al total")]
    public UnityEvent alCompletar;

    public int Cantidad { get; private set; }

    // Se llama desde los eventos del Inspector (por ejemplo "Al Sembrar" de cada Surco)
    public void Sumar()
    {
        if (Cantidad >= total) return;

        Cantidad++;
        if (Cantidad == total)
            alCompletar?.Invoke();
    }
}
