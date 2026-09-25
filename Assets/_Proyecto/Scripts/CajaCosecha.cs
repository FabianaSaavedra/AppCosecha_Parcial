using UnityEngine;
using UnityEngine.Events;

// Caja que guarda VARIOS objetos cosechados (a diferencia de PlateBehaviour, que guarda solo uno).
// Requisitos en la caja: Tag "Interactable" + un Collider.
// Cuando el jugador lleva un objeto y mira la caja, el objeto se acomoda dentro
// en una cuadricula y ya no se puede volver a coger.
public class CajaCosecha : MonoBehaviour
{
    [Header("Donde se acomodan los objetos")]
    [Tooltip("Punto vacio dentro de la caja (centro y un poco arriba del fondo)")]
    [SerializeField] private Transform puntoCaja;
    [Tooltip("Cuantos objetos caben por fila")]
    [SerializeField] private int columnas = 3;
    [Tooltip("Distancia entre un objeto y otro dentro de la caja")]
    [SerializeField] private float separacion = 0.18f;

    [Header("Meta")]
    [Tooltip("Cuantos objetos hay que guardar para completar la caja")]
    [SerializeField] private int totalParaCompletar = 3;

    [Header("Eventos")]
    [Tooltip("Se ejecuta cada vez que se guarda un objeto")]
    public UnityEvent alGuardar;
    [Tooltip("Se ejecuta una sola vez cuando se llega a la meta")]
    public UnityEvent alCompletar;

    public int Cantidad { get; private set; }
    public int Total => totalParaCompletar;
    public bool Completa => Cantidad >= totalParaCompletar;

    private GrabManager grabManager;

    void Start()
    {
        grabManager = GameObject.Find("GrabManager").GetComponent<GrabManager>();
    }

    // Lo llama el sistema de mirada (CameraPointerManager) al completar la seleccion
    public void OnPointerClickXR()
    {
        GameObject item = grabManager.heldItem;
        if (item == null) return;

        GrabObject grab = item.GetComponent<GrabObject>();
        if (grab == null) return;

        // Coloca el objeto en su lugar dentro de la caja
        grab.Place(CalcularPosicion(Cantidad));

        // Bloquea el objeto para que ya no se pueda volver a coger
        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        item.tag = "Untagged";
        item.transform.SetParent(transform, true);

        Cantidad++;
        alGuardar?.Invoke();

        if (Cantidad == totalParaCompletar)
        {
            alCompletar?.Invoke();
        }
    }

    // Acomoda los objetos en filas y columnas; si se llena una capa, apila la siguiente encima
    private Vector3 CalcularPosicion(int indice)
    {
        int porCapa = columnas * columnas;
        int capa = indice / porCapa;
        int resto = indice % porCapa;
        int fila = resto / columnas;
        int columna = resto % columnas;

        float centro = (columnas - 1) / 2f;
        Vector3 local = new Vector3(
            (columna - centro) * separacion,
            capa * separacion,
            (fila - centro) * separacion);

        return puntoCaja.TransformPoint(local);
    }
}
