using System.Collections.Generic;
using UnityEngine;

namespace Codex.Dialogue
{
    public static class ExampleDialogues
    {
        public static DialogueData CreateIntroDialogue()
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.dialogueId = "intro";
            data.lines = new List<DialogueLine>
            {
                new DialogueLine {
                    speakerName = "???",
                    text = "...",
                    letterDelay = 0.08f
                },
                new DialogueLine {
                    speakerName = "Systeme",
                    text = "Initialisation du protocole de secours... Connexion etablie."
                },
                new DialogueLine {
                    speakerName = "Systeme",
                    text = "Bienvenue, Architecte. Arcadia, notre cite numerique, a ete corrompue par un virus."
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Salut ! Je suis C-Sharp, ton drone compagnon. Je serai ton guide dans cette mission !"
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Pour reparer Arcadia, tu devras maitriser le langage C#. Chaque zone cache des terminaux de code a resoudre."
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Approche-toi d'un terminal et appuie sur [E] pour commencer. Je t'expliquerai chaque concept !"
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Commencons par le Temple des Variables. Suis-moi !"
                }
            };
            return data;
        }

        public static DialogueData CreateZoneIntro(string zoneName, string concept, string description)
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.dialogueId = "zone_intro_" + zoneName.ToLower().Replace(" ", "_");
            data.lines = new List<DialogueLine>
            {
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Nous voici dans " + zoneName + " !"
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Ici, tu vas apprendre les " + concept + "."
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = description
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Approche-toi d'un terminal pour commencer. Je suis la si tu as besoin d'aide !"
                }
            };
            return data;
        }

        public static DialogueData CreateZoneComplete(string zoneName)
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.dialogueId = "zone_complete_" + zoneName.ToLower().Replace(" ", "_");
            data.lines = new List<DialogueLine>
            {
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Incroyable ! Tu as complete tous les puzzles de " + zoneName + " !"
                },
                new DialogueLine {
                    speakerName = "C-Sharp",
                    text = "Arcadia reprend vie petit a petit grace a toi. La zone suivante t'attend !"
                },
                new DialogueLine {
                    speakerName = "Systeme",
                    text = "Zone restauree. Progression sauvegardee. Acces a la zone suivante debloque."
                }
            };
            return data;
        }

        public static DialogueData CreateTempleIntro()
        {
            return CreateZoneIntro(
                "le Temple des Variables",
                "variables et les types de donnees",
                "Les variables sont comme des boites nommees qui stockent des valeurs. "
                + "int pour les nombres, string pour le texte, bool pour vrai/faux, float pour les decimaux. "
                + "C'est la base de toute programmation !"
            );
        }

        public static DialogueData CreatePontIntro()
        {
            return CreateZoneIntro(
                "le Pont des Conditions",
                "conditions if/else",
                "Les conditions permettent au programme de prendre des decisions. "
                + "Si une condition est vraie, on execute un bloc de code. Sinon, un autre. "
                + "C'est comme un gardien qui verifie ton pass avant de te laisser passer !"
            );
        }

        public static DialogueData CreateTourIntro()
        {
            return CreateZoneIntro(
                "la Tour du Switch",
                "switch/case",
                "Le switch est comme un panneau de controle avec plusieurs boutons. "
                + "Selon la valeur d'une variable, un case different s'active. "
                + "Plus propre qu'une longue chaine de if/else !"
            );
        }

        public static DialogueData CreateValleeIntro()
        {
            return CreateZoneIntro(
                "la Vallee des Boucles",
                "boucles for, while et do...while",
                "Les boucles repetent du code plusieurs fois automatiquement. "
                + "for quand tu connais le nombre de tours, while quand c'est une condition, "
                + "do...while quand tu veux au moins une execution. Puissant !"
            );
        }

        public static DialogueData CreateForgeIntro()
        {
            return CreateZoneIntro(
                "la Forge des Fonctions",
                "fonctions",
                "Les fonctions sont des blocs de code reutilisables avec un nom. "
                + "Tu peux leur passer des parametres et recuperer un resultat. "
                + "C'est comme creer tes propres outils dans la forge !"
            );
        }
    }
}
