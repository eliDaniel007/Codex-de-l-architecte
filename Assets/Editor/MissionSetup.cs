#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Outils d'édition pour configurer le projet Clavier/RAM/Console en un clic.
/// Tout est accessible via le menu Unity → Mission.
/// </summary>
public static class MissionSetup
{
    const string CLAVIER_SCENE_PATH = "Assets/Scenes/Clavier.unity";

    // ──────────────────────────────────────────────────────────────────────
    // 1) Tout-en-un
    // ──────────────────────────────────────────────────────────────────────
    [MenuItem("Mission/Tout configurer (1 clic)")]
    public static void ToutConfigurer()
    {
        // 1. Créer Clavier.unity si manquante
        if (!File.Exists(CLAVIER_SCENE_PATH))
            CreerSceneClavier();

        // 2. Ajouter les 3 scènes dans Build Settings
        ConfigurerBuildSettings();

        // 3. Configurer la scène active
        ConfigurerSceneActive();

        EditorUtility.DisplayDialog("Mission Setup",
            "Configuration terminée.\n\n" +
            "• Clavier.unity créée\n" +
            "• Build Settings mis à jour (MainScene, RAM, Clavier)\n" +
            "• Scène active configurée\n\n" +
            "Ouvre RAM.unity et clique 'Mission/Configurer la scène active' pour ajouter le contrôleur RAM.",
            "OK");
    }

    // ──────────────────────────────────────────────────────────────────────
    // 2) Build Settings
    // ──────────────────────────────────────────────────────────────────────
    [MenuItem("Mission/Configurer Build Settings")]
    public static void ConfigurerBuildSettings()
    {
        var scenesVoulues = new[]
        {
            "Assets/Scenes/MainScene.unity",
            "Assets/Scenes/Clavier.unity",
            "Assets/Scenes/RAM.unity",
        };

        var actuelles = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var dejaPresentes = new HashSet<string>();
        foreach (var s in actuelles) dejaPresentes.Add(s.path);

        foreach (var path in scenesVoulues)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[MissionSetup] {path} n'existe pas — ignorée.");
                continue;
            }
            if (dejaPresentes.Contains(path)) continue;
            actuelles.Add(new EditorBuildSettingsScene(path, true));
            Debug.Log($"[MissionSetup] Build Settings + {path}");
        }

        EditorBuildSettings.scenes = actuelles.ToArray();
    }

    // ──────────────────────────────────────────────────────────────────────
    // 3) Création scène Clavier
    // ──────────────────────────────────────────────────────────────────────
    [MenuItem("Mission/Créer la scène Clavier")]
    public static void CreerSceneClavier()
    {
        if (File.Exists(CLAVIER_SCENE_PATH))
        {
            if (!EditorUtility.DisplayDialog("Scène Clavier existe déjà",
                "Assets/Scenes/Clavier.unity existe déjà. La recréer écrasera son contenu.",
                "Écraser", "Annuler"))
                return;
        }

        // Sauvegarde l'ouverture courante avant de partir
        if (EditorSceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags    = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
        camGO.tag = "MainCamera";
        SceneManager.MoveGameObjectToScene(camGO, scene);

        // EventSystem (compatible Input System via réflexion)
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        var uiTypeCreate = ResolveInputSystemUIModule();
        if (uiTypeCreate != null) esGO.AddComponent(uiTypeCreate);
        else esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        SceneManager.MoveGameObjectToScene(esGO, scene);

        // Contrôleur
        var ctrlGO = new GameObject("ClavierController");
        ctrlGO.AddComponent<ClavierSceneController>();
        SceneManager.MoveGameObjectToScene(ctrlGO, scene);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, CLAVIER_SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log($"[MissionSetup] Scène Clavier créée : {CLAVIER_SCENE_PATH}");
    }

    // ──────────────────────────────────────────────────────────────────────
    // 4) Configurer la scène ouverte
    // ──────────────────────────────────────────────────────────────────────
    [MenuItem("Mission/Configurer la scène active")]
    public static void ConfigurerSceneActive()
    {
        var scene = EditorSceneManager.GetActiveScene();
        string nom = scene.name;

        if (nom == "MainScene") ConfigurerMainScene();
        else if (nom == "RAM")  ConfigurerRamScene();
        else if (nom == "Clavier") ConfigurerClavierScene();
        else
        {
            Debug.LogWarning($"[MissionSetup] Scène active '{nom}' non reconnue. " +
                             "Ouvre MainScene ou RAM puis relance.");
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[MissionSetup] Scène '{nom}' configurée et sauvegardée.");
    }

    // ── MainScene ─────────────────────────────────────────────────────────
    static void ConfigurerMainScene()
    {
        // Supprime les anciens LoadSceneOnPlayerEnter (redondants maintenant)
        foreach (var c in Object.FindObjectsByType<LoadSceneOnPlayerEnter>(FindObjectsSortMode.None))
        {
            Debug.Log($"[MissionSetup] Suppression de LoadSceneOnPlayerEnter sur '{c.name}'");
            Object.DestroyImmediate(c);
        }

        // basic_keyboard → KeyboardTerminal + BoxCollider trigger
        var clavier = TrouverParNomFlexible("basic_keyboard", "keyboard", "clavier");
        if (clavier != null)
            ConfigurerTrigger<KeyboardTerminal>(clavier, "basic_keyboard");
        else
            Debug.LogWarning("[MissionSetup] Aucun objet 'basic_keyboard' trouvé.");

        // Quick memory → MemoryCell + BoxCollider trigger
        var ram = TrouverParNomFlexible("Quick memory", "ram", "quick");
        if (ram != null)
            ConfigurerTrigger<MemoryCell>(ram, "Quick memory");
        else
            Debug.LogWarning("[MissionSetup] Aucun objet 'Quick memory' trouvé.");

        // flat_screen_television → ConsoleScreen + BoxCollider trigger
        var ecran = TrouverParNomFlexible("flat_screen_television", "screen", "ecran", "console");
        if (ecran != null)
            ConfigurerTrigger<ConsoleScreen>(ecran, "flat_screen_television");
        else
            Debug.LogWarning("[MissionSetup] Aucun objet 'flat_screen_television' trouvé.");

        // BoxSpawnPoint (devant le clavier ou (0,1,0))
        if (GameObject.Find("BoxSpawnPoint") == null)
        {
            var sp = new GameObject("BoxSpawnPoint");
            if (clavier != null)
            {
                var rend = clavier.GetComponentInChildren<Renderer>();
                Vector3 basePos = rend != null ? rend.bounds.center : clavier.transform.position;
                sp.transform.position = basePos + Vector3.up * 1f + clavier.transform.forward * 1.2f;
            }
            else
            {
                sp.transform.position = new Vector3(0f, 1f, 0f);
            }
            Debug.Log("[MissionSetup] BoxSpawnPoint créé à " + sp.transform.position);
        }

        // GameState (au cas où l'utilisateur veut le voir dans la hiérarchie)
        if (Object.FindFirstObjectByType<GameState>() == null)
        {
            var gs = new GameObject("[GameState]");
            gs.AddComponent<GameState>();
            Debug.Log("[MissionSetup] GameState ajouté.");
        }
    }

    static void ConfigurerRamScene()
    {
        // Supprime les anciens LoadSceneOnPlayerEnter
        foreach (var c in Object.FindObjectsByType<LoadSceneOnPlayerEnter>(FindObjectsSortMode.None))
        {
            Debug.Log($"[MissionSetup] Suppression de LoadSceneOnPlayerEnter sur '{c.name}'");
            Object.DestroyImmediate(c);
        }

        if (Object.FindFirstObjectByType<RAMSceneController>() == null)
        {
            var go = new GameObject("RAMController");
            go.AddComponent<RAMSceneController>();
            Debug.Log("[MissionSetup] RAMSceneController ajouté.");
        }

        EnsureEventSystem();
    }

    static void ConfigurerClavierScene()
    {
        if (Object.FindFirstObjectByType<ClavierSceneController>() == null)
        {
            var go = new GameObject("ClavierController");
            go.AddComponent<ClavierSceneController>();
            Debug.Log("[MissionSetup] ClavierSceneController ajouté.");
        }

        EnsureEventSystem();
    }

    static System.Type ResolveInputSystemUIModule()
    {
        return System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
    }

    static void EnsureEventSystem()
    {
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        var uiType = ResolveInputSystemUIModule();

        if (es == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            if (uiType != null) go.AddComponent(uiType);
            else go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[MissionSetup] EventSystem créé.");
        }
        else if (uiType != null)
        {
            var old = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (old != null)
            {
                Object.DestroyImmediate(old);
                if (es.GetComponent(uiType) == null)
                    es.gameObject.AddComponent(uiType);
                Debug.Log("[MissionSetup] StandaloneInputModule remplacé par InputSystemUIInputModule.");
            }
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────
    static void ConfigurerTrigger<T>(GameObject go, string label) where T : Component
    {
        // BoxCollider trigger
        var col = go.GetComponent<Collider>();
        if (col == null)
        {
            var bc = go.AddComponent<BoxCollider>();
            bc.size     = new Vector3(3f, 2f, 3f);
            bc.isTrigger = true;
            Debug.Log($"[MissionSetup] BoxCollider trigger ajouté sur {label}");
        }
        else
        {
            col.isTrigger = true;
        }

        // Composant
        if (go.GetComponent<T>() == null)
        {
            go.AddComponent<T>();
            Debug.Log($"[MissionSetup] {typeof(T).Name} ajouté sur {label}");
        }
    }

    static GameObject TrouverParNomFlexible(params string[] motsCles)
    {
        var tous = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in tous)
        {
            string n = go.name.ToLower();
            foreach (var mc in motsCles)
                if (n.Contains(mc.ToLower())) return go;
        }
        return null;
    }
}
#endif
