using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Nature d'une quête : comment elle se valide.
/// </summary>
public enum QuestKind
{
    Tache,        // validée ailleurs (scénario...)
    Visite,       // validée en se rendant au CPU (briefing)
    Declaration,  // validée en déclarant une variable au clavier
    Question,     // validée en répondant correctement au clavier
    Affichage,    // validée en déposant une variable sur l'écran de la console
    Rangement,    // validée en déposant une variable dans la RAM
    Saisie        // validée en entrant une valeur au clavier (Console.ReadLine)
}

/// <summary>
/// Une mission / objectif affiché dans le CPU et le HUD.
/// Pour une quête Question, 'description' contient la question posée et
/// 'reponseAttendue' la réponse correcte (saisie au clavier).
/// 'indication' = consigne courte façon Hitman, affichée dans le HUD.
/// </summary>
[System.Serializable]
public class Quest
{
    public string    titre;
    [TextArea(1, 4)]
    public string    description;
    [Tooltip("Consigne courte affichée dans le HUD (où aller, quoi faire).")]
    public string    indication = "";
    public bool      complete;
    public QuestKind kind = QuestKind.Tache;

    [Header("Si kind == Question")]
    public string reponseAttendue = "";
    public bool   reponseInsensibleCasse = true;

    public Quest(string titre, string description, QuestKind kind = QuestKind.Tache, string indication = "")
    {
        this.titre       = titre;
        this.description = description;
        this.kind        = kind;
        this.indication  = indication;
        this.complete    = false;
    }

    /// <summary>Crée une quête-question (à répondre au clavier).</summary>
    public static Quest CreerQuestion(string titre, string question, string reponse,
                                      string indication = "", bool insensibleCasse = true)
    {
        return new Quest(titre, question, QuestKind.Question, indication)
        {
            reponseAttendue        = reponse,
            reponseInsensibleCasse = insensibleCasse
        };
    }
}

/// <summary>
/// Une case de RAM (une boîte sur les tablettes) : vide ou contenant une variable.
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

    [Header("RAM")]
    public bool     ramFilled;
    public string   ramVariable = "";
    public string   ramValue    = "";
    public string   ramType     = "int";
    public Color    ramColor    = Color.white;
    public Material ramMaterial;

    [Header("RAM — cases multiples (boîtes sur les tablettes)")]
    [Tooltip("Contenu des cases. Dimensionné automatiquement par la scène RAM.")]
    public List<RamSlot> ramSlots = new List<RamSlot>();

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
        MissionHUD.Ensure();
        ObjectiveMarker.Ensure();
        VoiceOver.Ensure();
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

        // ── Campagne façon Hitman : missions guidées, dans l'ordre ──────────
        quests.Add(new Quest(
            "Briefing au CPU",
            "Le CPU centralise tes objectifs. Rends-toi au CPU pour recevoir ta première mission.",
            QuestKind.Visite,
            "Approche-toi du CPU pour recevoir tes objectifs."));

        quests.Add(new Quest(
            "Déclarer une variable",
            "Toute donnée commence par une déclaration. Va au clavier et déclare une variable, ex : int nombre = 25",
            QuestKind.Declaration,
            "Va au clavier et déclare une variable (ex : int nombre = 25)."));

        quests.Add(new Quest(
            "Stocker en RAM",
            "Une variable doit vivre en mémoire. Déclare une variable au clavier, puis porte la box dans la RAM.",
            QuestKind.Rangement,
            "Porte ta box jusqu'au portail de la RAM."));

        quests.Add(new Quest(
            "Afficher une valeur",
            "Montre le résultat au monde. Déclare une variable (ou reprends-en une dans la RAM), puis porte la box jusqu'à l'écran de la console.",
            QuestKind.Affichage,
            "Porte une box jusqu'à l'écran de la console."));

        quests.Add(new Quest(
            "Console.ReadLine()",
            "Le CPU attend une entrée utilisateur. Va au clavier et entre un nombre entier — le CPU s'en servira pour calculer.",
            QuestKind.Saisie,
            "Va au clavier et entre un nombre (Console.ReadLine)."));

        quests.Add(Quest.CreerQuestion(
            "Calcul du CPU",
            "{auto}",            // généré quand la saisie précédente est connue
            "{auto}",
            "Réponds au calcul du CPU au clavier."));

        quests.Add(Quest.CreerQuestion(
            "Priorité des opérations",
            "Dernière mission : combien font 2 + 3 * 4 ?",
            "14",
            "Réponds à la question du CPU au clavier."));
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
        AudioFX.MissionValidee();
        MissionHUD.Refresh();
        ObjectiveMarker.Refresh();
        VoiceOver.AnnoncerMission(); // la radio annonce la mission suivante
    }

    /// <summary>
    /// Console.ReadLine : enregistre la valeur entrée au clavier, complète la
    /// mission Saisie et génère le calcul de la mission Question suivante.
    /// </summary>
    public string derniereSaisie = "";

    public void TerminerSaisie(string valeur)
    {
        derniereSaisie = valeur;
        CompleterSiKind(QuestKind.Saisie);

        // La mission suivante est un calcul basé sur la saisie ({auto}).
        var q = QueteActuelle();
        if (q != null && q.kind == QuestKind.Question && q.reponseAttendue == "{auto}"
            && long.TryParse(valeur, out long n))
        {
            long a = Random.Range(2, 10);
            long b = Random.Range(2, 6);
            q.description     = $"Le CPU a reçu {n} via Console.ReadLine(). Calcule : {n} + {a} * {b} = ?";
            q.reponseAttendue = (n + a * b).ToString();
        }
        MissionHUD.Refresh();
        ObjectiveMarker.Refresh();
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

    // ── RAM multi-cases (boîtes des tablettes) ────────────────────────────

    /// <summary>Garantit que la liste de cases a au moins 'count' éléments.</summary>
    public void EnsureRamSlots(int count)
    {
        if (ramSlots == null) ramSlots = new List<RamSlot>();
        while (ramSlots.Count < count) ramSlots.Add(new RamSlot());
    }

    /// <summary>Dépose la box tenue en main dans la première case libre. Retourne l'index, ou -1.</summary>
    public int DeposerAuto()
    {
        if (!boxExists) return -1;

        int libre = -1;
        for (int i = 0; i < ramSlots.Count; i++)
            if (!ramSlots[i].filled) { libre = i; break; }
        if (libre < 0) return -1; // RAM pleine : la box reste en main

        var s = ramSlots[libre];
        s.filled   = true;
        s.variable = boxVariable;
        s.value    = boxValue;
        s.type     = boxType;
        s.color    = boxColor;
        s.material = boxMaterialAsset;

        // La box quitte la main : plus rien à régénérer dans Main.
        boxExists       = false;
        boxVariable     = "";
        boxValue        = "";
        needsSpawn      = false;
        spawnDansLaMain = false;

        AudioFX.Depot();
        CompleterSiKind(QuestKind.Rangement); // valide la quête « stocker en RAM »
        return libre;
    }

    /// <summary>Reprend la box de la case i en main (cube régénéré au prochain Main load).</summary>
    public void PrendreSlot(int i)
    {
        if (i < 0 || i >= ramSlots.Count || !ramSlots[i].filled) return;

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
    }

    // ── RAM (un seul emplacement) ─────────────────────────────────────────

    public void DeposerEnRam()
    {
        ramFilled   = true;
        ramVariable = boxVariable;
        ramValue    = boxValue;
        ramType     = boxType;
        ramColor    = boxColor;
        ramMaterial = boxMaterialAsset;

        boxExists   = false;
        boxVariable = "";
        boxValue    = "";
    }

    /// <summary>
    /// Le joueur reprend la box stockée dans la RAM. Au prochain Main load,
    /// le cube apparaît directement dans sa main.
    /// </summary>
    public void PrendreDansRam()
    {
        if (!ramFilled) return;

        boxVariable      = ramVariable;
        boxValue         = ramValue;
        boxType          = ramType;
        boxColor         = ramColor;
        boxMaterialAsset = ramMaterial;
        boxExists        = true;
        needsSpawn       = true;
        spawnDansLaMain  = true;

        ramFilled   = false;
        ramVariable = "";
        ramValue    = "";
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
