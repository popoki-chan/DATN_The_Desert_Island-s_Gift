using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AmbientDust : MonoBehaviour
{
    [Header("Cấu hình hạt bụi")]
    public Color dustColor = new Color(1f, 1f, 1f, 0.2f);
    public float minSize = 0.05f;
    public float maxSize = 0.25f;
    public float minSpeed = 0.005f; 
    public float maxSpeed = 0.025f;  
    public int maxParticles = 40;   
    public float spawnRate = 5f;

    [Header("Cấu hình vùng xuất hiện")]
    public Vector3 spawnAreaSize = new Vector3(20f, 12f, 0f); 

    [Header("Cấu hình dập dềnh (Noise)")]
    public bool enableNoise = true;
    public float noiseStrength = 0.25f;   
    public float noiseFrequency = 0.35f;    
    public float noiseScrollSpeed = 0.15f;   

    void Awake()
    {
        ConfigureParticleSystem();
    }

    void ConfigureParticleSystem()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps == null) return;


        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);


        var main = ps.main;
        main.duration = 10f;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 12f); 
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = dustColor;
        main.gravityModifier = 0f;
        main.maxParticles = maxParticles;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = spawnRate;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = spawnAreaSize;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.2f, 1f);
        sizeCurve.AddKey(0.8f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);


        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();

        float maxAlpha = dustColor.a;            
        float minAlpha = dustColor.a * 0.15f;      

        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(Color.white, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0.0f),        
                new GradientAlphaKey(maxAlpha, 0.2f),  
                new GradientAlphaKey(minAlpha, 0.4f),   
                new GradientAlphaKey(maxAlpha, 0.6f), 
                new GradientAlphaKey(minAlpha, 0.8f),   
                new GradientAlphaKey(0f, 1.0f)          
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 8;

           
            if (renderer.sharedMaterial == null || renderer.sharedMaterial.name.Contains("Default-Material"))
            {
                Material defaultMat = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
                if (defaultMat == null)
                {
                    defaultMat = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
                }
                if (defaultMat != null)
                {
                    renderer.material = defaultMat;
                }
            }
        }

        var noise = ps.noise;
        noise.enabled = enableNoise;
        if (enableNoise)
        {
            noise.strength = noiseStrength;     
            noise.frequency = noiseFrequency;   
            noise.scrollSpeed = noiseScrollSpeed; 
            noise.damping = true;
        }
        ps.Play();
    }
}
