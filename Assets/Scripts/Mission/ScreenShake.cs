using UnityEngine;

/// <summary>
/// Secousse de caméra courte (validation de mission, événement fort).
/// S'applique APRÈS Cinemachine (ordre d'exécution tardif) en décalant la
/// caméra principale — le décalage est réécrit chaque frame par la brain,
/// donc aucune dérive. Singleton créé par GameState.
/// </summary>
[DefaultExecutionOrder(2000)] // après la CinemachineBrain
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private float _tempsRestant;
    private float _duree;
    private float _intensite;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[ScreenShake]");
            go.AddComponent<ScreenShake>();
        }
    }

    /// <summary>Secoue la caméra (intensité en mètres, durée en secondes).</summary>
    public static void Jouer(float intensite = 0.18f, float duree = 0.35f)
    {
        if (Instance == null) return;
        Instance._intensite    = intensite;
        Instance._duree        = duree;
        Instance._tempsRestant = duree;
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void LateUpdate()
    {
        if (_tempsRestant <= 0f) return;
        _tempsRestant -= Time.deltaTime;

        var cam = Camera.main;
        if (cam == null) return;

        // Amplitude décroissante au fil de la secousse
        float k = Mathf.Clamp01(_tempsRestant / _duree);
        Vector3 off = Random.insideUnitSphere * (_intensite * k * k);
        cam.transform.position += off;
    }
}
