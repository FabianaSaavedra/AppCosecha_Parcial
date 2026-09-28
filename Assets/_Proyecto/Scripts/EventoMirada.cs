using UnityEngine;
using UnityEngine.Events;

public class EventoMirada : MonoBehaviour
{
    
    public UnityEvent AlMirar;

    
    public bool soloUnaVez = true;
    private bool yaUsado = false;

    
    public void OnPointerClickXR()
    {
        if (soloUnaVez && yaUsado) return;

        yaUsado = true;
        AlMirar?.Invoke();
    }
}
