using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

/// <summary>
/// Controla todos los efectos visuales de pantalla del juego.
/// Gestiona volúmenes de post-procesamiento, materiales de pantalla completa y niebla basada en el estado de la criatura y el jugador.
/// </summary>
public class ScreenEffectsController : MonoBehaviour
{
    // Referencias a otros controladores principales del juego
    CreatureController creatureController;
    PlayerStatus playerStatus;
    PlayerStatusData playerStatusData;  

    public float transitionSpeed;

    [Header("Stalking Static Config")]
    public float stalkingSpeedLookAt = 5f; // Velocidad del ruido si el jugador mira a la criatura
    public float stalkingSpeedLookAway = 2f; // Velocidad del ruido si el jugador le da la espalda

    [Header("Global Volume Profiles")]
    // Cada volumen contiene un perfil de post-procesamiento diferente para cada estado del juego
    public Volume volumeBase; // Estado neutral/seguro
    public Volume volumeForHunting; // Cuando la criatura está cazando
    public Volume volumeForAttacking; // Cuando la criatura ataca
    public Volume volumeForFleeing; // Cuando la criatura huye
    public Volume volumeForAlert; // Cuando la criatura detecta al jugador
    public Volume volumeForSanityArea; // Zonas seguras que restauran cordura
    public Volume volumeForDeath; // Estado de muerte
    public Volume volumeForStalking; // Cuando la criatura acecha al jugador

    List<Volume> allVolumes = new List<Volume>(); // Lista de referencia de todos los volúmenes para facilitar gestión 
    [Header("URP Renderer features materials")]
    // Materiales para estados de la criatura (afectan todo el post-procesamiento de pantalla)
    public Material matForHunting;
    public Material matForAttacking;
    public Material matForFleeing;
    public Material matForAlert;

    // Materiales para estados del jugador (salud baja, oxígeno bajo)
    public Material matForLowHealth;
    public Material matForLowOxygen;

    // Materiales para efectos de áreas especiales (zonas de daño y seguridad)
    public Material matForPoisonArea;
    public Material matForAsphyxiationArea;
    public Material matForMadnessArea;
    public Material matForSanityArea;
    public Material matForScreenNoise; // Ruido de pantalla que cambia con la intensidad de amenaza

    [Header("Fog & static Config")]
    // Configuración de niebla dinámica que aumenta cuando la criatura se acerca
    public float maxFog = 0.1f; // Máxima densidad de niebla (cuando la criatura está cerca)
    public float minFog = 0.5f; // Mínima densidad de niebla (cuando la criatura está lejos)
    public float maxStatic = 0.1f; // Máxima intensidad de ruido visual
    public float minStatic = 0.5f; // Mínima intensidad de ruido visual
    public float maxDistanceFromPlayer = 10; // Distancia en la que se activan efectos máximos
    public float minDistanceFromPlayer = 2; // Distancia en la que comienzan los efectos visuales


    

    
    // Referencias a los Renderer Features del pipeline URP (obtenidas por reflexión en Awake)
    private FullScreenPassRendererFeature PlayerStatsRendererRef; // Efectos de salud/oxígeno
    private FullScreenPassRendererFeature AreaEffectsRendererRef; // Efectos de áreas de daño
    private FullScreenPassRendererFeature CreatureRendererRef; // Efectos relacionados con la criatura
    private FullScreenPassRendererFeature ScreenNoiseRendererRef; // Ruido dinámico de pantalla

    // Variables de control de estado
    bool canSwitchVolume = true; // Controla si se puede cambiar de volumen (evita cambios simultáneos)
    bool isInDanger = false; // Flag para saber si estamos en peligro (acechanza)
    Volume currentCreatureVolume; // Referencia al volumen activo de la criatura
    Volume currentAreaVolume; // Referencia al volumen activo del área

    #region MONOBEHAVIOUR

    private void Awake() {
        // Obtiene las referencias a los Renderer Features usando reflexión
        GetFullScreenFeatures();
        // Busca los controladores principales en la escena
        creatureController = GameObject.FindFirstObjectByType<CreatureController>();
        playerStatus = GameObject.FindFirstObjectByType<PlayerStatus>();      
        playerStatusData = playerStatus.playerStatusData;    
        
    }

    void Start()
    {
        // Agrega todos los volúmenes a la lista para gestión centralizada
        allVolumes.AddRange(new[] {volumeBase, volumeForHunting, volumeForAttacking, volumeForFleeing, volumeForAlert, volumeForSanityArea, volumeForDeath});
        
        // Desactiva todos los volúmenes inicialmente
        foreach (Volume n in allVolumes)
        {
            n.enabled = false;
        }
        
        // Activa el volumen base (estado neutral)
        allVolumes[0].enabled = true;
        currentCreatureVolume = null;
        currentAreaVolume = null;

        // Inicializa el renderer de ruido de pantalla
        ScreenNoiseRendererRef.SetActive(true);
        ScreenNoiseRendererRef.passMaterial = matForScreenNoise;
        ScreenNoiseRendererRef.SetActive(true);

        UpdateScreenStatic();
        matForScreenNoise.SetFloat("_StalkingSpeed", 0.2f);
    }

    private void LateUpdate() {
        // Actualiza la niebla y el ruido cada frame
        UpdateFog();
        UpdateScreenStatic();

        // Si la criatura está acechando, actualiza el ruido según a dónde mire el jugador
        if (creatureController.currentState == CreatureController.CreatureState.Stalking)
        {
            UpdateStalkingNoise();
        }
        else matForScreenNoise.SetFloat("_StalkingSpeed", 0.2f);

        
        
    }
    


    



    #region SUSCRIPCION EVENTOS
    // Se ejecuta cuando el script se habilita - Se suscribe a todos los eventos relevantes
    private void OnEnable() {
        // Eventos de estado de la criatura
        creatureController.OnAlert.AddListener(EffectsForAlert);
        creatureController.OnAttacking.AddListener(EffectsForAttacking);
        creatureController.OnFleeing.AddListener(EffectsForFleeing);
        creatureController.OnHunting.AddListener(EffectsForHunting);
        creatureController.OnStalking.AddListener(EffectsForStalking);
        creatureController.OnIdleOrPatrolling.AddListener(EffectsForOnIdleOrPatrolling);

        // Evento de salud del jugador
        playerStatus.OnHealthChanged.AddListener(EffectsForHealt);

        // Eventos de áreas especiales (daño, oxígeno, cordura)
        DamageZone.OnHealthDamageArea.AddListener(EffectsForPoisonArea);
        DamageZone.OnOxygenLossArea.AddListener(EffectsForPoisonArea);
        DamageZone.OnMadnessGainArea.AddListener(EffectsForPoisonArea);
        DamageZone.OnSanityGainArea.AddListener(EffectsForSafeArea);
        DamageZone.OnOutOfArea.AddListener(EffectsForOutOfArea);
    }

    // Se ejecuta cuando el script se desactiva - Desuscribe todos los listeners
    private void OnDisable() {
        // Desuscribe eventos de la criatura
        creatureController.OnAlert.RemoveListener(EffectsForAlert);
        creatureController.OnAttacking.RemoveListener(EffectsForAttacking);
        creatureController.OnFleeing.RemoveListener(EffectsForFleeing);
        creatureController.OnHunting.RemoveListener(EffectsForHunting);
        creatureController.OnStalking.RemoveListener(EffectsForStalking);
        creatureController.OnIdleOrPatrolling.RemoveListener(EffectsForOnIdleOrPatrolling);
        
        // Desuscribe evento de salud
        playerStatus.OnHealthChanged.RemoveListener(EffectsForHealt);

        // Desuscribe eventos de áreas
        DamageZone.OnHealthDamageArea.RemoveListener(EffectsForPoisonArea);
        DamageZone.OnOxygenLossArea.RemoveListener(EffectsForPoisonArea);
        DamageZone.OnMadnessGainArea.RemoveListener(EffectsForPoisonArea);
        DamageZone.OnSanityGainArea.RemoveListener(EffectsForSafeArea);
        DamageZone.OnOutOfArea.RemoveListener(EffectsForOutOfArea);
    }
    #endregion
    #endregion


    // Actualiza la densidad de niebla basada en la distancia a la criatura
    private void UpdateFog()
    {
        if (creatureController == null) return;

        float result;

        // Caso especial: Si la criatura está acechando, máxima niebla para crear tensión
        if (creatureController.currentState == CreatureController.CreatureState.Stalking)
        {
            result = maxFog; // Máxima densidad
        }
        else
        {
            // Mapea la distancia a un valor de niebla interpolado
            // InverseLerp convierte la distancia en un valor 0-1
            float t = Mathf.InverseLerp(minDistanceFromPlayer, maxDistanceFromPlayer, creatureController.distanceToPlayer);
            // Lerp interpola entre máxima niebla (cerca) y mínima niebla (lejos)
            result = Mathf.Lerp(maxFog, minFog, t);
        }

        RenderSettings.fogDensity = result;
        RenderSettings.fog = true;
    }

    // Actualiza el ruido visual de pantalla basado en la proximidad de la criatura
    private void UpdateScreenStatic()
    {
        // Convierte la distancia en un valor normalizado (0-1)
        float t = Mathf.InverseLerp(minDistanceFromPlayer, maxDistanceFromPlayer, creatureController.distanceToPlayer);        
        
        // Ajusta parámetros del material de ruido según la distancia
        ScreenNoiseRendererRef.passMaterial.SetFloat("_opacity", Mathf.Lerp(1f, 0.9f, t )); // Opacidad del ruido
        ScreenNoiseRendererRef.passMaterial.SetFloat("_spreadOnScreen", Mathf.Lerp(2f, 4f, t )); // Dispersión del ruido
    }

    // Actualiza el ruido de pantalla durante acechanza según la dirección de visión del jugador
    private void UpdateStalkingNoise()
    {
        // Obtiene la dirección hacia donde la cámara está mirando
        Vector3 cameraForward = Camera.main.transform.forward;
        
        // Calcula la dirección del jugador hacia la criatura
        Vector3 dirToCreature = (creatureController.transform.position - Camera.main.transform.position).normalized;

        // Calcula cuánto está mirando el jugador hacia la criatura
        // Resultado: 1 = mirando directo, 0 = perpendicular, -1 = mirando hacia atrás
        float lookAlignment = Vector3.Dot(cameraForward, dirToCreature);

        Debug.Log("Aligment to creature is " + lookAlignment);

        if(lookAlignment > 0.7){
            matForScreenNoise.SetFloat("_StalkingSpeed", 4f);
            ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 25f); 
        }
        else if(lookAlignment < -0.6)
        {
            matForScreenNoise.SetFloat("_StalkingSpeed", 0.5f);
            ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 10f); 
        }
        
         

        else
        {
            matForScreenNoise.SetFloat("_StalkingSpeed", 2f);
            ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 15f); 
        }
/*
        // Mapea el alineamiento a velocidad de ruido
        // Max(0, lookAlignment) hace que solo "mirando hacia adelante" aumente velocidad
        float targetSpeed = Mathf.Lerp(stalkingSpeedLookAway, stalkingSpeedLookAt, Mathf.Max(0, lookAlignment));

        // Suaviza la transición para evitar saltos visuales bruscos
        float currentSpeed = matForScreenNoise.GetFloat("_StalkingSpeed");
        float smoothedSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * transitionSpeed);
        
        matForScreenNoise.SetFloat("_StalkingSpeed", smoothedSpeed);*/
    }

#region EFECTOS ESTADO DEL JUGADOR
    // Aplica efectos visuales cuando la salud del jugador cambia
    public void EffectsForHealt(float newHealth, float maxHealth)
    {
        // Si la salud es baja, muestra efecto de salud crítica
        if(newHealth < playerStatusData.healthLowTreeshold) 
            UpdateMaterial(matForLowHealth, PlayerStatsRendererRef);
        // Si la salud se recupera, desactiva el efecto
        else if (PlayerStatsRendererRef.passMaterial == matForLowHealth) 
            UpdateMaterial(null, PlayerStatsRendererRef);
    }
#endregion

#region EFECTOS CREATURA
    // Aplica efectos visuales cuando la criatura comienza a cazar
    public void EffectsForHunting()
    {
        UpdateVolume(volumeForHunting);
        ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 15f); // Ruido moderado
    }

    // Aplica efectos intensos cuando la criatura ataca
    public void EffectsForAttacking()
    {
        UpdateVolume(volumeForAttacking);
        ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 25f); // Ruido muy intenso
    }

    // Aplica efectos cuando la criatura detecta al jugador
    public void EffectsForAlert()
    {
        UpdateVolume(volumeForAlert);
        ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 15f);        
    }

    // Aplica efectos cuando la criatura huye
    public void EffectsForFleeing()
    {
        UpdateVolume(volumeForFleeing);
        ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 25f); // Ruido intenso
    }

    // Aplica efectos especiales cuando la criatura está acechando al jugador
    public void EffectsForStalking()
    {       
        UpdateVolume(volumeForStalking);
        isInDanger = true; // Activa la bandera de peligro para UpdateStalkingNoise
    }

    // Desactiva los efectos de peligro cuando la criatura está ociosa o patrullando
    public void EffectsForOnIdleOrPatrolling()
    {
        UpdateVolume(null); // Vuelve al volumen base
        UpdateMaterial(null, CreatureRendererRef); // Desactiva efectos visuales
        isInDanger = false; // Desactiva la bandera de peligro
        ScreenNoiseRendererRef.passMaterial.SetFloat("_speed", 2f); // Ruido mínimo
    }
#endregion

#region EFECTOS AREAS DE ESTADO
    // Aplica efectos visuales cuando el jugador entra en un área de daño
    public void EffectsForPoisonArea()
    {
        UpdateMaterial(matForPoisonArea, AreaEffectsRendererRef);
    }

    // Aplica efectos relajantes en áreas seguras que restauran cordura
    public void EffectsForSafeArea()
    {
        UpdateVolume(volumeForSanityArea);
    }

    // Desactiva efectos de área cuando el jugador sale de ellas
    public void EffectsForOutOfArea()
    {
        Debug.Log("Got Out of Area");
        UpdateMaterial(null, AreaEffectsRendererRef); // Desactiva efectos de área
        // Si estábamos en un área de cordura, vuelve al volumen normal
        if (currentCreatureVolume == volumeForSanityArea) 
            UpdateVolume(null);
    }
#endregion
    

#region UPDATE VISUALS UTILITIES
    // Cambia suavemente el volumen activo con una transición interpolada
    private void UpdateVolume(Volume volumeToApply)
    {
        // Si no hay volumen a aplicar, desactivamos el actual
        if (volumeToApply == null)
        {
            if(currentCreatureVolume == null) return; // Ya está desactivado
            StartCoroutine(SwitchVolumes(transitionSpeed, volumeToApply));    
            return;
        }
        // Si es un volumen diferente al actual, inicia la transición
        else if (volumeToApply != currentCreatureVolume && canSwitchVolume)
        {
            StartCoroutine(SwitchVolumes(transitionSpeed, volumeToApply));         
        }
        else Debug.Log("No se pudo actualizar el volumen");
    }

    // Activa o desactiva un material en un Renderer Feature específico
    private void UpdateMaterial(Material matToApply, FullScreenPassRendererFeature RendererRefToUse)
    {
        if(matToApply != null)
        {
            // Activa el renderer feature y asigna el material
            RendererRefToUse.SetActive(true);
            RendererRefToUse.passMaterial = matToApply;
            RendererRefToUse.SetActive(true);
            print("SWITCH TO " + matToApply + " MATERIAL WAS SUCESSFULL");
        }
        else 
        {
            // Desactiva el material
            RendererRefToUse.passMaterial = null;
        }
    }    

    // Corrutina que interpola suavemente entre volúmenes
    private IEnumerator SwitchVolumes(float speed, Volume newVolume)
    {
        if (newVolume != null)
        {
            // Activa el nuevo volumen desde 0 peso
            newVolume.enabled = true;
            newVolume.weight = 0f;
            
            // Aumenta el peso del nuevo volumen mientras disminuye el antiguo
            while (newVolume.weight < 1)
            {
                newVolume.weight += Time.deltaTime * speed;
                if(currentCreatureVolume != null) 
                    currentCreatureVolume.weight -= Time.deltaTime * speed;
                yield return null;
            }
            
            // Asegura que el nuevo volumen está a peso máximo
            newVolume.weight = 1;

            // Desactiva el volumen anterior
            if(currentCreatureVolume != null)
            {
                currentCreatureVolume.enabled = false;
            }        
            
            currentCreatureVolume = newVolume;
            canSwitchVolume = true;
            print("SWITCH TO " + newVolume + " WAS SUCESSFULL");
        }

        else
        {
            while (currentCreatureVolume.weight > 0.01)
            {
                currentCreatureVolume.weight -= Time.deltaTime * speed;
                yield return null;
            }

            currentCreatureVolume.weight = 0;
            currentCreatureVolume = null;
        }
    }

    // Corrutina alternativa para cambiar Renderer Features (actualmente sin usar)
    private IEnumerator SwitchRenderFeature(float speed, Volume newVolume)
    {
        newVolume.enabled = true;
        newVolume.weight = 0f;
        while (newVolume.weight < 1)
        {
            newVolume.weight += Time.deltaTime * speed;
            if(currentCreatureVolume != null) 
                currentCreatureVolume.weight -= Time.deltaTime * speed;
            yield return null;
        }

        newVolume.weight = 1;
        if(currentCreatureVolume != null)
        {
            currentCreatureVolume.enabled = false;
        }        
        currentCreatureVolume = newVolume;

        canSwitchVolume = true;
        print("SWITCH TO " + newVolume + " WAS SUCESSFULL");
    }

#endregion


    // Usa reflexión para obtener referencias a los Renderer Features del pipeline URP
    // Esto es necesario porque los Features no se exponen directamente en el editor
    public void GetFullScreenFeatures()
    {
        // 1. Obtiene el Asset de URP actual (el que está activo en el proyecto)
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null) return; // Salida segura si no hay URP

        // 2. Obtiene el ScriptableRenderer (el que está en uso, índice 0 es el renderer principal)
        var renderer = urpAsset.GetRenderer(0);

        // 3. Usa reflexión para acceder a la lista privada de features del renderer
        // BindingFlags.NonPublic | BindingFlags.Instance permite acceder a propiedades privadas
        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        var features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

        if (features != null)
        {
            // 4. Itera sobre los features buscando los específicos por nombre
            // OfType<FullScreenPassRendererFeature>() filtra solo los Full Screen Pass features
            // FirstOrDefault() retorna el primero que coincida con el nombre o null si no encuentra
            
            PlayerStatsRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "PlayerStatsRendererRef");

            AreaEffectsRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "AreaEffectsRendererRef");

            CreatureRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "CreatureRendererRef");

            ScreenNoiseRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "ScreenNoiseRenererRef");

            // Verifica que todas las referencias se encontraron correctamente
            VerifyFeature(PlayerStatsRendererRef, "PlayerStatsRendererRef");
            VerifyFeature(AreaEffectsRendererRef, "AreaEffectsRendererRef");
            VerifyFeature(CreatureRendererRef, "CreatureRendererRef");
            VerifyFeature(ScreenNoiseRendererRef, "ScreenNoiseRenererRef");
        }
    }

    // Verifica que un feature se encontró correctamente y lo desactiva inicialmente
    private void VerifyFeature(FullScreenPassRendererFeature feature, string name)
    {
        if (feature == null) 
        {
            // Log de error si no se encuentra el feature
            Debug.LogError($"[ScreenEffects] ¡Cuidado! No se encontró el Feature: {name} en el Renderer Data.");
        }
        else 
        {
            // Comienza con todos los features desactivados (se activarán cuando sea necesario)
            feature.SetActive(false);
        }
    }
}
