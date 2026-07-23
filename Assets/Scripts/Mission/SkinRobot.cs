using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Skins du robot : teintes débloquées par le nombre de BADGES obtenus.
/// Le choix se fait dans le menu pause (rangée de pastilles colorées) et il
/// est appliqué au personnage à chaque chargement de la MainScene.
/// Singleton créé par GameState.
/// </summary>
public class SkinRobot : MonoBehaviour
{
    public static SkinRobot Instance { get; private set; }

    /// <summary>Catalogue : (nom, teinte, badges requis).</summary>
    public static readonly (string nom, Color couleur, int badgesRequis)[] Skins =
    {
        ("Standard", Color.white,                      0),
        ("Cyan",     new Color(0.55f, 0.9f, 1f),       2),
        ("Émeraude", new Color(0.55f, 1f, 0.72f),      4),
        ("Or",       new Color(1f, 0.85f, 0.45f),      6),
        ("Violet",   new Color(0.82f, 0.62f, 1f),      8),
        ("Rubis",    new Color(1f, 0.5f, 0.5f),       11), // TOUS les badges (chap. 1 + 2)
    };

    public static int SkinActuel => PlayerPrefs.GetInt("cda_skin", 0);

    public static int BadgesObtenus()
    {
        int n = 0;
        foreach (var (id, _, _) in Badges.Tous)
            if (Badges.EstDebloque(id)) n++;
        return n;
    }

    public static bool EstDebloque(int index) =>
        index >= 0 && index < Skins.Length && BadgesObtenus() >= Skins[index].badgesRequis;

    /// <summary>Choisit un skin (s'il est débloqué) et l'applique immédiatement.</summary>
    public static bool Choisir(int index)
    {
        if (!EstDebloque(index)) return false;
        PlayerPrefs.SetInt("cda_skin", index);
        PlayerPrefs.Save();
        AppliquerAuJoueur();
        return true;
    }

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[SkinRobot]");
            go.AddComponent<SkinRobot>();
        }
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        AppliquerAuJoueur();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Le personnage est rechargé avec la scène : on réapplique sa teinte.
        AppliquerAuJoueur();
    }

    /// <summary>Teinte tous les renderers du robot avec la couleur du skin choisi.</summary>
    public static void AppliquerAuJoueur()
    {
        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg == null) return;

        Color c = Skins[Mathf.Clamp(SkinActuel, 0, Skins.Length - 1)].couleur;
        foreach (var rend in pg.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.GetComponent<TMP_Text>() != null) continue;            // pas les textes
            if (rend.GetComponentInParent<PickupItem>() != null) continue;  // pas la boîte portée !
            if (rend.GetComponentInParent<DataBox>()    != null) continue;  // (double sécurité)
            foreach (var mat in rend.materials)
            {
                if      (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                else if (mat.HasProperty("_Color"))     mat.color = c;
            }
        }
    }
}
