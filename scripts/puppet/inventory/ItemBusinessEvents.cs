using System;
using Godot;

public class InventoryItemBusinessEventReporter
{
    public Action EquipBegin;
    public Action EquipEnd;
    public Action HolsterBegin;
    public Action HolsterEnd;
    public void ClearSubcsribers()
    {
        EquipBegin = null;
        EquipEnd = null;
        HolsterBegin = null;
        HolsterEnd = null;
    }
}