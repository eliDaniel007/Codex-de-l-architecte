using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// L'écran de la console dans MainScene.
///  • Console.WriteLine : on pose une box (reprise de la RAM) → la valeur s'affiche
///    sur le moniteur 3D. Missions 2 et 6 (LectureRam) validées ici.
///  • Console.ReadLine (mission 3, étape 1) : [E] → l'utilisateur « envoie » un
///    nombre aléatoire, qui devient la box  string y  directement en main.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ConsoleScreen : MonoBehaviour
{
    [Header("Configuration")]
    public string playerTag = "Player";
    public bool forceTrigger = true;

    [Header("Écran 3D")]
    public Transform screenFace;

    private Transform    _player;
    private PlayerHolder _holder;
    private TextMeshPro  _screenText;
    private bool         _inRange;
    private float        _prochainMsgErreur; // anti-spam des messages d'erreur

    void Start()
    {
        if (forceTrigger)
            foreach (var c in GetComponents<Collider>()) c.isTrigger = true;

        FindPlayer();

        // Initialisation de l'affichage 3D sur le moniteur
        Transform anchor = screenFace != null ? screenFace : transform;
        _screenText = Setup3DMonitorText(anchor);

    }

    private void FindPlayer()
    {
        var pg = GameObject.FindGameObjectWithTag(playerTag);
        if (pg != null)
        {
            _player = pg.transform;
            _holder = pg.GetComponent<PlayerHolder>();
            if (_holder == null) _holder = pg.GetComponentInChildren<PlayerHolder>();
        }
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag(playerTag)) _inRange = true; }
    void OnTriggerStay(Collider other) { if (other.CompareTag(playerTag)) _inRange = true; }
    void OnTriggerExit(Collider other) { if (other.CompareTag(playerTag)) { _inRange = false; PromptUI.Hide(); } }

    void Update()
    {
        if (!_inRange) return;
        if (_holder == null) { FindPlayer(); if (_holder == null) return; }

        var gs = GameState.I;
        var q  = gs.QueteActuelle();

        var held = _holder.HeldItem;
        var db   = held != null ? held.GetComponent<DataBox>() : null;

        if (db != null)
        {
            // ── Une box en main devant l'écran ──
            if (q != null && !q.complete && q.kind == QuestKind.LectureRam)
            {
                if (db.variableName == q.cibleVariable && gs.boxVientDeRam)
                {
                    AfficherEtConsommer(held, db, valider: true);
                }
                else if (Time.time >= _prochainMsgErreur)
                {
                    _prochainMsgErreur = Time.time + 4f;
                    AudioFX.Erreur();
                    gs.SignalerErreur();
                    PromptUI.Show($"L'écran attend <color=#00D9FF>{q.cibleVariable}</color> repris depuis la RAM. (Ta boîte n'est pas détruite.)");
                }
            }
            else if (q != null && !q.complete && q.kind == QuestKind.ConditionIf &&
                     gs.missionEtape == 1)
            {
                // Ligne 7 : ce n'est pas l'écran qui consomme le booléen — il faut
                // TRAVERSER la porte de la branche qui s'exécute.
                PromptUI.Show("Passe par la <b>PORTE</b> de la branche qui s'exécute — l'écran affichera son message.");
            }
            else if (q != null && !q.complete && q.kind == QuestKind.SaisieEcran && gs.missionEtape == 2)
            {
                PromptUI.Show("Cette boîte va dans la <color=#00D9FF>RAM</color>, pas sur l'écran. Range y en mémoire !");
            }
            else if (q == null || gs.ToutesQuetesTerminees())
            {
                // Campagne finie : affichage libre (bac à sable).
                AfficherEtConsommer(held, db, valider: false);
            }
            else if (!gs.EnGrace)
            {
                // Une mission est en cours et cette valeur ne va pas sur l'écran :
                // on NE la détruit PAS (elle sert ailleurs — CPU ou RAM).
                string ou = GameState.NomStation(gs.StationAttendue());
                PromptUI.Show($"<color=#FF6B6B>Pas sur l'écran !</color>  Cette valeur va vers <b>{ou}</b>.");
            }
        }
        else
        {
            // Mains vides devant l'écran alors que la mission attend ailleurs —
            // sauf juste après une interaction réussie (période de grâce).
            string attendue = gs.StationAttendue();
            if (!gs.EnGrace && attendue != "ecran" && attendue != "")
                PromptUI.Show($"<color=#FF6B6B>Rien à afficher ici !</color>  Va plutôt vers <b>{GameState.NomStation(attendue)}</b>.");
        }
    }

    // ── Console.WriteLine ─────────────────────────────────────────────────

    void AfficherEtConsommer(PickupItem item, DataBox db, bool valider)
    {
        var gs = GameState.I;
        string valeur    = db.value;
        bool   estString = db.typeName == "string";

        if (_holder != null) _holder.ConsumeHeld();
        item.OnDropped();
        Destroy(item.gameObject);
        gs.ConsommerPourEcran();
        AudioFX.Depot();
        PromptUI.Hide();

        if (valider) gs.CompleterQueteActuelle(); // missions 2 et 6

        MontrerSurMoniteur(estString ? $"\"{valeur}\"" : valeur);
    }

    // Accès statique au moniteur (utilisé par les portes du if).
    static ConsoleScreen _instance;

    /// <summary>Affiche un texte sur le moniteur 3D de la console.</summary>
    public static void AfficherSurMoniteur(string texte)
    {
        if (_instance == null) _instance = FindFirstObjectByType<ConsoleScreen>();
        if (_instance != null) _instance.MontrerSurMoniteur(texte);
    }

    void MontrerSurMoniteur(string texte)
    {
        if (_screenText == null) return;
        _screenText.text = texte;
        _screenText.gameObject.SetActive(true);
        _screenText.transform.parent.gameObject.SetActive(true);
    }

    static bool AppuyeE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    TextMeshPro Setup3DMonitorText(Transform anchor)
    {
        // Nettoyage si existant
        var old = anchor.Find("3D_Display_Root");
        if (old != null) Destroy(old.gameObject);

        var root = new GameObject("3D_Display_Root");
        root.transform.SetParent(anchor, false);

        // Positionnement sur la face avant qui s'affichait précédemment
        root.transform.localPosition = new Vector3(-18.7f, 35.8f, 0f);

        // On remet une rotation droite (0 sur Z)
        root.transform.localRotation = Quaternion.Euler(0, 90, 0);

        // Texte 3D (TextMeshPro)
        var txtGO = new GameObject("3D_Monitor_Text");
        txtGO.transform.SetParent(root.transform, false);
        var tmp = txtGO.AddComponent<TextMeshPro>();
        tmp.fontSize = 80f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0, 1, 0.5f); // Vert console
        tmp.richText = true;
        tmp.rectTransform.sizeDelta = new Vector2(110f, 65f);

        root.SetActive(false); // Caché par défaut

        return tmp;
    }
}
