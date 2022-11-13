using UnityEngine;



public class PolygonalExplosions_Light : MonoBehaviour
{
    public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Gradient LightColor = new Gradient();
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;
    public AnimationCurve LightCurveFadeOut = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float FadeOutTime = 1;
    public bool FadeOut;

    [HideInInspector] public bool canUpdate;
    private float startTime;
    Color startColor;
    private Light lightSource;
    private float fadeOutStartTime;
    private bool fadeOutStarted;


    public void SetStartColor(Color color)
    {
        startColor = color;
    }

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        startColor = lightSource.color;

        lightSource.intensity = LightCurve.Evaluate(0) * GraphIntensityMultiplier;
        lightSource.color = startColor * LightColor.Evaluate(0);

        startTime = Time.time;
        canUpdate = true;
    }

    private void OnEnable()
    {
        startTime = Time.time;
        canUpdate = true;
        if (lightSource != null)
        {
            lightSource.intensity = LightCurve.Evaluate(0) * GraphIntensityMultiplier;
            lightSource.color = startColor * LightColor.Evaluate(0);
        }
        FadeOut = false;
        fadeOutStarted = false;
    }

    private void Update()
    {
        
        var time = Time.time - startTime;
        if (canUpdate) {
            var eval = LightCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
            lightSource.intensity = eval;
            lightSource.color = startColor * LightColor.Evaluate(time / GraphTimeMultiplier);
        }
        if (time >= GraphTimeMultiplier) {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }

        if (FadeOut == true && fadeOutStarted == false)
        {
            fadeOutStarted = true;
            canUpdate = false;
            fadeOutStartTime = Time.time;
        }

        if (fadeOutStarted == true)
        {
            var fadeTime = Time.time - fadeOutStartTime;
            var evalFade = LightCurveFadeOut.Evaluate(fadeTime / FadeOutTime) * GraphIntensityMultiplier;
            lightSource.intensity = evalFade;
            // Debug.Log(fadeTime + "  " + evalFade);
        }
    }
}