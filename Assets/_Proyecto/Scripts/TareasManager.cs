using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class TareasManager : MonoBehaviour
{
    [System.Serializable]
    public class Tarea
    {
        public string nombre;

        [Tooltip("Donde se para Pancho para explicar esta tarea (un objeto vacio en la estacion)")]
        public Transform lugarPancho;

        [Tooltip("Lo que dice Pancho en la estacion: que hay que hacer y como")]
        [TextArea(2, 4)] public string instruccion;

        [Tooltip("Lo que dice Pancho al terminar la tarea (las monedas se agregan solas)")]
        [FormerlySerializedAs("mensajePancho")]
        [TextArea(2, 4)] public string felicitacion;

        [Tooltip("Cosas que aparecen al completarla (productos, letreros...)")]
        public GameObject[] activarAlCompletar;

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
    public Transform pancho;
    public TMP_Text textoPancho;
    [TextArea(2, 4)] public string mensajeInicial = "¡Buenos días! Soy Pancho. Hoy es la feria y Don Tomás nos necesita. Mira el tablero para ver tus tareas. ¡Te espero donde los caballos!";
    [TextArea(2, 4)] public string mensajeFinal = "¡Todo listo! ¡Nos vamos a la feria!";
    [Tooltip("Segundos que Pancho se queda felicitando antes de irse a la siguiente estacion")]
    public float esperaAntesDeMover = 5f;

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
    private bool recorridoEmpezado;
    private Vector3 escalaPancho;
    private Coroutine rutinaDia;
    private Coroutine rutinaPancho;

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
        if (pancho != null) escalaPancho = pancho.localScale;

        Decir(mensajeInicial);
        ActualizarTablero();
        AplicarMomento(0);
    }

    
    public void EmpezarRecorrido()
    {
        if (recorridoEmpezado || tareas.Length == 0) return;
        recorridoEmpezado = true;

        if (rutinaPancho != null) StopCoroutine(rutinaPancho);
        rutinaPancho = StartCoroutine(MoverPancho(0f, tareas[0].lugarPancho, tareas[0].instruccion));
    }

    
    public void CompletarTarea(int numero)
    {
        int i = numero - 1;
        if (i < 0 || i >= tareas.Length) return;
        if (tareas[i].completada) return;   

        tareas[i].completada = true;
        completadas++;
        MonedasGanadas += monedasPorTarea;
        recorridoEmpezado = true;

        foreach (GameObject go in tareas[i].activarAlCompletar)
            if (go != null) go.SetActive(true);

        ActualizarTablero();
        if (rutinaDia != null) StopCoroutine(rutinaDia);
        rutinaDia = StartCoroutine(TransicionMomento(completadas));

        if (rutinaPancho != null) StopCoroutine(rutinaPancho);

        if (completadas >= tareas.Length)
        {
            // Ultima tarea: Pancho se queda y anuncia la feria
            foreach (GameObject go in activarAlTerminarTodo)
                if (go != null) go.SetActive(true);
            Decir(mensajeFinal);
            Sonar(sonidoTodoCompletado != null ? sonidoTodoCompletado : sonidoTareaCompletada);
        }
        else
        {
            // Pancho felicita, espera un momento y se va a la siguiente estacion pendiente
            Decir("<b>+" + monedasPorTarea + " monedas</b>\n" + tareas[i].felicitacion);
            Sonar(sonidoTareaCompletada);

            Tarea siguiente = SiguienteTarea();
            if (siguiente != null)
                rutinaPancho = StartCoroutine(MoverPancho(esperaAntesDeMover, siguiente.lugarPancho, siguiente.instruccion));
        }
    }

    public bool EstaCompletada(int numero)
    {
        int i = numero - 1;
        return i >= 0 && i < tareas.Length && tareas[i].completada;
    }

    private Tarea SiguienteTarea()
    {
        foreach (Tarea t in tareas)
            if (!t.completada) return t;
        return null;
    }

    private void Decir(string mensaje)
    {
        if (textoPancho != null) textoPancho.text = mensaje;
    }

    // Pancho se encoge ("¡puf!"), aparece en el nuevo lugar y crece otra vez
    private IEnumerator MoverPancho(float espera, Transform lugar, string mensaje)
    {
        if (espera > 0f) yield return new WaitForSeconds(espera);

        if (pancho == null || lugar == null)
        {
            Decir(mensaje);
            yield break;
        }

        yield return Escalar(escalaPancho, Vector3.zero, 0.25f);

        pancho.SetPositionAndRotation(lugar.position, lugar.rotation);
        Decir(mensaje);

        yield return Escalar(Vector3.zero, escalaPancho * 1.15f, 0.2f);
        yield return Escalar(escalaPancho * 1.15f, escalaPancho, 0.1f);
    }

    private IEnumerator Escalar(Vector3 desde, Vector3 hasta, float duracion)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            pancho.localScale = Vector3.Lerp(desde, hasta, t);
            yield return null;
        }
        pancho.localScale = hasta;
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