// Fausto A. Gómez
// Este archivo NO es una clase, es un "contrato". 
// Cualquier objeto que quiera ser "tocado" por el jugador DEBE firmar este contrato.

using UnityEngine;

public interface IInteractable
{
    // Este método define qué pasa cuando el jugador presiona "E" mirando al objeto.
    // Recibe opcionalmente el 'itemInHand' para saber si tenemos la llave o herramienta correcta.
    void Interact(ItemData itemInHand = null);
}