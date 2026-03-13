using System.Collections.Generic;
using UnityEngine;

namespace Codex.Puzzle
{
    /// <summary>
    /// Utility class that generates example puzzle definitions at runtime.
    /// Use these for testing or as templates for creating ScriptableObject puzzles.
    /// In production, use the PuzzleDefinition ScriptableObjects created in the Unity Editor.
    /// </summary>
    public static class ExamplePuzzles
    {
        /// <summary>
        /// Niveau 1 - Le Temple : Variables et Types
        /// Le joueur doit initialiser le drone C-Sharp avec les bons types et valeurs.
        /// </summary>
        public static PuzzleDefinition CreateVariablesPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "temple_01";
            puzzle.puzzleName = "Initialisation du Drone";
            puzzle.description = "Le drone C-Sharp est désactivé. Déclare les bonnes variables pour le réactiver !";
            puzzle.concept = ConceptType.Variables;
            puzzle.difficulty = DifficultyLevel.Tutoriel;

            puzzle.templateCode =
@"using UnityEngine;

// Initialise le drone C-Sharp
public class DroneInit
{
    // Déclare le nom du drone (texte)
    ___GAP_0___ droneName = ___GAP_1___;

    // Déclare le niveau d'énergie (nombre entier)
    ___GAP_2___ energy = ___GAP_3___;

    // Le drone est-il actif ? (vrai/faux)
    ___GAP_4___ isActive = ___GAP_5___;
}";

            puzzle.solutionCode =
@"using UnityEngine;

// Initialise le drone C-Sharp
public class DroneInit
{
    // Déclare le nom du drone (texte)
    string droneName = ""C-Sharp"";

    // Déclare le niveau d'énergie (nombre entier)
    int energy = 100;

    // Le drone est-il actif ? (vrai/faux)
    bool isActive = true;
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Type pour le nom",
                    expectedValue = "string",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "int", "string", "bool", "float" },
                    placeholder = "type"
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Valeur du nom",
                    expectedValue = "\"C-Sharp\"",
                    alternativeValues = new List<string> { "\"c-sharp\"", "\"C-sharp\"" },
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\"",
                    ignoreCase = true
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Type pour l'énergie",
                    expectedValue = "int",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "int", "string", "bool", "float" },
                    placeholder = "type"
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Valeur de l'énergie",
                    expectedValue = "100",
                    inputType = GapInputType.TextInput,
                    placeholder = "nombre"
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Type pour isActive",
                    expectedValue = "bool",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "int", "string", "bool", "float" },
                    placeholder = "type"
                },
                new GapDefinition
                {
                    gapIndex = 5,
                    label = "Valeur de isActive",
                    expectedValue = "true",
                    alternativeValues = new List<string> { "True" },
                    inputType = GapInputType.Toggle,
                    ignoreCase = true
                }
            };

            puzzle.hints = new List<string>
            {
                "Un texte en C# s'appelle 'string'. Un nombre entier c'est 'int'. Vrai/faux c'est 'bool'.",
                "Le nom du drone est \"C-Sharp\" — n'oublie pas les guillemets pour un string !",
                "L'énergie est un nombre entier : 100. Et isActive doit être 'true' pour activer le drone."
            };

            puzzle.successMessage = "Compilation reussie -- Le drone C-Sharp s'active !";
            puzzle.failureMessage = "Erreur de compilation... Vérifie tes types et valeurs.";

            return puzzle;
        }

        /// <summary>
        /// Niveau 2 - Le Pont : Conditions (if/else)
        /// Le joueur doit programmer le Golem pour qu'il laisse passer.
        /// </summary>
        public static PuzzleDefinition CreateConditionsPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "pont_01";
            puzzle.puzzleName = "Le Golem de Sécurité";
            puzzle.description = "Le Golem bloque le passage. Programme sa condition pour qu'il te laisse passer !";
            puzzle.concept = ConceptType.Conditions;
            puzzle.difficulty = DifficultyLevel.Facile;

            puzzle.templateCode =
@"using UnityEngine;

public class GolemAI
{
    int playerLevel = 5;
    bool hasKey = true;

    void CheckAccess()
    {
        ___GAP_0___ (playerLevel ___GAP_1___ 3 ___GAP_2___ hasKey == ___GAP_3___)
        {
            Debug.Log(""Passage autorisé"");
            OpenGate();
        }
        ___GAP_4___
        {
            Debug.Log(""Accès refusé"");
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class GolemAI
{
    int playerLevel = 5;
    bool hasKey = true;

    void CheckAccess()
    {
        if (playerLevel >= 3 && hasKey == true)
        {
            Debug.Log(""Passage autorisé"");
            OpenGate();
        }
        else
        {
            Debug.Log(""Accès refusé"");
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Mot-clé de condition",
                    expectedValue = "if",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "if", "while", "for", "switch" }
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Opérateur de comparaison",
                    expectedValue = ">=",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "==", ">=", "<=", ">", "<", "!=" }
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Opérateur logique",
                    expectedValue = "&&",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "&&", "||", "!", "==" }
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Valeur de hasKey",
                    expectedValue = "true",
                    inputType = GapInputType.Toggle
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Mot-clé alternatif",
                    expectedValue = "else",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "else", "elif", "otherwise", "default" }
                }
            };

            puzzle.hints = new List<string>
            {
                "En C#, on utilise 'if' pour tester une condition et 'else' pour l'alternative.",
                "'>=' signifie 'supérieur ou égal à'. '&&' signifie 'ET' (les deux conditions doivent être vraies).",
                "Le joueur doit avoir un level >= 3 ET (&&) posséder la clé (hasKey == true)."
            };

            puzzle.successMessage = "Compilation reussie -- Le Golem s'ecarte !";
            puzzle.failureMessage = "Le Golem reste en travers... Vérifie ta condition.";

            return puzzle;
        }

        /// <summary>
        /// Niveau 4 - La Vallée : Boucles (for)
        /// Le joueur doit construire un pont bloc par bloc.
        /// </summary>
        public static PuzzleDefinition CreateLoopPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "vallee_01";
            puzzle.puzzleName = "Le Pont de la Vallée";
            puzzle.description = "Construis un pont de 10 blocs pour traverser le vide !";
            puzzle.concept = ConceptType.Loops;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class BridgeBuilder
{
    public void BuildBridge()
    {
        // Construire 10 blocs de pont
        ___GAP_0___ (int ___GAP_1___ = ___GAP_2___; i ___GAP_3___ ___GAP_4___; i___GAP_5___)
        {
            PlaceBlock(i);
            Debug.Log(""Bloc "" + i + "" placé"");
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class BridgeBuilder
{
    public void BuildBridge()
    {
        // Construire 10 blocs de pont
        for (int i = 0; i < 10; i++)
        {
            PlaceBlock(i);
            Debug.Log(""Bloc "" + i + "" placé"");
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0, label = "Mot-clé de boucle",
                    expectedValue = "for",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "for", "while", "if", "foreach" }
                },
                new GapDefinition
                {
                    gapIndex = 1, label = "Nom de la variable compteur",
                    expectedValue = "i",
                    inputType = GapInputType.TextInput, placeholder = "variable"
                },
                new GapDefinition
                {
                    gapIndex = 2, label = "Valeur initiale",
                    expectedValue = "0",
                    inputType = GapInputType.TextInput, placeholder = "début"
                },
                new GapDefinition
                {
                    gapIndex = 3, label = "Opérateur de condition",
                    expectedValue = "<",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "<", "<=", ">", ">=" }
                },
                new GapDefinition
                {
                    gapIndex = 4, label = "Nombre de blocs",
                    expectedValue = "10",
                    inputType = GapInputType.TextInput, placeholder = "nombre"
                },
                new GapDefinition
                {
                    gapIndex = 5, label = "Incrémentation",
                    expectedValue = "++",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "++", "--", "+=2", "+=1" }
                }
            };

            puzzle.hints = new List<string>
            {
                "Une boucle 'for' a 3 parties : initialisation (int i = 0), condition (i < 10), incrémentation (i++).",
                "On commence à 0 et on va jusqu'à 9 (avec '<'), ça fait bien 10 blocs !",
                "i++ signifie que i augmente de 1 à chaque tour de boucle."
            };

            puzzle.successMessage = "Compilation reussie -- Le pont se construit bloc par bloc !";
            puzzle.failureMessage = "Le pont s'effondre... Vérifie les paramètres de ta boucle.";

            return puzzle;
        }
    }
}
