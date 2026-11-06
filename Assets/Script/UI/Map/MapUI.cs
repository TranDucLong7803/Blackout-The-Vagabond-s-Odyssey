using UnityEngine;
using UnityEngine.InputSystem;

public class MapUI : MonoBehaviour, IUIScreen
{
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 100f;
    [SerializeField] private Camera mapCamera;
    [SerializeField] private GameObject minimap, map;
    private bool isOpen;

    private void Awake()
    {
        SetMapVisible(false);
    }

    public void Open()
    {
        SetMapVisible(true);
        InputManager.Instance.playerInput.MapUI.Enable();
    }

    public void Close()
    {
        SetMapVisible(false);
        if (InputManager.Instance != null) InputManager.Instance.playerInput.MapUI.Disable();
    }

    private void SetMapVisible(bool visible)
    {
        isOpen = visible;
        if (map != null) map.SetActive(visible);
        if (minimap != null) minimap.SetActive(!visible);
    }

    private void OnDisable()
    {
        if (!isOpen) return;

        var ui = UIManager.Instance;
        if (ui != null && ui.OpenedScreen == this)
            ui.CloseCurrentScreen();
        else
            Close();
    }

    private void LateUpdate()
    {
        if (!isOpen || mapCamera == null) return;

        var input = InputManager.Instance.playerInput.MapUI;
        float scroll = input.Zoommap.ReadValue<float>();
        if (scroll != 0f)
            mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = input.Panmap.ReadValue<Vector2>();
            mapCamera.transform.position -= (Vector3)delta * panSpeed;
        }
    }
}
