using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// LES DEUX PORTES DU  if  (ligne 7) : quand le joueur revient de l'UAL avec
/// son BOOLÉEN, deux portes apparaissent devant l'écran — une par branche :
///
///     [ VRAI  → "grand" ]      [ FAUX → "petit" ]
///
/// Seule la porte correspondant au booléen porté S'OUVRE. L'autre est murée :
/// la traverser est impossible, et s'y frotter rappelle qu'une branche dont la
/// condition est fausse NE S'EXÉCUTE JAMAIS. Passer la bonne porte exécute la
/// branche : l'écran affiche le message, la ligne est validée.
/// Auto-créé dans la MainScene ; les portes n'existent qu'à l'étape 1 du if.
/// </summary>
public class PortesCondition : MonoBehaviour
{
    private GameObject _racine;      // les deux portes (détruites hors étape)
    private float      _prochainMsg; // anti-spam du message « mauvaise porte »

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => CreerSiBesoin();
        CreerSiBesoin();
    }

    static void CreerSiBesoin()
    {
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (FindFirstObjectByType<PortesCondition>() != null) return;

        var go = new GameObject("[PortesCondition]");
        go.AddComponent<PortesCondition>();
    }

    void Update()
    {
        var gs = GameState.I;
        var q  = gs.QueteActuelle();
        bool actif = q != null && !q.complete && q.kind == QuestKind.ConditionIf &&
                     gs.missionEtape == 1;

        if (actif && _racine == null)       Construire();
        else if (!actif && _racine != null) { Destroy(_racine); _racine = null; }
    }

    // ── construction des deux portes ──────────────────────────────────────

    void Construire()
    {
        var ecran = FindFirstObjectByType<ConsoleScreen>();
        if (ecran == null) return;

        // Les portes se dressent entre le joueur et l'écran.
        Vector3 posEcran = ecran.transform.position;
        var joueurGO = GameObject.FindGameObjectWithTag("Player");
        Vector3 vers = joueurGO != null ? joueurGO.transform.position - posEcran : Vector3.forward;
        vers.y = 0f;
        vers = vers.sqrMagnitude < 0.01f ? Vector3.forward : vers.normalized;

        Vector3 centre = posEcran + vers * 11f; // bien détachées de l'écran
        if (Physics.Raycast(centre + Vector3.up * 10f, Vector3.down, out var hit, 30f))
            centre = hit.point;

        Vector3 droite = Vector3.Cross(Vector3.up, vers).normalized;
        Quaternion face = Quaternion.LookRotation(-vers); // les portes font face au joueur

        _racine = new GameObject("PortesDuIf");

        bool vrai = GameState.I.cpuIfVrai; // la branche qui s'exécute
        // Portes bien ÉCARTÉES (5.5 m de chaque côté) : leurs étiquettes VRAI /
        // FAUX ne se chevauchent plus et le choix est visuellement net.
        Porte(centre + droite * 5.5f, face, "VRAI",
              "if  →  \"grand\"", new Color(0.25f, 0.85f, 0.4f), estOuverte: vrai);
        Porte(centre - droite * 5.5f, face, "FAUX",
              "else  →  \"petit\"", new Color(0.9f, 0.3f, 0.3f), estOuverte: !vrai);

        Debug.Log($"[PortesCondition] Portes construites — branche exécutée : {(vrai ? "VRAI" : "FAUX")}.");
    }

    void Porte(Vector3 basePos, Quaternion rot, string nom, string sousLabel, Color couleur, bool estOuverte)
    {
        var porte = new GameObject("Porte_" + nom);
        porte.transform.SetParent(_racine.transform, false);
        porte.transform.SetPositionAndRotation(basePos, rot);

        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(sh);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", couleur);
        else mat.color = couleur;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", couleur * (estOuverte ? 1.2f : 0.35f));
        }

        // Montants + linteau (cadre de 2 m de large, 3 m de haut)
        foreach (float signe in new[] { -1f, 1f })
        {
            var poteau = GameObject.CreatePrimitive(PrimitiveType.Cube);
            poteau.name = "Montant";
            poteau.transform.SetParent(porte.transform, false);
            poteau.transform.localPosition = new Vector3(signe * 1.0f, 1.5f, 0f);
            poteau.transform.localScale    = new Vector3(0.28f, 3f, 0.28f);
            poteau.GetComponent<Renderer>().material = mat;
        }
        var linteau = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linteau.name = "Linteau";
        linteau.transform.SetParent(porte.transform, false);
        linteau.transform.localPosition = new Vector3(0f, 3.1f, 0f);
        linteau.transform.localScale    = new Vector3(2.35f, 0.3f, 0.3f);
        linteau.GetComponent<Renderer>().material = mat;

        // Étiquette au-dessus : la branche et ce qu'elle affiche
        var txtGO = new GameObject("Etiquette");
        txtGO.transform.SetParent(porte.transform, false);
        txtGO.transform.localPosition = new Vector3(0f, 3.8f, 0f);
        var tmp = txtGO.AddComponent<TextMeshPro>();
        tmp.text      = $"{nom}\n<size=55%>{sousLabel}</size>";
        tmp.fontSize  = 5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = couleur;
        tmp.outlineWidth = 0.22f;
        tmp.outlineColor = new Color32(0, 0, 0, 230);
        tmp.rectTransform.sizeDelta = new Vector2(6f, 2.4f);
        txtGO.AddComponent<LookAtCamera>();

        if (estOuverte)
        {
            // Passage LIBRE + détecteur de traversée (au milieu du cadre).
            var zone = new GameObject("ZoneTraversee");
            zone.transform.SetParent(porte.transform, false);
            zone.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var trig = zone.AddComponent<BoxCollider>();
            trig.isTrigger = true;
            trig.size = new Vector3(1.7f, 3f, 0.8f);
            var t = zone.AddComponent<TraverseePorte>();
            t.parent = this;
        }
        else
        {
            // Branche NON exécutée : passage MURÉ (mur semi-transparent) + zone
            // de rappel devant la porte.
            var murGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            murGO.name = "MurBrancheMorte";
            murGO.transform.SetParent(porte.transform, false);
            murGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            murGO.transform.localScale    = new Vector3(1.85f, 3f, 0.15f);
            var murMat = new Material(sh);
            var cMur = new Color(couleur.r, couleur.g, couleur.b, 0.35f);
            if (murMat.HasProperty("_BaseColor")) murMat.SetColor("_BaseColor", cMur);
            else murMat.color = cMur;
            // Rendu transparent (URP Lit : Surface Type = Transparent)
            if (murMat.HasProperty("_Surface"))
            {
                murMat.SetFloat("_Surface", 1f);
                murMat.SetFloat("_Blend", 0f);
                murMat.renderQueue = 3000;
                murMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                murMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                murMat.SetInt("_ZWrite", 0);
                murMat.DisableKeyword("_ALPHATEST_ON");
                murMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            murGO.GetComponent<Renderer>().material = murMat;

            var zone = new GameObject("ZoneRappel");
            zone.transform.SetParent(porte.transform, false);
            zone.transform.localPosition = new Vector3(0f, 1.5f, 1.1f); // devant la porte
            var trig = zone.AddComponent<BoxCollider>();
            trig.isTrigger = true;
            trig.size = new Vector3(2.4f, 3f, 1.6f);
            var t = zone.AddComponent<MauvaisePorte>();
            t.parent = this;
            t.nomBranche = nom;
        }
    }

    // ── traversée de la BONNE porte : la branche s'exécute ────────────────

    public void TraverserBonnePorte()
    {
        string verdict = GameState.I.ValiderConditionIf();
        if (verdict == null) return;

        // Retire physiquement le booléen porté sur la tête (il a été consommé).
        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg != null)
        {
            var holder = pg.GetComponentInChildren<PlayerHolder>();
            if (holder != null && holder.HeldItem != null)
            {
                var item = holder.HeldItem;
                holder.ConsumeHeld();
                item.OnDropped();
                Destroy(item.gameObject);
            }
        }

        ConsoleScreen.AfficherSurMoniteur($"\"{verdict}\"");
        PromptUI.Hide();
        Debug.Log($"[PortesCondition] Branche exécutée → affichage \"{verdict}\".");
        // Update() détruira les portes (la quête est complétée).
    }

    // ── frottement contre la MAUVAISE porte : leçon + erreur ──────────────

    public void ToucherMauvaisePorte(string nomBranche)
    {
        if (Time.time < _prochainMsg) return;
        _prochainMsg = Time.time + 3.5f;

        var gs = GameState.I;
        string boolPorte = gs.cpuIfVrai ? "true" : "false";
        AudioFX.Erreur();
        gs.SignalerErreur();
        PromptUI.Show($"<color=#FF6B6B>Branche {nomBranche} murée !</color>  Ta condition vaut <b>{boolPorte}</b> — " +
                      "une branche dont le test échoue ne s'exécute JAMAIS.");
    }

    // ── composants de zone ────────────────────────────────────────────────

    class TraverseePorte : MonoBehaviour
    {
        public PortesCondition parent;
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) parent.TraverserBonnePorte();
        }
    }

    class MauvaisePorte : MonoBehaviour
    {
        public PortesCondition parent;
        public string nomBranche;
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) parent.ToucherMauvaisePorte(nomBranche);
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) PromptUI.Hide();
        }
    }
}
