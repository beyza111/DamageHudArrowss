using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DamageHUDArrowsController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform forwardReference; // Main Camera
    public RectTransform crosshairAnchor;
    public GameObject arrowPrefab;

    [Header("Layout")]
    public float damageArrowRadius = 120f;
    [Range(0f, 90f)] public float damageArrowTolerance = 20f;

    [Header("Detection")]
    [Range(0f, 90f)] public float verticalAngleThreshold = 30f; // <=30° from up (or >=150°) counts as vertical

    [Header("Fix Options")]
    public bool invertFrontBack = false;
    public bool invertLeftRight = false;

    [Header("Behavior")]
    public bool showOnShield = true;

    [Header("Intensity Settings")]
    [Range(0f, 1f)] public float minIntensity = 0.2f;
    [Range(0f, 1f)] public float maxIntensity = 0.9f;
    public float minDamageThreshold = 5f;
    public float maxDamageThreshold = 50f;
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private class ArrowEntry
    {
        public GameObject arrow;
        public Vector3 dirFlat;
        public bool vertical;
        public bool verticalUp;  // true = up, false = down
        public float intensity;  // 0..1 mapped to visual scale/alpha
    }

    private readonly List<ArrowEntry> activeArrows = new();

    void Awake()
    {
        if (forwardReference == null && Camera.main != null)
            forwardReference = Camera.main.transform;
    }

    // Hook this to your damage relay
    public void OnDamaged(float amount, Vector3 hitPos, bool isShield)
    {
        if (!showOnShield && isShield) return;
        ReportDamage(hitPos, amount);
    }

    public void ReportDamage(Vector3 worldHitPosition, float amount)
    {
        
        float normalizedDamage = Mathf.InverseLerp(minDamageThreshold, maxDamageThreshold, amount);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, intensityCurve.Evaluate(normalizedDamage));

        // 1) Direction from player to damage source
        Vector3 toSource = worldHitPosition - player.position;
        if (toSource.sqrMagnitude < 0.000001f) return; // If the damage source is at the same position as the player (no distance), no need to calculate direction → exit the function

        Vector3 toSourceNorm = toSource.normalized;

        
        Vector3 flatDir = Vector3.ProjectOnPlane(toSourceNorm, Vector3.up).normalized;

        // Vertical detection via angle to up vector:
        float angleToUp = Vector3.Angle(toSourceNorm, Vector3.up);
        bool isVertical = (angleToUp <= verticalAngleThreshold) || (angleToUp >= 180f - verticalAngleThreshold);

        if (isVertical)
        {
            // Use 0° for up, 180° for down on the HUD ring
            bool fromAbove = (angleToUp <= verticalAngleThreshold);
            float vAngle = fromAbove ? 0f : 180f;

            ArrowEntry target = null;
            foreach (var a in activeArrows)
            {
                float aAngle = a.vertical ? (a.verticalUp ? 0f : 180f)
                                          : ComputeAngleFromDir(a.dirFlat);
                float delta = Mathf.Abs(Mathf.DeltaAngle(aAngle, vAngle));
                if (delta <= damageArrowTolerance)
                {
                    target = a;
                    break;
                }
            }

            if (target == null)
            {
                GameObject newArrow = Instantiate(arrowPrefab, crosshairAnchor);
                target = new ArrowEntry
                {
                    arrow = newArrow,
                    dirFlat = Vector3.zero,
                    vertical = true,
                    verticalUp = fromAbove,
                    intensity = targetIntensity
                };
                activeArrows.Add(target);
            }
            else
            {
                target.dirFlat = Vector3.zero;
                target.vertical = true;
                target.verticalUp = fromAbove;
                target.intensity = Mathf.Max(target.intensity, targetIntensity); // keep peak while effect lasts
            }

            ApplyIntensity(target);
            target.arrow.GetComponent<DamageArrowUI>()?.RestartCycle();
            PlaceArrow((RectTransform)target.arrow.transform, vAngle);
            return;
        }

        // 2) Camera axes
        Vector3 fwd = Vector3.ProjectOnPlane(forwardReference.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
        if (invertFrontBack) fwd = -fwd;
        if (invertLeftRight) right = -right;

        // 3) Calculate horizontal angle
        float forwardDot = Vector3.Dot(flatDir, fwd);
        float rightDot = Vector3.Dot(flatDir, right);
        float angle = Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 4) Tolerance check
        ArrowEntry targetHorizontal = null;
        foreach (var a in activeArrows)
        {
            float aAngle = a.vertical ? (a.verticalUp ? 0f : 180f)
                                      : ComputeAngleFromDir(a.dirFlat);
            float delta = Mathf.Abs(Mathf.DeltaAngle(aAngle, angle));
            if (delta <= damageArrowTolerance)
            {
                targetHorizontal = a;
                break;
            }
        }

        // 5) Create or update arrow
        if (targetHorizontal == null)
        {
            GameObject newArrow = Instantiate(arrowPrefab, crosshairAnchor);
            targetHorizontal = new ArrowEntry
            {
                arrow = newArrow,
                dirFlat = flatDir,
                vertical = false,
                verticalUp = false,
                intensity = targetIntensity
            };
            activeArrows.Add(targetHorizontal);
        }
        else
        {
            targetHorizontal.dirFlat = flatDir;
            targetHorizontal.vertical = false;
            targetHorizontal.verticalUp = false;
            targetHorizontal.intensity = Mathf.Max(targetHorizontal.intensity, targetIntensity);
        }

        ApplyIntensity(targetHorizontal);
        targetHorizontal.arrow.GetComponent<DamageArrowUI>()?.RestartCycle();
        PlaceArrow((RectTransform)targetHorizontal.arrow.transform, angle);
    }

    void LateUpdate()
    {
        // Remove hidden arrows
        for (int i = activeArrows.Count - 1; i >= 0; i--)
        {
            var ui = activeArrows[i].arrow.GetComponent<DamageArrowUI>();
            if (ui != null && ui.IsHidden())
            {
                Destroy(activeArrows[i].arrow);
                activeArrows.RemoveAt(i);
            }
        }

        if (activeArrows.Count == 0) return;

        // Re-align remaining arrows
        foreach (var a in activeArrows)
        {
            float angle = a.vertical ? (a.verticalUp ? 0f : 180f)
                                     : ComputeAngleFromDir(a.dirFlat);
            PlaceArrow((RectTransform)a.arrow.transform, angle);

            
            ApplyIntensity(a);
        }
    }

    // Apply visual intensity to arrow (scale + optional alpha)
    private void ApplyIntensity(ArrowEntry entry)
    {
        if (entry == null || entry.arrow == null) return;

        float s = Mathf.Lerp(0.85f, 1.35f, Mathf.Clamp01(entry.intensity));
        entry.arrow.transform.localScale = Vector3.one * s;

  
        var cg = entry.arrow.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(entry.intensity));
        }
        else
        {
            var img = entry.arrow.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(entry.intensity));
                img.color = c;
            }
        }

        
    }

    // Places the arrow on the circular HUD based on angle
    private void PlaceArrow(RectTransform rt, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 onCircle = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
        rt.anchoredPosition = onCircle * damageArrowRadius;
        rt.localEulerAngles = new Vector3(0, 0, -angle);
    }

    private float ComputeAngleFromDir(Vector3 dirFlat)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(forwardReference.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
        if (invertFrontBack) fwd = -fwd;
        if (invertLeftRight) right = -right;

        float fDot = Vector3.Dot(dirFlat, fwd);
        float rDot = Vector3.Dot(dirFlat, right);
        float angle = Mathf.Atan2(rDot, fDot) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }
}
