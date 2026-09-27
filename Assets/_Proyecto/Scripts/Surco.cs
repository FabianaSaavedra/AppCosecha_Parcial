using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Surco de tierra: si el jugador lo mira llevando la bolsa de semillas,
// el surco queda sembrado y aparecen semillitas una por una.
// Requisitos en el surco: Tag "Interactable" + un Collider.
public class Surco : MonoBehaviour
{
    [Header("Bolsa que se necesita para sembrar")]
    [SerializeField] private GameObject bolsaSemillas;

    [Header("Semillitas que aparecen")]
    [SerializeField] private Material materialSemilla;
    [SerializeField] private int filas = 4;
    [SerializeField] private int semillasPorFila = 4;
    [Tooltip("Ancho y largo del area sembrada, en metros")]
    [SerializeField] private float ancho = 1.4f;
    [SerializeField] private float largo = 1.4f;
    [Tooltip("Centro del area sembrada (por si el surco no esta centrado)")]
    [SerializeField] private Vector3 centro = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float tamanoSemilla = 0.07f;
    [SerializeField] private float tiempoEntreSemillas = 0.04f;

    [Header("Sonido (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoSembrar;

    [Header("Eventos")]
    public UnityEvent alSembrar;

    public bool Sembrado { get; private set; }

    private GrabManager grabManager;

    void Start()
    {
        grabManager = GameObject.Find("GrabManager").GetComponent<GrabManager>();
    }

    // Lo llama el sistema de mirada (CameraPointerManager) al completar la seleccion
    public void OnPointerClickXR()
    {
        if (Sembrado) return;

        // Solo se siembra si el jugador lleva la bolsa de semillas
        if (grabManager.heldItem == null || grabManager.heldItem != bolsaSemillas) return;

        Sembrado = true;
        gameObject.tag = "Untagged";   // ya no se resalta al mirarlo

        if (audioSource != null && sonidoSembrar != null)
            audioSource.PlayOneShot(sonidoSembrar);

        StartCoroutine(SembrarSemillas());
        alSembrar?.Invoke();
    }

    // Crea las semillitas en filas, una por una
    private IEnumerator SembrarSemillas()
    {
        for (int f = 0; f < filas; f++)
        {
            for (int s = 0; s < semillasPorFila; s++)
            {
                float x = (semillasPorFila > 1 ? (float)s / (semillasPorFila - 1) - 0.5f : 0f) * ancho;
                float z = (filas > 1 ? (float)f / (filas - 1) - 0.5f : 0f) * largo;

                GameObject semilla = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(semilla.GetComponent<Collider>());   // para que la mirada no choque con ellas
                semilla.name = "Semillita";
                semilla.transform.SetParent(transform, false);
                semilla.transform.localPosition = centro + new Vector3(x, 0f, z);
                semilla.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 90f), 0f);
                semilla.transform.localScale = Vector3.zero;

                if (materialSemilla != null)
                    semilla.GetComponent<Renderer>().sharedMaterial = materialSemilla;

                StartCoroutine(Aparecer(semilla.transform));
                yield return new WaitForSeconds(tiempoEntreSemillas);
            }
        }
    }

    // Pequena animacion: la semilla crece un poco mas de su tamano y luego se acomoda
    private IEnumerator Aparecer(Transform semilla)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.25f;
            float escala = t < 0.7f ? Mathf.Lerp(0f, 1.3f, t / 0.7f) : Mathf.Lerp(1.3f, 1f, (t - 0.7f) / 0.3f);
            semilla.localScale = Vector3.one * tamanoSemilla * escala;
            yield return null;
        }
        semilla.localScale = Vector3.one * tamanoSemilla;
    }
}
