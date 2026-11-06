using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class PlayerControlTests
{
    private Keyboard keyboard;
    private Mouse mouse;
    private InputSettings.UpdateMode previousUpdateMode;
    private InputSettings.BackgroundBehavior previousBackgroundBehavior;
    private InputSettings.EditorInputBehaviorInPlayMode previousEditorInputBehavior;
    private bool previousRunInBackground;
    private float previousTimeScale;
    private string previousBindingOverrides;

    [SetUp]
    public void SetUp()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        previousRunInBackground = Application.runInBackground;
        Application.runInBackground = true;
        previousUpdateMode = InputSystem.settings.updateMode;
        previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
        previousEditorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();
        var actions = GetActions();
        previousBindingOverrides = actions.SaveBindingOverridesAsJson();
        actions.RemoveAllBindingOverrides();
        SetField(GetInputManager(), "lastInteractionFrame", -1);
    }

    [TearDown]
    public void TearDown()
    {
        InputSystem.RemoveDevice(mouse);
        InputSystem.RemoveDevice(keyboard);
        InputSystem.settings.updateMode = previousUpdateMode;
        InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
        InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInputBehavior;
        Application.runInBackground = previousRunInBackground;
        Time.timeScale = previousTimeScale;
        GetActions().LoadBindingOverridesFromJson(previousBindingOverrides);
    }

    [Test]
    public void GameplayBindingsDistinguishPressHoldAndRelease()
    {
        var actions = GetActions();

        SetInput(new KeyboardState(Key.W, Key.D, Key.Digit1, Key.F, Key.E), new MouseState().WithButton(MouseButton.Left));
        Assert.That(actions["Ingame/Move"].ReadValue<Vector2>().magnitude, Is.EqualTo(1f).Within(0.001f));
        Assert.That(actions["Ingame/Move"].ReadValue<Vector2>().x, Is.GreaterThan(0f));
        Assert.That(actions["Ingame/Move"].ReadValue<Vector2>().y, Is.GreaterThan(0f));
        Assert.That(actions["Ingame/Previous"].WasPressedThisFrame(), Is.True);
        Assert.That(actions["Ingame/Interact"].WasPressedThisFrame(), Is.True);
        Assert.That(actions["Ingame/Skill1"].WasPressedThisFrame(), Is.True);
        Assert.That(actions["Ingame/Attack"].IsPressed(), Is.True);

        SetInput(new KeyboardState(Key.LeftArrow, Key.Digit2, Key.Q), new MouseState().WithButton(MouseButton.Left));
        Assert.That(actions["Ingame/Move"].ReadValue<Vector2>(), Is.EqualTo(Vector2.left));
        Assert.That(actions["Ingame/Next"].WasPressedThisFrame(), Is.True);
        Assert.That(actions["Ingame/Skill2"].WasPressedThisFrame(), Is.True);
        Assert.That(actions["Ingame/Skill1"].WasPressedThisFrame(), Is.False);
        Assert.That(actions["Ingame/Attack"].IsPressed(), Is.True);
        Assert.That(actions["Ingame/Attack"].WasPressedThisFrame(), Is.False);

        SetInput(default, default);
        Assert.That(actions["Ingame/Move"].ReadValue<Vector2>(), Is.EqualTo(Vector2.zero));
        Assert.That(actions["Ingame/Attack"].IsPressed(), Is.False);

        SetInput(new KeyboardState(Key.Enter), default);
        Assert.That(actions["Menu/Submit"].WasPressedThisFrame(), Is.True);
        Assert.That(actions["Ingame/Attack"].IsPressed(), Is.False);
    }

    [Test]
    public void DungeonMovementClearsVelocityWhenGameplayIsPaused()
    {
        var player = new GameObject("Movement test player");
        try
        {
            var body = player.AddComponent<Rigidbody2D>();
            var movement = player.AddComponent(GameType("PlayerMoveController"));
            SetField(movement, "rd", body);
            SetField(movement, "speed", 5f);

            SetInput(new KeyboardState(Key.W, Key.D), default);
            Assert.That(Invoke(movement, "MoveControl"), Is.True);
            Assert.That(body.linearVelocity.magnitude, Is.EqualTo(5f).Within(0.001f));

            Time.timeScale = 0f;
            Assert.That(Invoke(movement, "MoveControl"), Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));

            Time.timeScale = 1f;
            SetInput(default, default);
            Assert.That(Invoke(movement, "MoveControl"), Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void InteractionSurvivesUnrelatedTriggerExitAndRespectsPause()
    {
        var player = new GameObject("Interaction test player");
        var target = new GameObject("Interaction target");
        var unrelated = new GameObject("Unrelated trigger");
        try
        {
            var interactor = player.AddComponent(GameType("Interactor"));
            var outline = new GameObject("Outline");
            outline.transform.SetParent(target.transform);
            outline.SetActive(false);
            target.AddComponent(GameType("Interactable"));
            var targetCollider = target.AddComponent<BoxCollider2D>();
            var unrelatedCollider = unrelated.AddComponent<BoxCollider2D>();

            Invoke(interactor, "OnTriggerStay2D", targetCollider);
            Assert.That(outline.activeSelf, Is.True);
            Invoke(interactor, "OnTriggerExit2D", unrelatedCollider);
            Assert.That(outline.activeSelf, Is.True);

            Time.timeScale = 0f;
            SetInput(new KeyboardState(Key.F), default);
            Invoke(interactor, "Update");
            Assert.That(outline.activeSelf, Is.True);

            Time.timeScale = 1f;
            SetInput(default, default);
            SetInput(new KeyboardState(Key.F), default);
            Invoke(interactor, "Update");
            Assert.That(outline.activeSelf, Is.False);
            Assert.That(Invoke(GetInputManager(), "TryConsumeInteract"), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(unrelated);
        }
    }

    [Test]
    public void SettingsAndShopCloseOnceAndKeepGameplayBlockedWhileOpen()
    {
        var ui = new GameObject("UI test manager").AddComponent(GameType("UIManager"));
        var controller = ui.gameObject.AddComponent(GameType("UIController"));
        var settingsRoot = new GameObject("Settings test root");
        var settings = settingsRoot.AddComponent(GameType("SettingsMenuUI"));
        var panel = new GameObject("Settings panel");
        panel.transform.SetParent(settingsRoot.transform);
        panel.SetActive(false);
        SetField(settings, "settingsPanel", panel);
        var resume = new GameObject("Resume", typeof(RectTransform), typeof(Button));
        resume.transform.SetParent(panel.transform);
        var events = new GameObject("UI test events").AddComponent<EventSystem>();
        var shop = new GameObject("Shop test root").AddComponent(GameType("ShopUI"));
        try
        {
            PressUIKey(controller, Key.Escape);
            Assert.That(panel.activeInHierarchy, Is.True);
            Assert.That(events.currentSelectedGameObject, Is.EqualTo(resume));
            Assert.That(Time.timeScale, Is.Zero);

            SetInput(new KeyboardState(Key.Escape), default);
            Invoke(controller, "Update");
            Assert.That(panel.activeInHierarchy, Is.True);

            PressUIKey(controller, Key.Tab);
            Assert.That(panel.activeInHierarchy, Is.True);
            PressUIKey(controller, Key.Escape);
            Assert.That(panel.activeInHierarchy, Is.False);
            Assert.That(events.currentSelectedGameObject, Is.Null);
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            Invoke(shop, "Show");
            Assert.That(shop.gameObject.activeInHierarchy, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            PressUIKey(controller, Key.Escape);
            Assert.That(shop.gameObject.activeInHierarchy, Is.False);
            Assert.That(panel.activeInHierarchy, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
        finally
        {
            Object.DestroyImmediate(shop.gameObject);
            Object.DestroyImmediate(events.gameObject);
            Object.DestroyImmediate(settingsRoot);
            Object.DestroyImmediate(ui.gameObject);
        }
    }

    [Test]
    public void RebindingFromSettingsAppliesPersistsAndResetsControls()
    {
        const string prefsKey = "KeyboardMouseBindingOverrides";
        var actions = GetActions();
        string originalPreference = PlayerPrefs.HasKey(prefsKey) ? PlayerPrefs.GetString(prefsKey) : null;
        var ui = new GameObject("Controls test manager").AddComponent(GameType("UIManager"));
        var controller = ui.gameObject.AddComponent(GameType("UIController"));
        var canvas = new GameObject("Controls test canvas", typeof(Canvas));
        var panel = new GameObject("Settings panel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);
        var settings = panel.AddComponent(GameType("SettingsMenuUI"));
        var content = new GameObject("Settings content", typeof(RectTransform));
        content.transform.SetParent(panel.transform, false);
        var controlsButton = new GameObject("Controls", typeof(RectTransform), typeof(Button)).GetComponent<Button>();
        controlsButton.transform.SetParent(content.transform, false);
        var events = new GameObject("Controls test events").AddComponent<EventSystem>();
        SetField(settings, "settingsPanel", panel);
        SetField(settings, "settingsContent", content);
        SetField(settings, "controlsButton", controlsButton);
        panel.SetActive(false);

        try
        {
            PressUIKey(controller, Key.Escape);
            SetInput(default, default);
            Invoke(settings, "ShowControls");

            var screen = panel.transform.Find("Controls Screen");
            var move = screen.Find("Controls panel/Move up/Move up primary").GetComponent<Button>();
            var attack = screen.Find("Controls panel/Attack/Attack primary").GetComponent<Button>();
            var reset = screen.Find("Controls panel/Reset defaults").GetComponent<Button>();

            move.onClick.Invoke();
            PressUIKey(controller, Key.Escape);
            Assert.That(screen.gameObject.activeSelf, Is.True);
            Assert.That(Time.timeScale, Is.Zero);

            SetInput(default, default);
            move.onClick.Invoke();
            SetInput(new KeyboardState(Key.F), default);
            Assert.That(actions["Ingame/Move"].bindings[2].overridePath, Is.Null);

            SetInput(default, default);
            move.onClick.Invoke();
            SetInput(new KeyboardState(Key.R), default);
            Assert.That(actions["Ingame/Move"].bindings[2].effectivePath, Is.EqualTo("<Keyboard>/r").IgnoreCase);
            Assert.That(actions["Ingame/Move"].bindings[3].effectivePath, Is.EqualTo("<Keyboard>/upArrow").IgnoreCase);

            SetInput(default, default);
            attack.onClick.Invoke();
            SetInput(default, new MouseState().WithButton(MouseButton.Right));
            Assert.That(actions["Ingame/Attack"].bindings[1].effectivePath, Is.EqualTo("<Mouse>/rightButton").IgnoreCase);

            using (var reloaded = (IDisposable)Activator.CreateInstance(GameType("PlayerControls")))
            {
                Activator.CreateInstance(GameType("ControlBindings"), reloaded);
                var reloadedActions = (IInputActionCollection2)reloaded;
                Assert.That(reloadedActions.FindAction("Ingame/Move").bindings[2].effectivePath,
                    Is.EqualTo("<Keyboard>/r").IgnoreCase);
                Assert.That(reloadedActions.FindAction("Ingame/Attack").bindings[1].effectivePath,
                    Is.EqualTo("<Mouse>/rightButton").IgnoreCase);
            }

            screen.Find("Controls panel/Back").GetComponent<Button>().onClick.Invoke();
            Assert.That(content.activeInHierarchy, Is.True);
            Assert.That(events.currentSelectedGameObject, Is.EqualTo(controlsButton.gameObject));
            Assert.That(Time.timeScale, Is.Zero);
            Invoke(settings, "ResumeGame");

            SetInput(default, default);
            SetInput(new KeyboardState(Key.R), new MouseState().WithButton(MouseButton.Right));
            Assert.That(actions["Ingame/Move"].ReadValue<Vector2>(), Is.EqualTo(Vector2.up));
            Assert.That(actions["Ingame/Attack"].IsPressed(), Is.True);

            PressUIKey(controller, Key.Escape);
            Invoke(settings, "ShowControls");
            reset.onClick.Invoke();
            Assert.That(PlayerPrefs.HasKey(prefsKey), Is.True);
            reset.onClick.Invoke();
            Assert.That(PlayerPrefs.HasKey(prefsKey), Is.False);
            Assert.That(actions["Ingame/Move"].bindings[2].overridePath, Is.Null);
            Assert.That(actions["Ingame/Attack"].bindings[1].overridePath, Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(canvas);
            Object.DestroyImmediate(events.gameObject);
            Object.DestroyImmediate(ui.gameObject);
            if (originalPreference != null) PlayerPrefs.SetString(prefsKey, originalPreference);
            else PlayerPrefs.DeleteKey(prefsKey);
            PlayerPrefs.Save();
        }
    }

    [Test]
    public void MapHandlesPanAndZoomOnlyWhileOpenAndSuppressesHeldAttackOnClose()
    {
        var ui = new GameObject("UI test manager").AddComponent(GameType("UIManager"));
        var controller = ui.gameObject.AddComponent(GameType("UIController"));
        var root = new GameObject("Map test root");
        root.SetActive(false);
        var map = root.AddComponent(GameType("MapUI"));
        var bigMap = new GameObject("Full map");
        var minimap = new GameObject("Minimap");
        var camera = bigMap.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 20f;
        bigMap.transform.SetParent(root.transform);
        minimap.transform.SetParent(root.transform);
        SetField(map, "map", bigMap);
        SetField(map, "minimap", minimap);
        SetField(map, "mapCamera", camera);
        root.SetActive(true);

        try
        {
            var actions = GetActions();
            var inputManager = GetInputManager();
            var attackHeld = inputManager.GetType().GetProperty("IsAttackHeld");
            var drag = new MouseState { delta = new Vector2(3f, 2f), scroll = Vector2.up }.WithButton(MouseButton.Left);

            SetInput(default, drag);
            Invoke(map, "LateUpdate");
            Assert.That(actions["MapUI/Zoom map"].enabled, Is.False);
            Assert.That(camera.orthographicSize, Is.EqualTo(20f));

            PressUIKey(controller, Key.M);
            Assert.That(bigMap.activeInHierarchy, Is.True);
            Assert.That(minimap.activeInHierarchy, Is.False);
            Assert.That(actions["MapUI/Zoom map"].enabled, Is.True);
            SetInput(default, drag);
            Invoke(map, "LateUpdate");
            Assert.That(camera.orthographicSize, Is.LessThan(20f));
            Assert.That(camera.transform.position.x, Is.EqualTo(-3f).Within(0.001f));

            SetInput(new KeyboardState(Key.M), new MouseState().WithButton(MouseButton.Left));
            Invoke(controller, "Update");
            Assert.That(bigMap.activeInHierarchy, Is.False);
            Assert.That(minimap.activeInHierarchy, Is.True);
            Assert.That(actions["MapUI/Zoom map"].enabled, Is.False);
            Assert.That(attackHeld.GetValue(inputManager), Is.False);

            SetInput(default, default);
            Invoke(inputManager, "Update");
            SetInput(default, new MouseState().WithButton(MouseButton.Left));
            Assert.That(attackHeld.GetValue(inputManager), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(ui.gameObject);
        }
    }

#if UNITY_EDITOR
    [Test]
    public void TabAndEscapeControlTheInventoryPrefab()
    {
        var ui = new GameObject("UI test manager").AddComponent(GameType("UIManager"));
        var controller = ui.gameObject.AddComponent(GameType("UIController"));
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Inventory/Inventory.prefab");
        var staging = new GameObject("Inactive prefab staging");
        staging.SetActive(false);
        var inventoryRoot = Object.Instantiate(prefab, staging.transform);
        inventoryRoot.SetActive(false);
        inventoryRoot.transform.SetParent(null);
        Object.DestroyImmediate(staging);

        var inventoryView = inventoryRoot.GetComponent(GameType("UIInventory"));
        var actionPanel = new GameObject("Item actions");
        actionPanel.transform.SetParent(inventoryRoot.transform);
        SetField(inventoryView, "actionPanel", actionPanel.AddComponent(GameType("ItemActionPanel")));
        var mouseFollower = new GameObject("Dragged item");
        mouseFollower.transform.SetParent(inventoryRoot.transform);
        SetField(inventoryView, "mouseFw", mouseFollower.AddComponent(GameType("MouseFollower")));
        inventoryRoot.SetActive(true);

        var inventory = new GameObject("Test inventory owner").AddComponent(GameType("InventoryController"));
        var data = ScriptableObject.CreateInstance(GameType("InventorySO"));
        data.GetType().GetMethod("Initialize").Invoke(data, null);
        SetField(inventory, "inventoryData", data);
        try
        {
            PressUIKey(controller, Key.Tab);
            Assert.That(inventoryRoot.activeInHierarchy, Is.True);
            Assert.That(GameType("InputManager").GetProperty("GameplayBlocked").GetValue(null), Is.True);
            PressUIKey(controller, Key.Escape);
            Assert.That(inventoryRoot.activeInHierarchy, Is.False);
            Assert.That(GameType("InputManager").GetProperty("GameplayBlocked").GetValue(null), Is.False);
            PressUIKey(controller, Key.Tab);
            Assert.That(inventoryRoot.activeInHierarchy, Is.True);
            PressUIKey(controller, Key.Tab);
            Assert.That(inventoryRoot.activeInHierarchy, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(inventory.gameObject);
            Object.DestroyImmediate(inventoryRoot);
            Object.DestroyImmediate(ui.gameObject);
        }
    }

    [Test]
    public void HomeCharacterMovesAndStopsWhenEnteringDungeon()
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset/Prefab/Player/Assassin/Home_Assassin.prefab");
        var character = Object.Instantiate(prefab);
        var player = new GameObject("Home controls");
        player.transform.SetParent(character.transform);
        var movement = player.AddComponent(GameType("HomeMovement"));
        var entrance = new GameObject("Dungeon entrance").AddComponent(GameType("DungeonEntrance"));
        var effect = new GameObject("Entrance effect");
        effect.SetActive(false);
        SetField(entrance, "animationPrefab", effect);
        var unrelated = new GameObject("Unrelated body");
        unrelated.AddComponent<Rigidbody2D>();
        var unrelatedCollider = unrelated.AddComponent<BoxCollider2D>();
        var playerCollider = player.AddComponent<BoxCollider2D>();
        var body = character.GetComponent<Rigidbody2D>();

        try
        {
            Assert.That(character.activeInHierarchy, Is.True);
            Invoke(movement, "BindObj", character);
            SetInput(new KeyboardState(Key.D), default);
            Invoke(movement, "Update");
            Assert.That(body.linearVelocity.x, Is.EqualTo(5f).Within(0.001f));
            Assert.That(character.transform.localScale.x, Is.GreaterThan(0f));

            Invoke(entrance, "OnTriggerEnter2D", unrelatedCollider);
            SetInput(new KeyboardState(Key.D, Key.F), default);
            Invoke(entrance, "Update");
            Assert.That(((Behaviour)movement).enabled, Is.True);

            Invoke(entrance, "OnTriggerEnter2D", playerCollider);
            Invoke(entrance, "OnTriggerExit2D", unrelatedCollider);
            SetInput(new KeyboardState(Key.D), default);
            SetInput(new KeyboardState(Key.D, Key.F), default);
            Invoke(entrance, "Update");
            Assert.That(((Behaviour)movement).enabled, Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        }
        finally
        {
            var spawnedEffect = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in spawnedEffect)
                if (obj.scene.IsValid() && obj.name == "Entrance effect(Clone)") Object.DestroyImmediate(obj);
            Object.DestroyImmediate(unrelated);
            Object.DestroyImmediate(effect);
            Object.DestroyImmediate(entrance.gameObject);
            Object.DestroyImmediate(character);
        }
    }
#endif

    private void PressUIKey(Component controller, Key key)
    {
        SetInput(default, default);
        SetInput(new KeyboardState(key), default);
        Invoke(controller, "Update");
    }

    private static Type GameType(string name) => Type.GetType(name + ", Assembly-CSharp", true);

    private static void SetField(Component component, string name, object value)
    {
        component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .SetValue(component, value);
    }

    private static object Invoke(Component component, string name, params object[] args)
    {
        return component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Invoke(component, args);
    }

    private static Component GetInputManager()
    {
        var manager = (Component)GameType("InputManager").GetProperty("Instance").GetValue(null);
        Assert.That(manager, Is.Not.Null);
        return manager;
    }

    private static InputActionAsset GetActions()
    {
        var manager = GetInputManager();
        var controls = manager.GetType().GetProperty("playerInput").GetValue(manager);
        return ((IInputActionCollection2)controls).FindAction("Ingame/Move").actionMap.asset;
    }

    private void SetInput(KeyboardState keyboardState, MouseState mouseState)
    {
        InputSystem.QueueStateEvent(keyboard, keyboardState);
        InputSystem.QueueStateEvent(mouse, mouseState);
        InputSystem.Update();
    }
}
