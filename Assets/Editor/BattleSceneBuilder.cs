using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WordBattle.Game;
using WordBattle.Game.UI;

namespace WordBattle.Editor
{
    /// <summary>
    /// Builds the whole MVP scene (systems, EventSystem, and all UI screens) from code,
    /// so the layout is reproducible and reviewable. Re-running replaces the generated objects.
    /// </summary>
    public static class BattleSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/WordBattle.unity";
        private const string DictionaryPath = "Assets/Data/WordBattleDictionary.txt";
        private const string SystemsName = "[Systems]";
        private const string CanvasName = "[UI] Canvas";
        private const string EventSystemName = "EventSystem";
        private const int TileCount = 8;

        private static readonly Color Background = new Color32(0x1B, 0x1F, 0x2A, 0xFF);
        private static readonly Color Panel = new Color32(0x12, 0x15, 0x1D, 0xFF);
        private static readonly Color TextLight = new Color32(0xEE, 0xEE, 0xEE, 0xFF);
        private static readonly Color TextMuted = new Color32(0xA0, 0xA6, 0xB4, 0xFF);
        private static readonly Color Gold = new Color32(0xF2, 0xBF, 0x33, 0xFF);
        private static readonly Color TileFace = new Color32(0xF2, 0xE6, 0xC9, 0xFF);
        private static readonly Color TileText = new Color32(0x2A, 0x22, 0x18, 0xFF);
        private static readonly Color ButtonPrimary = new Color32(0x3E, 0x7C, 0xE0, 0xFF);
        private static readonly Color ButtonSecondary = new Color32(0x4A, 0x50, 0x60, 0xFF);
        private static readonly Color InputBackground = new Color32(0x2A, 0x30, 0x3E, 0xFF);

        private static TMP_DefaultControls.Resources resources;

        [MenuItem("Word Battle/Rebuild Battle Scene")]
        private static void RebuildFromMenu()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Rebuild Battle Scene",
                $"This replaces {SystemsName}, {CanvasName} and {EventSystemName} in {ScenePath}. Manual edits to those objects will be lost.",
                "Rebuild",
                "Cancel");

            if (confirmed)
            {
                Build();
            }
        }

        public static void Build()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }
                scene = EditorSceneManager.OpenScene(ScenePath);
            }

            foreach (string name in new[] { SystemsName, CanvasName, EventSystemName })
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == name)
                    {
                        Undo.DestroyObjectImmediate(root);
                    }
                }
            }

            resources = new TMP_DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            };

            Camera camera = Camera.main;
            if (camera != null)
            {
                Undo.RecordObject(camera, "Set camera background");
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Background;
            }

            var eventSystem = new GameObject(EventSystemName, typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");

            // --- Systems ---
            var systems = new GameObject(SystemsName);
            Undo.RegisterCreatedObjectUndo(systems, "Create systems");
            var dictionaryManager = systems.AddComponent<DictionaryManager>();
            var battleManager = systems.AddComponent<BattleManager>();
            var gameModeManager = systems.AddComponent<GameModeManager>();
            SetRef(dictionaryManager, "dictionaryFile", AssetDatabase.LoadAssetAtPath<TextAsset>(DictionaryPath));

            // --- Canvas ---
            var canvasObject = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create canvas");
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            Transform canvas = canvasObject.transform;

            Image background = CreateUIObject("Background", canvas).gameObject.AddComponent<Image>();
            background.color = Background;
            Stretch(background.rectTransform);

            GameObject mainMenu = BuildMainMenu(canvas, gameModeManager);
            GameObject gameplay = BuildGameplay(canvas, battleManager);
            GameObject victory = BuildVictory(canvas, gameModeManager, battleManager);

            SetRef(gameModeManager, "battleManager", battleManager);
            SetRef(gameModeManager, "mainMenuScreen", mainMenu);
            SetRef(gameModeManager, "gameplayScreen", gameplay);
            SetRef(gameModeManager, "victoryScreen", victory);

            gameplay.SetActive(false);
            victory.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Word Battle scene rebuilt and saved to {ScenePath}.");
        }

        // --- Screens ---

        private static GameObject BuildMainMenu(Transform canvas, GameModeManager gameModeManager)
        {
            RectTransform screen = CreateScreen("MainMenuScreen", canvas, 40);

            CreateText(screen, "Title", "WORD BATTLE", 150, Gold, FontStyles.Bold, 180);
            CreateText(screen, "Tagline", "Same tiles. Thirty seconds. Best word wins.", 44, TextMuted, FontStyles.Normal, 70);
            CreateSpacer(screen, 40);
            RectTransform startRow = CreateRow(screen, "StartRow", 120, 0, TextAnchor.MiddleCenter);
            Button start = CreateButton(startRow, "StartButton", "Start Game", ButtonPrimary, 480, 120, out _);

            var view = screen.gameObject.AddComponent<MainMenuView>();
            SetRef(view, "gameModeManager", gameModeManager);
            SetRef(view, "startButton", start);
            return screen.gameObject;
        }

        private static GameObject BuildGameplay(Transform canvas, BattleManager battleManager)
        {
            RectTransform screen = CreateScreen("GameplayScreen", canvas, 24, TextAnchor.UpperCenter);
            screen.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(80, 80, 50, 50);

            // Header: round info | timer | match score
            RectTransform header = CreateRow(screen, "Header", 110, 20, TextAnchor.MiddleCenter);
            TMP_Text round = CreateText(header, "RoundText", "Round 1", 40, TextMuted, FontStyles.Normal, 110, TextAlignmentOptions.Left);
            Flexible(round.gameObject);
            TMP_Text timer = CreateText(header, "TimerText", "30", 100, TextLight, FontStyles.Bold, 110);
            timer.GetComponent<LayoutElement>().preferredWidth = 240;
            TMP_Text score = CreateText(header, "MatchScoreText", "You 0 – 0 Pip", 40, TextMuted, FontStyles.Normal, 110, TextAlignmentOptions.Right);
            Flexible(score.gameObject);

            TMP_Text opponentStatus = CreateText(screen, "OpponentStatusText", "Pip is thinking…", 38, TextMuted, FontStyles.Italic, 60);

            // Rack
            RectTransform rack = CreateRow(screen, "Rack", 170, 22, TextAnchor.MiddleCenter);
            var tiles = new GameObject[TileCount];
            var letters = new TMP_Text[TileCount];
            var scores = new TMP_Text[TileCount];
            for (int i = 0; i < TileCount; i++)
            {
                RectTransform tile = CreateUIObject($"Tile{i + 1}", rack);
                tile.gameObject.AddComponent<Image>().color = TileFace;
                var layout = tile.gameObject.AddComponent<LayoutElement>();
                layout.preferredWidth = 150;
                layout.preferredHeight = 150;

                letters[i] = CreateFreeText(tile, "Letter", "A", 100, TileText, FontStyles.Bold, TextAlignmentOptions.Center);
                Stretch(letters[i].rectTransform);

                scores[i] = CreateFreeText(tile, "Score", "1", 32, TileText, FontStyles.Bold, TextAlignmentOptions.BottomRight);
                Stretch(scores[i].rectTransform);
                scores[i].margin = new Vector4(0, 0, 12, 6);

                tiles[i] = tile.gameObject;
            }

            CreateSpacer(screen, 20);

            // Word entry
            RectTransform inputRow = CreateRow(screen, "InputRow", 130, 0, TextAnchor.MiddleCenter);
            TMP_InputField input = CreateInputField(inputRow, 900, 130);

            TMP_Text feedback = CreateText(screen, "FeedbackText", "Type a word using the tiles above", 40, TextMuted, FontStyles.Normal, 60);

            RectTransform submitRow = CreateRow(screen, "SubmitRow", 110, 0, TextAnchor.MiddleCenter);
            Button submit = CreateButton(submitRow, "SubmitButton", "Lock In", ButtonPrimary, 420, 110, out _);

            TMP_Text playerStatus = CreateText(screen, "PlayerStatusText", "Make the best word you can", 38, TextMuted, FontStyles.Italic, 60);

            // Results overlay (ignores the screen's layout and covers it)
            RectTransform results = CreateUIObject("ResultsPanel", screen);
            results.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Stretch(results);
            results.gameObject.AddComponent<Image>().color = Panel;
            var resultsLayout = results.gameObject.AddComponent<VerticalLayoutGroup>();
            ConfigureColumn(resultsLayout, 26, TextAnchor.MiddleCenter);

            TMP_Text resultsTitle = CreateText(results, "ResultsTitleText", "You win the round!", 96, Gold, FontStyles.Bold, 120);
            TMP_Text resultsReason = CreateText(results, "ResultsReasonText", "You scored higher", 44, TextMuted, FontStyles.Normal, 60);
            CreateSpacer(results, 20);
            TMP_Text playerResult = CreateText(results, "PlayerResultText", "You: WORD — 0 pts", 56, TextLight, FontStyles.Normal, 75);
            TMP_Text opponentResult = CreateText(results, "OpponentResultText", "Pip: WORD — 0 pts", 56, TextLight, FontStyles.Normal, 75);
            TMP_Text bestWord = CreateText(results, "BestWordText", "Best possible: —", 36, TextMuted, FontStyles.Italic, 55);
            CreateSpacer(results, 30);
            RectTransform continueRow = CreateRow(results, "ContinueRow", 110, 0, TextAnchor.MiddleCenter);
            Button continueButton = CreateButton(continueRow, "ContinueButton", "Next round", ButtonPrimary, 480, 110, out TMP_Text continueLabel);
            results.gameObject.SetActive(false);

            var view = screen.gameObject.AddComponent<BattleView>();
            SetRef(view, "battleManager", battleManager);
            SetRef(view, "roundText", round);
            SetRef(view, "matchScoreText", score);
            SetRef(view, "timerText", timer);
            SetArray(view, "tiles", tiles);
            SetArray(view, "tileLetters", letters);
            SetArray(view, "tileScores", scores);
            SetRef(view, "wordInput", input);
            SetRef(view, "feedbackText", feedback);
            SetRef(view, "submitButton", submit);
            SetRef(view, "playerStatusText", playerStatus);
            SetRef(view, "opponentStatusText", opponentStatus);
            SetRef(view, "resultsPanel", results.gameObject);
            SetRef(view, "resultsTitleText", resultsTitle);
            SetRef(view, "resultsReasonText", resultsReason);
            SetRef(view, "playerResultText", playerResult);
            SetRef(view, "opponentResultText", opponentResult);
            SetRef(view, "bestWordText", bestWord);
            SetRef(view, "continueButton", continueButton);
            SetRef(view, "continueButtonLabel", continueLabel);
            return screen.gameObject;
        }

        private static GameObject BuildVictory(Transform canvas, GameModeManager gameModeManager, BattleManager battleManager)
        {
            RectTransform screen = CreateScreen("VictoryScreen", canvas, 40);

            TMP_Text title = CreateText(screen, "TitleText", "Victory!", 170, Gold, FontStyles.Bold, 200);
            TMP_Text score = CreateText(screen, "ScoreText", "You 2 – 1 Pip", 64, TextLight, FontStyles.Normal, 90);
            CreateSpacer(screen, 40);
            RectTransform buttons = CreateRow(screen, "Buttons", 110, 40, TextAnchor.MiddleCenter);
            Button playAgain = CreateButton(buttons, "PlayAgainButton", "Play Again", ButtonPrimary, 420, 110, out _);
            Button mainMenu = CreateButton(buttons, "MainMenuButton", "Main Menu", ButtonSecondary, 420, 110, out _);

            var view = screen.gameObject.AddComponent<VictoryView>();
            SetRef(view, "gameModeManager", gameModeManager);
            SetRef(view, "battleManager", battleManager);
            SetRef(view, "titleText", title);
            SetRef(view, "scoreText", score);
            SetRef(view, "playAgainButton", playAgain);
            SetRef(view, "mainMenuButton", mainMenu);
            return screen.gameObject;
        }

        // --- UI helpers ---

        private static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateScreen(string name, Transform canvas, float spacing, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            RectTransform screen = CreateUIObject(name, canvas);
            Stretch(screen);
            ConfigureColumn(screen.gameObject.AddComponent<VerticalLayoutGroup>(), spacing, alignment);
            return screen;
        }

        private static void ConfigureColumn(VerticalLayoutGroup layout, float spacing, TextAnchor alignment)
        {
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static RectTransform CreateRow(Transform parent, string name, float height, float spacing, TextAnchor alignment)
        {
            RectTransform row = CreateUIObject(name, parent);
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return row;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            CreateUIObject("Spacer", parent).gameObject.AddComponent<LayoutElement>().preferredHeight = height;
        }

        private static void Flexible(GameObject go)
        {
            go.GetComponent<LayoutElement>().flexibleWidth = 1;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, float size, Color color,
            FontStyles style, float height, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            TMP_Text tmp = CreateFreeText(parent, name, text, size, color, style, alignment);
            tmp.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return tmp;
        }

        private static TMP_Text CreateFreeText(Transform parent, string name, string text, float size, Color color,
            FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject go = TMP_DefaultControls.CreateText(resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color color,
            float width, float height, out TMP_Text labelText)
        {
            GameObject go = TMP_DefaultControls.CreateButton(resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            labelText = go.GetComponentInChildren<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 48;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = Color.white;
            return go.GetComponent<Button>();
        }

        private static TMP_InputField CreateInputField(Transform parent, float width, float height)
        {
            GameObject go = TMP_DefaultControls.CreateInputField(resources);
            go.name = "WordInput";
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = InputBackground;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            var input = go.GetComponent<TMP_InputField>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = TileCount;
            input.pointSize = 80;

            var text = (TextMeshProUGUI)input.textComponent;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 12;
            text.color = TextLight;

            var placeholder = (TextMeshProUGUI)input.placeholder;
            placeholder.text = "YOUR WORD";
            placeholder.alignment = TextAlignmentOptions.Center;
            placeholder.fontSize = 60;
            placeholder.color = new Color(1f, 1f, 1f, 0.25f);
            return input;
        }

        // --- Serialized field wiring (fields are private [SerializeField]) ---

        private static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"BattleSceneBuilder: {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"BattleSceneBuilder: {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
