using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codex.Puzzle
{
    /// <summary>
    /// Defines a single coding puzzle.
    /// Each puzzle presents code with gaps (blanks) the player must fill.
    /// The validation system compares player input to expected solutions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPuzzle", menuName = "Codex/Puzzle Definition")]
    public class PuzzleDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string puzzleId;
        public string puzzleName;
        [TextArea(2, 4)]
        public string description;

        [Header("Concept")]
        public ConceptType concept;
        public DifficultyLevel difficulty;

        [Header("Code")]
        [TextArea(5, 20)]
        [Tooltip("Le code affiché au joueur. Utilisez ___GAP_0___, ___GAP_1___ etc. pour les zones à remplir.")]
        public string templateCode;

        [TextArea(5, 20)]
        [Tooltip("Le code complet correct (pour référence).")]
        public string solutionCode;

        [Header("Gaps")]
        public List<GapDefinition> gaps = new List<GapDefinition>();

        [Header("Files")]
        [Tooltip("Fichiers additionnels affichés en lecture seule (ex: GameManager.cs)")]
        public List<AdditionalFile> additionalFiles = new List<AdditionalFile>();

        [Header("Hints")]
        [Tooltip("Indices progressifs donnés par le drone C-Sharp")]
        public List<string> hints = new List<string>();

        [Header("Feedback")]
        [TextArea(2, 3)]
        public string successMessage = "Compilation réussie !";
        [TextArea(2, 3)]
        public string failureMessage = "Erreur de compilation...";
    }

    [Serializable]
    public class GapDefinition
    {
        [Tooltip("Identifiant du gap (correspond à ___GAP_X___)")]
        public int gapIndex;

        [Tooltip("Label affiché au-dessus du champ de saisie")]
        public string label;

        [Tooltip("La réponse attendue (ex: 'int', '100', 'true')")]
        public string expectedValue;

        [Tooltip("Réponses alternatives acceptées")]
        public List<string> alternativeValues = new List<string>();

        [Tooltip("Type d'input : Text, Dropdown, DragDrop")]
        public GapInputType inputType = GapInputType.TextInput;

        [Tooltip("Options pour Dropdown ou DragDrop")]
        public List<string> options = new List<string>();

        [Tooltip("Placeholder dans le champ de saisie")]
        public string placeholder = "...";

        [Tooltip("Ignorer la casse lors de la validation")]
        public bool ignoreCase = false;

        [Tooltip("Supprimer les espaces avant validation")]
        public bool trimWhitespace = true;
    }

    [Serializable]
    public class AdditionalFile
    {
        public string fileName;
        [TextArea(5, 15)]
        public string code;
    }

    public enum ConceptType
    {
        Variables,
        Conditions,
        SwitchCase,
        Loops,
        Functions
    }

    public enum DifficultyLevel
    {
        Tutoriel,
        Facile,
        Moyen,
        Difficile
    }

    public enum GapInputType
    {
        TextInput,
        Dropdown,
        DragDrop,
        Toggle
    }
}
