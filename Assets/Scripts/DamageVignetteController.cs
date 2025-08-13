using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DamageVignetteController : MonoBehaviour
{
    [Header("UI")]
    public Image vignetteImage;

    [Header("Settings")]
    public float fadeInDuration = 0.15f;
    public float sustainDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    public bool useUnscaledTime = true;

    [Header("Intensity")]
    [Range(0f, 1f)] public float minIntensity = 0.2f;
    [Range(0f, 1f)] public float maxIntensity = 0.8f;

    public float minDamageThreshold = 5f;
    public float maxDamageThreshold = 50f;

    [Header("Curves")]
    public AnimationCurve damageIntensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Accumulation")]
    public float accumulationWindow = 0.5f;

    private float currentAlpha = 0f;
    private float accumulatedDamage = 0f;
    private float lastDamageTime;
    private Coroutine vignetteRoutine;

    

    void Awake()
    {
        if (vignetteImage == null) vignetteImage = GetComponent<Image>();
        
        var c = vignetteImage.color;
        c.a = 0f;
        vignetteImage.color = c;
    }
   
    public void OnDamaged(float amount, Vector3 hitPos, bool isShield) 
    {
       
        ShowDamage(amount, maxDamageThreshold); 
    }


    public void ShowDamage(float damage, float maxDamage)
    {
        // Clamp: if the damage is less than minDamageThreshold, set it to min; 
        // if it's greater than maxDamageThreshold, set it to max.
        // InverseLerp: converts the damage value between the two thresholds into a value between 0 and 1.

        float clamped = Mathf.Clamp(damage, minDamageThreshold, maxDamageThreshold);
        float damage01 = Mathf.InverseLerp(minDamageThreshold, maxDamageThreshold, clamped);

        float curved = Mathf.Clamp01(damageIntensityCurve.Evaluate(damage01));

        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, curved);

        /////////
        if (Time.time - lastDamageTime <= accumulationWindow)
            accumulatedDamage += targetIntensity;
        else
            accumulatedDamage = targetIntensity;

        accumulatedDamage = Mathf.Clamp(accumulatedDamage, minIntensity, maxIntensity);
        lastDamageTime = Time.time;

       
        if(vignetteRoutine != null) StopCoroutine(vignetteRoutine);
        vignetteRoutine = StartCoroutine(VignetteSequence(accumulatedDamage));

    }
    private System.Collections.IEnumerator VignetteSequence(float peakIntensity)
    { //fadein
        float t = 0;

        while (t < fadeInDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeInDuration);
            float k = Mathf.Clamp01(fadeInCurve.Evaluate(u));
            currentAlpha = Mathf.Lerp(0f, peakIntensity, k);
            SetAlpha(currentAlpha);
            yield return null;

        }
        //sustain
        float sustainT = 0f;
        while (sustainT < sustainDuration)
        {
            sustainT += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetAlpha(peakIntensity);
            yield return null;
        }

        //fadee out
        t = 0;
        while(t< fadeOutDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeOutDuration);
            float k = Mathf.Clamp01(fadeOutCurve.Evaluate(u));
            currentAlpha = Mathf.Lerp(peakIntensity, 0f, k);
            SetAlpha(currentAlpha);
            yield return null;
        }
        SetAlpha(0);
    }

    private void SetAlpha(float alpha)
    {
        if(vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = alpha;
            vignetteImage.color = c;
        }
    }

}
