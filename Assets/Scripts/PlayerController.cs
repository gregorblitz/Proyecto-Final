using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class PlayerController : MonoBehaviour
{
    public InputActionAsset inputActions;
    //acciones
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crawlAction;

    private Vector2 moveAnim;
    private Vector2 lookAnim;
    private Animator animator;
    private Rigidbody rigidbody;
    private CapsuleCollider capsuleCollider; // Para saber la altura del jugador
     //***Constantes***
    
    [Header("Configuración de movimiento")]
    public float walkSpeed = 5;
    public float rotateSpeed = 150;
    public float jumpSpeed = 5;

    // Configuracion de la cápsula al gatear
    [Header("Configuracion de gateo")]
    public float crawlHeight = 1f; // Altura al gatear
    // Variables para la memoria del tamaño
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrawling = false;

    private void Awake()
    {
        // ***Para resolver el problema del giro fantansma**
        // Busca acciones de la variable publica inputActions
        // Primero aseguramos que estamos en el mapa "Player"
        var playerMap = inputActions.FindActionMap("Player");
        //
        //***Se lee lo del mapa local en lugar del global
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        jumpAction = playerMap.FindAction("Jump");
        crawlAction = playerMap.FindAction("Crawl");

        // Guarda referencia del Rigidbody 
        rigidbody = GetComponent<Rigidbody>();
        //guarda referencia animator
        animator = GetComponentInChildren<Animator>();

        // Guarda el collider para medir la distancia al suelo
        // para evitar problema del salto
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Memoriza las medidas exactas del collider del jugador antes de empezar
        if (capsuleCollider != null)
        {
            originalHeight = capsuleCollider.height;
            originalCenter = capsuleCollider.center;
        }

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
        // Lee el movimiento W, A, S, D
        moveAnim = moveAction.ReadValue<Vector2>();
        // isCrawling es true si se oprime el botón
        isCrawling = crawlAction.IsPressed();

        // Modificacion fisica de la capsula si esta gateando
        if (isCrawling)
        {
            capsuleCollider.height = crawlHeight;
            // Bajamos el centro a la mitad de la nueva altura para que no flote
            float offset = (originalHeight - crawlHeight) / 2f;
            capsuleCollider.center = new Vector3(originalCenter.x, originalCenter.y - offset, originalCenter.z);
        }
        else
        {
            capsuleCollider.height = originalHeight; // vuelve al tamaño normal
            capsuleCollider.center = originalCenter; // vuelve al centro normal
        }

        // Le pasa estado final al Animator
        animator.SetBool("isCrawling", isCrawling);

        //**Solo salta si se oprime el boton y si esta tocando el suelo y si no esta gateando
        if (jumpAction.WasPressedThisFrame() && IsGrounded() && !isCrawling)
        {
            Jump();
        }
    }
    
    // Verifica toca el suelo
    private bool IsGrounded()
    {
        // Calcula distancia desde el centro del personaje (.extents) hasta sus pies
        float distanceToGround = capsuleCollider.bounds.extents.y;
        
        // Dispara un rayo invisible hacia abajo (posicion, hacia donde apunta, distancia) 
        // Le suma 0.1f como margen de error para detectar el suelo correctamente.
        return Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.1f);
    }

    public void Jump()
    {

        // Resetea la velocidad en Y antes de saltar para evitar acumulacion de saltos
        rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x, 0, rigidbody.linearVelocity.z);

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

         //Guarda el input original (que presiona el jugador en W o S)
        float inputY = moveAnim.y;

        // Calcula la velocidad
        float currentSpeed = walkSpeed;

        //Si gatea, forzar el valor a 1 (como si presionara la W)
        if (isCrawling)
        {
           // Si está agachado (ya sea por el botón o por el techo), camina a la mitad de velocidad
            currentSpeed = walkSpeed * 0.5f;
        }
        //Aplica la animación y el movimiento con las nuevas variables 
        animator.SetFloat("Speed", inputY);
        rigidbody.MovePosition(rigidbody.position + transform.forward * inputY * currentSpeed * Time.deltaTime);
    }

    private void Rotating()
    {
        // Solo gira si el valor de movimiento es mayor a 0.05 para evitar el giro fantasma
        if (Mathf.Abs(moveAnim.x) > 0.05f)
        {
            float rotationAmount = moveAnim.x * rotateSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
            rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
        }

        else 
        {
            // Freno de emergencia para rotacion
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
