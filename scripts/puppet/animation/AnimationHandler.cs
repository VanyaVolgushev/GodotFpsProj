using Godot;
using System;
using System.Collections.Generic;

public class AnimationHandler
{
    InventoryItemAnimationEventReporter[] _itemAnimationEventReporters;
    AnimationTree _armsTree;
    Random _random;
    AnimationNodeAnimation _baseAnimNode;
    AnimationNodeAnimation _oneshotAnimNode;
    public AnimationHandler(InventoryAnimationEventReporter inventoryAnimationEventReporter, AnimationTree armsTree)
    {
        inventoryAnimationEventReporter.ActiveItemsChanged += OnActiveItemsChanged;
        // . . .
        _armsTree = armsTree;
        _random = new Random();
        AnimationNodeBlendTree animTreeRoot = (AnimationNodeBlendTree)_armsTree.TreeRoot;
        _baseAnimNode = (AnimationNodeAnimation)animTreeRoot.GetNode("BaseAnim");
        _oneshotAnimNode = (AnimationNodeAnimation)animTreeRoot.GetNode("OneShotAnim");
    }
    private void OnActiveItemsChanged(InventoryItemAnimationEventReporter[] itemAnimEventReporters)
    {
        foreach (var reporter in itemAnimEventReporters)
        {
        //    reporter.ClearSubcsribers();
        //    reporter.EquipBegin += OnEquipBegin;
        //    reporter.EquipEnd += OnEquipEnd;
        //    reporter.HolsterBegin += OnHolsterBegin;
        //    reporter.PrimaryUseBegin += OnPrimaryUseBegin;
        }
        _itemAnimationEventReporters = itemAnimEventReporters;
    }
    private void OnPlayOneShot(string baseAnimName, string oneshotAnimName)
    {
        SetBaseAnimArms(baseAnimName);
        StartOneshotArms(oneshotAnimName);
    }
    private void SetBaseAnimArms(string animName)
    {
        _baseAnimNode.Animation = animName;
    }
    private void StartOneshotArms(string animName)
    {
        _oneshotAnimNode.Animation = animName;
        _armsTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    private void SetBaseAnimItem(string animName, AnimationTree itemTree)
    {
        ((itemTree.TreeRoot as AnimationNodeBlendTree).GetNode("BaseAnim") as AnimationNodeAnimation).Animation = animName;
    }
    private void StartOneshotItem(string animName, AnimationTree itemTree)
    {
        ((itemTree.TreeRoot as AnimationNodeBlendTree).GetNode("OneShotAnim") as AnimationNodeAnimation).Animation = animName;
        itemTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
}