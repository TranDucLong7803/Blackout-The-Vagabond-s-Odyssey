using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[DefaultExecutionOrder(-15)]
public class Interactor : MonoBehaviour
{
    [SerializeField] private float range = 1.5f;
    private Interactable currentInteractable;

    void Awake()
    {
        var trigger = GetComponent<CircleCollider2D>();
        trigger.radius = range;
        trigger.isTrigger = true;
        trigger.includeLayers = LayerMask.GetMask("Interactable");
    }

    private void Update()
    {
        if (currentInteractable != null && InputManager.Instance.TryConsumeInteract())
            Interact();
    }

    private void OnDisable()
    {
        ClearInteractable();
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActiveAndEnabled) return;

        if (collision.TryGetComponent(out Interactable newInteractable))
        {
            if (currentInteractable != newInteractable)
            {
                if (currentInteractable != null) currentInteractable.HighLightOff();
                newInteractable.HighLightOn();
                currentInteractable = newInteractable;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (currentInteractable != null && collision.gameObject == currentInteractable.gameObject)
            ClearInteractable();
    }

    private void Interact()
    {
        var interactable = currentInteractable;
        if (interactable == null) return;

        ClearInteractable();
        interactable.Interact(this);
    }

    private void ClearInteractable()
    {
        if (currentInteractable != null) currentInteractable.HighLightOff();
        currentInteractable = null;
    }
}
