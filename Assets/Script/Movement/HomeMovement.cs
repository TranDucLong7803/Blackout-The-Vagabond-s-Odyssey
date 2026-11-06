using UnityEngine;

public class HomeMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isFacingRight;
    private int moveAnimation;
    private GameObject bindedObj;

    void Awake()
    {
        moveAnimation = Animator.StringToHash("isMoving");
    }

    public void BindObj(GameObject obj)
    {
        StopMoving();
        bindedObj = obj;
        rb = bindedObj.GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        animator = bindedObj.GetComponent<Animator>();
        isFacingRight = bindedObj.transform.localScale.x > 0;
    }

    private void Update()
    {
        if(bindedObj) animator.SetBool(moveAnimation, MoveControl());
    }

    private void OnDisable()
    {
        StopMoving();
    }

    private void StopMoving()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool(moveAnimation, false);
    }

    private bool MoveControl()
    {
        Vector2 moveInput = InputManager.GameplayBlocked
            ? Vector2.zero
            : InputManager.Instance.playerInput.Ingame.Move.ReadValue<Vector2>();
        rb.linearVelocity = Vector2.ClampMagnitude(moveInput, 1f) * speed;

        if (moveInput.x > 0 && !isFacingRight)
            Flip();
        else if (moveInput.x < 0 && isFacingRight)
            Flip();

        return moveInput != Vector2.zero;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = bindedObj.transform.localScale;
        scale.x *= -1;
        bindedObj.transform.localScale = scale;
    }
}
