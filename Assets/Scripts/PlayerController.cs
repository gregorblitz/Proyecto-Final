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
    public float walkSpeed = 5;
    public float rotateSpeed = 5;
    public float jumpSpeed = 5;

    private void Awake()
    {
        // ***Para resolver el rpoblema del giro fantansma**
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

        //**Solo salta si se oprime el boton y si esta tocando el suelo
        if (jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

        // IsPressed() es true si se presiona C, y false si se suelta.
        // Se le pasa directamente al Animator.
        animator.SetBool("isCrawling", crawlAction.IsPressed());
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

        //Se comenta para que tambien avance cuando solo presiona la letra C
        // moveAnim.y es 1 (W) o -1 (S) o 0 (nada). 
        //animator.SetFloat("Speed", moveAnim.y);
        //rigidbody.MovePosition(rigidbody.position + transform.forward * moveAnim.y * walkSpeed * Time.deltaTime);

        //Guarda el input original (que presiona el jugador en W o S)
        float inputY = moveAnim.y;
        
        //Revisa si el botón de gatear se presiono en este momento
        bool isCrawling = crawlAction.IsPressed();

        //Si gatea, forzar el valor a 1 (como si presionara la W)
        if (isCrawling)
        {
            // Si se presiona la tecla S o joystick abajo para retroceder
            if (moveAnim.y < -0.1f)
            {
                inputY = -1f; // Forzar el valor para retroceder por defecto
            }
            else
            {
                inputY = 1f; // si no toca S, avanza adelante
            }
        }

        //Reducir velocidad al gatear
        float currentSpeed = walkSpeed;
        if (isCrawling)
        {
            currentSpeed = walkSpeed * 0.5f; // Gatea a la mitad de velocidad normal
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
