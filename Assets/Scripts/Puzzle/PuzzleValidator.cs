using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Codex.Puzzle
{
    /// <summary>
    /// Validates player answers against the puzzle definition.
    /// Uses pattern matching - no actual C# compilation.
    /// Returns detailed results per gap for targeted feedback.
    /// </summary>
    public static class PuzzleValidator
    {
        public static ValidationResult Validate(PuzzleDefinition puzzle, Dictionary<int, string> playerAnswers)
        {
            var result = new ValidationResult
            {
                IsSuccess = true,
                GapResults = new List<GapResult>()
            };

            if (puzzle == null || puzzle.gaps == null)
            {
                result.IsSuccess = false;
                result.Message = "Puzzle invalide.";
                return result;
            }

            int correctCount = 0;

            foreach (var gap in puzzle.gaps)
            {
                var gapResult = new GapResult
                {
                    GapIndex = gap.gapIndex,
                    Label = gap.label
                };

                if (!playerAnswers.ContainsKey(gap.gapIndex))
                {
                    gapResult.IsCorrect = false;
                    gapResult.Message = $"Champ [{gap.gapIndex}] non rempli.";
                    result.IsSuccess = false;
                    result.GapResults.Add(gapResult);
                    continue;
                }

                string answer = playerAnswers[gap.gapIndex];

                if (gap.trimWhitespace)
                    answer = answer.Trim();

                bool isCorrect = CompareAnswer(answer, gap.expectedValue, gap.ignoreCase);

                if (!isCorrect && gap.alternativeValues != null)
                {
                    foreach (string alt in gap.alternativeValues)
                    {
                        if (CompareAnswer(answer, alt, gap.ignoreCase))
                        {
                            isCorrect = true;
                            break;
                        }
                    }
                }

                gapResult.IsCorrect = isCorrect;
                gapResult.PlayerAnswer = answer;
                gapResult.ExpectedAnswer = gap.expectedValue;

                if (isCorrect)
                {
                    correctCount++;
                    gapResult.Message = "Correct !";
                }
                else
                {
                    result.IsSuccess = false;
                    gapResult.Message = GenerateHint(gap, answer);
                }

                result.GapResults.Add(gapResult);
            }

            result.CorrectCount = correctCount;
            result.TotalCount = puzzle.gaps.Count;
            result.Score = puzzle.gaps.Count > 0
                ? (float)correctCount / puzzle.gaps.Count
                : 0f;

            result.Message = result.IsSuccess
                ? puzzle.successMessage
                : puzzle.failureMessage;

            return result;
        }

        static bool CompareAnswer(string answer, string expected, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(answer) || string.IsNullOrEmpty(expected))
                return false;

            return ignoreCase
                ? answer.Equals(expected, System.StringComparison.OrdinalIgnoreCase)
                : answer == expected;
        }

        static string GenerateHint(GapDefinition gap, string playerAnswer)
        {
            if (string.IsNullOrEmpty(playerAnswer))
                return $"Le champ '{gap.label}' ne peut pas être vide.";

            // Type-specific hints
            if (gap.expectedValue == "int" || gap.expectedValue == "float" ||
                gap.expectedValue == "string" || gap.expectedValue == "bool")
            {
                return $"Vérifie le type attendu pour '{gap.label}'. " +
                       "Rappel : int = nombre entier, float = décimal, string = texte, bool = vrai/faux.";
            }

            if (gap.expectedValue == "true" || gap.expectedValue == "false")
            {
                return $"'{gap.label}' attend une valeur booléenne (true ou false).";
            }

            if (int.TryParse(gap.expectedValue, out _) && !int.TryParse(playerAnswer, out _))
            {
                return $"'{gap.label}' attend un nombre entier.";
            }

            if (playerAnswer.Length > gap.expectedValue.Length * 2)
            {
                return $"Ta réponse pour '{gap.label}' semble trop longue.";
            }

            return $"Vérifie ta réponse pour '{gap.label}'. Réessaie !";
        }

        /// <summary>
        /// Builds the final code by replacing gap placeholders with player answers.
        /// Useful for displaying the "compiled" result.
        /// </summary>
        public static string BuildFinalCode(PuzzleDefinition puzzle, Dictionary<int, string> playerAnswers)
        {
            string code = puzzle.templateCode;
            foreach (var kvp in playerAnswers)
            {
                code = code.Replace($"___GAP_{kvp.Key}___", kvp.Value);
            }
            return code;
        }

        public static ValidationResult ValidateFreeWrite(PuzzleDefinition puzzle, string playerCode)
        {
            var result = new ValidationResult
            {
                IsSuccess = true,
                GapResults = new List<GapResult>()
            };

            if (puzzle == null || puzzle.requiredPatterns == null || puzzle.requiredPatterns.Count == 0)
            {
                result.IsSuccess = false;
                result.Message = "Puzzle invalide.";
                return result;
            }

            string code = playerCode ?? "";
            int correct = 0;

            for (int i = 0; i < puzzle.requiredPatterns.Count; i++)
            {
                var pat = puzzle.requiredPatterns[i];
                bool match = false;
                try { match = Regex.IsMatch(code, pat.pattern); }
                catch { }

                var gr = new GapResult
                {
                    GapIndex = i,
                    Label = pat.description,
                    IsCorrect = match,
                    Message = match ? pat.successMessage : pat.failureMessage,
                    PlayerAnswer = match ? "OK" : "manquant"
                };
                result.GapResults.Add(gr);
                if (match) correct++;
                else result.IsSuccess = false;
            }

            result.CorrectCount = correct;
            result.TotalCount = puzzle.requiredPatterns.Count;
            result.Score = puzzle.requiredPatterns.Count > 0
                ? (float)correct / puzzle.requiredPatterns.Count : 0f;
            result.Message = result.IsSuccess
                ? puzzle.successMessage : puzzle.failureMessage;
            return result;
        }
    }

    public class ValidationResult
    {
        public bool IsSuccess;
        public string Message;
        public int CorrectCount;
        public int TotalCount;
        public float Score;
        public List<GapResult> GapResults;
    }

    public class GapResult
    {
        public int GapIndex;
        public string Label;
        public bool IsCorrect;
        public string Message;
        public string PlayerAnswer;
        public string ExpectedAnswer;
    }
}
