using System.Collections.Generic;
using Labyrinthino.Core;
using Labyrinthino.Data;
using Labyrinthino.MiniGames;
using Labyrinthino.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.Editor
{
    /// <summary>
    /// Scene builder part that creates every window of the game.
    /// </summary>
    public static partial class SceneBuilder
    {
        private static readonly string[] GameIconNames =
        {
            "icon_game_quiz", "icon_game_findpath", "icon_game_meadow", "icon_game_dots"
        };

        private sealed class MiniGameRefs
        {
            public LoadingScreen Loading;
            public MiniGamesScreen Map;
            public DetailsScreen Details;
            public SettingsScreen Settings;
            public ResultScreen Victory;
            public ResultScreen Defeat;
            public MiniGameBase[] Games;
        }

        private static ScreenEntry[] BuildAllScreens(RectTransform canvas, GameRoot root,
            Audio.AudioController audio, out MiniGameRefs refs)
        {
            refs = new MiniGameRefs();
            List<ScreenEntry> entries = new List<ScreenEntry>();
            refs.Loading = BuildLoading(canvas, entries);
            refs.Map = BuildMap(canvas, entries);
            refs.Details = BuildDetails(canvas, entries);
            refs.Settings = BuildSettings(canvas, entries);
            MiniGameBase quiz = BuildQuiz(canvas, root, entries);
            MiniGameBase findPath = BuildFindPath(canvas, root, entries);
            MiniGameBase meadow = BuildMeadow(canvas, root, entries);
            MiniGameBase dots = BuildConnectDots(canvas, root, entries);
            refs.Games = new[] { quiz, findPath, meadow, dots };
            refs.Victory = BuildResult(canvas, entries, true);
            refs.Defeat = BuildResult(canvas, entries, false);
            return entries.ToArray();
        }

        private static void Register(List<ScreenEntry> entries, ScreenId id, CanvasGroup group)
        {
            entries.Add(new ScreenEntry { Id = id, Group = group });
        }

        private static LoadingScreen BuildLoading(RectTransform canvas, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_Loading", canvas, out RectTransform content);
            Register(entries, ScreenId.Loading, group);

            Image logo = NewImage("Logo", content, LoadSprite("Branding/logo_labyrinthino"), Color.white);
            Place(logo.rectTransform, 0f, 330f, 900f, 450f);
            Image spinner = NewImage("Spinner", content, Icon("icon_restart"), Palette.Teal);
            Place(spinner.rectTransform, 0f, -90f, 130f, 130f);
            Text stage = NewText("StageText", content, "Загрузка...", 52, Palette.Cream);
            Place(stage.rectTransform, 0f, -260f, 900f, 80f);

            RectTransform bar = Place(NewRect("ProgressBar", content), 0f, -380f, 800f, 56f);
            Image barBackground = bar.gameObject.AddComponent<Image>();
            barBackground.sprite = Ui("ui_bar");
            barBackground.type = Image.Type.Sliced;
            barBackground.color = new Color(0f, 0f, 0f, 0.4f);
            Image fill = NewImage("Fill", bar, Ui("ui_bar"), Palette.Gold, true);
            Stretch(fill.rectTransform);
            fill.rectTransform.offsetMin = new Vector2(5f, 5f);
            fill.rectTransform.offsetMax = new Vector2(-5f, -5f);
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            ProgressBar progress = bar.gameObject.AddComponent<ProgressBar>();
            Wire(progress, "_fill", fill.rectTransform);

            Text percent = NewText("PercentText", content, "0%", 44, Palette.Gold);
            Place(percent.rectTransform, 0f, -460f, 300f, 70f);

            LoadingScreen screen = group.gameObject.AddComponent<LoadingScreen>();
            Wire(screen, "_logo", logo.rectTransform);
            Wire(screen, "_spinner", spinner.rectTransform);
            Wire(screen, "_stageText", stage);
            Wire(screen, "_percentText", percent);
            Wire(screen, "_progressBar", progress);
            return screen;
        }

        private static MiniGamesScreen BuildMap(RectTransform canvas, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_MiniGames", canvas, out RectTransform content);
            Register(entries, ScreenId.MiniGames, group);

            Image logo = NewImage("Logo", content, LoadSprite("Branding/logo_labyrinthino"), Color.white);
            Place(logo.rectTransform, 0f, 720f, 760f, 380f);
            Button settings = NewIconButton("SettingsButton", content, "icon_gear", Palette.Gold, 120f);
            Place((RectTransform)settings.transform, 440f, 840f, 120f, 120f);
            Text title = NewText("Title", content, "Выбери мини-игру", 64, Palette.Cream);
            Place(title.rectTransform, 0f, 500f, 900f, 90f);
            AddShadow(title);
            Text summary = NewText("SummaryText", content, "Пройдено игр: 0 из 4", 40, Palette.Gold, false);
            Place(summary.rectTransform, 0f, 425f, 900f, 60f);

            RectTransform cardsRoot = Place(NewRect("Cards", content), 0f, -250f, 1000f, 1200f);
            GridLayoutGroup grid = cardsRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(480f, 570f);
            grid.spacing = new Vector2(40f, 50f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.MiddleCenter;

            MiniGameCard[] cards = new MiniGameCard[4];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = BuildCard(cardsRoot, i);
            }

            MiniGamesScreen screen = group.gameObject.AddComponent<MiniGamesScreen>();
            Wire(screen, "_cards", cards);
            Wire(screen, "_settingsButton", settings);
            Wire(screen, "_summaryText", summary);
            return screen;
        }

        private static MiniGameCard BuildCard(RectTransform parent, int index)
        {
            RectTransform slot = NewRect("Card_" + MiniGameCatalog.All[index].Id, parent);
            RectTransform visual = Stretch(NewRect("Visual", slot));
            Image glow = NewImage("Glow", visual, Ui("ui_card"), Palette.Gold, true);
            Stretch(glow.rectTransform);
            glow.rectTransform.offsetMin = new Vector2(-14f, -14f);
            glow.rectTransform.offsetMax = new Vector2(14f, 14f);
            glow.enabled = false;

            Image background = visual.gameObject.AddComponent<Image>();
            background.sprite = Ui("ui_card");
            background.type = Image.Type.Sliced;
            Button button = visual.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            visual.gameObject.AddComponent<PressScale>();
            visual.gameObject.AddComponent<Audio.ButtonSound>();
            FloatAnimator bob = visual.gameObject.AddComponent<FloatAnimator>();
            Wire(bob, "_amplitude", 9f);
            Wire(bob, "_speed", 1.8f);
            Wire(bob, "_scaleAmount", 0f);
            Wire(bob, "_phase", index * 1.3f);

            Image icon = NewImage("Icon", visual, Icon(GameIconNames[index]), Color.white);
            Place(icon.rectTransform, 0f, 120f, 270f, 270f);
            Text title = NewText("Title", visual, "Игра", 56, Palette.DarkText);
            Place(title.rectTransform, 0f, -70f, 440f, 80f);
            Text status = NewText("Status", visual, "Новая игра", 34, Palette.DarkText, false);
            Place(status.rectTransform, 0f, -140f, 440f, 56f);

            RectTransform bar = Place(NewRect("Progress", visual), 0f, -215f, 380f, 36f);
            Image barBackground = bar.gameObject.AddComponent<Image>();
            barBackground.sprite = Ui("ui_bar");
            barBackground.type = Image.Type.Sliced;
            barBackground.color = new Color(0f, 0f, 0f, 0.3f);
            Image fill = NewImage("Fill", bar, Ui("ui_bar"), Color.white, true);
            Stretch(fill.rectTransform);
            fill.rectTransform.offsetMin = new Vector2(4f, 4f);
            fill.rectTransform.offsetMax = new Vector2(-4f, -4f);
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            ProgressBar progress = bar.gameObject.AddComponent<ProgressBar>();
            Wire(progress, "_fill", fill.rectTransform);

            Image badge = NewImage("Badge", visual, Ui("ui_circle"), Palette.Gold);
            Place(badge.rectTransform, 190f, 240f, 100f, 100f);
            Image check = NewImage("Check", badge.rectTransform, Icon("icon_check"), Palette.DarkText);
            Place(check.rectTransform, 0f, 0f, 60f, 60f);

            MiniGameCard card = slot.gameObject.AddComponent<MiniGameCard>();
            Wire(card, "_button", button);
            Wire(card, "_visual", visual);
            Wire(card, "_background", background);
            Wire(card, "_glow", glow);
            Wire(card, "_badge", badge);
            Wire(card, "_title", title);
            Wire(card, "_status", status);
            Wire(card, "_progress", progress);
            Wire(card, "_icon", icon);
            return card;
        }

        private static DetailsScreen BuildDetails(RectTransform canvas, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_MiniGame", canvas, out RectTransform content);
            Register(entries, ScreenId.MiniGameDetails, group);

            Button back = NewIconButton("BackButton", content, "icon_back", Palette.Gold, 120f);
            Place((RectTransform)back.transform, -440f, 840f, 120f, 120f);
            Text title = NewText("Title", content, "Мини-игра", 96, Palette.Gold);
            Place(title.rectTransform, 40f, 830f, 700f, 130f);
            AddShadow(title);

            Image icon = NewImage("Icon", content, Icon(GameIconNames[0]), Color.white);
            Place(icon.rectTransform, 0f, 520f, 340f, 340f);
            FloatAnimator bob = icon.gameObject.AddComponent<FloatAnimator>();
            Wire(bob, "_amplitude", 12f);
            Wire(bob, "_scaleAmount", 0.03f);

            Image panel = NewPanel("DescriptionPanel", content, 0f, 130f, 940f, 360f, Palette.Panel);
            Text description = NewText("Description", panel.transform, "Описание", 44, Palette.Cream, false);
            Place(description.rectTransform, 0f, 0f, 840f, 300f);

            Text status = NewText("Status", content, "Уровней: 3", 42, Palette.Gold, false);
            Place(status.rectTransform, 0f, -110f, 900f, 70f);
            Text levelsLabel = NewText("LevelsLabel", content, "Уровни", 54, Palette.Cream);
            Place(levelsLabel.rectTransform, 0f, -215f, 500f, 80f);

            RectTransform levels = Place(NewRect("Levels", content), 0f, -330f, 980f, 130f);
            HorizontalLayoutGroup row = levels.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 26f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            Image chip = NewImage("ChipTemplate", content, Ui("ui_circle"), Palette.Locked);
            Place(chip.rectTransform, 0f, 0f, 120f, 120f);
            Text chipLabel = NewText("Label", chip.transform, "1", 60, Palette.DarkText);
            Stretch(chipLabel.rectTransform);
            Image mark = NewImage("Mark", chip.transform, Icon("icon_check"), Color.white);
            Place(mark.rectTransform, 0f, 0f, 70f, 70f);
            chip.gameObject.SetActive(false);

            Button play = NewButton("PlayButton", content, "Играть", Palette.Gold, 700f, 170f, 78);
            Place((RectTransform)play.transform, 0f, -610f, 700f, 170f);
            FloatAnimator pulse = play.gameObject.AddComponent<FloatAnimator>();
            Wire(pulse, "_amplitude", 0f);
            Wire(pulse, "_scaleAmount", 0.025f);
            Wire(pulse, "_speed", 3f);

            DetailsScreen screen = group.gameObject.AddComponent<DetailsScreen>();
            Wire(screen, "_title", title);
            Wire(screen, "_description", description);
            Wire(screen, "_statusText", status);
            Wire(screen, "_icon", icon);
            Sprite[] icons = new Sprite[GameIconNames.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = Icon(GameIconNames[i]);
            }

            Wire(screen, "_icons", icons);
            Wire(screen, "_levelsRoot", levels);
            Wire(screen, "_levelChipTemplate", chip.gameObject);
            Wire(screen, "_checkSprite", Icon("icon_check"));
            Wire(screen, "_playButton", play);
            Wire(screen, "_playLabel", play.GetComponentInChildren<Text>());
            Wire(screen, "_playBackground", play.GetComponent<Image>());
            Wire(screen, "_backButton", back);
            return screen;
        }

        private static SettingsScreen BuildSettings(RectTransform canvas, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_Settings", canvas, out RectTransform content);
            Register(entries, ScreenId.Settings, group);

            Button back = NewIconButton("BackButton", content, "icon_back", Palette.Gold, 120f);
            Place((RectTransform)back.transform, -440f, 840f, 120f, 120f);
            Text title = NewText("Title", content, "Настройки", 96, Palette.Gold);
            Place(title.rectTransform, 40f, 830f, 700f, 130f);
            AddShadow(title);

            Image panel = NewPanel("Panel", content, 0f, 280f, 960f, 820f, Palette.Panel);
            Slider music = BuildVolumeRow(panel.transform, "Music", "Музыка", "icon_music", 190f, out Text musicValue);
            Slider sound = BuildVolumeRow(panel.transform, "Sound", "Звуки", "icon_sound", -150f, out Text soundValue);

            Button reset = NewButton("ResetButton", content, "Сбросить прогресс", Palette.Coral, 720f, 140f, 56);
            Place((RectTransform)reset.transform, 0f, -400f, 720f, 140f);
            Text hint = NewText("Hint", content, "Громкость сохраняется автоматически", 36, Palette.Cream, false);
            Place(hint.rectTransform, 0f, -540f, 900f, 60f);

            SettingsScreen screen = group.gameObject.AddComponent<SettingsScreen>();
            Wire(screen, "_musicSlider", music);
            Wire(screen, "_soundSlider", sound);
            Wire(screen, "_musicValue", musicValue);
            Wire(screen, "_soundValue", soundValue);
            Wire(screen, "_backButton", back);
            Wire(screen, "_resetButton", reset);
            Wire(screen, "_resetLabel", reset.GetComponentInChildren<Text>());
            return screen;
        }

        private static Slider BuildVolumeRow(Transform parent, string id, string label, string iconName, float y,
            out Text valueText)
        {
            Image icon = NewImage(id + "Icon", parent, Icon(iconName), Palette.Gold);
            Place(icon.rectTransform, -370f, y + 90f, 90f, 90f);
            Text caption = NewText(id + "Label", parent, label, 62, Palette.Cream, true, TextAnchor.MiddleLeft);
            Place(caption.rectTransform, -90f, y + 90f, 480f, 90f);
            valueText = NewText(id + "Value", parent, "70%", 54, Palette.Gold, false, TextAnchor.MiddleRight);
            Place(valueText.rectTransform, 340f, y + 90f, 200f, 80f);

            RectTransform sliderRect = Place(NewRect(id + "Slider", parent), 0f, y - 40f, 780f, 80f);
            Image background = NewImage("Background", sliderRect, Ui("ui_bar"), new Color(0f, 0f, 0f, 0.4f), true);
            Place(background.rectTransform, 0f, 0f, 780f, 36f);
            RectTransform fillArea = Place(NewRect("FillArea", sliderRect), 0f, 0f, 780f, 36f);
            Image fill = NewImage("Fill", fillArea, Ui("ui_bar"), Palette.Teal, true);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;
            RectTransform handleArea = Stretch(NewRect("HandleArea", sliderRect));
            handleArea.offsetMin = new Vector2(40f, 0f);
            handleArea.offsetMax = new Vector2(-40f, 0f);
            Image handle = NewImage("Handle", handleArea, Ui("ui_circle"), Palette.Gold);
            handle.raycastTarget = true;
            handle.rectTransform.sizeDelta = new Vector2(84f, 84f);

            Slider slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.7f;
            return slider;
        }

        private static MiniGameBase BuildQuiz(RectTransform canvas, GameRoot root, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_Quiz", canvas, out RectTransform content);
            Register(entries, ScreenId.Quiz, group);
            GameHeader header = NewHeader(content, "Викторина");
            UiConfetti confetti = NewConfetti((RectTransform)group.transform);

            Image[] hearts = new Image[3];
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i] = NewImage("Heart" + i, content, Icon("icon_heart"), Palette.Coral);
                Place(hearts[i].rectTransform, (i - 1) * 90f, 665f, 76f, 76f);
            }

            NewPanel("PictureFrame", content, 0f, 245f, 780f, 780f, Palette.Gold);
            Image picture = NewImage("Picture", content, null, Color.white);
            Place(picture.rectTransform, 0f, 245f, 720f, 720f);
            RectTransform maskRoot = Place(NewRect("Mask", content), 0f, 245f, 720f, 720f);
            GridLayoutGroup maskGrid = maskRoot.gameObject.AddComponent<GridLayoutGroup>();
            maskGrid.cellSize = new Vector2(180f, 180f);
            maskGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            maskGrid.constraintCount = 4;
            CanvasGroup[] tiles = new CanvasGroup[16];
            for (int i = 0; i < tiles.Length; i++)
            {
                Image tile = NewImage("Tile" + i, maskRoot, Ui("ui_square"), new Color32(78, 50, 150, 255));
                Text mark = NewText("Mark", tile.transform, "?", 110, new Color(1f, 1f, 1f, 0.35f));
                Stretch(mark.rectTransform);
                tiles[i] = tile.gameObject.AddComponent<CanvasGroup>();
                tiles[i].blocksRaycasts = false;
            }

            Text message = NewText("Message", content, "Что это?", 48, Palette.Cream);
            Place(message.rectTransform, 0f, -205f, 940f, 100f);

            RectTransform fieldRect = Place(NewRect("AnswerField", content), 0f, -330f, 780f, 130f);
            Image fieldBackground = fieldRect.gameObject.AddComponent<Image>();
            fieldBackground.sprite = Ui("ui_panel");
            fieldBackground.type = Image.Type.Sliced;
            fieldBackground.color = Palette.Cream;
            Text fieldText = NewText("Text", fieldRect, string.Empty, 84, Palette.DarkText);
            Stretch(fieldText.rectTransform);
            Text placeholder = NewText("Placeholder", fieldRect, "Введи слово", 54, new Color(0.3f, 0.2f, 0.5f, 0.4f));
            Stretch(placeholder.rectTransform);
            InputField field = fieldRect.gameObject.AddComponent<InputField>();
            field.targetGraphic = fieldBackground;
            field.textComponent = fieldText;
            field.placeholder = placeholder;
            field.readOnly = true;
            field.characterLimit = 10;

            RectTransform letters = Place(NewRect("Letters", content), 0f, -560f, 900f, 320f);
            GridLayoutGroup letterGrid = letters.gameObject.AddComponent<GridLayoutGroup>();
            letterGrid.cellSize = new Vector2(150f, 130f);
            letterGrid.spacing = new Vector2(18f, 18f);
            letterGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            letterGrid.constraintCount = 5;
            letterGrid.childAlignment = TextAnchor.UpperCenter;
            Button letterTemplate = NewButton("LetterTemplate", content, "А", Palette.Gold, 150f, 130f, 78);
            letterTemplate.gameObject.SetActive(false);
            Destroy(letterTemplate.GetComponent<Audio.ButtonSound>());

            Button erase = NewButton("EraseButton", content, "Стереть", Palette.Coral, 430f, 130f, 58);
            Place((RectTransform)erase.transform, -235f, -815f, 430f, 130f);
            Button submit = NewButton("SubmitButton", content, "Отправить", Palette.Teal, 430f, 130f, 58);
            Place((RectTransform)submit.transform, 235f, -815f, 430f, 130f);

            QuizController controller = group.gameObject.AddComponent<QuizController>();
            WireBase(controller, root, header, confetti);
            Wire(controller, "_picture", picture);
            Sprite[] pictures =
            {
                LoadSprite("Sprites/Quiz/quiz_house"), LoadSprite("Sprites/Quiz/quiz_fish"),
                LoadSprite("Sprites/Quiz/quiz_mushroom"), LoadSprite("Sprites/Quiz/quiz_boat"),
                LoadSprite("Sprites/Quiz/quiz_sun")
            };
            Wire(controller, "_pictures", pictures);
            Wire(controller, "_maskTiles", tiles);
            Wire(controller, "_messageText", message);
            Wire(controller, "_hearts", hearts);
            Wire(controller, "_answerField", field);
            Wire(controller, "_lettersRoot", letters);
            Wire(controller, "_letterTemplate", letterTemplate);
            Wire(controller, "_eraseButton", erase);
            Wire(controller, "_submitButton", submit);
            return controller;
        }

        private static MiniGameBase BuildFindPath(RectTransform canvas, GameRoot root, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_FindPath", canvas, out RectTransform content);
            Register(entries, ScreenId.FindPath, group);
            GameHeader header = NewHeader(content, "Найди путь");
            UiConfetti confetti = NewConfetti((RectTransform)group.transform);

            Text status = NewText("StatusText", content, "Запомни путь!", 54, Palette.Cream);
            Place(status.rectTransform, 0f, 655f, 940f, 90f);
            BoardView board = NewBoard(content, 90f, out RectTransform cells);
            Button reset = NewButton("ResetButton", content, "Сбросить", Palette.Coral, 560f, 140f, 64);
            Place((RectTransform)reset.transform, 0f, -570f, 560f, 140f);

            FindPathController controller = group.gameObject.AddComponent<FindPathController>();
            WireBase(controller, root, header, confetti);
            Wire(controller, "_board", board);
            Wire(controller, "_statusText", status);
            Wire(controller, "_resetButton", reset);
            Wire(controller, "_startIcon", Icon("icon_play"));
            Wire(controller, "_finishIcon", Icon("icon_star"));
            Wire(controller, "_obstacleIcon", Icon("icon_close"));
            Wire(controller, "_markerIcon", Ui("ui_circle"));
            return controller;
        }

        private static MiniGameBase BuildMeadow(RectTransform canvas, GameRoot root, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_Meadow", canvas, out RectTransform content);
            Register(entries, ScreenId.Meadow, group);
            GameHeader header = NewHeader(content, "Лугомания");
            UiConfetti confetti = NewConfetti((RectTransform)group.transform);

            Text moves = NewText("MovesText", content, "Ходов: 9", 54, Palette.Gold);
            Place(moves.rectTransform, 0f, 655f, 940f, 90f);
            BoardView board = NewBoard(content, 90f, out RectTransform cells);
            Text hint = NewText("HintText", content, "Нажимай на коричневые клетки", 48, Palette.Cream);
            Place(hint.rectTransform, 0f, -520f, 940f, 90f);

            MeadowController controller = group.gameObject.AddComponent<MeadowController>();
            WireBase(controller, root, header, confetti);
            Wire(controller, "_board", board);
            Wire(controller, "_movesText", moves);
            Wire(controller, "_hintText", hint);
            Wire(controller, "_grassIcon", Icon("icon_check"));
            return controller;
        }

        private static MiniGameBase BuildConnectDots(RectTransform canvas, GameRoot root, List<ScreenEntry> entries)
        {
            CanvasGroup group = NewScreen("Screen_ConnectDots", canvas, out RectTransform content);
            Register(entries, ScreenId.ConnectDots, group);
            GameHeader header = NewHeader(content, "Соединяй точки");
            UiConfetti confetti = NewConfetti((RectTransform)group.transform);

            Text hint = NewText("HintText", content, "Начни с подсвеченной точки", 50, Palette.Cream);
            Place(hint.rectTransform, 0f, 655f, 940f, 90f);
            BoardView board = NewBoard(content, 90f, out RectTransform cells);
            RectTransform lineLayer = Place(NewRect("LineLayer", board.transform), 0f, 0f, 900f, 900f);
            Button reset = NewButton("ResetButton", content, "Сбросить маршрут", Palette.Coral, 640f, 140f, 56);
            Place((RectTransform)reset.transform, 0f, -570f, 640f, 140f);

            ConnectDotsController controller = group.gameObject.AddComponent<ConnectDotsController>();
            WireBase(controller, root, header, confetti);
            Wire(controller, "_board", board);
            Wire(controller, "_lineLayer", lineLayer);
            Wire(controller, "_squareSprite", Ui("ui_square"));
            Wire(controller, "_circleSprite", Ui("ui_circle"));
            Wire(controller, "_hintText", hint);
            Wire(controller, "_resetButton", reset);
            Wire(controller, "_startIcon", Icon("icon_play"));
            Wire(controller, "_mandatoryIcon", Icon("icon_star"));
            Wire(controller, "_destinationIcon", Icon("icon_home"));
            return controller;
        }

        private static void WireBase(MiniGameBase game, GameRoot root, GameHeader header, UiConfetti confetti)
        {
            Wire(game, "_root", root);
            Wire(game, "_header", header);
            Wire(game, "_confetti", confetti);
        }

        private static ResultScreen BuildResult(RectTransform canvas, List<ScreenEntry> entries, bool victory)
        {
            CanvasGroup group = NewScreen(victory ? "Screen_Victory" : "Screen_Defeat", canvas, out RectTransform content);
            Register(entries, victory ? ScreenId.Victory : ScreenId.Defeat, group);

            Image dim = NewImage("Dim", group.transform, Ui("ui_square"), new Color(0.05f, 0.02f, 0.15f, 0.55f));
            dim.transform.SetAsFirstSibling();
            Stretch(dim.rectTransform);
            dim.preserveAspect = false;
            dim.raycastTarget = true;

            Color accent = victory ? Palette.Gold : Palette.Coral;
            RectTransform window = Place(NewRect("Window", content), 0f, 0f, 900f, 1280f);
            Image border = NewImage("Border", window, Ui("ui_card"), accent, true);
            Stretch(border.rectTransform);
            border.rectTransform.offsetMin = new Vector2(-10f, -10f);
            border.rectTransform.offsetMax = new Vector2(10f, 10f);
            Image panelImage = NewImage("Panel", window, Ui("ui_panel"), new Color32(46, 28, 92, 255), true);
            Stretch(panelImage.rectTransform);

            Image rays = NewImage("Rays", window, LoadSprite("Sprites/UI/art_rays"), Color.white);
            Place(rays.rectTransform, 0f, 360f, 780f, 780f);
            rays.enabled = victory;
            Image emblem = NewImage("Emblem", window,
                LoadSprite(victory ? "Sprites/UI/art_victory_medal" : "Sprites/UI/art_defeat_cloud"), Color.white);
            Place(emblem.rectTransform, 0f, 360f, 440f, 440f);

            Text title = NewText("Title", window, victory ? "Победа!" : "Поражение", 140, accent);
            Place(title.rectTransform, 0f, 70f, 820f, 170f);
            AddShadow(title);
            Text subtitle = NewText("Subtitle", window, string.Empty, 46, Palette.Cream, false);
            Place(subtitle.rectTransform, 0f, -85f, 780f, 140f);

            RectTransform[] stars = new RectTransform[3];
            for (int i = 0; i < stars.Length; i++)
            {
                Color starColor = victory ? Palette.Gold : new Color(1f, 1f, 1f, 0.2f);
                Image star = NewImage("Star" + i, window, Icon("icon_star"), starColor);
                float size = i == 1 ? 150f : 120f;
                Place(star.rectTransform, (i - 1) * 160f, -230f + (i == 1 ? 15f : 0f), size, size);
                stars[i] = star.rectTransform;
            }

            RectTransform buttons = Place(NewRect("Buttons", window), 0f, -460f, 800f, 420f);
            CanvasGroup buttonsGroup = buttons.gameObject.AddComponent<CanvasGroup>();
            Button primary = NewButton("PrimaryButton", buttons, victory ? "Продолжить" : "Попробовать ещё раз",
                victory ? Palette.Gold : Palette.Teal, 740f, 160f, 62);
            Place((RectTransform)primary.transform, 0f, 80f, 740f, 160f);
            Button secondary = NewButton("SecondaryButton", buttons, "На главную", Palette.Cream, 740f, 140f, 56);
            Place((RectTransform)secondary.transform, 0f, -100f, 740f, 140f);

            UiConfetti confetti = NewConfetti((RectTransform)group.transform);
            ResultScreen screen = group.gameObject.AddComponent<ResultScreen>();
            Wire(screen, "_isVictory", victory);
            Wire(screen, "_panel", window);
            Wire(screen, "_emblem", emblem.rectTransform);
            Wire(screen, "_rays", rays.rectTransform);
            Wire(screen, "_title", title);
            Wire(screen, "_subtitle", subtitle);
            Wire(screen, "_stars", stars);
            Wire(screen, "_primaryButton", primary);
            Wire(screen, "_primaryLabel", primary.GetComponentInChildren<Text>());
            Wire(screen, "_secondaryButton", secondary);
            Wire(screen, "_secondaryLabel", secondary.GetComponentInChildren<Text>());
            Wire(screen, "_confetti", confetti);
            Wire(screen, "_buttonsGroup", buttonsGroup);
            return screen;
        }

        private static void Destroy(Object target)
        {
            Object.DestroyImmediate(target);
        }
    }
}
