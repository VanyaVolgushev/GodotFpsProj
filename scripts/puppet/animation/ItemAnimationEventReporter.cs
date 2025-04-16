using System;
using Godot;

public class InventoryItemAnimationEventReporter
{
    public Action<AnimationTree, string> EquipBegin;
    public Action<AnimationTree, string> EquipEnd;
    public Action<AnimationTree, string> HolsterBegin;
    // had problems with holster end because then you don't have the animation tree anymore
    public Action<AnimationTree, string> PrimaryUseBegin;
    public Action<AnimationTree, string> SecondaryUseBegin;
    public void ClearSubcsribers()
    {
        EquipBegin = null;
        EquipEnd = null;
        HolsterBegin = null;
        PrimaryUseBegin = null;
        SecondaryUseBegin = null;
    }
}