using Labyrinthino.Audio;
using Labyrinthino.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.Editor
{
    /// <summary>
    /// Small factory helpers that create uGUI objects for the scene builder.
    /// </summary>
    public static partial class SceneBuilder
    {
        private static RectTransform NewRect(string name, Transform parent)
        {
            GameObject instance = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            instance.transform.SetParent(parent, false);
            return (RectTransform)instance.transform;
        }

        private static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool sliced = false)
        {
            RectTransform rect = NewRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = !sliced;
            image.raycastTarget = false;
            return image;
        }

        private static Text NewText(string name, Transform parent, string content, int size, Color color,
            bool useTitleFont = true, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            RectTransform rect = NewRect(name, parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = useTitleFont ? _titleFont : _bodyFont;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(14, size / 2);
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static Button NewButton(string name, Transform parent, string label, Color color, float width,
            float height, int fontSize)
        {
            RectTransform rect = NewRect(name, parent);
            Place(rect, 0f, 0f, width, height);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = Ui("ui_button");
            image.type = Image.Type.Sliced;
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            rect.gameObject.AddComponent<PressScale>();
            rect.gameObject.AddComponent<ButtonSound>();
            Text text = NewText("Label", rect, label, fontSize, Palette.DarkText);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMax = new Vector2(-20f, -8f);
            text.rectTransform.offsetMin = new Vector2(20f, 12f);
            return button;
        }

        private static Button NewIconButton(string name, Transform parent, string iconName, Color color, float size)
        {
            RectTransform rect = NewRect(name, parent);
            Place(rect, 0f, 0f, size, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = Ui("ui_circle");
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            rect.gameObject.AddComponent<PressScale>();
            rect.gameObject.AddComponent<ButtonSound>();
            Image icon = NewImage("Icon", rect, Icon(iconName), Palette.DarkText);
            Place(icon.rectTransform, 0f, 0f, size * 0.55f, size * 0.55f);
            return button;
        }

        private static Image NewPanel(string name, Transform parent, float x, float y, float width, float height,
            Color color)
        {
            Image image = NewImage(name, parent, Ui("ui_panel"), color, true);
            Place(image.rectTransform, x, y, width, height);
            return image;
        }

        private static CanvasGroup NewScreen(string name, Transform canvas, out RectTransform content)
        {
            RectTransform rect = Stretch(NewRect(name, canvas));
            CanvasGroup group = rect.gameObject.AddComponent<CanvasGroup>();
            content = Place(NewRect("Content", rect), 0f, 0f, ContentWidth, ContentHeight);
            return group;
        }

        private static void AddShadow(Text text)
        {
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(3f, -4f);
        }

        private static GameHeader NewHeader(RectTransform content, string title)
        {
            RectTransform root = Place(NewRect("Header", content), 0f, 820f, ContentWidth, 240f);
            Button back = NewIconButton("BackButton", root, "icon_back", Palette.Gold, 120f);
            Place((RectTransform)back.transform, -440f, 0f, 120f, 120f);

            Text titleText = NewText("Title", root, title, 70, Palette.Gold);
            Place(titleText.rectTransform, 0f, 28f, 560f, 100f);
            AddShadow(titleText);
            Text levelText = NewText("LevelText", root, "Уровень 1 из 3", 38, Palette.Cream, false);
            Place(levelText.rectTransform, 0f, -45f, 560f, 60f);

            RectTransform timerRoot = Place(NewRect("Timer", root), 385f, 0f, 250f, 100f);
            Image timerBackground = NewImage("Background", timerRoot, Ui("ui_panel"), Palette.Panel, true);
            Stretch(timerBackground.rectTransform);
            Image timerIcon = NewImage("Icon", timerRoot, Icon("icon_timer"), Palette.Gold);
            Place(timerIcon.rectTransform, -78f, 0f, 60f, 60f);
            Text timerText = NewText("TimerText", timerRoot, "00:60", 58, Palette.Cream);
            Place(timerText.rectTransform, 32f, 0f, 150f, 80f);

            GameHeader header = root.gameObject.AddComponent<GameHeader>();
            Wire(header, "_backButton", back);
            Wire(header, "_titleText", titleText);
            Wire(header, "_levelText", levelText);
            Wire(header, "_timerRoot", timerRoot.gameObject);
            Wire(header, "_timerText", timerText);
            return header;
        }

        private static BoardView NewBoard(RectTransform content, float centerY, out RectTransform cellContainer)
        {
            RectTransform root = Place(NewRect("Board", content), 0f, centerY, 960f, 960f);
            Image frame = root.gameObject.AddComponent<Image>();
            frame.sprite = Ui("ui_panel");
            frame.type = Image.Type.Sliced;
            frame.color = Palette.Panel;
            frame.raycastTarget = false;

            cellContainer = Place(NewRect("Cells", root), 0f, 0f, 900f, 900f);
            GridLayoutGroup grid = cellContainer.gameObject.AddComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;

            RectTransform template = Place(NewRect("CellTemplate", root), 0f, 0f, 100f, 100f);
            Image cellImage = template.gameObject.AddComponent<Image>();
            cellImage.sprite = Ui("ui_cell");
            cellImage.type = Image.Type.Sliced;
            Button cellButton = template.gameObject.AddComponent<Button>();
            cellButton.targetGraphic = cellImage;
            Image cellIcon = NewImage("Icon", template, null, Color.white);
            Place(cellIcon.rectTransform, 0f, 0f, 60f, 60f);
            cellIcon.rectTransform.anchorMin = new Vector2(0.15f, 0.15f);
            cellIcon.rectTransform.anchorMax = new Vector2(0.85f, 0.85f);
            cellIcon.rectTransform.offsetMin = Vector2.zero;
            cellIcon.rectTransform.offsetMax = Vector2.zero;
            BoardCell cell = template.gameObject.AddComponent<BoardCell>();
            Wire(cell, "_background", cellImage);
            Wire(cell, "_icon", cellIcon);
            Wire(cell, "_button", cellButton);
            template.gameObject.SetActive(false);

            BoardView board = root.gameObject.AddComponent<BoardView>();
            Wire(board, "_cellContainer", cellContainer);
            Wire(board, "_cellTemplate", cell);
            Wire(board, "_boardSize", 900f);
            return board;
        }

        private static UiConfetti NewConfetti(RectTransform screenRoot)
        {
            RectTransform area = Stretch(NewRect("ConfettiLayer", screenRoot));
            UiConfetti confetti = area.gameObject.AddComponent<UiConfetti>();
            Wire(confetti, "_area", area);
            Wire(confetti, "_pieceSprite", Ui("ui_square"));
            return confetti;
        }
    }
}
