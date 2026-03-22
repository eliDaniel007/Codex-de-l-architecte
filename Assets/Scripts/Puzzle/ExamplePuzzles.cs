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
        /// Niveau 1b - Le Temple : Variables (Pratique)
        /// </summary>
        public static PuzzleDefinition CreateVariablesPracticePuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "temple_02";
            puzzle.puzzleName = "Le Registre du Temple";
            puzzle.description = "Remplis le registre du temple avec les bons types et valeurs !";
            puzzle.concept = ConceptType.Variables;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class TempleRegister
{
    // Vitesse du drone (nombre decimal)
    ___GAP_0___ speed = ___GAP_1___;

    // Message d'accueil
    ___GAP_2___ welcomeMsg = ___GAP_3___;

    // Nombre de gardes
    ___GAP_4___ guardCount = ___GAP_5___;

    // Porte ouverte ?
    bool doorOpen = ___GAP_6___;
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class TempleRegister
{
    float speed = 2.5f;
    string welcomeMsg = ""Bienvenue"";
    int guardCount = 4;
    bool doorOpen = false;
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Type pour un nombre decimal",
                    expectedValue = "float", inputType = GapInputType.Dropdown,
                    options = new List<string> { "float", "int", "double", "string" } },
                new GapDefinition { gapIndex = 1, label = "Valeur decimale (avec f)",
                    expectedValue = "2.5f", inputType = GapInputType.TextInput,
                    placeholder = "nombre.decimalf",
                    alternativeValues = new List<string> { "2.5F", "2.50f" } },
                new GapDefinition { gapIndex = 2, label = "Type pour du texte",
                    expectedValue = "string", inputType = GapInputType.TextInput, placeholder = "type" },
                new GapDefinition { gapIndex = 3, label = "Message d'accueil (avec guillemets)",
                    expectedValue = "\"Bienvenue\"", inputType = GapInputType.TextInput,
                    placeholder = "\"...\"",
                    alternativeValues = new List<string> { "\"bienvenue\"" }, ignoreCase = true },
                new GapDefinition { gapIndex = 4, label = "Type pour un nombre entier",
                    expectedValue = "int", inputType = GapInputType.TextInput, placeholder = "type" },
                new GapDefinition { gapIndex = 5, label = "Nombre de gardes",
                    expectedValue = "4", inputType = GapInputType.TextInput, placeholder = "nombre" },
                new GapDefinition { gapIndex = 6, label = "La porte est-elle ouverte ?",
                    expectedValue = "false", inputType = GapInputType.Toggle }
            };

            puzzle.hints = new List<string>
            {
                "Un nombre decimal utilise le type 'float'. La valeur doit finir par 'f' : 2.5f",
                "Un texte (string) doit etre entre guillemets : \"Bienvenue\"",
                "int pour les entiers, bool pour vrai/faux. La porte est fermee donc false."
            };

            puzzle.successMessage = "Compilation reussie -- Le registre du temple est complet !";
            puzzle.failureMessage = "Le registre est invalide... Verifie les types et valeurs.";

            return puzzle;
        }

        /// <summary>
        /// Niveau 1c - Le Temple : Variables (Maitrise)
        /// </summary>
        public static PuzzleDefinition CreateVariablesMasteryPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "temple_03";
            puzzle.puzzleName = "Le Code d'Activation";
            puzzle.description = "Ecris toutes les variables du code d'activation sans aide !";
            puzzle.concept = ConceptType.Variables;
            puzzle.difficulty = DifficultyLevel.Difficile;

            puzzle.templateCode =
@"using UnityEngine;

public class ActivationCode
{
    // Code secret (texte)
    ___GAP_0___ code = ___GAP_1___;

    // Tentatives restantes (entier)
    ___GAP_2___ attempts = ___GAP_3___;

    // Temps limite en secondes (decimal)
    ___GAP_4___ timeLimit = ___GAP_5___;

    // Systeme actif (vrai/faux)
    ___GAP_6___ isActive = ___GAP_7___;
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class ActivationCode
{
    string code = ""ARCADIA"";
    int attempts = 3;
    float timeLimit = 30.0f;
    bool isActive = true;
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Type pour le code secret",
                    expectedValue = "string", inputType = GapInputType.TextInput, placeholder = "type" },
                new GapDefinition { gapIndex = 1, label = "Valeur du code (avec guillemets)",
                    expectedValue = "\"ARCADIA\"", inputType = GapInputType.TextInput,
                    placeholder = "\"...\"",
                    alternativeValues = new List<string> { "\"Arcadia\"", "\"arcadia\"" }, ignoreCase = true },
                new GapDefinition { gapIndex = 2, label = "Type pour les tentatives",
                    expectedValue = "int", inputType = GapInputType.TextInput, placeholder = "type" },
                new GapDefinition { gapIndex = 3, label = "Nombre de tentatives",
                    expectedValue = "3", inputType = GapInputType.TextInput, placeholder = "nombre" },
                new GapDefinition { gapIndex = 4, label = "Type pour le temps (decimal)",
                    expectedValue = "float", inputType = GapInputType.TextInput, placeholder = "type" },
                new GapDefinition { gapIndex = 5, label = "Temps en secondes (avec f)",
                    expectedValue = "30.0f", inputType = GapInputType.TextInput, placeholder = "nombre",
                    alternativeValues = new List<string> { "30f", "30.0F" } },
                new GapDefinition { gapIndex = 6, label = "Type vrai/faux",
                    expectedValue = "bool", inputType = GapInputType.TextInput, placeholder = "type" },
                new GapDefinition { gapIndex = 7, label = "Le systeme est actif",
                    expectedValue = "true", inputType = GapInputType.TextInput, placeholder = "valeur" }
            };

            puzzle.hints = new List<string>
            {
                "Les 4 types de base : string (texte), int (entier), float (decimal), bool (vrai/faux).",
                "Les strings sont entre guillemets. Les floats finissent par 'f'. bool = true ou false.",
                "code = \"ARCADIA\", attempts = 3, timeLimit = 30.0f, isActive = true"
            };

            puzzle.successMessage = "Compilation reussie -- Code d'activation accepte !";
            puzzle.failureMessage = "Code rejete... Ecris chaque type et valeur correctement.";

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
        /// Niveau 2b - Le Pont : Conditions (Pratique)
        /// </summary>
        public static PuzzleDefinition CreateConditionsPracticePuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "pont_02";
            puzzle.puzzleName = "Le Systeme d'Alarme";
            puzzle.description = "Programme les conditions du systeme d'alarme du temple !";
            puzzle.concept = ConceptType.Conditions;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class AlarmSystem
{
    int temperature = 85;
    bool fireDetected = true;

    void CheckAlarm()
    {
        ___GAP_0___ (temperature ___GAP_1___ 80)
        {
            Debug.Log(""Alerte temperature haute !"");
        }
        ___GAP_2___ ___GAP_3___ (___GAP_4___)
        {
            Debug.Log(""INCENDIE DETECTE !"");
            ActivateSprinklers();
        }
        ___GAP_5___
        {
            Debug.Log(""Tout est normal."");
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class AlarmSystem
{
    int temperature = 85;
    bool fireDetected = true;

    void CheckAlarm()
    {
        if (temperature > 80)
        {
            Debug.Log(""Alerte temperature haute !"");
        }
        else if (fireDetected)
        {
            Debug.Log(""INCENDIE DETECTE !"");
            ActivateSprinklers();
        }
        else
        {
            Debug.Log(""Tout est normal."");
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Premiere condition",
                    expectedValue = "if", inputType = GapInputType.Dropdown,
                    options = new List<string> { "if", "while", "for", "switch" } },
                new GapDefinition { gapIndex = 1, label = "Operateur (temp au-dessus de 80)",
                    expectedValue = ">", inputType = GapInputType.Dropdown,
                    options = new List<string> { ">", "<", ">=", "==" } },
                new GapDefinition { gapIndex = 2, label = "Mot-cle sinon",
                    expectedValue = "else", inputType = GapInputType.TextInput, placeholder = "..." },
                new GapDefinition { gapIndex = 3, label = "Mot-cle condition",
                    expectedValue = "if", inputType = GapInputType.TextInput, placeholder = "..." },
                new GapDefinition { gapIndex = 4, label = "Variable booleenne a tester",
                    expectedValue = "fireDetected", inputType = GapInputType.TextInput, placeholder = "variable" },
                new GapDefinition { gapIndex = 5, label = "Dernier cas (sinon)",
                    expectedValue = "else", inputType = GapInputType.TextInput, placeholder = "..." }
            };

            puzzle.hints = new List<string>
            {
                "if teste la premiere condition. 'else if' teste une deuxieme condition si la premiere est fausse.",
                "'else if (fireDetected)' verifie si un incendie est detecte. Un bool se teste directement sans == true.",
                "'else' a la fin gere le cas ou aucune condition n'est vraie."
            };

            puzzle.successMessage = "Compilation reussie -- Le systeme d'alarme est operationnel !";
            puzzle.failureMessage = "L'alarme est defectueuse... Verifie les if/else if/else.";

            return puzzle;
        }

        /// <summary>
        /// Niveau 2c - Le Pont : Conditions (Maitrise)
        /// </summary>
        public static PuzzleDefinition CreateConditionsMasteryPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "pont_03";
            puzzle.puzzleName = "Le Portail Magique";
            puzzle.description = "Programme les conditions du portail : niveau, cle, et mot de passe !";
            puzzle.concept = ConceptType.Conditions;
            puzzle.difficulty = DifficultyLevel.Difficile;

            puzzle.templateCode =
@"using UnityEngine;

public class MagicPortal
{
    void TryOpen(int level, bool hasKey, string password)
    {
        ___GAP_0___ (level ___GAP_1___ 10 ___GAP_2___ hasKey ___GAP_3___ password ___GAP_4___ ___GAP_5___)
        {
            Debug.Log(""Le portail s'ouvre !"");
        }
        ___GAP_6___
        {
            Debug.Log(""Conditions non remplies."");
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class MagicPortal
{
    void TryOpen(int level, bool hasKey, string password)
    {
        if (level >= 10 && hasKey && password == ""Arcadia"")
        {
            Debug.Log(""Le portail s'ouvre !"");
        }
        else
        {
            Debug.Log(""Conditions non remplies."");
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Mot-cle de condition",
                    expectedValue = "if", inputType = GapInputType.TextInput, placeholder = "..." },
                new GapDefinition { gapIndex = 1, label = "Operateur (level suffisant)",
                    expectedValue = ">=", inputType = GapInputType.TextInput, placeholder = "op" },
                new GapDefinition { gapIndex = 2, label = "Operateur logique ET",
                    expectedValue = "&&", inputType = GapInputType.TextInput, placeholder = "op" },
                new GapDefinition { gapIndex = 3, label = "Operateur logique ET",
                    expectedValue = "&&", inputType = GapInputType.TextInput, placeholder = "op" },
                new GapDefinition { gapIndex = 4, label = "Operateur egal",
                    expectedValue = "==", inputType = GapInputType.TextInput, placeholder = "op" },
                new GapDefinition { gapIndex = 5, label = "Mot de passe (avec guillemets)",
                    expectedValue = "\"Arcadia\"", inputType = GapInputType.TextInput,
                    placeholder = "\"...\"",
                    alternativeValues = new List<string> { "\"arcadia\"", "\"ARCADIA\"" }, ignoreCase = true },
                new GapDefinition { gapIndex = 6, label = "Sinon",
                    expectedValue = "else", inputType = GapInputType.TextInput, placeholder = "..." }
            };

            puzzle.hints = new List<string>
            {
                "Il faut 3 conditions vraies en meme temps : level >= 10 ET hasKey ET password == \"Arcadia\".",
                "'&&' lie les conditions. Un bool comme hasKey se teste directement (pas besoin de == true).",
                "Pour comparer un string, on utilise == et la valeur entre guillemets."
            };

            puzzle.successMessage = "Compilation reussie -- Le portail magique s'ouvre !";
            puzzle.failureMessage = "Le portail reste ferme... Verifie chaque condition.";

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

        /// <summary>
        /// Niveau 4b - La Vallee : Boucle While
        /// </summary>
        public static PuzzleDefinition CreateWhileLoopPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "vallee_02";
            puzzle.puzzleName = "La Patrouille du Garde";
            puzzle.description = "Le garde patrouille tant qu'il a de l'energie. Programme sa boucle while !";
            puzzle.concept = ConceptType.Loops;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class GuardPatrol
{
    public void Patrol()
    {
        int energy = 50;

        ___GAP_0___ (___GAP_1___ ___GAP_2___ ___GAP_3___)
        {
            Debug.Log(""Patrouille... Energie : "" + energy);
            energy ___GAP_4___ ___GAP_5___;
        }

        Debug.Log(""Le garde s'arrete."");
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class GuardPatrol
{
    public void Patrol()
    {
        int energy = 50;

        while (energy > 0)
        {
            Debug.Log(""Patrouille... Energie : "" + energy);
            energy -= 10;
        }

        Debug.Log(""Le garde s'arrete."");
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Mot-cle de boucle (tant que...)",
                    expectedValue = "while", inputType = GapInputType.Dropdown,
                    options = new List<string> { "while", "for", "if", "do" } },
                new GapDefinition { gapIndex = 1, label = "Variable a tester",
                    expectedValue = "energy", inputType = GapInputType.Dropdown,
                    options = new List<string> { "energy", "i", "health", "0" } },
                new GapDefinition { gapIndex = 2, label = "Operateur (tant que energy est positif)",
                    expectedValue = ">", inputType = GapInputType.Dropdown,
                    options = new List<string> { ">", "<", ">=", "==" } },
                new GapDefinition { gapIndex = 3, label = "Valeur limite",
                    expectedValue = "0", inputType = GapInputType.TextInput, placeholder = "nombre" },
                new GapDefinition { gapIndex = 4, label = "Operateur pour reduire l'energie",
                    expectedValue = "-=", inputType = GapInputType.Dropdown,
                    options = new List<string> { "-=", "+=", "*=", "=" } },
                new GapDefinition { gapIndex = 5, label = "Combien d'energie par tour",
                    expectedValue = "10", inputType = GapInputType.TextInput, placeholder = "nombre" }
            };

            puzzle.hints = new List<string>
            {
                "'while' signifie 'tant que'. La boucle tourne tant que la condition est vraie.",
                "On veut patrouiller tant que energy > 0. A chaque tour, on perd de l'energie.",
                "energy -= 10 retire 10 a chaque tour. 50, 40, 30, 20, 10, puis 0 -> stop !"
            };

            puzzle.successMessage = "Compilation reussie -- Le garde patrouille jusqu'a epuisement !";
            puzzle.failureMessage = "Le garde est perdu... Verifie la condition du while.";

            return puzzle;
        }

        /// <summary>
        /// Niveau 4c - La Vallee : Boucle While (Pratique)
        /// </summary>
        public static PuzzleDefinition CreateWhilePracticePuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "vallee_03";
            puzzle.puzzleName = "Le Chercheur de Tresor";
            puzzle.description = "Cherche le tresor en creusant tant que tu ne l'as pas trouve !";
            puzzle.concept = ConceptType.Loops;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class TreasureHunter
{
    public void Dig()
    {
        bool found = ___GAP_0___;
        int depth = 0;

        ___GAP_1___ (___GAP_2___found)
        {
            depth++;
            Debug.Log(""Profondeur : "" + depth);

            if (depth == 5)
            {
                found = ___GAP_3___;
            }
        }

        Debug.Log(""Tresor trouve a "" + depth + "" metres !"");
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class TreasureHunter
{
    public void Dig()
    {
        bool found = false;
        int depth = 0;

        while (!found)
        {
            depth++;
            Debug.Log(""Profondeur : "" + depth);

            if (depth == 5)
            {
                found = true;
            }
        }

        Debug.Log(""Tresor trouve a "" + depth + "" metres !"");
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Valeur initiale de found (pas encore trouve)",
                    expectedValue = "false", inputType = GapInputType.Toggle },
                new GapDefinition { gapIndex = 1, label = "Mot-cle de boucle",
                    expectedValue = "while", inputType = GapInputType.TextInput, placeholder = "..." },
                new GapDefinition { gapIndex = 2, label = "Operateur NON (tant que PAS trouve)",
                    expectedValue = "!", inputType = GapInputType.Dropdown,
                    options = new List<string> { "!", "==", "!=", ">" } },
                new GapDefinition { gapIndex = 3, label = "Valeur quand le tresor est trouve",
                    expectedValue = "true", inputType = GapInputType.Toggle }
            };

            puzzle.hints = new List<string>
            {
                "On commence avec found = false (pas encore trouve).",
                "'!found' signifie 'NOT found' = tant que found est false, on continue.",
                "Quand depth atteint 5, on met found = true et la boucle s'arrete."
            };

            puzzle.successMessage = "Compilation reussie -- Le tresor est deterre !";
            puzzle.failureMessage = "Le tresor reste cache... Verifie la logique du while.";

            return puzzle;
        }

        /// <summary>
        /// Niveau 4d - La Vallee : Boucle Do...While
        /// </summary>
        public static PuzzleDefinition CreateDoWhileLoopPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "vallee_04";
            puzzle.puzzleName = "Le Scanner de Zone";
            puzzle.description = "Le scanner doit analyser au moins une fois. Utilise do...while !";
            puzzle.concept = ConceptType.Loops;
            puzzle.difficulty = DifficultyLevel.Difficile;

            puzzle.templateCode =
@"using UnityEngine;

public class AreaScanner
{
    public void ScanZone()
    {
        int threats = 3;

        ___GAP_0___
        {
            Debug.Log(""Scan... Menaces : "" + threats);
            threats___GAP_1___;
        }
        ___GAP_2___ (threats ___GAP_3___ ___GAP_4___);

        Debug.Log(""Zone securisee !"");
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class AreaScanner
{
    public void ScanZone()
    {
        int threats = 3;

        do
        {
            Debug.Log(""Scan... Menaces : "" + threats);
            threats--;
        }
        while (threats > 0);

        Debug.Log(""Zone securisee !"");
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Mot-cle pour executer au moins une fois",
                    expectedValue = "do", inputType = GapInputType.Dropdown,
                    options = new List<string> { "do", "while", "for", "repeat" } },
                new GapDefinition { gapIndex = 1, label = "Decrementation (retirer 1)",
                    expectedValue = "--", inputType = GapInputType.Dropdown,
                    options = new List<string> { "--", "++", "-=", "+=" } },
                new GapDefinition { gapIndex = 2, label = "Mot-cle de condition (apres le bloc)",
                    expectedValue = "while", inputType = GapInputType.TextInput, placeholder = "..." },
                new GapDefinition { gapIndex = 3, label = "Operateur (tant qu'il reste des menaces)",
                    expectedValue = ">", inputType = GapInputType.Dropdown,
                    options = new List<string> { ">", "<", ">=", "==" } },
                new GapDefinition { gapIndex = 4, label = "Valeur limite",
                    expectedValue = "0", inputType = GapInputType.TextInput, placeholder = "nombre" }
            };

            puzzle.hints = new List<string>
            {
                "'do { } while (condition);' execute le bloc AU MOINS une fois.",
                "threats-- retire 1 a chaque scan. On continue tant que threats > 0.",
                "Le 'while' vient APRES le bloc do { }, avec un point-virgule a la fin !"
            };

            puzzle.successMessage = "Compilation reussie -- La zone est scannee et securisee !";
            puzzle.failureMessage = "Le scanner bugge... Verifie la structure do...while.";

            return puzzle;
        }

        // ==================== SWITCH / CASE ====================

        /// <summary>
        /// Tour 01 - Decouverte du Switch
        /// Le joueur apprend la structure switch/case avec des menus deroulants.
        /// </summary>
        public static PuzzleDefinition CreateSwitchDiscoveryPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "tour_01";
            puzzle.puzzleName = "Le Panneau de Controle";
            puzzle.description = "Le panneau affiche un message selon le niveau d'alerte. Utilise un switch !";
            puzzle.concept = ConceptType.SwitchCase;
            puzzle.difficulty = DifficultyLevel.Facile;

            puzzle.templateCode =
@"using UnityEngine;

public class AlertPanel
{
    int alertLevel = 2;

    void DisplayAlert()
    {
        ___GAP_0___ (___GAP_1___)
        {
            ___GAP_2___ 1:
                Debug.Log(""Niveau faible"");
                ___GAP_3___;
            ___GAP_4___ 2:
                Debug.Log(""Niveau moyen"");
                ___GAP_5___;
            case 3:
                Debug.Log(""Niveau critique"");
                break;
            ___GAP_6___:
                Debug.Log(""Niveau inconnu"");
                break;
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class AlertPanel
{
    int alertLevel = 2;

    void DisplayAlert()
    {
        switch (alertLevel)
        {
            case 1:
                Debug.Log(""Niveau faible"");
                break;
            case 2:
                Debug.Log(""Niveau moyen"");
                break;
            case 3:
                Debug.Log(""Niveau critique"");
                break;
            default:
                Debug.Log(""Niveau inconnu"");
                break;
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Mot-cle de structure",
                    expectedValue = "switch",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "switch", "if", "for", "while" }
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Variable a tester",
                    expectedValue = "alertLevel",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "alertLevel", "level", "alert", "status" }
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Mot-cle de cas (1)",
                    expectedValue = "case",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "case", "when", "if", "match" }
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Sortir du cas (1)",
                    expectedValue = "break",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "break", "stop", "end", "return" }
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Mot-cle de cas (2)",
                    expectedValue = "case",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "case", "when", "if", "match" }
                },
                new GapDefinition
                {
                    gapIndex = 5,
                    label = "Sortir du cas (2)",
                    expectedValue = "break",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "break", "stop", "end", "return" }
                },
                new GapDefinition
                {
                    gapIndex = 6,
                    label = "Cas par defaut",
                    expectedValue = "default",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "default", "else", "other", "finally" }
                }
            };

            puzzle.hints = new List<string>
            {
                "Le mot-cle 'switch' permet de tester une variable contre plusieurs valeurs possibles.",
                "Chaque valeur a tester commence par 'case' suivi de la valeur, puis ':' pour delimiter.",
                "'break' permet de sortir du switch apres chaque cas. 'default' gere tous les cas non prevus."
            };

            puzzle.successMessage = "Compilation reussie -- Le panneau affiche le bon message !";
            puzzle.failureMessage = "Erreur de compilation... Verifie la structure du switch.";

            return puzzle;
        }

        /// <summary>
        /// Tour 02 - Pratique du Switch avec des strings
        /// Le joueur utilise switch/case avec des chaines de caracteres.
        /// </summary>
        public static PuzzleDefinition CreateSwitchPracticePuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "tour_02";
            puzzle.puzzleName = "Le Traducteur de Runes";
            puzzle.description = "Chaque rune a une signification. Programme le traducteur avec un switch !";
            puzzle.concept = ConceptType.SwitchCase;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class RuneTranslator
{
    string Translate(string rune)
    {
        string result = """";

        switch (___GAP_0___)
        {
            case ___GAP_1___:
                result = ""Feu"";
                break;
            case ___GAP_2___:
                result = ""Eau"";
                break;
            case ___GAP_3___:
                result = ""Sol"";
                ___GAP_5___;
            default:
                result = ___GAP_4___;
                break;
        }

        return result;
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class RuneTranslator
{
    string Translate(string rune)
    {
        string result = """";

        switch (rune)
        {
            case ""Ignis"":
                result = ""Feu"";
                break;
            case ""Aqua"":
                result = ""Eau"";
                break;
            case ""Terre"":
                result = ""Sol"";
                break;
            default:
                result = ""Rune inconnue"";
                break;
        }

        return result;
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Variable a tester",
                    expectedValue = "rune",
                    inputType = GapInputType.TextInput,
                    placeholder = "variable"
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Rune du feu",
                    expectedValue = "\"Ignis\"",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "\"Ignis\"", "\"Fire\"", "\"Feu\"", "\"Pyro\"" }
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Rune de l'eau",
                    expectedValue = "\"Aqua\"",
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\""
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Rune de la terre",
                    expectedValue = "\"Terre\"",
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\""
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Message par defaut",
                    expectedValue = "\"Rune inconnue\"",
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\""
                },
                new GapDefinition
                {
                    gapIndex = 5,
                    label = "Sortir du cas",
                    expectedValue = "break",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "break", "stop", "end", "return" }
                }
            };

            puzzle.hints = new List<string>
            {
                "Un switch peut aussi tester des chaines de caracteres (string), pas seulement des nombres.",
                "Les valeurs string dans un case doivent etre entre guillemets : case \"Ignis\":",
                "N'oublie pas le 'break' apres chaque cas, sinon le code continue dans le cas suivant !"
            };

            puzzle.successMessage = "Compilation reussie -- Le traducteur de runes fonctionne !";
            puzzle.failureMessage = "Erreur de compilation... Verifie les valeurs des runes.";

            return puzzle;
        }

        /// <summary>
        /// Tour 03 - Maitrise du Switch
        /// Le joueur ecrit un switch complet en saisie libre (TextInput).
        /// </summary>
        public static PuzzleDefinition CreateSwitchMasteryPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "tour_03";
            puzzle.puzzleName = "Le Commandant du Drone";
            puzzle.description = "Programme les commandes vocales du drone avec un switch complet !";
            puzzle.concept = ConceptType.SwitchCase;
            puzzle.difficulty = DifficultyLevel.Difficile;

            puzzle.templateCode =
@"using UnityEngine;

public class DroneCommander
{
    void ExecuteCommand(string command)
    {
        ___GAP_0___ (___GAP_1___)
        {
            ___GAP_2___ ___GAP_3___:
                Debug.Log(""Decollage !"");
                ___GAP_4___;
            case ___GAP_5___:
                Debug.Log(""Atterrissage"");
                break;
            case ""scan"":
                Debug.Log(""Scan en cours..."");
                break;
            ___GAP_6___:
                Debug.Log(""Commande inconnue"");
                ___GAP_7___;
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class DroneCommander
{
    void ExecuteCommand(string command)
    {
        switch (command)
        {
            case ""fly"":
                Debug.Log(""Decollage !"");
                break;
            case ""land"":
                Debug.Log(""Atterrissage"");
                break;
            case ""scan"":
                Debug.Log(""Scan en cours..."");
                break;
            default:
                Debug.Log(""Commande inconnue"");
                break;
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Mot-cle de structure",
                    expectedValue = "switch",
                    inputType = GapInputType.TextInput,
                    placeholder = "mot-cle"
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Variable a tester",
                    expectedValue = "command",
                    inputType = GapInputType.TextInput,
                    placeholder = "variable"
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Mot-cle de cas",
                    expectedValue = "case",
                    inputType = GapInputType.TextInput,
                    placeholder = "mot-cle"
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Commande de vol",
                    expectedValue = "\"fly\"",
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\""
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Sortir du cas",
                    expectedValue = "break",
                    inputType = GapInputType.TextInput,
                    placeholder = "mot-cle"
                },
                new GapDefinition
                {
                    gapIndex = 5,
                    label = "Commande d'atterrissage",
                    expectedValue = "\"land\"",
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\""
                },
                new GapDefinition
                {
                    gapIndex = 6,
                    label = "Cas par defaut",
                    expectedValue = "default",
                    inputType = GapInputType.TextInput,
                    placeholder = "mot-cle"
                },
                new GapDefinition
                {
                    gapIndex = 7,
                    label = "Sortir du defaut",
                    expectedValue = "break",
                    inputType = GapInputType.TextInput,
                    placeholder = "mot-cle"
                }
            };

            puzzle.hints = new List<string>
            {
                "'switch' teste une variable contre plusieurs valeurs. N'oublie pas les parentheses autour de la variable.",
                "Chaque commande est un 'case' avec une valeur string entre guillemets, comme \"fly\".",
                "'break' empeche de tomber dans le cas suivant. 'default' attrape toutes les commandes non reconnues."
            };

            puzzle.successMessage = "Compilation reussie -- Le drone repond aux commandes vocales !";
            puzzle.failureMessage = "Erreur de compilation... Verifie chaque partie du switch.";

            return puzzle;
        }

        // ==================== FONCTIONS ====================

        /// <summary>
        /// Forge 01 - Decouverte des Fonctions
        /// Le joueur cree sa premiere fonction void et apprend a l'appeler.
        /// </summary>
        public static PuzzleDefinition CreateFunctionDiscoveryPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "forge_01";
            puzzle.puzzleName = "La Premiere Fonction";
            puzzle.description = "Cree ta premiere fonction pour que le drone puisse saluer les habitants !";
            puzzle.concept = ConceptType.Functions;
            puzzle.difficulty = DifficultyLevel.Facile;

            puzzle.templateCode =
@"using UnityEngine;

public class DroneGreeter
{
    ___GAP_0___ ___GAP_1___()
    {
        Debug.Log(""Bonjour, citoyen !"");
    }

    void Start()
    {
        ___GAP_2___;
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class DroneGreeter
{
    void Greet()
    {
        Debug.Log(""Bonjour, citoyen !"");
    }

    void Start()
    {
        Greet();
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Type de retour",
                    expectedValue = "void",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "void", "int", "string", "bool" }
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Nom de la fonction",
                    expectedValue = "Greet",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "Greet", "Hello", "Print", "Say" }
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Appel de la fonction",
                    expectedValue = "Greet()",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "Greet()", "Greet", "void Greet()", "call Greet" }
                }
            };

            puzzle.hints = new List<string>
            {
                "'void' signifie que la fonction ne retourne rien -- elle fait juste une action.",
                "Le nom d'une fonction doit correspondre a sa declaration. Ici c'est 'Greet'.",
                "Pour appeler une fonction, ecris son nom suivi de parentheses : Greet();"
            };

            puzzle.successMessage = "Compilation reussie -- Le drone salue les citoyens !";
            puzzle.failureMessage = "Erreur de compilation... Verifie la declaration et l'appel de la fonction.";

            return puzzle;
        }

        /// <summary>
        /// Forge 02 - Fonctions avec Parametres
        /// Le joueur ajoute un parametre string pour personnaliser le comportement.
        /// </summary>
        public static PuzzleDefinition CreateFunctionParamsPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "forge_02";
            puzzle.puzzleName = "Fonction avec Parametres";
            puzzle.description = "Le drone doit saluer chaque citoyen par son nom. Ajoute un parametre !";
            puzzle.concept = ConceptType.Functions;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class DroneGreeter
{
    void Greet(___GAP_0___ ___GAP_1___)
    {
        Debug.Log(""Bonjour, "" + ___GAP_2___ + "" !"");
    }

    void Start()
    {
        Greet(___GAP_3___);
        Greet(___GAP_4___);
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class DroneGreeter
{
    void Greet(string name)
    {
        Debug.Log(""Bonjour, "" + name + "" !"");
    }

    void Start()
    {
        Greet(""Aria"");
        Greet(""Kael"");
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Type du parametre",
                    expectedValue = "string",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "string", "int", "bool", "float" }
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Nom du parametre",
                    expectedValue = "name",
                    inputType = GapInputType.TextInput,
                    placeholder = "nom"
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Utiliser le parametre",
                    expectedValue = "name",
                    inputType = GapInputType.TextInput,
                    placeholder = "variable"
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Premier appel",
                    expectedValue = "\"Aria\"",
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\""
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Deuxieme appel",
                    expectedValue = "\"Kael\"",
                    alternativeValues = new List<string> { "\"kael\"", "\"KAEL\"" },
                    inputType = GapInputType.TextInput,
                    placeholder = "\"...\"",
                    ignoreCase = true
                }
            };

            puzzle.hints = new List<string>
            {
                "Un parametre se declare avec son type et son nom : string name",
                "Dans le corps de la fonction, utilise le nom du parametre pour acceder a sa valeur.",
                "Pour passer un string en argument, mets-le entre guillemets : Greet(\"Aria\");"
            };

            puzzle.successMessage = "Compilation reussie -- Le drone salue Aria et Kael !";
            puzzle.failureMessage = "Erreur de compilation... Verifie le parametre et les appels.";

            return puzzle;
        }

        /// <summary>
        /// Forge 03 - Fonctions avec Retour
        /// Le joueur utilise return pour renvoyer une valeur depuis une fonction.
        /// </summary>
        public static PuzzleDefinition CreateFunctionReturnPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "forge_03";
            puzzle.puzzleName = "Fonction avec Retour";
            puzzle.description = "Cree une fonction qui calcule les degats et retourne le resultat !";
            puzzle.concept = ConceptType.Functions;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class DamageCalculator
{
    ___GAP_0___ CalculateDamage(int baseDamage, int multiplier)
    {
        int total = baseDamage * multiplier;
        ___GAP_1___ ___GAP_2___;
    }

    void Start()
    {
        int damage = ___GAP_3___;
        Debug.Log(""Degats : "" + damage);
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class DamageCalculator
{
    int CalculateDamage(int baseDamage, int multiplier)
    {
        int total = baseDamage * multiplier;
        return total;
    }

    void Start()
    {
        int damage = CalculateDamage(10, 3);
        Debug.Log(""Degats : "" + damage);
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Type de retour",
                    expectedValue = "int",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "int", "void", "string", "bool" }
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Mot-cle de retour",
                    expectedValue = "return",
                    inputType = GapInputType.Dropdown,
                    options = new List<string> { "return", "send", "output", "give" }
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Valeur a retourner",
                    expectedValue = "total",
                    inputType = GapInputType.TextInput,
                    placeholder = "variable"
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Appel de la fonction",
                    expectedValue = "CalculateDamage(10, 3)",
                    alternativeValues = new List<string> { "CalculateDamage(10,3)", "CalculateDamage( 10, 3 )" },
                    inputType = GapInputType.TextInput,
                    placeholder = "fonction(args)"
                }
            };

            puzzle.hints = new List<string>
            {
                "Si une fonction retourne un int, son type de retour doit etre 'int' (pas 'void').",
                "'return' renvoie une valeur a l'endroit ou la fonction a ete appelee.",
                "Pour appeler une fonction avec des arguments : CalculateDamage(10, 3)"
            };

            puzzle.successMessage = "Compilation reussie -- 30 points de degats infliges !";
            puzzle.failureMessage = "Erreur de compilation... Verifie le type de retour et l'appel.";

            return puzzle;
        }

        /// <summary>
        /// Forge 04 - Maitrise des Fonctions
        /// Le joueur ecrit une fonction complete avec parametres, retour et logique.
        /// </summary>
        public static PuzzleDefinition CreateFunctionMasteryPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "forge_04";
            puzzle.puzzleName = "La Forge Complete";
            puzzle.description = "Ecris une fonction complete qui verifie l'acces a la zone securisee !";
            puzzle.concept = ConceptType.Functions;
            puzzle.difficulty = DifficultyLevel.Difficile;

            puzzle.templateCode =
@"using UnityEngine;

public class SecurityCheck
{
    ___GAP_0___ ___GAP_1___(___GAP_2___ level, ___GAP_3___ hasPass)
    {
        if (level >= 5 && hasPass)
        {
            ___GAP_4___ ___GAP_5___;
        }
        else
        {
            return false;
        }
    }

    void Start()
    {
        bool access = HasAccess(___GAP_6___, ___GAP_7___);
        Debug.Log(""Acces : "" + access);
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class SecurityCheck
{
    bool HasAccess(int level, bool hasPass)
    {
        if (level >= 5 && hasPass)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Start()
    {
        bool access = HasAccess(7, true);
        Debug.Log(""Acces : "" + access);
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition
                {
                    gapIndex = 0,
                    label = "Type de retour",
                    expectedValue = "bool",
                    inputType = GapInputType.TextInput,
                    placeholder = "type"
                },
                new GapDefinition
                {
                    gapIndex = 1,
                    label = "Nom de la fonction",
                    expectedValue = "HasAccess",
                    inputType = GapInputType.TextInput,
                    placeholder = "nom"
                },
                new GapDefinition
                {
                    gapIndex = 2,
                    label = "Type du niveau",
                    expectedValue = "int",
                    inputType = GapInputType.TextInput,
                    placeholder = "type"
                },
                new GapDefinition
                {
                    gapIndex = 3,
                    label = "Type du pass",
                    expectedValue = "bool",
                    inputType = GapInputType.TextInput,
                    placeholder = "type"
                },
                new GapDefinition
                {
                    gapIndex = 4,
                    label = "Mot-cle de retour",
                    expectedValue = "return",
                    inputType = GapInputType.TextInput,
                    placeholder = "mot-cle"
                },
                new GapDefinition
                {
                    gapIndex = 5,
                    label = "Valeur si acces autorise",
                    expectedValue = "true",
                    inputType = GapInputType.TextInput,
                    placeholder = "valeur"
                },
                new GapDefinition
                {
                    gapIndex = 6,
                    label = "Niveau du joueur",
                    expectedValue = "7",
                    alternativeValues = new List<string> { "5", "6", "8", "9", "10" },
                    inputType = GapInputType.TextInput,
                    placeholder = "nombre"
                },
                new GapDefinition
                {
                    gapIndex = 7,
                    label = "Possede le pass ?",
                    expectedValue = "true",
                    inputType = GapInputType.Toggle
                }
            };

            puzzle.hints = new List<string>
            {
                "Une fonction qui retourne vrai ou faux a le type de retour 'bool'.",
                "Les parametres ont chacun un type : 'int' pour un nombre, 'bool' pour vrai/faux.",
                "'return true' renvoie vrai, 'return false' renvoie faux. Le nom doit correspondre exactement."
            };

            puzzle.successMessage = "Compilation reussie -- Acces a la zone securisee autorise !";
            puzzle.failureMessage = "Erreur de compilation... Verifie les types, le retour et les arguments.";

            return puzzle;
        }


        // === PHASE CREER (Free Write) ===

        public static PuzzleDefinition CreateVariablesCreerPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "temple_creer";
            puzzle.puzzleName = "Creation : Variables";
            puzzle.description = "Ecris toi-meme les declarations de variables pour un personnage de jeu !";
            puzzle.concept = ConceptType.Variables;
            puzzle.difficulty = DifficultyLevel.Difficile;
            puzzle.isFreeWrite = true;
            puzzle.instructions =
                "Ecris une classe 'Hero' avec les variables suivantes :\n"
                + "- Un string 'name' avec un nom de ton choix\n"
                + "- Un int 'health' initialise a 100\n"
                + "- Un float 'speed' initialise a un nombre decimal\n"
                + "- Un bool 'isAlive' initialise a true\n\n"
                + "Exemple de format attendu :\n"
                + "  string name = \"Aria\";\n"
                + "  int health = 100;";
            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Declaration string name",
                    pattern = @"\bstring\s+name\s*=\s*""[^""]+""",
                    successMessage = "Variable string name declaree !",
                    failureMessage = "Il manque : string name = \"...\"; (avec guillemets)" },
                new CodePattern { description = "Declaration int health = 100",
                    pattern = @"\bint\s+health\s*=\s*100",
                    successMessage = "Variable int health declaree !",
                    failureMessage = "Il manque : int health = 100;" },
                new CodePattern { description = "Declaration float speed",
                    pattern = @"\bfloat\s+speed\s*=\s*\d+\.?\d*f?",
                    successMessage = "Variable float speed declaree !",
                    failureMessage = "Il manque : float speed = nombre decimal (ex: 3.5f)" },
                new CodePattern { description = "Declaration bool isAlive = true",
                    pattern = @"\bbool\s+isAlive\s*=\s*true",
                    successMessage = "Variable bool isAlive declaree !",
                    failureMessage = "Il manque : bool isAlive = true;" }
            };
            puzzle.hints = new List<string>
            {
                "Chaque variable suit le format : type nom = valeur;",
                "Les strings sont entre guillemets. Les floats peuvent finir par 'f'.",
                "string name = \"Aria\"; int health = 100; float speed = 3.5f; bool isAlive = true;"
            };
            puzzle.successMessage = "Compilation reussie -- Ton heros est cree !";
            puzzle.failureMessage = "Il manque des declarations... Verifie chaque variable.";
            return puzzle;
        }

        public static PuzzleDefinition CreateConditionsCreerPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "pont_creer";
            puzzle.puzzleName = "Creation : Conditions";
            puzzle.description = "Ecris une fonction avec des conditions if/else pour gerer l'acces !";
            puzzle.concept = ConceptType.Conditions;
            puzzle.difficulty = DifficultyLevel.Difficile;
            puzzle.isFreeWrite = true;
            puzzle.instructions =
                "Ecris un bloc if/else if/else qui verifie :\n"
                + "- Si level >= 10 : affiche \"Acces VIP\"\n"
                + "- Sinon si level >= 5 : affiche \"Acces standard\"\n"
                + "- Sinon : affiche \"Acces refuse\"\n\n"
                + "Utilise Debug.Log() pour afficher.";
            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Bloc if avec condition level",
                    pattern = @"\bif\s*\(\s*level\s*>=\s*10\s*\)",
                    successMessage = "Condition if (level >= 10) presente !",
                    failureMessage = "Il manque : if (level >= 10)" },
                new CodePattern { description = "Bloc else if",
                    pattern = @"\belse\s+if\s*\(\s*level\s*>=\s*5\s*\)",
                    successMessage = "Condition else if (level >= 5) presente !",
                    failureMessage = "Il manque : else if (level >= 5)" },
                new CodePattern { description = "Bloc else final",
                    pattern = @"\belse\s*\{",
                    successMessage = "Bloc else present !",
                    failureMessage = "Il manque le bloc else { ... } final" },
                new CodePattern { description = "Debug.Log utilise",
                    pattern = @"Debug\.Log\s*\(",
                    successMessage = "Debug.Log utilise pour afficher !",
                    failureMessage = "Utilise Debug.Log(\"...\") pour afficher les messages." }
            };
            puzzle.hints = new List<string>
            {
                "Commence par if (level >= 10) { Debug.Log(\"Acces VIP\"); }",
                "Ajoute else if (level >= 5) { ... } pour le deuxieme cas.",
                "Termine par else { Debug.Log(\"Acces refuse\"); }"
            };
            puzzle.successMessage = "Compilation reussie -- Le systeme d'acces fonctionne !";
            puzzle.failureMessage = "Les conditions sont incompletes... Verifie if/else if/else.";
            return puzzle;
        }

        public static PuzzleDefinition CreateSwitchCreerPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "tour_creer";
            puzzle.puzzleName = "Creation : Switch";
            puzzle.description = "Ecris un switch complet pour gerer les commandes du drone !";
            puzzle.concept = ConceptType.SwitchCase;
            puzzle.difficulty = DifficultyLevel.Difficile;
            puzzle.isFreeWrite = true;
            puzzle.instructions =
                "Ecris un switch sur la variable 'command' (string) avec :\n"
                + "- case \"fly\" : Debug.Log(\"Envol !\")\n"
                + "- case \"scan\" : Debug.Log(\"Scan en cours...\")\n"
                + "- case \"land\" : Debug.Log(\"Atterrissage\")\n"
                + "- default : Debug.Log(\"Commande inconnue\")\n\n"
                + "N'oublie pas les break; apres chaque case !";
            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Switch sur command",
                    pattern = @"\bswitch\s*\(\s*command\s*\)",
                    successMessage = "switch (command) present !",
                    failureMessage = "Il manque : switch (command)" },
                new CodePattern { description = "Case fly",
                    pattern = "case\\s+\"fly\"",
                    successMessage = "case \"fly\" present !",
                    failureMessage = "Il manque : case \"fly\":" },
                new CodePattern { description = "Case scan",
                    pattern = "case\\s+\"scan\"",
                    successMessage = "case \"scan\" present !",
                    failureMessage = "Il manque : case \"scan\":" },
                new CodePattern { description = "Case land",
                    pattern = "case\\s+\"land\"",
                    successMessage = "case \"land\" present !",
                    failureMessage = "Il manque : case \"land\":" },
                new CodePattern { description = "Default",
                    pattern = @"\bdefault\s*:",
                    successMessage = "default present !",
                    failureMessage = "Il manque le cas default:" },
                new CodePattern { description = "Break statements",
                    pattern = @"\bbreak\s*;",
                    successMessage = "break; utilise !",
                    failureMessage = "N'oublie pas les break; apres chaque case !" }
            };
            puzzle.hints = new List<string>
            {
                "switch (command) { case \"fly\": ... break; case \"scan\": ... break; ... }",
                "Chaque case se termine par break; pour eviter le 'fall-through'.",
                "Le default gere tous les cas non prevus."
            };
            puzzle.successMessage = "Compilation reussie -- Le drone comprend toutes les commandes !";
            puzzle.failureMessage = "Le switch est incomplet... Verifie chaque case et les break.";
            return puzzle;
        }

        public static PuzzleDefinition CreateLoopsCreerPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "vallee_creer";
            puzzle.puzzleName = "Creation : Boucles";
            puzzle.description = "Ecris 3 boucles differentes : for, while et do...while !";
            puzzle.concept = ConceptType.Loops;
            puzzle.difficulty = DifficultyLevel.Difficile;
            puzzle.isFreeWrite = true;
            puzzle.instructions =
                "Ecris les 3 types de boucles :\n\n"
                + "1. Une boucle for qui compte de 0 a 4 (i < 5)\n"
                + "2. Une boucle while avec une condition\n"
                + "3. Une boucle do...while\n\n"
                + "Chaque boucle doit contenir un Debug.Log().";
            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Boucle for",
                    pattern = @"\bfor\s*\(\s*(int\s+)?i\s*=\s*0\s*;\s*i\s*<\s*5\s*;\s*i\s*\+\+\s*\)",
                    successMessage = "Boucle for presente !",
                    failureMessage = "Il manque : for (int i = 0; i < 5; i++)" },
                new CodePattern { description = "Boucle while",
                    pattern = @"\bwhile\s*\([^)]+\)\s*\{",
                    successMessage = "Boucle while presente !",
                    failureMessage = "Il manque une boucle while (condition) { ... }" },
                new CodePattern { description = "Boucle do...while",
                    pattern = @"\bdo\s*\{",
                    successMessage = "Boucle do...while presente !",
                    failureMessage = "Il manque : do { ... } while (condition);" },
                new CodePattern { description = "Debug.Log dans les boucles",
                    pattern = @"Debug\.Log\s*\(",
                    successMessage = "Debug.Log utilise !",
                    failureMessage = "Ajoute Debug.Log() dans tes boucles." }
            };
            puzzle.hints = new List<string>
            {
                "for (int i = 0; i < 5; i++) { Debug.Log(i); }",
                "int x = 10; while (x > 0) { Debug.Log(x); x--; }",
                "int n = 0; do { n++; Debug.Log(n); } while (n < 3);"
            };
            puzzle.successMessage = "Compilation reussie -- Les 3 boucles fonctionnent !";
            puzzle.failureMessage = "Il manque une boucle... Verifie for, while et do...while.";
            return puzzle;
        }

        public static PuzzleDefinition CreateFunctionsCreerPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "forge_creer";
            puzzle.puzzleName = "Creation : Fonctions";
            puzzle.description = "Ecris une fonction avec parametre et retour !";
            puzzle.concept = ConceptType.Functions;
            puzzle.difficulty = DifficultyLevel.Difficile;
            puzzle.isFreeWrite = true;
            puzzle.instructions =
                "Ecris une fonction 'CalculerDegats' qui :\n"
                + "- Prend un int 'force' en parametre\n"
                + "- Prend un int 'defense' en parametre\n"
                + "- Retourne un int (les degats = force - defense)\n"
                + "- Utilise return pour renvoyer le resultat\n\n"
                + "Format : int CalculerDegats(int force, int defense)";
            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Signature de fonction avec int retour",
                    pattern = @"\bint\s+CalculerDegats\s*\(",
                    successMessage = "Signature int CalculerDegats(...) presente !",
                    failureMessage = "Il manque : int CalculerDegats(...)" },
                new CodePattern { description = "Parametre int force",
                    pattern = @"\bint\s+force\b",
                    successMessage = "Parametre int force present !",
                    failureMessage = "Il manque le parametre : int force" },
                new CodePattern { description = "Parametre int defense",
                    pattern = @"\bint\s+defense\b",
                    successMessage = "Parametre int defense present !",
                    failureMessage = "Il manque le parametre : int defense" },
                new CodePattern { description = "Return avec calcul",
                    pattern = @"\breturn\s+force\s*-\s*defense",
                    successMessage = "return force - defense correct !",
                    failureMessage = "Il manque : return force - defense;" }
            };
            puzzle.hints = new List<string>
            {
                "int CalculerDegats(int force, int defense) { ... }",
                "Dans le corps, utilise return force - defense;",
                "int CalculerDegats(int force, int defense) { return force - defense; }"
            };
            puzzle.successMessage = "Compilation reussie -- Ta fonction de combat est operationnelle !";
            puzzle.failureMessage = "La fonction est incomplete... Verifie signature et return.";
            return puzzle;
        }


        // === FIND THE BUG PUZZLES ===

        public static PuzzleDefinition CreateFindBugVariablesPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "defi_bug_01";
            puzzle.puzzleName = "Trouve le Bug : Variables";
            puzzle.description = "Ce code contient des erreurs ! Trouve et corrige les bugs.";
            puzzle.concept = ConceptType.Variables;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class BuggyDrone
{
    // 3 bugs se cachent dans ce code !
    int name = ""C-Sharp"";           // Ligne 1
    string health = 100;              // Ligne 2
    bool speed = 5.5f;                // Ligne 3

    void ShowInfo()
    {
        Debug.Log(name + "" - HP:"" + health);
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class BuggyDrone
{
    string name = ""C-Sharp"";
    int health = 100;
    float speed = 5.5f;

    void ShowInfo()
    {
        Debug.Log(name + "" - HP:"" + health);
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Ligne 1 : quel type pour un texte ?",
                    expectedValue = "string", inputType = GapInputType.Dropdown,
                    options = new List<string> { "string", "int", "bool", "float" } },
                new GapDefinition { gapIndex = 1, label = "Ligne 2 : quel type pour un nombre entier ?",
                    expectedValue = "int", inputType = GapInputType.Dropdown,
                    options = new List<string> { "int", "string", "bool", "float" } },
                new GapDefinition { gapIndex = 2, label = "Ligne 3 : quel type pour un nombre decimal ?",
                    expectedValue = "float", inputType = GapInputType.Dropdown,
                    options = new List<string> { "float", "bool", "int", "string" } }
            };

            puzzle.hints = new List<string>
            {
                "Regarde le type et la valeur de chaque variable. Le type doit correspondre a la valeur !",
                "\"C-Sharp\" est un texte -> string. 100 est un entier -> int. 5.5f est un decimal -> float.",
                "Ligne 1: string, Ligne 2: int, Ligne 3: float"
            };

            puzzle.successMessage = "Bugs corriges ! Les types correspondent aux valeurs maintenant.";
            puzzle.failureMessage = "Il reste des bugs... Verifie que chaque type correspond a sa valeur.";

            return puzzle;
        }

        public static PuzzleDefinition CreateFindBugConditionsPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "defi_bug_02";
            puzzle.puzzleName = "Trouve le Bug : Conditions";
            puzzle.description = "La logique des conditions est cassee ! Trouve les erreurs.";
            puzzle.concept = ConceptType.Conditions;
            puzzle.difficulty = DifficultyLevel.Moyen;

            puzzle.templateCode =
@"using UnityEngine;

public class BuggyDoor
{
    void CheckAccess(int level, bool hasKey)
    {
        // Bug 1 : mauvais operateur (= au lieu de ==)
        if (level ___GAP_0___ 5)
        {
            Debug.Log(""Acces niveau OK"");
        }

        // Bug 2 : operateur logique incorrect (|| au lieu de &&)
        if (level >= 5 ___GAP_1___ hasKey)
        {
            Debug.Log(""Acces complet"");
        }

        // Bug 3 : condition inversee (> au lieu de <)
        if (level ___GAP_2___ 0)
        {
            Debug.Log(""Niveau invalide !"");
        }
    }
}";

            puzzle.solutionCode =
@"using UnityEngine;

public class BuggyDoor
{
    void CheckAccess(int level, bool hasKey)
    {
        if (level == 5)
        {
            Debug.Log(""Acces niveau OK"");
        }

        if (level >= 5 && hasKey)
        {
            Debug.Log(""Acces complet"");
        }

        if (level < 0)
        {
            Debug.Log(""Niveau invalide !"");
        }
    }
}";

            puzzle.gaps = new List<GapDefinition>
            {
                new GapDefinition { gapIndex = 0, label = "Bug 1 : operateur de comparaison (egal a 5)",
                    expectedValue = "==", inputType = GapInputType.Dropdown,
                    options = new List<string> { "==", "=", "!=", ">=" } },
                new GapDefinition { gapIndex = 1, label = "Bug 2 : on veut les DEUX conditions vraies",
                    expectedValue = "&&", inputType = GapInputType.Dropdown,
                    options = new List<string> { "&&", "||", "!=", "==" } },
                new GapDefinition { gapIndex = 2, label = "Bug 3 : level negatif (inferieur a 0)",
                    expectedValue = "<", inputType = GapInputType.Dropdown,
                    options = new List<string> { "<", ">", "==", "<=" } }
            };

            puzzle.hints = new List<string>
            {
                "Bug 1 : en C#, = est l'assignation, == est la comparaison !",
                "Bug 2 : && veut dire ET (les deux vraies), || veut dire OU (au moins une).",
                "Bug 3 : un niveau invalide est NEGATIF, donc < 0 (pas > 0)."
            };

            puzzle.successMessage = "Bugs corriges ! La porte fonctionne correctement.";
            puzzle.failureMessage = "Il reste des bugs... Relis chaque commentaire d'indice.";

            return puzzle;
        }

        // === PREDICT THE OUTPUT PUZZLES ===

        public static PuzzleDefinition CreatePredictOutputLoopsPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "defi_predict_01";
            puzzle.puzzleName = "Predis la Sortie : Boucles";
            puzzle.description = "Lis le code et tape ce que Debug.Log va afficher !";
            puzzle.concept = ConceptType.Loops;
            puzzle.difficulty = DifficultyLevel.Moyen;
            puzzle.isFreeWrite = true;

            puzzle.instructions =
                "Lis ce code et tape EXACTEMENT ce que la console affichera :\n\n"
                + "for (int i = 1; i <= 3; i++)\n"
                + "{\n"
                + "    Debug.Log(\"Tour \" + i);\n"
                + "}\n"
                + "Debug.Log(\"Fin\");\n\n"
                + "Ecris chaque ligne de sortie, une par ligne :\n"
                + "Tour 1\n"
                + "Tour 2\n"
                + "...etc";

            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Premiere sortie : Tour 1",
                    pattern = "Tour 1",
                    successMessage = "Tour 1 correct !",
                    failureMessage = "La boucle commence a i=1 -> 'Tour 1'" },
                new CodePattern { description = "Deuxieme sortie : Tour 2",
                    pattern = "Tour 2",
                    successMessage = "Tour 2 correct !",
                    failureMessage = "i++ fait passer i a 2 -> 'Tour 2'" },
                new CodePattern { description = "Troisieme sortie : Tour 3",
                    pattern = "Tour 3",
                    successMessage = "Tour 3 correct !",
                    failureMessage = "i=3, i<=3 est vrai -> 'Tour 3'" },
                new CodePattern { description = "Derniere sortie : Fin",
                    pattern = "Fin",
                    successMessage = "Fin correct ! La boucle s'arrete quand i=4 (4 <= 3 est faux).",
                    failureMessage = "Apres la boucle, Debug.Log(\"Fin\") s'execute." },
                new CodePattern { description = "Pas de Tour 4",
                    pattern = "^(?!.*Tour 4)",
                    successMessage = "Correct : pas de Tour 4 !",
                    failureMessage = "Attention : i <= 3, donc i=4 ne rentre PAS dans la boucle !" }
            };

            puzzle.hints = new List<string>
            {
                "i commence a 1, et la boucle tourne tant que i <= 3.",
                "A chaque tour : i=1 -> Tour 1, i=2 -> Tour 2, i=3 -> Tour 3.",
                "Quand i=4, la condition i<=3 est fausse, la boucle s'arrete. Puis 'Fin' s'affiche."
            };

            puzzle.successMessage = "Bravo ! Tu sais lire le flux d'execution d'une boucle !";
            puzzle.failureMessage = "Relis le code etape par etape...";

            return puzzle;
        }

        public static PuzzleDefinition CreatePredictOutputConditionsPuzzle()
        {
            var puzzle = ScriptableObject.CreateInstance<PuzzleDefinition>();
            puzzle.puzzleId = "defi_predict_02";
            puzzle.puzzleName = "Predis la Sortie : Conditions";
            puzzle.description = "Lis le code et tape ce que la console affichera !";
            puzzle.concept = ConceptType.Conditions;
            puzzle.difficulty = DifficultyLevel.Moyen;
            puzzle.isFreeWrite = true;

            puzzle.instructions =
                "Lis ce code avec level = 7 et hasKey = false :\n\n"
                + "int level = 7;\n"
                + "bool hasKey = false;\n\n"
                + "if (level >= 10)\n"
                + "    Debug.Log(\"VIP\");\n"
                + "else if (level >= 5 && hasKey)\n"
                + "    Debug.Log(\"Standard\");\n"
                + "else if (level >= 5)\n"
                + "    Debug.Log(\"Basique\");\n"
                + "else\n"
                + "    Debug.Log(\"Refuse\");\n\n"
                + "Quelle est LA SEULE ligne affichee ?";

            puzzle.requiredPatterns = new List<CodePattern>
            {
                new CodePattern { description = "Sortie correcte : Basique",
                    pattern = "Basique",
                    successMessage = "Correct ! level=7 >= 5 mais hasKey=false, donc 'Basique' !",
                    failureMessage = "level=7: pas >= 10, pas (>=5 && hasKey car false), mais >= 5 -> Basique" },
                new CodePattern { description = "Pas VIP",
                    pattern = "^(?!.*VIP)",
                    successMessage = "Correct : pas VIP (level < 10).",
                    failureMessage = "7 < 10, donc la condition level >= 10 est fausse." },
                new CodePattern { description = "Pas Standard",
                    pattern = "^(?!.*Standard)",
                    successMessage = "Correct : pas Standard (hasKey est false).",
                    failureMessage = "hasKey = false, donc level >= 5 && hasKey est faux." }
            };

            puzzle.hints = new List<string>
            {
                "level = 7 : est-ce que 7 >= 10 ? Non. On passe au else if.",
                "7 >= 5 && false = false. On passe au else if suivant. 7 >= 5 ? Oui !",
                "La reponse est 'Basique'. Les else if sont evalues dans l'ordre."
            };

            puzzle.successMessage = "Excellent ! Tu maitrises le flux des conditions !";
            puzzle.failureMessage = "Suis le code ligne par ligne avec les valeurs donnees...";

            return puzzle;
        }

    }
}