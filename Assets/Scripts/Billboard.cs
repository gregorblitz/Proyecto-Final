using UnityEngine;
//Script que obliga a la barra de vida a mirar siempre a nuestra camara
public class Billboard : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        // Busca la cam principal del jugador automaticamente
        cam = Camera.main; 
    }

    private void LateUpdate()
    {
        // Obliga al canvas a girar siempre en direccion de la cam
        transform.LookAt(transform.position + cam.transform.forward);
    }
}