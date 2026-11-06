using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
internal sealed class ControlBindingHint : MonoBehaviour
{
    [SerializeField] private string actionPath;
    [SerializeField] private string part;
    [SerializeField] private string prefix;
    [SerializeField] private string suffix;

    private ControlBindings bindings;
    private ControlBindings.Entry entry;
    private TMP_Text label;

    private void OnEnable()
    {
        if (InputManager.Instance == null) return;
        var input = InputManager.Instance;
        var action = input.playerInput.asset.FindAction(actionPath);
        bindings = input.Bindings;
        foreach (var candidate in bindings.Entries)
        {
            if (candidate.Action != action) continue;
            if (!string.IsNullOrEmpty(part) && action.bindings[candidate.Indices[0]].name != part) continue;

            entry = candidate;
            label = GetComponent<TMP_Text>();
            bindings.Changed += Refresh;
            Refresh();
            break;
        }
    }

    private void OnDisable()
    {
        if (entry == null) return;

        bindings.Changed -= Refresh;
        entry = null;
    }

    private void Refresh() => label.text = prefix + entry.Display(0) + suffix;
}
