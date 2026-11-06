using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

internal sealed class ControlBindings
{
    private const string PreferencesKey = "KeyboardMouseBindingOverrides";

    internal sealed class Entry
    {
        public readonly string Label;
        public readonly InputAction Action;
        public readonly int[] Indices;

        public Entry(string label, InputAction action, int[] indices)
        {
            Label = label;
            Action = action;
            Indices = indices;
        }

        public string Display(int slot) => Action.GetBindingDisplayString(
            Indices[slot], InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
    }

    private readonly InputActionAsset asset;
    private readonly List<Entry> entries = new();
    public IReadOnlyList<Entry> Entries => entries;
    public event Action Changed;

    public ControlBindings(PlayerControls controls)
    {
        asset = controls.asset;
        var ingame = controls.Ingame;
        var menu = controls.Menu;
        Add("Move up", ingame.Move, "up");
        Add("Move down", ingame.Move, "down");
        Add("Move left", ingame.Move, "left");
        Add("Move right", ingame.Move, "right");
        Add("Map", menu.Togglemap);
        Add("Inventory", menu.ToggleInventory);
        Add("Attack", ingame.Attack);
        Add("Interact", ingame.Interact);
        Add("Weapon 1", ingame.Previous);
        Add("Weapon 2", ingame.Next);
        Add("Normal skill", ingame.Skill1);
        Add("Ultimate skill", ingame.Skill2);

        var saved = PlayerPrefs.GetString(PreferencesKey);
        if (string.IsNullOrEmpty(saved)) return;

        try
        {
            asset.LoadBindingOverridesFromJson(saved);
        }
        catch (ArgumentException exception)
        {
            asset.RemoveAllBindingOverrides();
            Debug.LogWarning($"Could not load control bindings. Using defaults. {exception.Message}");
        }
    }

    private void Add(string label, InputAction action, string direction = null)
    {
        var indices = new List<int>();
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isComposite || binding.isPartOfComposite != (direction != null)) continue;
            if (direction != null && binding.name != direction) continue;
            if (binding.path.StartsWith("<Keyboard>/", StringComparison.OrdinalIgnoreCase)
                || binding.path.StartsWith("<Mouse>/", StringComparison.OrdinalIgnoreCase))
                indices.Add(i);
        }

        if (indices.Count > 0)
            entries.Add(new Entry(label, action, indices.ToArray()));
    }

    public string Apply(Entry target, int slot, InputControl control, string path)
    {
        int targetIndex = target.Indices[slot];
        foreach (var entry in entries)
        {
            foreach (int index in entry.Indices)
            {
                if (entry == target && index == targetIndex) continue;

                if (InputControlPath.Matches(entry.Action.bindings[index].effectivePath, control))
                    return $"{target.Label} unchanged. {control.displayName} is assigned to {entry.Label}.";
            }
        }

        target.Action.ApplyBindingOverride(targetIndex, path);
        PlayerPrefs.SetString(PreferencesKey, asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
        Changed?.Invoke();
        return $"{target.Label} set to {target.Display(slot)}.";
    }

    public void Reset()
    {
        asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PreferencesKey);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
