using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

public class ScreenEffectsController : MonoBehaviour
{

    CreatureController creatureController;
    PlayerStatus playerStatus;
    PlayerStatusData playerStatusData;  

    public float transitionSpeed;

    [Header("Global Volume Profiles")]
    public Volume volumeBase;
    public Volume volumeForHunting;
    public Volume volumeForAttacking;
    public Volume volumeForFleeing;
    public Volume volumeForAlert;
    public Volume volumeForSanityArea;
    public Volume volumeForDeath;
    public Volume volumeForStalking;

    List<Volume> allVolumes = new List<Volume>(); 
    /*
    public VolumeProfile profileBase;
    public VolumeProfile profileForHunting;
    public VolumeProfile profileForAttacking;
    public VolumeProfile profileForFleeing;
    public VolumeProfile profileForAlert;
    public VolumeProfile profileForSanityArea;
    public VolumeProfile profileForDeath;
    */

    [Header("URP Renderer features materials")]
    public Material matForHunting;
    public Material matForAttacking;
    public Material matForFleeing;
    public Material matForAlert;

    public Material matForLowHealth;
    public Material matForLowOxygen;

    public Material matForPoisonArea;
    public Material matForAsphyxiationArea;
    public Material matForMadnessArea;
    public Material matForSanityArea;
    public Material matForStalking;

    [Header("Fog Config")]

    public float maxFog = 0.1f;
    public float minFog = 0.5f;
    public float maxDistanceFromPlayer = 10;
    public float minDistanceFromPlayer = 2;

    public float currentDistance;
    public float currentFog;

    

    private FullScreenPassRendererFeature PlayerStatsRendererRef;
    private FullScreenPassRendererFeature AreaEffectsRendererRef;
    private FullScreenPassRendererFeature CreatureRendererRef;

    bool canSwitchVolume = true;
    Volume currentVolume;

#region MONOBEHAVIOUR
    private void Awake() {
        GetFullScreenFeatures();
        creatureController = GameObject.FindFirstObjectByType<CreatureController>();
        playerStatus = GameObject.FindFirstObjectByType<PlayerStatus>();      
        playerStatusData = playerStatus.playerStatusData;    
    }

    void Start()
    {
        allVolumes.AddRange(new[] {volumeBase, volumeForHunting, volumeForAttacking, volumeForFleeing, volumeForAlert, volumeForSanityArea, volumeForDeath});
        foreach (Volume n in allVolumes)
        {
            n.enabled = false;
        }
        allVolumes[0].enabled = true;
        currentVolume = null;            
    }

    private void LateUpdate() {
        UpdateFog();
    }

    #region SUSCRIPCION EVENTOS
    private void OnEnable() {
        creatureController.OnAlert.AddListener(EffectsForAlert);
        creatureController.OnAttacking.AddListener(EffectsForAttacking);
        creatureController.OnFleeing.AddListener(EffectsForFleeing);
        creatureController.OnHunting.AddListener(EffectsForHunting);
        creatureController.OnStalking.AddListener(EffectsForStalking);
        creatureController.OnIdleOrPatrolling.AddListener(EffectsForOnIdleOrPatrolling);


        playerStatus.OnHealthChanged.AddListener(EffectsForHealt);

        DamageZone.OnHealthDamageArea.AddListener(EffectsForPoisonArea);
        DamageZone.OnOxygenLossArea.AddListener(EffectsForPoisonArea);
        DamageZone.OnMadnessGainArea.AddListener(EffectsForPoisonArea);
        DamageZone.OnSanityGainArea.AddListener(EffectsForSafeArea);
        DamageZone.OnOutOfArea.AddListener(EffectsForOutOfArea);
    }

    private void OnDisable() {
        creatureController.OnAlert.RemoveListener(EffectsForAlert);
        creatureController.OnAttacking.RemoveListener(EffectsForAttacking);
        creatureController.OnFleeing.RemoveListener(EffectsForFleeing);
        creatureController.OnHunting.RemoveListener(EffectsForHunting);
        creatureController.OnStalking.RemoveListener(EffectsForStalking);
        creatureController.OnIdleOrPatrolling.RemoveListener(EffectsForOnIdleOrPatrolling);
        
        playerStatus.OnHealthChanged.RemoveListener(EffectsForHealt);

        DamageZone.OnHealthDamageArea.RemoveListener(EffectsForPoisonArea);
        DamageZone.OnOxygenLossArea.RemoveListener(EffectsForPoisonArea);
        DamageZone.OnMadnessGainArea.RemoveListener(EffectsForPoisonArea);
        DamageZone.OnSanityGainArea.RemoveListener(EffectsForSafeArea);
        DamageZone.OnOutOfArea.RemoveListener(EffectsForOutOfArea);
    }
    #endregion
#endregion

    private void UpdateFog()
    {
        if (creatureController == null) return;

        float result;

        // Si estamos en Stalking, ignoramos la distancia y forzamos niebla máxima
        if (creatureController.currentState == CreatureController.CreatureState.Stalking)
        {
            result = maxFog; // El valor más denso definido en tu Inspector
        }
        else
        {
            // Comportamiento estándar basado en distancia para otros estados
            float t = Mathf.InverseLerp(minDistanceFromPlayer, maxDistanceFromPlayer, creatureController.distanceToPlayer);
            result = Mathf.Lerp(maxFog, minFog, t);
        }

        RenderSettings.fogDensity = result;
        RenderSettings.fog = true;

        //Debug.Log("Current fog density is " + RenderSettings.fogDensity + "\nCurrent Distance to player is " + currentDistance);
    }

#region EFECTOS ESTADO DEL JUGADOR
    public void EffectsForHealt(float newHealth, float maxHealth)
    {
        if(newHealth < playerStatusData.healthLowTreeshold) UpdateMaterial(matForLowHealth, PlayerStatsRendererRef);
        else if (PlayerStatsRendererRef.passMaterial == matForLowHealth) UpdateMaterial(null, PlayerStatsRendererRef);
    }
#endregion

#region EFECTOS CREATURA
    public void EffectsForHunting()
    {
        UpdateVolume(volumeForHunting);
    }

    public void EffectsForAttacking()
    {
        UpdateVolume(volumeForAttacking);
    }
    public void EffectsForAlert()
    {
        UpdateVolume(volumeForAlert);
    }
    public void EffectsForFleeing()
    {
        UpdateVolume(volumeForFleeing);
    }
    public void EffectsForStalking()
    {
        UpdateVolume(volumeForStalking);
    }

    public void EffectsForOnIdleOrPatrolling()
    {
        UpdateVolume(null);
    }
#endregion

#region EFECTOS AREAS DE ESTADO
    public void EffectsForPoisonArea()
    {
        UpdateMaterial(matForPoisonArea, AreaEffectsRendererRef);

    }

    public void EffectsForSafeArea()
    {
        UpdateVolume(volumeForSanityArea);
    }

    public void EffectsForOutOfArea()
    {
        UpdateMaterial(null, AreaEffectsRendererRef);
        if (currentVolume == volumeForSanityArea) UpdateVolume(null);

    }
#endregion
    

#region UPDATE VISUALS UTILITIES
    private void UpdateVolume(Volume volumeToApply)
    {

        if (volumeToApply == null)
        {
            if(currentVolume == null) return;
            currentVolume.enabled = false;
            return;
        }
        if (volumeToApply != currentVolume && canSwitchVolume)
        {
            canSwitchVolume = false;
            StartCoroutine(SwitchVolumes(transitionSpeed, volumeToApply));         
        }

        else Debug.Log("No se pudo actualizar el volumen");
    }


    private void UpdateMaterial(Material matToApply, FullScreenPassRendererFeature RendererRefToUse)
    {
        RendererRefToUse.SetActive(true);
        RendererRefToUse.passMaterial = matToApply;
        RendererRefToUse.SetActive(true);
        print("SWITCH TO " + matToApply + " MATERIAL WAS SUCESSFULL");
    }    

    private IEnumerator SwitchVolumes(float speed, Volume newVolume)
    {
        newVolume.enabled = true;
        newVolume.weight = 0f;
        while (newVolume.weight < 1)
        {
            newVolume.weight += Time.deltaTime * speed;
            if(currentVolume != null) currentVolume.weight -= Time.deltaTime * speed;
            yield return null;
        }

        newVolume.weight = 1;
        if(currentVolume != null)
        {
            currentVolume.enabled = false;
        }        
        currentVolume = newVolume;

        canSwitchVolume = true;
        print("SWITCH TO " + newVolume + " WAS SUCESSFULL");
    }

    private IEnumerator SwitchRenderFeature(float speed, Volume newVolume)
    {
        newVolume.enabled = true;
        newVolume.weight = 0f;
        while (newVolume.weight < 1)
        {
            newVolume.weight += Time.deltaTime * speed;
            if(currentVolume != null) currentVolume.weight -= Time.deltaTime * speed;
            yield return null;
        }

        newVolume.weight = 1;
        if(currentVolume != null)
        {
            currentVolume.enabled = false;
        }        
        currentVolume = newVolume;

        canSwitchVolume = true;
        print("SWITCH TO " + newVolume + " WAS SUCESSFULL");
    }

#endregion


    public void GetFullScreenFeatures() // Renombrado a plural para mayor claridad
    {
        // 1. Obtener el Asset de URP actual
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null) return;

        // 2. Obtener el ScriptableRenderer (el que está en uso)
        var renderer = urpAsset.GetRenderer(0);

        // 3. Reflexión para acceder a la lista privada de features
        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        var features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

        if (features != null)
        {
            // 4. Buscamos y asignamos por nombre exacto usando OfType para filtrar solo los FullScreenPass
            PlayerStatsRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "PlayerStatsRendererRef");

            AreaEffectsRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "AreaEffectsRendererRef");

            CreatureRendererRef = features.OfType<FullScreenPassRendererFeature>()
                .FirstOrDefault(f => f.name == "CreatureRendererRef");

            // Debug para confirmar que todo se cargó bien
            VerifyFeature(PlayerStatsRendererRef, "PlayerStatsRendererRef");
            VerifyFeature(AreaEffectsRendererRef, "AreaEffectsRendererRef");
            VerifyFeature(CreatureRendererRef, "CreatureRendererRef");
        }
    }

    private void VerifyFeature(FullScreenPassRendererFeature feature, string name)
    {
        if (feature == null) 
            Debug.LogError($"[ScreenEffects] ¡Cuidado! No se encontró el Feature: {name} en el Renderer Data.");
        else 
            feature.SetActive(false); // Empezamos con todos apagados
    }



    //Suscripción a eventos creatura
    
}
