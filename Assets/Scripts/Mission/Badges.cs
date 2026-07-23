using UnityEngine;

/// <summary>
/// Badges (succès) du Codex de l'Architecte — version CHAPITRE 1 :
/// un badge par NOTION apprise (déclaration, affichage, saisie, conversion,
/// calcul) + des badges de performance et un secret.
///
/// Débloqués une seule fois, sauvegardés dans PlayerPrefs (clés cda_badge_*),
/// annoncés par une notification toast avec voix off. « Recommencer » efface
/// tout (nouvelle partie). Appelés depuis GameState.
/// </summary>
public static class Badges
{
    static readonly Color OrBadge = new Color(1f, 0.82f, 0.25f);

    /// <summary>Catalogue complet (affiché dans le journal de mission).</summary>
    public static readonly (string id, string titre, string description)[] Tous =
    {
        // Un badge par notion du chapitre 1 (dans l'ordre du programme)
        ("premiere_variable", "Première variable", "Exécuter  int x = 4;  — ta première déclaration."),
        ("afficheur",         "Afficheur",         "Afficher une variable sur l'écran (Console.WriteLine)."),
        ("a_l_ecoute",        "À l'écoute",        "Récupérer la saisie de l'utilisateur (Console.ReadLine)."),
        ("convertisseur",     "Convertisseur",     "Convertir du texte en entier (Int32.Parse)."),
        ("calculateur",       "Calculateur",       "Faire additionner deux valeurs par l'unité arithmétique."),
        // Performance
        ("sans_faute",        "Sans faute",        "Une ligne exécutée sans aucune erreur."),
        ("eclair",            "Exécution éclair",  "Une ligne terminée en moins de 60 secondes."),
        ("logicien",          "Logicien",          "Exécuter ta première condition if (chapitre 2)."),
        // Performance globale
        ("programme_fini",    "Programme exécuté", "Terminer toutes les lignes du programme."),
        ("perfection",        "Compilation parfaite", "Toute la campagne avec zéro erreur."),
        // Secret
        ("chasseur_bug",      "Chasseur de bugs",  "Trouver l'insecte caché sur la carte mère."),
    };

    /// <summary>Débloque un badge s'il ne l'est pas déjà. Retourne vrai si nouveau.</summary>
    public static bool Debloquer(string id, string titre, string description)
    {
        string cle = "cda_badge_" + id;
        if (PlayerPrefs.GetInt(cle, 0) == 1) return false;

        PlayerPrefs.SetInt(cle, 1);
        PlayerPrefs.Save();
        NotificationsUI.Afficher($"BADGE DÉBLOQUÉ : {titre}", description, OrBadge, "badge");
        Debug.Log($"[Badges] Débloqué : {titre}");
        return true;
    }

    public static bool EstDebloque(string id) => PlayerPrefs.GetInt("cda_badge_" + id, 0) == 1;

    // ── événements de jeu ─────────────────────────────────────────────────

    /// <summary>Une ligne du programme vient d'être exécutée : badge de la notion.</summary>
    public static void LigneTerminee(QuestKind kind)
    {
        switch (kind)
        {
            case QuestKind.DeclarationRam:
                Debloquer("premiere_variable", "Première variable",
                          "int x = 4;  — ta première variable est en mémoire.");
                break;
            case QuestKind.LectureRam:
                Debloquer("afficheur", "Afficheur",
                          "Console.WriteLine — une valeur copiée de la RAM, affichée à l'écran.");
                break;
            case QuestKind.SaisieEcran:
                Debloquer("a_l_ecoute", "À l'écoute",
                          "Console.ReadLine — la saisie de l'utilisateur est devenue la valeur de y.");
                break;
            case QuestKind.Parse:
                Debloquer("convertisseur", "Convertisseur",
                          "Int32.Parse — du texte transformé en entier par l'unité arithmétique.");
                break;
            case QuestKind.Calcul:
                Debloquer("calculateur", "Calculateur",
                          "x + z — l'unité arithmétique a fait sa première addition pour toi.");
                break;
        }
    }

    /// <summary>Performance de la ligne : zéro-erreur et rapidité.</summary>
    public static void MissionTerminee(float dureeSecondes, int erreurs)
    {
        if (erreurs == 0)
            Debloquer("sans_faute", "Sans faute",
                      "Une ligne du programme exécutée sans aucune erreur.");
        if (dureeSecondes < 60f)
            Debloquer("eclair", "Exécution éclair",
                      "Une ligne terminée en moins de 60 secondes.");
    }

    /// <summary>Campagne complète (chapitre 1).</summary>
    public static void CampagneTerminee(int erreursTotales)
    {
        Debloquer("programme_fini", "Programme exécuté",
                  "Les 6 lignes du chapitre 1 sont exécutées. Bravo, architecte !");
        if (erreursTotales == 0)
            Debloquer("perfection", "Compilation parfaite",
                      "Campagne entière terminée avec zéro erreur. Respect.");
    }

    /// <summary>Le joueur a trouvé l'insecte caché (clin d'œil à Grace Hopper).</summary>
    public static void ChasseurDeBug() =>
        Debloquer("chasseur_bug", "Chasseur de bugs",
                  "Tu as trouvé le premier bug de l'histoire de l'informatique (1947).");

    // ── réservés aux chapitres 2-3 (dormants, pour le prochain développeur) ──

    /// <summary>Première condition if exécutée (chapitre 2).</summary>
    public static void Logicien() =>
        Debloquer("logicien", "Logicien",
                  "Le CPU a évalué ta première condition if. VRAI ou FAUX, telle est la question.");

    /// <summary>Première boucle for exécutée jusqu'au test FAUX (chapitre 3).</summary>
    public static void Boucleur() =>
        Debloquer("boucleur", "Boucleur",
                  "0, 1, 2... et FAUX : ta première boucle for est allée au bout.");

    /// <summary>Efface tous les badges (nouvelle partie).</summary>
    public static void ToutEffacer()
    {
        foreach (var (id, _, _) in Tous)
            PlayerPrefs.DeleteKey("cda_badge_" + id);
        PlayerPrefs.DeleteKey("cda_badge_logicien");
        PlayerPrefs.DeleteKey("cda_badge_boucleur");
        PlayerPrefs.Save();
    }
}
