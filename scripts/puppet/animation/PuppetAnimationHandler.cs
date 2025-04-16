using Godot;
using System;
using System.Collections.Generic;

public class PuppetAnimationHandler
{
    InventoryItemAnimationEventReporter[] _itemAnimationEventReporters;
    AnimationTree _armsTree;
    Random _random;
    AnimationNodeAnimation _baseAnimNode;
    AnimationNodeAnimation _oneshotAnimNode;
    AnimationNodeAnimation _blendedOneshotAnimNode1;
    AnimationNodeAnimation _blendedOneshotAnimNode2;
    public PuppetAnimationHandler(InventoryAnimationEventReporter inventoryAnimationEventReporter, AnimationTree armsTree)
    {
        inventoryAnimationEventReporter.ActiveItemsChanged += OnActiveItemsChanged;
        // . . .
        _armsTree = armsTree;
        _random = new Random();
        AnimationNodeBlendTree animTreeRoot = (AnimationNodeBlendTree)_armsTree.TreeRoot;
        _baseAnimNode = (AnimationNodeAnimation)animTreeRoot.GetNode("BaseAnim");
        _oneshotAnimNode = (AnimationNodeAnimation)animTreeRoot.GetNode("OneShotAnim");
        _blendedOneshotAnimNode1= (AnimationNodeAnimation)animTreeRoot.GetNode("BlendAnim1");
        _blendedOneshotAnimNode2= (AnimationNodeAnimation)animTreeRoot.GetNode("BlendAnim2");
    }
    private void OnActiveItemsChanged(InventoryItemAnimationEventReporter[] itemAnimEventReporters)
    {
        foreach (var reporter in itemAnimEventReporters)
        {
            reporter.ClearSubcsribers();
            reporter.EquipBegin += OnEquipBegin;
            reporter.EquipEnd += OnEquipEnd;
            reporter.HolsterBegin += OnHolsterBegin;
            reporter.PrimaryUseBegin += OnPrimaryUseBegin;
        }
        _itemAnimationEventReporters = itemAnimEventReporters;
    }
    private void OnEquipBegin(AnimationTree itemTree, string animPostfix)
    {
        SetBaseAnimArms("idle_" + animPostfix);
        StartOneshotArms("equip_to_idle_" + animPostfix);
    }
    private void OnEquipEnd(AnimationTree itemTree, string animPostfix)
    {
        
    }
    private void OnHolsterBegin(AnimationTree itemTree, string animPostfix)
    {
        SetBaseAnimArms("idle");
        StartOneshotArms("holster_ready_" + animPostfix);
    }
    private void OnPrimaryUseBegin(AnimationTree itemTree, string animPostfix)
    {
        SetBaseAnimArms("idle_" + animPostfix);
        float var = (float)_random.NextDouble();
        Debugger.Instance.SetProperty("BlendFactor", var);
        StartBlendedOneshotArms("shotleft_" + animPostfix, "shotright_" + animPostfix, var);
        SetBaseAnimItem("idle_" + animPostfix + "_item", itemTree);
        StartOneshotItem("shot" + animPostfix + "_item", itemTree);
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
    private void StartBlendedOneshotArms(string animName1, string animName2, float blendFactor)
    {
        _blendedOneshotAnimNode1.Animation = animName1;
        _blendedOneshotAnimNode2.Animation = animName2;
        _armsTree.Set("parameters/Blend/blend_amount", blendFactor);
        _armsTree.Set("parameters/BlendOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    private void SetBaseAnimItem(string animName, AnimationTree itemTree)
    {
        ((itemTree.TreeRoot as AnimationNodeBlendTree).GetNode("BaseAnim") as AnimationNodeAnimation).Animation = animName;
    }
    private void StartOneshotItem(string animName, AnimationTree itemTree)
    {
        itemTree.Set("parameters/Blend/blend_amount", 0f);
        ((itemTree.TreeRoot as AnimationNodeBlendTree).GetNode("OneShotAnim") as AnimationNodeAnimation).Animation = animName;
        itemTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }   
}
