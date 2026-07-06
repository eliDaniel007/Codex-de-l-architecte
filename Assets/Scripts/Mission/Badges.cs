using UnityEngine;

/// <summary>
/// Badges (succès) du Codex de l'Architecte. Débloqués une seule fois,
/// sauvegardés dans PlayerPrefs, annoncés par une notification toast.
///
/// Appelés depuis GameState aux moments clés (déclaration, dépôt RAM,
/// mission terminée, campagne finie).
/// </summary>
public static class Badges
{
    static readonly Color OrBadge = new Color(1f, 0.82f, 0.25f);

    /// <summary>Catalogue complet (pour le journal de mission).</summary>
    public static readonly (string id, string titre, string description)[] Tous =
    {
        ("premiere_boite", "Première boîte",       "Déclarer sa première variable en mémoire."),
        ("premier_float",  "Virgule flottante",    "Déclarer son premier float."),
        ("memoire_pleine", "Mémoire bien remplie", "Quatre variables en RAM en même temps."),
        ("zero_erreur",    "Sans faute",           "Une ligne exécutée sans aucune erreur."),
        ("eclair",         "Exécution éclair",     "Une ligne terminée en moins de 60 secondes."),
        ("programme_fini", "Programme exécuté",    "Terminer toutes les lignes du programme."),
        ("perfection",     "Compilation parfaite", "Campagne entière avec zéro erreur."),
        ("logicien",       "Logicien",             "Exécuter ta première condition if."),
        ("boucleur",       "Boucleur",             "Exécuter ta première boucle for jusqu'au bout."),
        ("chasseur_bug",   "Chasseur de bugs",     "Trouver l'insecte caché sur la carte mère."),
    };

    /// <summary>Débloque un badge s'il ne l'est pas déjà. Retourne vrai si nouveau.</summary>
    public static bool Debloquer(string id, string titre, string description)
    {
        string cle = "cda_badge_" + id;
        if (PlayerPrefs.GetInt(cle, 0) == 1) return false;

        PlayerPrefs.SetInt(cle, 1);
        PlayerPrefs.Save();
        NotificationsUI.Afficher($"BADGE DÉBLOQUÉ : {titre}", description, OrBadge);
        Debug.Log($"[Badges] Débloqué : {titre}");
        return true;
    }

    public static bool EstDebloque(string id) => PlayerPrefs.GetInt("cda_badge_" + id, 0) == 1;

    // ── événements de jeu ─────────────────────────────────────────────────

    /// <summary>Première variable déclarée dans la RAM.</summary>
    public static void PremiereBoite() =>
        Debloquer("premiere_boite", "Première boîte",
                  "Tu as déclaré ta première variable en mémoire.");

    /// <summary>Un float déclaré pour la première fois.</summary>
    public static void PremierFloat() =>
        Debloquer("premier_float", "Virgule flottante",
                  "Ton premier float est en RAM.");

    /// <summary>4 variables ou plus en RAM en même temps.</summary>
    public static void MemoireBienRemplie(int nbOccupees)
    {
        if (nbOccupees >= 4)
            Debloquer("memoire_pleine", "Mémoire bien remplie",
                      "Quatre variables rangées en RAM en même temps.");
    }

    /// <summary>Mission terminée : vérifie zéro-erreur et rapidité.</summary>
    public static void MissionTerminee(float dureeSecondes, int erreurs)
    {
        if (erreurs == 0)
            Debloquer("zero_erreur", "Sans faute",
                      "Une ligne du programme exécutée sans aucune erreur.");
        if (dureeSecondes < 60f)
            Debloquer("eclair", "Exécution éclair",
                      "Une ligne terminée en moins de 60 secondes.");
    }

    /// <summary>Campagne complète.</summary>
    public static void CampagneTerminee(int erreursTotales)
    {
        Debloquer("programme_fini", "Programme exécuté",
                  "Toutes les lignes du programme sont terminées. Bravo, architecte !");
        if (erreursTotales == 0)
            Debloquer("perfection", "Compilation parfaite",
                      "Campagne entière terminée avec zéro erreur. Respect.");
    }

    /// <summary>Première condition if exécutée (ligne 7).</summary>
    public static void Logicien() =>
        Debloquer("logicien", "Logicien",
                  "Le CPU a évalué ta première condition if. VRAI ou FAUX, telle est la question.");

    /// <summary>Première boucle for exécutée jusqu'au test FAUX (ligne 8).</summary>
    public static void Boucleur() =>
        Debloquer("boucleur", "Boucleur",
                  "0, 1, 2... et FAUX : ta première boucle for est allée au bout.");

    /// <summary>Le joueur a trouvé l'insecte caché (clin d'œil à Grace Hopper).</summary>
    public static void ChasseurDeBug() =>
        Debloquer("chasseur_bug", "Chasseur de bugs",
                  "Tu as trouvé le premier bug de l'histoire de l'informatique.");

    /// <summary>Efface tous les badges (reset campagne complète — optionnel).</summary>
    public static void ToutEffacer()
    {
        foreach (var (id, _, _) in Tous)
            PlayerPrefs.DeleteKey("cda_badge_" + id);
        PlayerPrefs.Save();
    }
}
