using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Nature d'une quête : comment elle se valide.
/// </summary>
public enum QuestKind
{
    Tache,        // validée ailleurs (scénario...)
    Declaration,  // validée en déclarant une variable au clavier
    Question,     // validée en répondant correctement au clavier
    Affichage,    // validée en déposant une variable sur l'écran de la console
    Rangement     // validée en déposant une variable dans une case de la RAM
}

/// <summary>
/// Une quête / objectif affiché dans le CPU.
/// Pour une quête Question, 'description' contient la question posée et
/// 'reponseAttendue' la réponse correcte (saisie au clavier).
/// </summary>
[System.Serializable]
public class Quest
{
    public string    titre;
    [TextArea(1, 4)]
    public string    description;
    public bool      complete;
    public QuestKind kind = QuestKind.Tache;

    [Header("Si kind == Question")]
    public string reponseAttendue = "";
    public bool   reponseInsensibleCasse = true;

    public Quest(string titre, string description, QuestKind kind = QuestKind.Tache)
    {
        this.titre       = titre;
        this.description = description;
        this.kind        = kind;
        this.complete    = false;
    }

    /// <summary>Crée une quête-question (à répondre au clavier).</summary>
    public static Quest CreerQuestion(string titre, string question, string reponse, bool insensibleCasse = true)
    {
        return new Quest(titre, question, QuestKind.Question)
        {
            reponseAttendue        = reponse,
            reponseInsensibleCasse = insensibleCasse
        };
    }
}

/// <summary>
/// Une cellule de RAM : vide, ou contenant une variable.
/// </summary>
[System.Serializable]
public class RamSlot
{
    public bool     filled;
    public string   variable = "";
    public string   value    = "";
    public string   type     = "int";
    public Color    color    = Color.white;
    public Material material;

    public void Vider()
    {
        filled = false; variable = ""; value = ""; type = "int";
        color = Color.white; material = null;
    }
}

/// <summary>
/// État global qui survit aux changements de scène.
/// Singleton auto-créé au premier accès. Traverse Main ↔ Clavier ↔ RAM ↔ CPU.
/// </summary>
public class GameState : MonoBehaviour
{
    private static GameState _i;
    public static GameState I
    {
        get
        {
            if (_i == null)
            {
                var go = new GameObject("[GameState]");
                _i = go.AddComponent<GameState>();
            }
            return _i;
        }
    }

    [Header("Box active (variable manipulée)")]
    public string   boxVariable = "";
    public string   boxValue    = "";
    public string   boxType     = "int";
    public Color    boxColor    = new Color(0f, 0.85f, 1f); // Default cyan
    public Material boxMaterialAsset; // On stocke le matériau d'origine
    public bool     boxExists;     // une box logique existe (sol, main, en transit)

    [Header("RAM (multi-cellules)")]
    [Tooltip("Contenu des cases de la RAM. Rempli dynamiquement selon le nombre de cases de la scène.")]
    public List<RamSlot> ramSlots = new List<RamSlot>();
    [Tooltip("Vrai juste après être sorti de la scène RAM (anti-boucle de re-entrée).")]
    public bool ramJustVisited;

    [Header("CPU — Quêtes")]
    [Tooltip("Liste des objectifs. La quête active est celle à l'index 'questIndex'.")]
    public List<Quest> quests = new List<Quest>();
    [Tooltip("Index de la quête actuellement active.")]
    public int  questIndex = 0;
    [Tooltip("Vrai juste après être sorti de la scène CPU (anti-boucle de re-entrée).")]
    public bool cpuJustVisited;
    [Tooltip("Vrai juste après être sorti de la scène Clavier (anti-boucle de re-entrée).")]
    public bool clavierJustVisited;
    private bool _questsInit;

    [Header("Flux scènes")]
[Tooltip("Au prochain chargement de MainScene, fait apparaître un cube physique.")]
    public bool   needsSpawn;
    [Tooltip("Si true, le cube respawné est immédiatement collé à la main du joueur (sans le ramasser).")]
    public bool   spawnDansLaMain;
    [Tooltip("Nom de la scène principale à recharger via les boutons Retour.")]
    public string mainSceneName = "MainScene";

    void Awake()
    {
        if (_i != null && _i != this) { Destroy(gameObject); return; }
        _i = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitQuests();
    }

    void OnDestroy()
    {
        if (_i == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ── quêtes / objectifs (CPU) ──────────────────────────────────────────

    /// <summary>Remplit la liste de quêtes par défaut si elle est vide.</summary>
    public void InitQuests()
    {
        if (_questsInit) return;
        _questsInit = true;

        if (quests == null) quests = new List<Quest>();
        if (quests.Count > 0) return; // déjà peuplée (ex: depuis l'inspecteur)

        quests.Add(new Quest(
            "1. Déclarer et afficher une variable",
            "1) Approche le clavier et déclare une variable, ex : int nombre = 25\n" +
            "2) Récupère la box générée dans ta main\n" +
            "3) Approche l'écran de la console pour afficher sa valeur",
            QuestKind.Affichage));
        quests.Add(Quest.CreerQuestion(
            "2. Répondre à une question du CPU",
            "Combien font 2 + 3 * 4 ?  (réponds au clavier)",
            "14"));
        quests.Add(new Quest(
            "3. Stocker une variable en RAM",
            "Déclare une variable au clavier, approche la RAM, puis clique une case vide pour y déposer la box.",
            QuestKind.Rangement));
    }

    /// <summary>Quête actuellement active (ou null si toutes terminées).</summary>
    public Quest QueteActuelle()
    {
        InitQuests();
        if (questIndex < 0 || questIndex >= quests.Count) return null;
        return quests[questIndex];
    }

    /// <summary>Marque la quête active comme terminée et passe à la suivante.</summary>
    public void CompleterQueteActuelle()
    {
        var q = QueteActuelle();
        if (q == null) return;
        q.complete = true;
        if (questIndex < quests.Count - 1) questIndex++;
    }

    /// <summary>Complète la quête active seulement si elle est du type donné.</summary>
    public bool CompleterSiKind(QuestKind kind)
    {
        var q = QueteActuelle();
        if (q == null || q.complete || q.kind != kind) return false;
        CompleterQueteActuelle();
        return true;
    }

    public bool ToutesQuetesTerminees()
    {
        InitQuests();
        return questIndex >= quests.Count - 1 && quests.Count > 0 && quests[quests.Count - 1].complete;
    }

    // ── transitions ───────────────────────────────────────────────────────

    public void EnregistrerBox(string variable, string valeur, string type = "int", Color? color = null, Material mat = null)
    {
        boxVariable = variable;
        boxValue    = valeur;
        boxType     = type;
        if (color.HasValue) boxColor = color.Value;
        boxMaterialAsset = mat;
        boxExists   = true;
        needsSpawn  = true;
    }

    public void RetourSansDepot()
    {
        // Si une box logique existait, on demande à Main de la régénérer
        if (boxExists) needsSpawn = true;
    }

    // ── RAM multi-cellules ────────────────────────────────────────────────

    /// <summary>Garantit que la liste de cases a au moins 'count' éléments.</summary>
    public void EnsureRamSlots(int count)
    {
        if (ramSlots == null) ramSlots = new List<RamSlot>();
        while (ramSlots.Count < count) ramSlots.Add(new RamSlot());
    }

    /// <summary>Dépose la box tenue en main dans la case i (qui doit être vide).</summary>
    public bool DeposerDansCase(int i)
    {
        if (!boxExists) return false;
        if (i < 0 || i >= ramSlots.Count) return false;
        if (ramSlots[i].filled) return false;

        var s = ramSlots[i];
        s.filled   = true;
        s.variable = boxVariable;
        s.value    = boxValue;
        s.type     = boxType;
        s.color    = boxColor;
        s.material = boxMaterialAsset;

        // La box quitte la main : plus rien à régénérer.
        boxExists       = false;
        boxVariable     = "";
        boxValue        = "";
        needsSpawn      = false;
        spawnDansLaMain = false;
        return true;
    }

    /// <summary>Reprend la box de la case i en main (cube régénéré au prochain Main load).</summary>
    public bool PrendreDeCase(int i)
    {
        if (i < 0 || i >= ramSlots.Count) return false;
        if (!ramSlots[i].filled) return false;

        var s = ramSlots[i];
        boxVariable      = s.variable;
        boxValue         = s.value;
        boxType          = s.type;
        boxColor         = s.color;
        boxMaterialAsset = s.material;
        boxExists        = true;
        needsSpawn       = true;
        spawnDansLaMain  = true;

        s.Vider();
        return true;
    }

    /// <summary>
    /// Appelé par l'écran (ConsoleScreen) quand il consomme la box pour l'afficher.
    /// </summary>
    public void ConsommerPourEcran()
    {
        boxExists       = false;
        boxVariable     = "";
        boxValue        = "";
        needsSpawn      = false;
        spawnDansLaMain = false;
    }

    // ── spawn du cube physique dans MainScene ─────────────────────────────

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainSceneName) return;
        if (!needsSpawn || !boxExists)   return;

        var cube = SpawnCubePhysique();

        // Si demandé : coller directement le cube dans la main du joueur
        if (spawnDansLaMain && cube != null)
        {
            var pg = GameObject.FindGameObjectWithTag("Player");
            if (pg != null)
            {
                var holder = pg.GetComponentInChildren<PlayerHolder>();
                var pickup = cube.GetComponent<PickupItem>();
                if (holder != null && pickup != null)
                {
                    holder.TryPickup(pickup);
                    Debug.Log("[GameState] Cube collé à la main du joueur.");
                }
            }
        }

        needsSpawn      = false;
        spawnDansLaMain = false;
    }

    GameObject SpawnCubePhysique()
    {
        Vector3 pos = TrouverPointDeSpawn();

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"DataBox_{boxVariable}";
        cube.transform.position   = pos;
        cube.transform.localScale = Vector3.one * 0.4f;

        // Matériau : Utilise le matériau d'origine si possible, sinon en crée un
        var rend = cube.GetComponent<Renderer>();
        if (boxMaterialAsset != null)
        {
            // On crée une instance pour pouvoir appliquer la couleur spécifique
            var mat = new Material(boxMaterialAsset);
            // On essaie d'appliquer la couleur capturée
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", boxColor);
            else if (mat.HasProperty("_Color")) mat.color = boxColor;
            
            rend.material = mat;
        }
        else
        {
var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            mat.color = boxColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", boxColor * 0.3f);
            }
            rend.material = mat;
        }

        // Composants logiques
var db = cube.AddComponent<DataBox>();
        db.variableName = boxVariable;
        db.value        = boxValue;
        db.typeName     = boxType;

        var pi = cube.AddComponent<PickupItem>();
        pi.itemId      = "data_" + boxVariable;
        pi.displayText = boxValue;
        pi.codeLabel   = $"{boxType} {boxVariable} = {boxValue};";

        Debug.Log($"[GameState] Cube physique respawnu à {pos} : {boxType} {boxVariable} = {boxValue}");
        return cube;
    }

    Vector3 TrouverPointDeSpawn()
{
        // 1) GameObject nommé "BoxSpawnPoint" dans la scène ?
        var marker = GameObject.Find("BoxSpawnPoint");
        if (marker != null) return marker.transform.position;

        // 2) Sinon, devant le joueur
        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg != null) return pg.transform.position + pg.transform.forward * 1.5f + Vector3.up * 0.6f;

        // 3) Fallback
        return new Vector3(0f, 1f, 0f);
    }
}
