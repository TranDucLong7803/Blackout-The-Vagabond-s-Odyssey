using UnityEngine;

public class AimPoint : MonoBehaviour
{
    public static AimPoint Instance;

    [SerializeField] private Texture2D cursorTexture;

    private void OnEnable()
    {
        if (cursorTexture == null) return;

        var cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.ForceSoftware);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void Awake()
    {
        Instance = this;
    }

    public void DisableAim()
    {
        gameObject.SetActive(false);
        Cursor.visible = true;
    }

    public void EnableAim()
    {
        gameObject.SetActive(true);
    }
}
