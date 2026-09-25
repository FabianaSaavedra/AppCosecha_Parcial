using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena : MonoBehaviour
{

    public void CambioEscene (int numeroEscena){

        SceneManager.LoadScene(numeroEscena);
    }
}
