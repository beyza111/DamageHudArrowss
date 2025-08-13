using UnityEngine;

public class DamageArrowUI : MonoBehaviour
{
    public float fadeInDuration = 0.08f;
    public float sustainDuration = 0.35f;
    public float fadeOutDuration = 0.6f;

    private enum State { Idle,FadeIn, Sustain,FadeOut}
    private State state = State.Idle;
    private float t;
    private CanvasGroup cg;

    void Awake()
    {
        cg=GetComponent<CanvasGroup>();
        cg.alpha = 0f;
    }

    public void RestartCycle()
    {
        state = State.FadeIn;
        t = 0f;
        cg.alpha = 0f;
    }

    void Update()
    {
        switch (state)
        {
            case State.FadeIn:
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(t / Mathf.Max(0.01f, fadeInDuration));
                if(cg.alpha >= 1f) { state = State.Sustain; t = 0f; }
                break;

            case State.Sustain:
                t += Time.unscaledDeltaTime;
                cg.alpha = 1f;
                if(t>=sustainDuration) { state = State.FadeOut; t = 0f; }
                break;

            case State.FadeOut:
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, fadeOutDuration));
                cg.alpha = 1f - k;
                if (cg.alpha <= 0f) state = State.Idle;
                break;

            case State.Idle:
                break;
        }
    }
    public bool IsHidden()=> state == State.Idle && cg.alpha <= 0.001f;
}
