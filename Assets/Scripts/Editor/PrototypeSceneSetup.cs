#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Codex.Core;
using Codex.UI;

namespace Codex.EditorTools
{
    public static class PrototypeSceneSetup
    {
        [MenuItem("Codex/Create Prototype Scene")]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Creer Scene Prototype",
                "Cela va creer une nouvelle scene avec :\n"
                + "- Terrain + eclairage\n"
                + "- 3 terminaux de puzzle\n"
                + "- WorldCodexBridge\n"
                + "- CodexTestUI\n\n"
                + "Tu devras ensuite ajouter manuellement :\n"
                + "- Le prefab PlayerArmature (StarterAssets)\n"
                + "- La camera Cinemachine\n\n"
                + "Continuer ?", "Creer", "Annuler"))
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // --- Lighting ---
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.75f);

            var sun = GameObject.Find("Directional Light");
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
                var light = sun.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(1f, 0.95f, 0.85f);
                    light.intensity = 1.2f;
                }
            }

            // --- Ground plane ---
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            var groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.25f, 0.35f, 0.2f);
            ground.GetComponent<Renderer>().material = groundMat;

            // --- 3 Puzzle Terminals ---
            Vector3[] positions = {
                new Vector3(0f, 0.75f, 8f),
                new Vector3(-6f, 0.75f, 4f),
                new Vector3(6f, 0.75f, 4f)
            };
            string[] puzzleIds = { "temple_01", "pont_01", "tour_01" };
            string[] names = { "Terminal_Variables", "Terminal_Conditions", "Terminal_Switch" };
            Color[] colors = {
                new Color(0.2f, 0.6f, 1f),
                new Color(1f, 0.5f, 0.2f),
                new Color(0.6f, 0.2f, 1f)
            };

            for (int i = 0; i < 3; i++)
            {
                var terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                terminal.name = names[i];
                terminal.transform.position = positions[i];
                terminal.transform.localScale = new Vector3(1f, 1.5f, 0.3f);

                var mat = new Material(Shader.Find("Standard"));
                mat.color = colors[i];
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", colors[i] * 0.2f);
                terminal.GetComponent<Renderer>().material = mat;

                var col = terminal.GetComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(3f, 2f, 3f);

                var pt = terminal.AddComponent<PuzzleTerminal>();
                var so = new SerializedObject(pt);
                so.FindProperty("puzzleId").stringValue = puzzleIds[i];
                so.ApplyModifiedProperties();
            }

            // --- Player spawn marker ---
            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, 0f, 0f);
            var icon = spawn.AddComponent<SpriteRenderer>();
            icon.color = Color.green;

            // --- WorldCodexBridge ---
            var bridgeGO = new GameObject("WorldCodexBridge");
            bridgeGO.AddComponent<WorldCodexBridge>();

            // --- CodexTestUI ---
            var codexGO = new GameObject("CodexTestUI");
            codexGO.AddComponent<CodexTestUI>();

            // --- DialogueUI ---
            var dialogueGO = new GameObject("DialogueUI");
            dialogueGO.AddComponent<Codex.Dialogue.DialogueUI>();

            // --- ZoneManager ---
            var zoneGO = new GameObject("ZoneManager");
            zoneGO.AddComponent<ZoneManager>();

            // --- Instructions label ---
            Debug.Log("=== SCENE PROTOTYPE CREEE ===");
            Debug.Log("Etapes manuelles restantes :");
            Debug.Log("1. Drag 'StarterAssets/ThirdPersonController/Prefabs/PlayerArmature' dans la scene");
            Debug.Log("2. Place-le sur PlayerSpawnPoint (0,0,0)");
            Debug.Log("3. Assure-toi que PlayerArmature a le tag 'Player'");
            Debug.Log("4. Sur WorldCodexBridge : assigne 'Player' = PlayerArmature");
            Debug.Log("5. Sur WorldCodexBridge : assigne 'Codex UI' = CodexTestUI");
            Debug.Log("6. Sauvegarde la scene (Ctrl+S) dans Assets/Scenes/Prototype.unity");

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = spawn;

            EditorUtility.DisplayDialog("Scene creee !",
                "Scene prototype creee avec succes !\n\n"
                + "Etapes restantes :\n"
                + "1. Drag PlayerArmature (StarterAssets) dans la scene\n"
                + "2. Place-le a (0,0,0)\n"
                + "3. Verifie le tag 'Player' sur PlayerArmature\n"
                + "4. Assigne les references sur WorldCodexBridge\n"
                + "5. Sauvegarde la scene (Ctrl+S)\n\n"
                + "Voir la Console pour le detail.",
                "OK");
        }
    }
}
#endif
