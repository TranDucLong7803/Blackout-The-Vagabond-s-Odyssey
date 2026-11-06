using UnityEngine;

[DefaultExecutionOrder(-20)]
public class UIController : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        var ui = UIManager.Instance;
        if (ui == null) return;

        var input = InputManager.Instance.playerInput.Menu;
        if (input.Cancel.WasPressedThisFrame())
        {
            if (ui.OpenedScreen != null)
                ui.CloseCurrentScreen();
            else if (SettingsMenuUI.Instance != null)
                SettingsMenuUI.Instance.Show();
            return;
        }

        if (Time.timeScale <= 0f) return;

        if (input.ToggleInventory.WasPressedThisFrame() && InventoryController.Instance != null && UIInventory.Instance != null)
        {
            var health = InventoryController.Instance.GetComponent<PlayerHealthController>();
            if (health == null || !health.IsDead)
                ui.ToggleScreen(InventoryController.Instance);
        }
        else if (input.Togglemap.WasPressedThisFrame())
        {
            var map = FindFirstObjectByType<MapUI>();
            if (map != null) ui.ToggleScreen(map);
        }
    }
}
