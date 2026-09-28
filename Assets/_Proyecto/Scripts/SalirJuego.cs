using UnityEngine;

public class SalirJuego : MonoBehaviour
{
    
    public void Salir()
    {
        Application.Quit();

#if UNITY_EDITOR
       
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
