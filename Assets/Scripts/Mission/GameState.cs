using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    Saisie,       // validée en entrant une valeur au clavier (Console.ReadLine)
    Condition,    // validée en affichant une valeur qui rend un test if vrai
    Compteur,     // validée en répétant un dépôt RAM N fois (boucle)
    LectureRam,   // validée en affichant une variable reprise depuis la RAM
    Correction,   // validée en corrigeant une ligne de code buggée au clavier
    Calcul,       // somme = x + z : apporter x puis z au CPU, ranger somme en RAM
    DeclarationRam, // validée en déclarant une variable directement dans la RAM (formulaire)
    SaisieEcran,  // Console.ReadLine() sur l'écran : taper une valeur puis ranger la box en RAM
    Parse,        // z = Int32.Parse(y) : apporter y au CPU, ranger z en RAM
    ConditionIf,  // if (somme > seuil) : apporter somme au CPU, afficher le message de la branche
    Boucle        // for (i = 0; i < 3; i++) : le CPU donne i à chaque tour, ranger en RAM
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

    [Header("Si kind == Condition")]
    public string conditionOp    = ">";
    public double conditionSeuil = 10;

    [Header("Si kind == Compteur")]
    public int objectifCompteur = 3;
    public int compteur         = 0;

    [Header("Si kind == Correction")]
    public string bugType = "";   // type attendu de la ligne corrigée
    public string bugNom  = "";   // nom de variable attendu

    [Header("Si kind == LectureRam")]
    [Tooltip("Nom de la variable attendue sur l'écran (ex : x, somme).")]
    public string cibleVariable = "";

    /// <summary>Teste la condition if (valeur op seuil).</summary>
    public bool TesterCondition(double v)
    {
        switch (conditionOp)
        {
            case ">":  return v >  conditionSeuil;
            case "<":  return v <  conditionSeuil;
            case ">=": return v >= conditionSeuil;
            case "<=": return v <= conditionSeuil;
            case "==": return System.Math.Abs(v - conditionSeuil) < 1e-9;
            case "!=": return System.Math.Abs(v - conditionSeuil) > 1e-9;
            default:   return false;
        }
    }

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

    // Prefab de la boîte portée (celui de la RAM) — fourni par MissionConfig.
    [System.NonSerialized] public GameObject boxPrefab;
    [System.NonSerialized] public float      boxScale = 0.6f;

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
    [Tooltip("Vrai si la box en main a été reprise depuis la RAM (mission Lecture mémoire).")]
    public bool boxVientDeRam;

    [Header("CPU — Quêtes")]
    [Tooltip("Liste des objectifs. La quête active est celle à l'index 'questIndex'.")]
    public List<Quest> quests = new List<Quest>();
    [Tooltip("Index de la quête actuellement active.")]
    public int  questIndex = 0;
    [Tooltip("Index de la dernière mission révélée au CPU (-1 = aucune). Les missions suivantes sont cachées.")]
    public int  missionRevelee = -1;
    [Tooltip("Vrai juste après être sorti de la scène CPU (anti-boucle de re-entrée).")]
    public bool cpuJustVisited;
    [Tooltip("Vrai juste après être sorti de la scène Clavier (anti-boucle de re-entrée).")]
    public bool clavierJustVisited;
    private bool _questsInit;

    [Header("Score (rating de fin)")]
    [Tooltip("Erreurs commises (mauvaises réponses, conditions ratées...).")]
    public int nbErreurs;
    private float _sessionDebut;    // realtimeSinceStartup au lancement de la session
    private float _tempsAnterieur;  // temps cumulé des sessions précédentes (sauvegarde)

    /// <summary>Temps total de la campagne en secondes (sessions cumulées).</summary>
    public float TempsCampagne => _tempsAnterieur + (Time.realtimeSinceStartup - _sessionDebut);

    public void SignalerErreur()
    {
        nbErreurs++;
        HumourOS.Erreur(); // l'OS commente (avec ironie bienveillante)
        Sauvegarder();
    }

    [Header("Accessibilité")]
    [Tooltip("Mode Zen : le rating ignore le chrono (aucune pression de temps).")]
    public bool modeZen;

    public void BasculerModeZen()
    {
        modeZen = !modeZen;
        PlayerPrefs.SetInt("cda_zen", modeZen ? 1 : 0);
        PlayerPrefs.Save();
    }

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
        _sessionDebut = Time.realtimeSinceStartup;
        InitQuests();
        Charger(); // restaure la progression sauvegardée (le cas échéant)
        modeZen = PlayerPrefs.GetInt("cda_zen", 0) == 1; // réglage indépendant de la campagne
        MissionHUD.Ensure();
        ObjectiveMarker.Ensure();
        VoiceOver.Ensure();
        EcranTitre.Ensure();      // écran titre au lancement (avant le briefing)
        BriefingCinematic.Ensure();
        PauseMenu.Ensure();
        NotificationsUI.Ensure(); // toasts (badges, rating de mission)
        ScreenShake.Ensure();     // secousse caméra à la validation
        JournalMissions.Ensure(); // journal de mission (touche J)
        HumourOS.Ensure();        // répliques ironiques de l'OS
        MiniCarte.Ensure();       // minimap circuit imprimé (bas-droit)
        SkinRobot.Ensure();       // teinte du robot (débloquée par badges)
        DemarrerChronoMission();  // base de temps de la 1re mission
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

        // ── Le PROGRAMME : chaque mission est une ligne de code à exécuter ──
        quests.Add(new Quest(
            "1.  int x = 4;",
            "// déclare une variable avec le nom x et la valeur 4",
            QuestKind.DeclarationRam,
            "Dans la RAM : clique la boîte int → nom x, valeur 4, sauvegarde."));

        quests.Add(new Quest(
            "2.  Console.WriteLine(x);",
            "// affiche la variable x",
            QuestKind.LectureRam,
            "Prends une copie de x dans la RAM et pose-la sur l'écran.") { cibleVariable = "x" });

        quests.Add(new Quest(
            "3.  string y = Console.ReadLine();",
            "// déclare d'abord string y, puis récupère le nombre de l'utilisateur et mets-le dans y",
            QuestKind.SaisieEcran,
            "Dans la RAM : clique la boîte string → déclare y, sauvegarde."));

        quests.Add(new Quest(
            "4.  z = Int32.Parse(y);",
            "// convertit y (caractères) en entier et le range dans z",
            QuestKind.Parse,
            "Prends y dans la RAM et apporte-le au CPU."));

        quests.Add(new Quest(
            "5.  somme = x + z;",
            "// additionne x et z",
            QuestKind.Calcul,
            "Prends x dans la RAM et apporte-le au CPU."));

        quests.Add(new Quest(
            "6.  Console.WriteLine(somme);",
            "// affiche la somme",
            QuestKind.LectureRam,
            "Prends une copie de somme dans la RAM et pose-la sur l'écran.") { cibleVariable = "somme" });

        // ── CHAPITRE 2 : la condition ──
        quests.Add(new Quest(
            "7.  if (somme > 50) ... else ...",
            "// si somme > 50 : affiche \"grand\" — sinon : affiche \"petit\"",
            QuestKind.ConditionIf,
            "Prends une copie de somme dans la RAM et apporte-la au CPU."));

        // ── CHAPITRE 3 : la boucle ──
        quests.Add(new Quest(
            "8.  for (int i = 0; i < 3; i++)",
            "// à chaque tour : range i dans la RAM (0, puis 1, puis 2) — même adresse !",
            QuestKind.Boucle,
            "Va au CPU : il lance la boucle et te donne la boîte i."));
    }

    // ── Étapes internes de la mission active ─────────────────────────────
    // SaisieEcran : 0 = déclarer string y en RAM, 1 = ReadLine à l'écran, 2 = ranger y en RAM.
    // Parse       : 0 = apporter y au CPU,        1 = ranger z en RAM.
    // Calcul      : 0 = apporter x, 1 = apporter z, 2 = ranger somme en RAM.
    // ConditionIf : 0 = apporter somme au CPU,    1 = afficher le message à l'écran.
    [System.NonSerialized] public int missionEtape;

    /// <summary>Seuil du if de la ligne 7 : if (somme > SEUIL_IF).</summary>
    public const long SEUIL_IF = 50;

    // Valeurs mémorisées par le CPU (affichage de la scène Calculateur).
    [System.NonSerialized] public long   cpuX, cpuZ, cpuSomme;
    [System.NonSerialized] public string cpuY = "";
    [System.NonSerialized] public string cpuVerdict = ""; // "grand" / "petit" (ligne 7)

    /// <summary>Couleur standard d'un type (utilisée sur les boîtes et les textes RAM).</summary>
    public static Color CouleurType(string type)
    {
        switch (type)
        {
            case "int":    return new Color(1f, 0.25f, 0.25f); // rouge
            case "float":  return new Color(1f, 0.4f, 0.7f);   // rose
            case "string": return new Color(0.8f, 0.4f, 1f);   // violet
            case "bool":   return new Color(0.4f, 0.9f, 0.5f); // vert
            default:       return Color.white;
        }
    }

    /// <summary>Consigne courte du HUD selon la mission active et son étape.</summary>
    public string IndicationActuelle()
    {
        var q = QueteActuelle();
        if (q == null) return "";
        switch (q.kind)
        {
            case QuestKind.DeclarationRam:
                return "Dans la RAM : clique la boîte int → nom x, valeur 4, sauvegarde.";
            case QuestKind.LectureRam:
                return $"Prends une copie de {q.cibleVariable} dans la RAM et pose-la sur l'écran.";
            case QuestKind.SaisieEcran:
                if (missionEtape == 0) return "Dans la RAM : clique la boîte string → déclare y, sauvegarde.";
                if (missionEtape == 1) return "Va à l'écran : [E] Console.ReadLine() — récupère la valeur.";
                return "Range la boîte y dans la RAM (elle écrase l'ancienne).";
            case QuestKind.Parse:
                return missionEtape == 0
                    ? "Prends y dans la RAM et apporte-le au CPU."
                    : "Range la boîte z (int) dans la RAM.";
            case QuestKind.Calcul:
                if (missionEtape == 0) return "Prends x dans la RAM et apporte-le au CPU.";
                if (missionEtape == 1) return "Prends z dans la RAM et apporte-le au CPU.";
                return "Range la boîte somme dans la RAM.";
            case QuestKind.ConditionIf:
                return missionEtape == 0
                    ? "Prends une copie de somme dans la RAM et apporte-la au CPU."
                    : "Pose la boîte message sur l'écran (le résultat du if).";
            case QuestKind.Boucle:
                if (boxExists && boxVariable == "i")
                    return $"Range la boîte i = {boxValue} dans la RAM (tour {missionEtape + 1}/3).";
                return missionEtape < 3
                    ? $"Va au CPU : itération i = {missionEtape} (tour {missionEtape + 1}/3)."
                    : "Retourne au CPU pour le test final : i < 3 ?";
            default:
                return q.indication;
        }
    }

    /// <summary>Met à jour la consigne du HUD + marqueur après un changement d'étape.</summary>
    void MajIndication()
    {
        var q = QueteActuelle();
        if (q != null) q.indication = IndicationActuelle();
        MissionHUD.Refresh();
        ObjectiveMarker.Refresh();
    }

    bool RamContient(string nom)
    {
        foreach (var s in ramSlots) if (s.filled && s.variable == nom) return true;
        return false;
    }

    int IndexSlot(string nom)
    {
        for (int i = 0; i < ramSlots.Count; i++)
            if (ramSlots[i].filled && ramSlots[i].variable == nom) return i;
        return -1;
    }

    // ── Déclaration directe dans la RAM (formulaire de la scène RAM) ─────

    /// <summary>
    /// Déclare une variable directement dans la RAM (mission 1 et usage libre).
    /// Retourne null si OK, sinon le message d'erreur à afficher dans le formulaire.
    /// </summary>
    public string DeclarerEnRam(string type, string nom, string valeur)
    {
        nom    = (nom    ?? "").Trim();
        valeur = (valeur ?? "").Trim();

        if (!System.Text.RegularExpressions.Regex.IsMatch(nom, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            return "Nom invalide : lettres/chiffres sans espaces (ex : x).";

        switch (type)
        {
            case "int":
                if (!long.TryParse(valeur, out _)) return "Un int est un nombre entier (ex : 4).";
                break;
            case "float":
                if (!double.TryParse(valeur.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                    return "Un float est un nombre (ex : 2.5).";
                break;
            case "bool":
                valeur = valeur.ToLowerInvariant();
                if (valeur != "true" && valeur != "false") return "Un bool vaut true ou false.";
                break;
            default: // string (peut être vide : on la remplira plus tard, ex. y)
                valeur = valeur.Trim('"');
                break;
        }

        // Mission 1 : la ligne à exécuter est exactement  int x = 4;
        var q  = QueteActuelle();
        bool m1 = q != null && !q.complete && q.kind == QuestKind.DeclarationRam;
        if (m1 && (nom != "x" || type != "int" || valeur != "4"))
            return "La mission demande exactement :  int x = 4;";

        EnsureRamSlots(12);
        int i = IndexSlot(nom); // même nom → on écrase sa case (même adresse mémoire)
        if (i < 0)
            for (int k = 0; k < ramSlots.Count; k++)
                if (!ramSlots[k].filled) { i = k; break; }
        if (i < 0) return "RAM pleine !";

        var s = ramSlots[i];
        s.filled = true; s.variable = nom; s.value = valeur; s.type = type;
        s.color  = CouleurType(type);

        AudioFX.Depot();

        // Badges liés à la déclaration
        Badges.PremiereBoite();
        if (type == "float") Badges.PremierFloat();
        int occupees = 0;
        foreach (var slot in ramSlots) if (slot.filled) occupees++;
        Badges.MemoireBienRemplie(occupees);

        if (m1)
        {
            CompleterQueteActuelle();
        }
        else if (q != null && !q.complete && q.kind == QuestKind.SaisieEcran &&
                 missionEtape == 0 && nom == "y" && type == "string")
        {
            // Mission 3, étape 1 franchie : string y déclarée → direction l'écran.
            missionEtape = 1;
            AudioFX.Succes();
            MajIndication();
        }
        Sauvegarder();
        return null;
    }

    // ── Console.ReadLine() sur l'écran (mission 3) ────────────────────────

    /// <summary>La valeur reçue par l'écran devient la box  string y  en main.</summary>
    public void SaisirLigne(string valeur)
    {
        derniereSaisie = valeur;
        EnregistrerBox("y", valeur, "string", CouleurType("string"));
        spawnDansLaMain = true;
        missionEtape = 2; // il reste à ranger y dans la RAM
        AudioFX.Succes();
        MajIndication();
        Sauvegarder();
        SpawnBoxMaintenant(); // la boîte apparaît directement dans la main
    }

    // ── Le CPU reçoit une box (missions 4 et 5) ───────────────────────────

    /// <summary>Le CPU traite la box portée. Retourne un message, ou null si rien à faire.</summary>
    public string CpuRecevoir()
    {
        var q = QueteActuelle();
        if (q == null || !boxExists) return null;

        // Mission 4 : z = Int32.Parse(y)
        if (q.kind == QuestKind.Parse)
        {
            if (missionEtape == 0 && boxVariable == "y")
            {
                cpuY = boxValue;
                long.TryParse(boxValue, out cpuZ);
                ConsommerBoxEnMain();
                EnregistrerBox("z", cpuZ.ToString(), "int", CouleurType("int"));
                spawnDansLaMain = true; // le CPU te rend z en main
                missionEtape = 1;
                AudioFX.MissionValidee(); MajIndication(); Sauvegarder();
                return $"z = Int32.Parse(\"{cpuY}\") = {cpuZ}.  Range la boîte z dans la RAM.";
            }
            return "Le CPU attend la boîte y (prends-la dans la RAM).";
        }

        // Mission 5 : somme = x + z
        if (q.kind == QuestKind.Calcul)
        {
            if (missionEtape == 0 && boxVariable == "x")
            {
                long.TryParse(boxValue, out cpuX);
                ConsommerBoxEnMain();
                missionEtape = 1;
                AudioFX.Succes(); MajIndication(); Sauvegarder();
                return $"x = {cpuX} reçu.  Apporte maintenant z.";
            }
            if (missionEtape == 1 && boxVariable == "z")
            {
                long.TryParse(boxValue, out cpuZ);
                cpuSomme = cpuX + cpuZ;
                ConsommerBoxEnMain();
                EnregistrerBox("somme", cpuSomme.ToString(), "int", CouleurType("int"));
                spawnDansLaMain = true; // le CPU te rend somme en main
                missionEtape = 2;
                AudioFX.MissionValidee(); MajIndication(); Sauvegarder();
                return $"somme = x + z = {cpuX} + {cpuZ} = {cpuSomme}.  Range la boîte somme dans la RAM.";
            }
            return missionEtape == 0 ? "Le CPU attend la boîte x (prends-la dans la RAM)."
                                     : "Le CPU attend la boîte z (prends-la dans la RAM).";
        }

        // Ligne 7 : if (somme > SEUIL) → le CPU teste et donne le message de la branche
        if (q.kind == QuestKind.ConditionIf)
        {
            if (missionEtape == 0 && boxVariable == "somme")
            {
                long.TryParse(boxValue, out cpuSomme);
                bool vrai  = cpuSomme > SEUIL_IF;
                cpuVerdict = vrai ? "grand" : "petit";
                ConsommerBoxEnMain();
                EnregistrerBox("message", cpuVerdict, "string", CouleurType("string"));
                spawnDansLaMain = true; // le CPU te rend le message en main
                missionEtape = 1;
                AudioFX.MissionValidee(); MajIndication(); Sauvegarder();
                return $"if ({cpuSomme} > {SEUIL_IF}) → {(vrai ? "VRAI" : "FAUX")}.  " +
                       $"La branche exécutée donne \"{cpuVerdict}\" : affiche ce message sur l'écran.";
            }
            return "Le CPU attend la boîte somme (prends une copie dans la RAM).";
        }
        return null;
    }

    /// <summary>
    /// Ligne 8 : for (int i = 0; i &lt; 3; i++) — chaque visite au CPU exécute un
    /// tour de boucle : test du i courant, puis il te donne la boîte i à ranger.
    /// missionEtape = valeur courante de i (0..3). Retourne le message du CPU.
    /// </summary>
    public string BoucleCpu()
    {
        var q = QueteActuelle();
        if (q == null || q.complete || q.kind != QuestKind.Boucle) return null;

        if (boxExists)
            return $"Range d'abord la boîte i = {boxValue} dans la RAM (le tour n'est pas fini).";

        if (missionEtape < 3)
        {
            // Test VRAI → le CPU exécute le corps du tour : il te donne i.
            EnregistrerBox("i", missionEtape.ToString(), "int", CouleurType("int"));
            spawnDansLaMain = true;
            AudioFX.Succes(); MajIndication(); Sauvegarder();
            return $"for :  i = {missionEtape} ;  i < 3 → VRAI.  " +
                   $"Range la boîte i dans la RAM (elle écrase l'ancienne valeur !).";
        }

        // i = 3 : le test échoue → la boucle se termine, la ligne est exécutée.
        CompleterQueteActuelle();
        return "for :  i = 3 ;  i < 3 → FAUX.  La boucle s'arrête : ligne terminée !";
    }

    void ConsommerBoxEnMain()
    {
        boxExists       = false;
        boxVariable     = "";
        boxValue        = "";
        needsSpawn      = false;
        spawnDansLaMain = false;
        boxVientDeRam   = false;
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

        // ── Rating de la ligne : durée + erreurs depuis sa révélation ──
        float dureeMission   = TempsCampagne - _missionChronoDebut;
        int   erreursMission = nbErreurs - _missionErreursDebut;
        AnnoncerRatingMission(q, dureeMission, erreursMission);
        Badges.MissionTerminee(dureeMission, erreursMission);
        if (q.kind == QuestKind.ConditionIf) Badges.Logicien();
        if (q.kind == QuestKind.Boucle)      Badges.Boucleur();

        if (questIndex < quests.Count - 1) questIndex++;
        missionEtape = 0; // chaque mission repart à son étape 0
        AudioFX.MissionValidee();
        ScreenShake.Jouer(); // petite secousse de validation
        MajIndication();
        Sauvegarder();

        if (ToutesQuetesTerminees())
        {
            Badges.CampagneTerminee(nbErreurs);
            VoiceOver.AnnoncerMission(); // joue la réplique de fin
            RatingScreen.Afficher();     // écran de rating façon Hitman
        }
        // sinon : la mission suivante sera annoncée quand le joueur ira au CPU.
    }

    // ── rating par mission (façon Hitman, en toast) ───────────────────────

    [System.NonSerialized] private float _missionChronoDebut;
    [System.NonSerialized] private int   _missionErreursDebut;

    /// <summary>Démarre le chrono de la mission (à sa révélation au CPU).</summary>
    void DemarrerChronoMission()
    {
        _missionChronoDebut  = TempsCampagne;
        _missionErreursDebut = nbErreurs;
    }

    void AnnoncerRatingMission(Quest q, float duree, int erreurs)
    {
        // Mode Zen : le temps ne compte pas, seule la justesse est notée.
        int etoiles = modeZen
            ? (erreurs == 0 ? 3 : erreurs <= 1 ? 2 : 1)
            : (erreurs == 0 && duree < 90f) ? 3
            : (erreurs <= 1)                ? 2
            :                                 1;
        string titreNote = etoiles == 3 ? "Architecte Élégant"
                         : etoiles == 2 ? "Exécution Solide"
                         :                "En Rodage";
        string detail = modeZen
            ? $"{titreNote} — {erreurs} erreur{(erreurs > 1 ? "s" : "")} (mode Zen)"
            : $"{titreNote} — {duree:0} s, {erreurs} erreur{(erreurs > 1 ? "s" : "")}";
        NotificationsUI.Afficher(
            $"LIGNE TERMINÉE   <color=#FFD24F>{etoiles}/3</color>",
            detail,
            new Color(0f, 1f, 0.55f));

        EnregistrerStatLigne(questIndex, etoiles, duree, erreurs);
    }

    /// <summary>Garde le MEILLEUR résultat de chaque ligne (étoiles, puis temps).</summary>
    void EnregistrerStatLigne(int index, int etoiles, float duree, int erreurs)
    {
        string cle = "cda_stat_" + index;
        var ancien = PlayerPrefs.GetString(cle, "").Split('|');
        if (ancien.Length == 3 &&
            int.TryParse(ancien[0], out int etOld) && float.TryParse(ancien[1], out float durOld))
        {
            // On ne remplace que si c'est mieux (plus d'étoiles, ou aussi bien mais plus vite).
            if (etOld > etoiles || (etOld == etoiles && durOld <= duree)) return;
        }
        PlayerPrefs.SetString(cle, $"{etoiles}|{duree:0.#}|{erreurs}");
        PlayerPrefs.Save();
    }

    /// <summary>Meilleur résultat d'une ligne : (étoiles, durée, erreurs) ou null.</summary>
    public (int etoiles, float duree, int erreurs)? StatLigne(int index)
    {
        var p = PlayerPrefs.GetString("cda_stat_" + index, "").Split('|');
        if (p.Length == 3 &&
            int.TryParse(p[0], out int et) &&
            float.TryParse(p[1], out float du) &&
            int.TryParse(p[2], out int er))
            return (et, du, er);
        return null;
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
        GenererCalculAuto();
        MissionHUD.Refresh();
        ObjectiveMarker.Refresh();
        Sauvegarder();
    }

    /// <summary>Génère la question-calcul ({auto}) à partir de la dernière saisie.</summary>
    void GenererCalculAuto()
    {
        var q = QueteActuelle();
        if (q != null && q.kind == QuestKind.Question && q.reponseAttendue == "{auto}"
            && long.TryParse(derniereSaisie, out long n))
        {
            long a = Random.Range(2, 10);
            long b = Random.Range(2, 6);
            q.description     = $"Le CPU a reçu {n} via Console.ReadLine(). Calcule : {n} + {a} * {b} = ?";
            q.reponseAttendue = (n + a * b).ToString();
        }
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

    // ── briefing : révéler les missions une par une au CPU ────────────────

    /// <summary>Vrai si la mission active n'a pas encore été vue au CPU (briefing à faire).</summary>
    public bool BriefingEnAttente()
    {
        return !ToutesQuetesTerminees() && missionRevelee < questIndex;
    }

    /// <summary>Appelée quand le joueur entre au CPU : révèle la mission active.</summary>
    public void RevelerMissionActuelle()
    {
        if (questIndex > missionRevelee)
        {
            missionRevelee = questIndex;
            DemarrerChronoMission();     // le chrono de la ligne démarre au briefing
            AudioFX.Succes();
            VoiceOver.AnnoncerMission(); // la radio détaille la mission révélée
            MissionHUD.Refresh();
            ObjectiveMarker.Refresh();
            Sauvegarder();
        }
    }

    // ── sauvegarde de progression (PlayerPrefs) ───────────────────────────

    /// <summary>Sauvegarde missions, compteurs, erreurs, temps et contenu RAM.</summary>
    public void Sauvegarder()
    {
        PlayerPrefs.SetInt("cda_actif", 1);
        PlayerPrefs.SetInt("cda_questIndex", questIndex);
        PlayerPrefs.SetInt("cda_erreurs", nbErreurs);
        PlayerPrefs.SetFloat("cda_temps", TempsCampagne);
        PlayerPrefs.SetString("cda_saisie", derniereSaisie);

        var completes = new System.Text.StringBuilder();
        foreach (var q in quests)
        {
            completes.Append(q.complete ? '1' : '0');
            if (q.kind == QuestKind.Compteur) PlayerPrefs.SetInt("cda_compteur", q.compteur);
        }
        PlayerPrefs.SetString("cda_completes", completes.ToString());

        var ram = new System.Text.StringBuilder();
        foreach (var s in ramSlots)
        {
            if (ram.Length > 0) ram.Append('\n');
            ram.Append(s.filled ? '1' : '0').Append('|')
               .Append(s.variable).Append('|')
               .Append(s.value).Append('|')
               .Append(s.type).Append('|')
               .Append(ColorUtility.ToHtmlStringRGBA(s.color));
        }
        PlayerPrefs.SetString("cda_ram", ram.ToString());

        PlayerPrefs.SetInt("cda_version", 2); // format campagne « programme 6 lignes »
        PlayerPrefs.SetInt("cda_etape", missionEtape);
        PlayerPrefs.SetString("cda_calc", $"{cpuX}|{cpuZ}|{cpuSomme}|{cpuY}");
        PlayerPrefs.SetInt("cda_revelee", missionRevelee);
        PlayerPrefs.Save();
    }

    /// <summary>Restaure la progression sauvegardée (si elle existe).</summary>
    void Charger()
    {
        if (PlayerPrefs.GetInt("cda_actif", 0) != 1) return;

        // Ancienne sauvegarde (campagne 3 missions) : incompatible → on repart à zéro.
        if (PlayerPrefs.GetInt("cda_version", 1) < 2)
        {
            Debug.Log("[GameState] Ancienne sauvegarde détectée : campagne réinitialisée (nouveau programme).");
            EffacerSauvegarde();
            return;
        }

        questIndex      = Mathf.Clamp(PlayerPrefs.GetInt("cda_questIndex", 0), 0, quests.Count - 1);
        missionRevelee  = PlayerPrefs.GetInt("cda_revelee", -1);
        nbErreurs       = PlayerPrefs.GetInt("cda_erreurs", 0);
        _tempsAnterieur = PlayerPrefs.GetFloat("cda_temps", 0f);
        derniereSaisie  = PlayerPrefs.GetString("cda_saisie", "");

        string completes = PlayerPrefs.GetString("cda_completes", "");
        for (int i = 0; i < quests.Count && i < completes.Length; i++)
        {
            quests[i].complete = completes[i] == '1';
            if (quests[i].kind == QuestKind.Compteur && !quests[i].complete)
                quests[i].compteur = PlayerPrefs.GetInt("cda_compteur", 0);
        }

        // Le programme s'est allongé depuis la sauvegarde (ex : ligne 7 ajoutée) :
        // si la quête courante est déjà terminée, on avance à la première non faite.
        while (questIndex < quests.Count - 1 && quests[questIndex].complete) questIndex++;

        // Étape de la mission active + valeurs mémorisées par le CPU
        missionEtape = PlayerPrefs.GetInt("cda_etape", 0);
        var calc = PlayerPrefs.GetString("cda_calc", "").Split('|');
        if (calc.Length >= 3)
        {
            long.TryParse(calc[0], out cpuX);
            long.TryParse(calc[1], out cpuZ);
            long.TryParse(calc[2], out cpuSomme);
            if (calc.Length >= 4) cpuY = calc[3];
        }
        var qc = QueteActuelle();
        if (qc != null) qc.indication = IndicationActuelle();

        string ram = PlayerPrefs.GetString("cda_ram", "");
        if (!string.IsNullOrEmpty(ram))
        {
            var lignes = ram.Split('\n');
            EnsureRamSlots(lignes.Length);
            for (int i = 0; i < lignes.Length; i++)
            {
                var p = lignes[i].Split('|');
                if (p.Length < 5) continue;
                var s = ramSlots[i];
                s.filled   = p[0] == "1";
                s.variable = p[1];
                s.value    = p[2];
                s.type     = p[3];
                if (ColorUtility.TryParseHtmlString("#" + p[4], out var c)) s.color = c;
            }
        }

        Debug.Log($"[GameState] Progression chargée : mission {questIndex + 1}/{quests.Count}, {nbErreurs} erreur(s).");

        // Campagne déjà terminée → on remontre le rating (et l'option Recommencer).
        if (ToutesQuetesTerminees()) RatingScreen.Afficher();
    }

    /// <summary>Efface uniquement les clés de sauvegarde (sans relancer la scène).</summary>
    void EffacerSauvegarde()
    {
        foreach (var k in new[] { "cda_actif", "cda_questIndex", "cda_erreurs", "cda_temps",
                                  "cda_saisie", "cda_completes", "cda_compteur", "cda_ram",
                                  "cda_calcStep", "cda_calc", "cda_revelee", "cda_etape", "cda_version" })
            PlayerPrefs.DeleteKey(k);
        for (int i = 0; i < 10; i++) PlayerPrefs.DeleteKey("cda_stat_" + i); // stats par ligne
        PlayerPrefs.Save();
    }

    /// <summary>Efface la sauvegarde et relance la campagne du début.</summary>
    public void ReinitialiserCampagne()
    {
        EffacerSauvegarde();

        quests.Clear();
        _questsInit = false;
        InitQuests();
        questIndex = 0;
        missionRevelee = -1;
        nbErreurs  = 0;
        _tempsAnterieur = 0f;
        _sessionDebut   = Time.realtimeSinceStartup;
        derniereSaisie  = "";

        foreach (var s in ramSlots) s.Vider();
        boxExists = false; boxVariable = ""; boxValue = "";
        needsSpawn = false; spawnDansLaMain = false; boxVientDeRam = false;
        missionEtape = 0; cpuX = cpuZ = cpuSomme = 0; cpuY = "";

        VoiceOver.Reinitialiser();
        BriefingCinematic.Reinitialiser(); // la cinématique rejouera
        MissionHUD.Refresh();
        ObjectiveMarker.Refresh();
        SceneManager.LoadScene(mainSceneName);
    }

    // ── transitions ───────────────────────────────────────────────────────

    public void EnregistrerBox(string variable, string valeur, string type = "int", Color? color = null, Material mat = null)
    {
        boxVariable = variable;
        boxValue    = valeur;
        boxType     = type;
        if (color.HasValue) boxColor = color.Value;
        boxMaterialAsset = mat;
        boxExists      = true;
        needsSpawn     = true;
        boxVientDeRam  = false; // box fraîchement déclarée, pas reprise de la RAM
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

    /// <summary>
    /// Dépose la box tenue en main dans la RAM. Si une case porte déjà ce nom,
    /// elle est ÉCRASÉE (même adresse mémoire) ; sinon première case libre.
    /// Retourne l'index de la case, ou -1 (RAM pleine).
    /// </summary>
    public int DeposerAuto()
    {
        if (!boxExists) return -1;
        EnsureRamSlots(12);

        int i = IndexSlot(boxVariable); // même nom → écrase sa case
        if (i < 0)
            for (int k = 0; k < ramSlots.Count; k++)
                if (!ramSlots[k].filled) { i = k; break; }
        if (i < 0) return -1; // RAM pleine : la box reste en main

        string nomDepose = boxVariable;
        var s = ramSlots[i];
        s.filled   = true;
        s.variable = boxVariable;
        s.value    = boxValue;
        s.type     = boxType;
        s.color    = boxColor;
        s.material = boxMaterialAsset;

        ConsommerBoxEnMain(); // la box quitte la main
        AudioFX.Depot();

        // Badges liés au contenu de la RAM
        int occupees = 0;
        foreach (var slot in ramSlots) if (slot.filled) occupees++;
        Badges.MemoireBienRemplie(occupees);

        // Missions validées par un dépôt en RAM
        var q = QueteActuelle();
        if (q != null && !q.complete)
        {
            if      (q.kind == QuestKind.SaisieEcran && missionEtape == 2 && nomDepose == "y")     CompleterQueteActuelle();
            else if (q.kind == QuestKind.Parse       && missionEtape == 1 && nomDepose == "z")     CompleterQueteActuelle();
            else if (q.kind == QuestKind.Calcul      && missionEtape == 2 && nomDepose == "somme") CompleterQueteActuelle();
            else if (q.kind == QuestKind.Boucle      && nomDepose == "i")
            {
                // Tour de boucle accompli : i++ (retourne au CPU pour le test suivant).
                missionEtape++;
                AudioFX.Succes();
                MajIndication();
            }
            else if (q.kind == QuestKind.Rangement) CompleterQueteActuelle(); // legacy
        }
        Sauvegarder();
        return i;
    }

    /// <summary>
    /// Prend une COPIE de la box de la case i (la case reste remplie, comme une
    /// vraie RAM : lire ne détruit pas). Le cube apparaît en main dans Main.
    /// </summary>
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
        boxVientDeRam    = true; // reprise depuis la RAM

        // La case N'EST PAS vidée : on repart avec une copie.
        Sauvegarder();
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
        SpawnBoxMaintenant();
    }

    /// <summary>Fait apparaître la box immédiatement (et en main si demandé).</summary>
    public void SpawnBoxMaintenant()
    {
        if (!boxExists) return;

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
        GameObject box;

        if (boxPrefab != null)
        {
            // Boîte du prof (modèle de la RAM).
            box = Instantiate(boxPrefab, pos, Quaternion.identity);
            box.transform.localScale = Vector3.one * Mathf.Max(0.01f, boxScale);
            foreach (var sel in box.GetComponentsInChildren<RAMBoxSelector>(true)) Destroy(sel);
            AppliquerTextesBoite(box.transform);
            TeinterBoite(box.transform, boxColor); // même couleur que la boîte en RAM
        }
        else
        {
            // Fallback : cube coloré généré par code.
            box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position   = pos;
            box.transform.localScale = Vector3.one * 0.4f;
            var rend   = box.GetComponent<Renderer>();
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

        box.name = $"DataBox_{boxVariable}";

        // Collider requis par PickupItem.
        if (box.GetComponentInChildren<Collider>() == null)
            box.AddComponent<BoxCollider>();

        // Composants logiques.
        var db = box.GetComponent<DataBox>() ?? box.AddComponent<DataBox>();
        db.variableName = boxVariable;
        db.value        = boxValue;
        db.typeName     = boxType;

        var pi = box.GetComponent<PickupItem>() ?? box.AddComponent<PickupItem>();
        pi.itemId          = "data_" + boxVariable;
        pi.displayText     = boxValue;
        pi.codeLabel       = $"{boxType} {boxVariable} = {boxValue};";
        pi.heldScaleFactor = 1f; // déjà à la bonne échelle

        AjouterLabelBoite(box.transform);
        AjouterParticulesBoite(box.transform, boxColor); // étincelles à la couleur du type

        Debug.Log($"[GameState] Boîte respawnée à {pos} : {boxType} {boxVariable} = {boxValue}");
        return box;
    }

    // ── particules colorées de la boîte portée ────────────────────────────

    static Texture2D _texEtincelle;

    /// <summary>Douces étincelles autour de la boîte, à la couleur de son type.</summary>
    void AjouterParticulesBoite(Transform box, Color c)
    {
        var go = new GameObject("ParticulesType");
        go.transform.SetParent(box, false);
        go.transform.localPosition = Vector3.zero;

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
        main.startColor      = new Color(c.r, c.g, c.b, 0.9f);
        main.gravityModifier = -0.05f; // les étincelles montent doucement
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 40;

        var emission = ps.emission;
        emission.rateOverTime = 9f;

        var forme = ps.shape;
        forme.shapeType = ParticleSystemShapeType.Sphere;
        forme.radius    = 0.5f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var rend = go.GetComponent<ParticleSystemRenderer>();
        var sh   = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Sprites/Default");
        var mat  = new Material(sh);
        if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", Color.white);
        mat.mainTexture = TexEtincelle();
        rend.material = mat;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static Texture2D TexEtincelle()
    {
        if (_texEtincelle != null) return _texEtincelle;
        const int S = 32;
        _texEtincelle = new Texture2D(S, S, TextureFormat.RGBA32, false);
        _texEtincelle.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            float dx = (x - S / 2f) / (S / 2f), dy = (y - S / 2f) / (S / 2f);
            float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
            _texEtincelle.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
        }
        _texEtincelle.Apply();
        return _texEtincelle;
    }

    /// <summary>Teinte le carton de la boîte avec la couleur EXACTE donnée (sans toucher aux textes).</summary>
    public static void TeinterBoite(Transform box, Color c)
    {
        foreach (var rend in box.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.GetComponent<TMP_Text>() != null) continue; // pas les textes TMP
            foreach (var mat in rend.materials)
            {
                if      (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                else if (mat.HasProperty("_Color"))     mat.color = c;
            }
        }
    }

    /// <summary>Renseigne les textes 'nom' / 'valeur' / 'type' du prefab (s'ils existent).</summary>
    void AppliquerTextesBoite(Transform box)
    {
        foreach (var tmp in box.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = tmp.gameObject.name.Trim().ToLowerInvariant();
            if      (n == "nom")    tmp.text = boxVariable;
            else if (n == "valeur") tmp.text = boxValue;
            else if (n == "type")   tmp.text = boxType;
        }
    }

    /// <summary>Label flottant nom = valeur au-dessus de la boîte (face caméra).</summary>
    void AjouterLabelBoite(Transform box)
    {
        var old = box.Find("BoxLabel");
        if (old != null) Destroy(old.gameObject);

        var go = new GameObject("BoxLabel");
        go.transform.SetParent(box, false);
        go.transform.localPosition = Vector3.up * 1.3f;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text      = string.IsNullOrEmpty(boxValue) ? $"{boxType} {boxVariable}"
                                                       : $"{boxVariable} = {boxValue}";
        tmp.fontSize  = 3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 200);
        tmp.rectTransform.sizeDelta = new Vector2(8f, 2f);

        go.AddComponent<LookAtCamera>();
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
