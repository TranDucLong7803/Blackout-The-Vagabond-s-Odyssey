using UnityEngine;

public class BackgroundPan : MonoBehaviour
{
    public float panStrength = 1f; //smooth
    public float maxPanX = 10f;        //limit
    public float maxPanY = 10f;        //limit

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        Vector2 mousePos = InputManager.PointerPosition;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offsetFromCenter = (mousePos - screenCenter) / screenCenter;

        float xOffset = Mathf.Clamp(offsetFromCenter.x * panStrength, -maxPanX, maxPanX);
        float yOffset = Mathf.Clamp(offsetFromCenter.y * panStrength, -maxPanY, maxPanY);

        transform.localPosition = initialPosition + new Vector3(xOffset, yOffset, 0f);
    }
}
