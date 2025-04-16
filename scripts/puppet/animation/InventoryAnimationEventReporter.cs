using System;
using System.Collections.Generic;
using Godot;

public class InventoryAnimationEventReporter
{
    public delegate void ActiveItemsChangedDelegate(InventoryItemAnimationEventReporter[] newReporters);
    public ActiveItemsChangedDelegate ActiveItemsChanged;
}
