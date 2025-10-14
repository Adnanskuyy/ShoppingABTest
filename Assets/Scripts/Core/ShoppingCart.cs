using System;
using System.Collections.Generic;

// This is a Plain Old C# Object (POCO). It does not inherit from MonoBehaviour.
// Its only job is to manage the cart data.
public class ShoppingCart
{
    public Dictionary<string, int> items { get; private set; } = new Dictionary<string, int>();

    // Event fired whenever the cart's contents change.
    // The payload is the updated dictionary of items.
    public event Action<Dictionary<string, int>> onCartUpdated;

    public void AddItem(string productName)
    {
        if (items.ContainsKey(productName))
        {
            items[productName]++;
        }
        else
        {
            items[productName] = 1;
        }
        onCartUpdated?.Invoke(items);
    }

    public Dictionary<string, int> GetItems()
    {
        return items;
    }
}
