using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Affiche UNIQUEMENT la valeur sur le moniteur 3D dans l'environnement.
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

        var held = _holder.HeldItem;
        var db   = held != null ? held.GetComponent<DataBox>() : null;

        // Dès qu'on entre dans le périmètre de l'écran avec une box en main,
        // on dépose automatiquement (plus de bouton E).
        if (db != null)
            Afficher(held, db);
    }

    void Afficher(PickupItem item, DataBox db)
    {
        var gs = GameState.I;
        bool   vientDeRam = gs.boxVientDeRam; // à capturer AVANT consommation
        string valeur     = db.value;

        if (_holder != null) _holder.ConsumeHeld();
        item.OnDropped();
        Destroy(item.gameObject);
        gs.ConsommerPourEcran();
        AudioFX.Depot();
        PromptUI.Hide();

        // Validation des missions liées à l'écran
        var q = gs.QueteActuelle();
        if (q != null && !q.complete)
        {
            switch (q.kind)
            {
                case QuestKind.Affichage:
                    gs.CompleterQueteActuelle();
                    break;

                case QuestKind.Condition:
                    if (double.TryParse(valeur.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double v)
                        && q.TesterCondition(v))
                    {
                        gs.CompleterQueteActuelle();
                    }
                    else
                    {
                        AudioFX.Erreur();
                        PromptUI.Show($"if (valeur {q.conditionOp} {q.conditionSeuil}) → <color=#FF6464>FAUX</color> avec {valeur}. Réessaie !");
                        Invoke(nameof(CacherPrompt), 3.5f);
                    }
                    break;

                case QuestKind.LectureRam:
                    if (vientDeRam)
                    {
                        gs.CompleterQueteActuelle();
                    }
                    else
                    {
                        AudioFX.Erreur();
                        PromptUI.Show("Cette variable ne vient pas de la RAM. Va d'abord la reprendre dans la RAM !");
                        Invoke(nameof(CacherPrompt), 3.5f);
                    }
                    break;
            }
        }

        if (_screenText != null)
        {
            // Affiche UNIQUEMENT le nombre (ex: 25)
            _screenText.text = valeur;
            _screenText.gameObject.SetActive(true);
            _screenText.transform.parent.gameObject.SetActive(true);
        }
    }

    void CacherPrompt() => PromptUI.Hide();

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
        
        // Plus de fond noir ("gros truc noir") comme demandé
        
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