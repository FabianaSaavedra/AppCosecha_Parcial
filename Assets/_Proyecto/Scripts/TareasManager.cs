using System.Collections;
using UnityEngine;
using TMPro;

// Cerebro de la granja: lleva la lista de tareas del dia, marca las que se completan,
// suma monedas, cambia la hora del dia y le dice a Pancho que decir.
public class TareasManager : MonoBehaviour
{
    [System.Serializable]
    public class Tarea
    {
        public string nombre;
        [TextArea(2, 4)] public string mensajePancho;   // lo que dice Pancho al completar esta tarea
        public GameObject[] activarAlCompletar;          // cosas que aparecen al completarla (letrero "Hecho", productos...)
        [HideInInspector] public bool completada;
    }

    // Se guarda entre escenas para mostrarlo en la feria
    public static int MonedasGanadas;

    [Header("Tareas del dia (en orden)")]
    public Tarea[] tareas;
    public int monedasPorTarea = 10;

    [Header("Tablero")]
    public TMP_Text textoTablero;
    public TMP_Text textoMonedas;

    [Header("Pancho")]
    public TMP_Text textoPancho;
    [TextArea(2, 4)] public string mensajeInicial = "¡Buenos días! Estas son las tareas de hoy. Cada una vale 10 monedas. ¡Empecemos!";
    [TextArea(2, 4)] public string mensajeFinal = "¡Todo listo! ¡Nos vamos a la feria!";

    [Header("Al terminar todas las tareas")]
    public GameObject[] activarAlTerminarTodo;

    [Header("Ciclo del dia (un valor por cada momento: inicio + una por tarea)")]
    public Light sol;
    public Camera camara;
    public float duracionTransicion = 2f;
    public Color[] coloresSol = {
        new Color(1f, 0.62f, 0.40f), new Color(1f, 0.80f, 0.60f), new Color(1f, 0.93f, 0.80f),
        new Color(1f, 1f, 0.95f),    new Color(1f, 0.92f, 0.78f), new Color(1f, 0.72f, 0.50f),
        new Color(1f, 0.55f, 0.40f) };
    public Color[] coloresCielo = {
        new Color(0.98f, 0.78f, 0.62f), new Color(0.85f, 0.88f, 0.90f), new Color(0.75f, 0.90f, 0.96f),
        new Color(0.75f, 0.90f, 0.96f), new Color(0.80f, 0.88f, 0.92f), new Color(0.98f, 0.75f, 0.60f),
        new Color(0.93f, 0.60f, 0.62f) };
    public float[] alturasSol = { 10f, 20f, 35f, 55f, 40f, 20f, 8f };
    public float[] intensidadesSol = { 0.7f, 0.9f, 1.05f, 1.2f, 1.05f, 0.85f, 0.7f };

    [Header("Sonido (opcional)")]
    public AudioSource audioSource;
    public AudioClip sonidoTareaCompletada;
    public AudioClip sonidoTodoCompletado;

    private int completadas;
    private float giroSolY;

    void Start()
    {
        MonedasGanadas = 0;

        // Todo lo que aparece "despues" empieza escondido
        foreach (Tarea t in tareas)
            foreach (GameObject go in t.activarAlCompletar)
                if (go != null) go.SetActive(false);

        foreach (GameObject go in activarAlTerminarTodo)
            if (go != null) go.SetActive(false);

        if (sol != null) giroSolY = sol.transform.eulerAngles.y;

        if (textoPancho != null) textoPancho.text = mensajeInicial + SiguienteTarea();
        ActualizarTablero();
        AplicarMomento(0);
    }

    // Se llama desde los eventos del Inspector (por ejemplo "Al Completar" de CajaCosecha).
    // El numero es el mismo del tablero: 1, 2, 3...
    public void CompletarTarea(int numero)
    {
        int i = numero - 1;
        if (i < 0 || i >= tareas.Length) return;
        if (tareas[i].completada) return;   // evita contarla dos veces

        tareas[i].completada = true;
        completadas++;
        MonedasGanadas += monedasPorTarea;

        foreach (GameObject go in tareas[i].activarAlCompletar)
            if (go != null) go.SetActive(true);

        ActualizarTablero();
        StopAllCoroutines();
        StartCoroutine(TransicionMomento(completadas));

        if (completadas >= tareas.Length)
        {
            foreach (GameObject go in activarAlTerminarTodo)
                if (go != null) go.SetActive(true);
            if (textoPancho != null) textoPancho.text = mensajeFinal;
            Sonar(sonidoTodoCompletado != null ? sonidoTodoCompletado : sonidoTareaCompletada);
        }
        else
        {
            if (textoPancho != null)
                textoPancho.text = tareas[i].mensajePancho + "\n+" + monedasPorTarea + " monedas" + SiguienteTarea();
            Sonar(sonidoTareaCompletada);
        }
    }

    public bool EstaCompletada(int numero)
    {
        int i = numero - 1;
        return i >= 0 && i < tareas.Length && tareas[i].completada;
    }

    private string SiguienteTarea()
    {
        foreach (Tarea t in tareas)
            if (!t.completada) return "\n\nSiguiente: " + t.nombre;
        return "";
    }

    private void ActualizarTablero()
    {
        if (textoTablero != null)
        {
            string texto = "<b>TAREAS DEL DÍA</b>\n\n";
            for (int i = 0; i < tareas.Length; i++)
            {
                if (tareas[i].completada)
                    texto += "<color=#4E8A3A><s>" + (i + 1) + ". " + tareas[i].nombre + "</s>  ¡Hecho!</color>\n";
                else
                    texto += (i + 1) + ". " + tareas[i].nombre + "\n";
            }
            textoTablero.text = texto;
        }

        if (textoMonedas != null)
            textoMonedas.text = "Monedas: " + MonedasGanadas;
    }

    private void Sonar(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    // ---------- Ciclo del dia ----------

    private void AplicarMomento(int m)
    {
        if (sol != null && m < coloresSol.Length)
        {
            sol.color = coloresSol[m];
            sol.intensity = intensidadesSol[m];
            sol.transform.rotation = Quaternion.Euler(alturasSol[m], giroSolY, 0f);
        }
        if (camara != null && m < coloresCielo.Length)
            camara.backgroundColor = coloresCielo[m];
    }

    private IEnumerator TransicionMomento(int m)
    {
        if (m >= coloresSol.Length) yield break;

        Color solInicio = sol != null ? sol.color : Color.white;
        float intensidadInicio = sol != null ? sol.intensity : 1f;
        float alturaInicio = sol != null ? sol.transform.eulerAngles.x : 0f;
        Color cieloInicio = camara != null ? camara.backgroundColor : Color.white;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionTransicion;
            if (sol != null)
            {
                sol.color = Color.Lerp(solInicio, coloresSol[m], t);
                sol.intensity = Mathf.Lerp(intensidadInicio, intensidadesSol[m], t);
                float altura = Mathf.LerpAngle(alturaInicio, alturasSol[m], t);
                sol.transform.rotation = Quaternion.Euler(altura, giroSolY, 0f);
            }
            if (camara != null)
                camara.backgroundColor = Color.Lerp(cieloInicio, coloresCielo[m], t);
            yield return null;
        }
        AplicarMomento(m);
    }
}
