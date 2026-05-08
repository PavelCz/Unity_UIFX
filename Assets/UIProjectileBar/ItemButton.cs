using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [Header("UI References")]
    public Image borderImage;
    public Button buttonComponent;

    [Header("Projectile Settings")]
    public UIProjectile projectilePrefab; 
    public Transform destinationTarget;   
    public Transform projectileRoot;
    public Slider targetSlider;
    public float sliderIncrement;
    public bool resetSliderOnClick;

    [Header("Glow Settings")]
    [ColorUsage(true, true)] 
    public Color activeGlowColor;
    [ColorUsage(true, true)] 
    public Color inactiveColor = Color.clear;
    public float fadeInDuration = 0.1f; 
    public float fadeDuration = 0.5f; 
    
    private Material borderMaterialInstance;
    private Coroutine glowCoroutine;

    void Start()
    {
        // Setup unique material instance
        borderMaterialInstance = new Material(borderImage.material);
        borderImage.material = borderMaterialInstance;
        borderMaterialInstance.SetColor("_GlowColor", inactiveColor);

        buttonComponent.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        // 1. Handle the glowing border flash
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        glowCoroutine = StartCoroutine(GlowSequence());

        if (resetSliderOnClick)
            ResetSlider();

        // 2. Fire the projectile
        if (projectilePrefab != null && destinationTarget != null)
        {
            // Spawn the projectile
            UIProjectile newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            
            if (projectileRoot != null)
                // Keep world-space VFX organized under the scene root assigned in the Inspector.
                newProjectile.transform.SetParent(projectileRoot, true); 
            
            // Launch it
            newProjectile.Fire(transform.position, destinationTarget, activeGlowColor, OnProjectileHit);
        }
    }

    private void OnProjectileHit()
    {
        IncreaseSlider();
        TriggerTargetGlow();
    }

    private void IncreaseSlider()
    {
        if (targetSlider == null || sliderIncrement <= 0f)
            return;

        EnergySliderAnimator animator = targetSlider.GetComponent<EnergySliderAnimator>();
        if (animator != null)
            animator.AddValue(sliderIncrement);
        else
            targetSlider.value = Mathf.Clamp(targetSlider.value + sliderIncrement, targetSlider.minValue, targetSlider.maxValue);
    }

    private void ResetSlider()
    {
        if (targetSlider == null)
            return;

        EnergySliderAnimator animator = targetSlider.GetComponent<EnergySliderAnimator>();
        if (animator != null)
            animator.ResetValue();
        else
            targetSlider.value = targetSlider.minValue;
    }

    private void TriggerTargetGlow()
    {
        if (destinationTarget == null)
            return;

        IconImpactGlow targetGlow = destinationTarget.GetComponent<IconImpactGlow>();
        if (targetGlow == null)
            targetGlow = destinationTarget.GetComponentInParent<IconImpactGlow>();

        if (targetGlow != null)
            targetGlow.Play(activeGlowColor);
    }

    private IEnumerator GlowSequence()
    {
        float elapsedTime = 0f;
        Color startColor = borderMaterialInstance.GetColor("_GlowColor");

        // FADE IN
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            borderMaterialInstance.SetColor("_GlowColor", Color.Lerp(startColor, activeGlowColor, t));
            yield return null; 
        }
        
        borderMaterialInstance.SetColor("_GlowColor", activeGlowColor);

        // FADE OUT
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            borderMaterialInstance.SetColor("_GlowColor", Color.Lerp(activeGlowColor, inactiveColor, t));
            yield return null; 
        }
        
        borderMaterialInstance.SetColor("_GlowColor", inactiveColor);
    }
}
