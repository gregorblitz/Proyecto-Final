using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class PlayerController : MonoBehaviour
{
    public InputActionAsset inputActions;
    //acciones
    private InputAction moveAction; //Accion para mover
    private InputAction lookAction;
    private InputAction jumpAction; //Accion para saltar
    private InputAction crawlAction;//Accion para arrastrarse 
    private InputAction runAction; // Accion para correr
    private InputAction attackAction; // Accion para atacar

    private Vector2 moveAnim;
    private Vector2 lookAnim;  // Guarda movimiento raton
    private Animator animator;
    private Rigidbody rigidbody;
    private CapsuleCollider capsuleCollider; // Para saber la altura del jugador
                                             //***Constantes***

    [Header("Variables movimiento")]
    public float walkSpeed = 5;
    public float rotateSpeed = 15; // Sensibilidad del raton
    public float jumpSpeed = 5;
    public float runSpeed = 7; // Velocidad al correr

    // Configuracion de la cápsula al gatear
    [Header("Variables gateo")]
    public float crawlHeight = 1f; // Altura al gatear

    [Header("Variables Ataque")]
    public float attackCooldown = 1.2f; // Tiempo entre cada golpe
    public float freezeDuration = 0.8f; // Tiempo de congelacion pies luego de atacar
    private float nextAttackTime = 0f;  // Reloj interno
    private float unfreezeTime = 0f;    // Reloj interno de descongelamiento
    //***COMBATE -- Se agrega para encender cuando mgolpee y apagar cuando termine el ataque
    [Header("Combate")]
    public Collider weaponCollider; // Box Collider de la pica

    // Variables para la memoria del tamaño
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrawling = false;
    private bool isRunning = false; // Variable para saber si corre
    [HideInInspector] public bool isDead = false; //congela al jugador cuando muere

    private void Awake()
    {
        // ***Para resolver el problema del giro fantansma**
        // Busca acciones de la variable publica inputActions
        // Primero aseguramos que estamos en el mapa "Player"
        var playerMap = inputActions.FindActionMap("Player");
        //
        //***Se lee lo del mapa local en lugar del global
        moveAction = playerMap.FindAction("Move");   // Busca la accion Move
        lookAction = playerMap.FindAction("Look");   // Busca la accion Look
        jumpAction = playerMap.FindAction("Jump");   // Busca la accion Jump
        crawlAction = playerMap.FindAction("Crawl"); // Busca la accion Crawl
        runAction = playerMap.FindAction("Sprint");  // Busca la accion Sprint
        attackAction = playerMap.FindAction("Attack"); // Busca accion Attack

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

        // Bloquea el raton 
        // Oculta el cursor y lo bloquea en el centro para que no se salga de la ventana al girar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
        if (isDead) return; // Si esta muerto, no lee ninguna tecla
        // Lee el movimiento W, A, S, D
        moveAnim = moveAction.ReadValue<Vector2>();

        // Lee el movimiento del raton
        lookAnim = lookAction.ReadValue<Vector2>();

        // isCrawling es true si se oprime el botón
        if (CanStand())
        {
            isCrawling = crawlAction.IsPressed();
        }
        else if (isCrawling)
        {
            isCrawling = true;
        }
        //isCrawling = crawlAction.IsPressed();
        isRunning = runAction.IsPressed(); // Lee si tiene presionado Shift

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

        // No salta si esta congelado por atacar
        bool isFrozen = Time.time < unfreezeTime;

        //**Solo salta si se oprime el boton y si esta tocando el suelo y si no esta gateando
        if (jumpAction.WasPressedThisFrame() && IsGrounded() && !isCrawling && !isFrozen)
        {
            Jump();
        }

        // ***ATAQUE***
        // Presiona el boton && paso el tiempo para siguiente ataque && no esta gateando
        if (attackAction.WasPressedThisFrame() && Time.time >= nextAttackTime && !isCrawling)
        {
            animator.SetTrigger("Attack"); // Dispara la animación
            nextAttackTime = Time.time + attackCooldown; // Reinicia reloj para siguiente ataque
            // Congela los pies hasta dentro de 0.8s
            unfreezeTime = Time.time + freezeDuration;
        }
    }

    bool CanStand() {
        // Defines the height of the ceiling check
        float checkHeight = 2.0f; 
        // Cast a ray upward from the bottom of the player
        return !Physics.Raycast(transform.position, Vector3.up, checkHeight);
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
        if (isDead) return; // Si esta muerto no aplica fisicas de movimiento
        Walking();
        Rotating();
    }
    private void Walking()
    {
        // Si el tiempo no supera al de descongelamiento se apaga el movimiento.
        if (Time.time < unfreezeTime)
        {
            animator.SetFloat("Speed", 0); // Le dice a la animacion --> piernas quietas
            return; // corta aqui y lo demas no se ejecuta
        }

        float inputY = moveAnim.y; // W y S Adelante/Atras
        float inputX = moveAnim.x; // A y D Izq/Der

        // Calcula la velocidad
        float currentSpeed = walkSpeed;

        //Si gatea, forzar el valor a 1 (como si presionara la W)
        if (isCrawling)
        {
            // Si está agachado (ya sea por el botón o por el techo), camina a la mitad de velocidad
            currentSpeed = walkSpeed * 0.5f;
        }
        // si esta corriendo y se presiona W o S
        else if (isRunning && inputY > 0)
        {
            // Solo permite correr si no esta agachado y va hacia adelante (inputY > 0)
            currentSpeed = runSpeed;

            // Fuerza el inputY a 2 para activar la animacion de correr
            inputY = 2f;
        }
        // Movimiento multidireccional
        // Combina vector frontal (Z) con vector lateral (X)
        Vector3 moveDirection = (transform.forward * inputY) + (transform.right * inputX);

        // Normaliza para no correr mas rapido al ir en diagonal (W + D mismo tiempo)
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // ANIMACION ACTUALIZADA
        float speedParam = inputY;
        if (isRunning && inputY > 0) speedParam = 2f;

        // Le envia al animator la velocidad Frontal (W/S) y lateral (A/D)
        animator.SetFloat("Speed", speedParam);
        animator.SetFloat("DirX", inputX);

        // Aplica el movimiento fisico en todas las direcciones
        rigidbody.MovePosition(rigidbody.position + moveDirection * currentSpeed * Time.deltaTime);
    }

    private void Rotating()
    {
        // los deltas del ratón pueden ser pequeños o negativos, por esto su valor absoluto
        if (Mathf.Abs(lookAnim.x) > 0.01f)
        {
            float rotationAmount = lookAnim.x * rotateSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
            rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
        }

        else
        {
            // Freno de emergencia para rotacion
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

    // --- EVENTOS DE ANIMACIÓN PARA EL DAÑO ---
    public void ActivarDaño()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            Debug.Log("Collider de la Pica ON");
        }
        else
        {
            Debug.LogWarning("El evento funciona pero falta arrastrar la pica al hueco 'Weapon Collider' en el PlayerController.");
        }
    }

    public void DesactivarDaño()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            Debug.Log("Collider de la Pica OFF.");
        }
    }
    
    // Reproducir animacion de recoger
    public void PlayPickupAnimation()
    {
        if (isDead) return; // Si esta muerto no recoge nada

        if (animator != null)
        {
            animator.SetTrigger("Pickup"); // Dispara el trigger del Animator
            Debug.Log("Animacion de recoger activada.");
        }
    }
}