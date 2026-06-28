using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class KitchenObjectSO : ScriptableObject
{
    private static Dictionary<string, KitchenObjectSO> _registry = null;

    public static void BuildRegistry()
    {
        if (_registry != null) return;
        _registry = new Dictionary<string, KitchenObjectSO>();
        KitchenObjectSO[] all = Resources.LoadAll<KitchenObjectSO>("KitchenObjectSOs");
        foreach (var so in all)
        {
            if (!string.IsNullOrEmpty(so.objectName) && !_registry.ContainsKey(so.objectName))
                _registry[so.objectName] = so;
        }
    }

    public static KitchenObjectSO FindByName(string name)
    {
        BuildRegistry();
        if (_registry.TryGetValue(name, out var so)) return so;
        return null;
    }

    public GameObject prefab;
    public Sprite sprite;
    public string objectName;
}