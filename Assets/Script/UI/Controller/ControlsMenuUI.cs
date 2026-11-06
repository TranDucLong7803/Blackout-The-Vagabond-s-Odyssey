using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

internal sealed class ControlsMenuUI : MonoBehaviour
{
    private static readonly Vector2 PanelSize = new(1280, 800);
    private static readonly Color Background = new Color32(30, 25, 30, 255);
    private static readonly Color RowColor = new Color32(43, 36, 43, 255);
    private static readonly Color Border = new Color32(96, 73, 80, 255);
    private static readonly Color KeyColor = new Color32(70, 53, 63, 255);
    private static readonly Color Accent = new Color32(238, 175, 167, 255);
    private static readonly Color Foreground = new Color32(253, 240, 235, 255);
    private static readonly Color Muted = new Color32(210, 189, 187, 255);

    private const string Instructions = "Select a binding, then press a key or mouse button.";

    private readonly List<(ControlBindings.Entry entry, int slot, TMP_Text label)> labels = new();
    private ControlBindings bindings;
    private GameObject settingsContent;
    private Button entryButton;
    private Button firstBinding;
    private Button backButton;
    private TMP_Text status;
    private TMP_Text resetLabel;
    private RectTransform panel;
    private CanvasGroup panelGroup;
    private InputActionRebindingExtensions.RebindingOperation rebind;
    private int finishedFrame = -1;
    private bool confirmReset;

    public static ControlsMenuUI Create(RectTransform parent, GameObject settingsContent, Button entryButton)
    {
        var root = new GameObject("Controls Screen", typeof(RectTransform));
        root.layer = parent.gameObject.layer;
        root.SetActive(false);
        var rect = (RectTransform)root.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var menu = root.AddComponent<ControlsMenuUI>();
        menu.settingsContent = settingsContent;
        menu.entryButton = entryButton;
        menu.bindings = InputManager.Instance.Bindings;
        menu.Build(rect);
        return menu;
    }

    public void Show()
    {
        settingsContent.SetActive(false);
        ResetPrompt();
        RefreshLabels();
        status.text = Instructions;
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        FitPanel();
        Select(firstBinding != null ? firstBinding : backButton);
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
        settingsContent.SetActive(true);
        if (entryButton.gameObject.activeInHierarchy) Select(entryButton);
    }

    public bool TryHandleCancel()
    {
        if (!gameObject.activeInHierarchy) return false;
        if (finishedFrame == Time.frameCount) return true;

        if (rebind != null) rebind.Cancel();
        else Hide();
        return true;
    }

    private void Build(RectTransform root)
    {
        root.gameObject.AddComponent<Image>().color = new Color32(12, 9, 13, 238);
        panel = Place(root, "Controls panel", 0, 0, PanelSize.x, PanelSize.y);
        panel.anchorMin = panel.anchorMax = panel.pivot = Vector2.one * 0.5f;
        panel.anchoredPosition = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = Background;
        panelGroup = panel.gameObject.AddComponent<CanvasGroup>();

        Fill(panel, "Top rule", Border, 0, 0, 1280, 2);
        Fill(panel, "Header accent", Accent, 48, 40, 30, 3);
        var category = Label(panel, "SETTINGS", 20, Accent, 92, 26, 300, 32);
        category.characterSpacing = 5;
        var title = Label(panel, "Controls", 64, Foreground, 48, 66, 500, 88);
        var entryLabel = entryButton.GetComponentInChildren<TMP_Text>(true);
        if (entryLabel != null) title.font = entryLabel.font;
        Label(panel, "Keyboard & mouse", 27, Muted, 51, 154, 470, 38);
        backButton = MakeButton(panel, "Back", Hide, 1058, 60, 174, 60, out _);

        Label(panel, "MOVEMENT", 21, Accent, 48, 206, 245, 35);
        Label(panel, "Primary", 20, Muted, 337, 206, 124, 35, TextAlignmentOptions.Center);
        Label(panel, "Alternate", 20, Muted, 476, 206, 124, 35, TextAlignmentOptions.Center);
        Label(panel, "INTERFACE", 21, Accent, 48, 500, 245, 34);
        Label(panel, "ACTIONS", 21, Accent, 656, 206, 245, 35);

        int movementRow = 0;
        int interfaceRow = 0;
        int actionRow = 0;
        foreach (var entry in bindings.Entries)
        {
            if (entry.Action.bindings[entry.Indices[0]].isPartOfComposite)
                BuildRow(entry, 48, 246 + 62 * movementRow++);
            else if (entry.Action.actionMap.name == "Menu")
                BuildRow(entry, 48, 538 + 62 * interfaceRow++);
            else
                BuildRow(entry, 656, 246 + 62 * actionRow++);
        }

        Fill(panel, "Footer rule", Border, 48, 680, 1184, 2);
        status = Label(panel, Instructions, 25, Foreground, 48, 695, 870, 58);
        MakeButton(panel, "Reset defaults", ResetDefaults, 967, 695, 265, 58, out resetLabel);
    }

    private void BuildRow(ControlBindings.Entry entry, float x, float y)
    {
        var row = Place(panel, entry.Label, x, y, 576, 54);
        row.gameObject.AddComponent<Image>().color = RowColor;
        Label(row, entry.Label, 27, Foreground, 16, 0, 263, 54);

        bool hasAlternate = entry.Indices.Length > 1;
        for (int i = 0; i < entry.Indices.Length; i++)
        {
            int slot = i;
            var button = MakeButton(row, entry.Display(slot), null, 289 + 139 * slot, 4,
                hasAlternate ? 124 : 263, 46, out var label);
            button.name = $"{entry.Label} {(slot == 0 ? "primary" : "alternate")}";
            button.onClick.AddListener(() => StartRebind(entry, slot, button, label));
            if (firstBinding == null) firstBinding = button;
            labels.Add((entry, slot, label));
        }
    }

    private void StartRebind(ControlBindings.Entry entry, int slot, Button button, TMP_Text label)
    {
        ResetPrompt();
        status.text = $"Press a key or mouse button for {entry.Label.ToLowerInvariant()}. Esc cancels.";
        label.text = "...";
        panelGroup.interactable = false;
        Select(null);

        string result = null;
        // Capture separately so the live actions stay enabled and only validated paths are applied.
        rebind = new InputActionRebindingExtensions.RebindingOperation()
            .WithExpectedControlType<ButtonControl>()
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithControlsHavingToMatchPath("<Mouse>")
            .WithControlsExcluding("<Keyboard>/anyKey")
            .WithCancelingThrough("<Keyboard>/escape")
            .WithMatchingEventsBeingSuppressed()
            .WithTimeout(10)
            .OnApplyBinding((operation, path) => result = bindings.Apply(entry, slot, operation.selectedControl, path))
            .OnComplete(_ => FinishRebind(button, result))
            .OnCancel(_ => FinishRebind(button, "Binding unchanged. Select a binding to try again."));
        rebind.Start();
    }

    private void FinishRebind(Button button, string message)
    {
        rebind.Dispose();
        rebind = null;
        finishedFrame = Time.frameCount;
        panelGroup.interactable = true;
        RefreshLabels();
        status.text = message;
        Select(button);
    }

    private void OnDisable()
    {
        if (rebind == null) return;

        rebind.Dispose();
        rebind = null;
        panelGroup.interactable = true;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (panel != null) FitPanel();
    }

    private void FitPanel()
    {
        var available = ((RectTransform)transform).rect.size;
        float scale = Mathf.Min(available.x / (PanelSize.x + 64), available.y / (PanelSize.y + 64));
        panel.localScale = Vector3.one * scale;
    }

    private void ResetDefaults()
    {
        if (!confirmReset)
        {
            confirmReset = true;
            resetLabel.text = "Confirm reset";
            status.text = "Select Confirm reset to restore all default bindings.";
            return;
        }

        bindings.Reset();
        RefreshLabels();
        ResetPrompt();
        status.text = "Default bindings restored.";
    }

    private void ResetPrompt()
    {
        confirmReset = false;
        resetLabel.text = "Reset defaults";
    }

    private void RefreshLabels()
    {
        foreach (var (entry, slot, label) in labels)
            label.text = entry.Display(slot);
    }

    private static void Select(Button button)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button != null ? button.gameObject : null);
    }

    private static RectTransform Place(RectTransform parent, string name, float x, float y, float width, float height)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static void Fill(RectTransform parent, string name, Color color, float x, float y, float w, float h)
    {
        var image = Place(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static TMP_Text Label(RectTransform parent, string value, float size, Color color,
        float x, float y, float w, float h, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var label = Place(parent, value, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        label.fontSizeMax = size;
        label.fontSizeMin = size * 0.65f;
        label.enableAutoSizing = true;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.alignment = alignment;
        label.color = color;
        label.richText = false;
        label.raycastTarget = false;
        return label;
    }

    private static Button MakeButton(RectTransform parent, string value, UnityEngine.Events.UnityAction click,
        float x, float y, float w, float h, out TMP_Text label)
    {
        var rect = Place(parent, value, x, y, w, h);
        var image = rect.gameObject.AddComponent<Image>();
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = new ColorBlock
        {
            normalColor = KeyColor,
            highlightedColor = new Color32(112, 76, 89, 255),
            selectedColor = new Color32(141, 91, 99, 255),
            pressedColor = new Color32(92, 58, 72, 255),
            disabledColor = RowColor,
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };
        label = Label(rect, value, 26, Foreground, 8, 0, w - 16, h, TextAlignmentOptions.Center);
        if (click != null) button.onClick.AddListener(click);
        return button;
    }
}
