using System.Collections;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    private float baseSpeed;
    private Rigidbody2D rd;
    private Coroutine currentSpeedBoost;

    private void Start()
    {
        baseSpeed = GetComponent<Player>().speed;
        speed = baseSpeed;
        rd = GetComponent<Rigidbody2D>();
    }

    public bool MoveControl()
    {
        Vector2 moveInput = InputManager.GameplayBlocked
            ? Vector2.zero
            : InputManager.Instance.playerInput.Ingame.Move.ReadValue<Vector2>();
        rd.linearVelocity = Vector2.ClampMagnitude(moveInput, 1f) * speed;
        return moveInput != Vector2.zero;
    }

    public void ApplySpeedBoost(float percentBoost, float duration)
    {
        if (currentSpeedBoost != null)
        {
            StopCoroutine(currentSpeedBoost);
        }

        currentSpeedBoost = StartCoroutine(SpeedBoostRoutine(percentBoost, duration));
    }

    private IEnumerator SpeedBoostRoutine(float percentBoost, float duration)
    {
        float multiplier = 1f + (percentBoost / 100f);
        speed = baseSpeed * multiplier;

        yield return new WaitForSeconds(duration);

        speed = baseSpeed;
        currentSpeedBoost = null;
    }

    public void ResetSpeed()
    {
        if (currentSpeedBoost != null)
        {
            StopCoroutine(currentSpeedBoost);
            currentSpeedBoost = null;
        }
        speed = baseSpeed;
    }


}
