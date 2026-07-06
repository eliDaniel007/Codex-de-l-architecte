using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Rapport de progression de l'élève : un fichier texte lisible, exporté
/// depuis le journal de mission ([J] → EXPORTER LE RAPPORT).
/// Écrit sur le Bureau si possible, sinon dans le dossier de données du jeu.
/// Pensé pour être remis au professeur (ou collé dans un courriel).
/// </summary>
public static class RapportEleve
{
    /// <summary>Génère et écrit le rapport. Retourne le chemin du fichier, ou null.</summary>
    public static string Exporter()
    {
        var gs = GameState.I;
        var sb = new StringBuilder();

        // ── En-tête ──
        sb.AppendLine("=====================================================");
        sb.AppendLine("   LE CODEX DE L'ARCHITECTE — RAPPORT DE PROGRESSION");
        sb.AppendLine("=====================================================");
        sb.AppendLine($"Date : {DateTime.Now:dd/MM/yyyy HH:mm}");
        int mn = Mathf.FloorToInt(gs.TempsCampagne / 60f);
        int sec = Mathf.FloorToInt(gs.TempsCampagne % 60f);
        sb.AppendLine($"Temps de jeu total : {mn} min {sec:00} s");
        sb.AppendLine($"Erreurs cumulées   : {gs.nbErreurs}");
        sb.AppendLine($"Mode Zen           : {(gs.modeZen ? "activé (la note ignore le chrono)" : "désactivé")}");
        sb.AppendLine();

        // ── Le programme ──
        sb.AppendLine("LE PROGRAMME");
        sb.AppendLine("-----------------------------------------------------");
        int terminees = 0;
        for (int i = 0; i < gs.quests.Count; i++)
        {
            var q = gs.quests[i];
            string etat;
            if (q.complete) { etat = "[OK]      "; terminees++; }
            else if (i == gs.questIndex && i <= gs.missionRevelee) etat = "[EN COURS]";
            else etat = "[A FAIRE] ";

            sb.Append($"{etat} {q.titre}");

            var stat = gs.StatLigne(i);
            if (q.complete && stat.HasValue)
                sb.Append($"   --> {stat.Value.etoiles}/3, {stat.Value.duree:0} s, {stat.Value.erreurs} erreur(s)");
            sb.AppendLine();
        }
        sb.AppendLine($"Progression : {terminees}/{gs.quests.Count} lignes exécutées.");
        sb.AppendLine();

        // ── Badges ──
        sb.AppendLine("BADGES");
        sb.AppendLine("-----------------------------------------------------");
        int obtenus = 0;
        foreach (var (id, titre, description) in Badges.Tous)
        {
            bool ok = Badges.EstDebloque(id);
            if (ok) obtenus++;
            sb.AppendLine($"{(ok ? "[X]" : "[ ]")} {titre} — {description}");
        }
        sb.AppendLine($"Total : {obtenus}/{Badges.Tous.Length} badges.");
        sb.AppendLine();
        sb.AppendLine("=====================================================");
        sb.AppendLine("Généré par Le Codex de l'Architecte.");

        // ── Écriture : Bureau d'abord, sinon dossier de données du jeu ──
        string nomFichier = $"rapport_codex_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt";
        string chemin = null;
        try
        {
            string bureau = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (!string.IsNullOrEmpty(bureau) && Directory.Exists(bureau))
            {
                chemin = Path.Combine(bureau, nomFichier);
                File.WriteAllText(chemin, sb.ToString(), Encoding.UTF8);
            }
        }
        catch { chemin = null; }

        if (chemin == null)
        {
            try
            {
                chemin = Path.Combine(Application.persistentDataPath, nomFichier);
                File.WriteAllText(chemin, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Rapport] Impossible d'écrire le rapport : {e.Message}");
                return null;
            }
        }

        Debug.Log($"[Rapport] Exporté : {chemin}");
        return chemin;
    }
}
