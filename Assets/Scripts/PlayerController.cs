using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class PlayerController : MonoBehaviour
{
    public InputActionAsset inputActions;
    //acciones
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private Vector2 moveAnim;
    private Vector2 lookAnim;
    private Animator animator;
    private Rigidbody rigidbody;
    public float walkSpeed = 5;
    public float rotateSpeed = 5;
    public float jumpSpeed = 5;

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        // Guarda referencia del Rigidbody 
        rigidbody = GetComponent<Rigidbody>();
        //guarda referencia animator
        animator = GetComponentInChildren<Animator>();

    }

    //ENCENDIDO Y APAGADO DE LOS CONTROLES
    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable(); // Activacion mapa de acciones "Player"
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable(); // Se desactiva si el jugador muere o se pausa
    }

    // LA LECTURA DE BOTONES
    private void Update()
    {
        // Lee Vector2 del Input System (X para Izquierda/Derecha, Y para Arriba/Abajo)
        moveAnim = moveAction.ReadValue<Vector2>();
        //lookAnim = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame())
        {
            Jump();
        }
    }

    public void Jump()
{
    rigidbody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
    
    animator.SetTrigger("Jump");
}

    //LA APLICACIÓN DE FÍSICAS
    private void FixedUpdate()
    {
        Walking();
        Rotating();
    }
    private void Walking()
    {
        // moveAnim.y es 1 (W) o -1 (S) o 0 (nada). 
        animator.SetFloat("Speed", moveAnim.y);
        rigidbody.MovePosition(rigidbody.position + transform.forward * moveAnim.y * walkSpeed * Time.deltaTime);
    }

    private void Rotating()
    {

        float rotationAmount = moveAnim.x * rotateSpeed * Time.fixedDeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
        rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
    }
}
