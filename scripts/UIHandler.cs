using System.Collections.Generic;
using System;
using Godot;
public partial class UIHandler : Node
{
    public static UIHandler Instance { get; private set; }
    Puppet _ControlledPuppet;
    PuppetUI _PuppetUI;
    public void SetControlledPuppet(Puppet informer)
    {
        if(_ControlledPuppet != null)
            _ControlledPuppet.OnInventoryChanged -= UpdateItemPreviews;
        
        _ControlledPuppet = informer;
        _ControlledPuppet.OnInventoryChanged += UpdateItemPreviews;
    }
    public override void _Ready()
    {
        Instance = this;
        _PuppetUI = ((PackedScene)GD.Load("res://scenes/UI/PuppetUI.tscn")).Instantiate() as PuppetUI;
        AddChild(_PuppetUI);
    }
    public override void _Process(double delta)
    {

    }

    void UpdateItemPreviews(List<InventoryItem> items, InventoryState state)
    {
        _PuppetUI.ClearPreviews();
        int i = 0;
        foreach(InventoryItem item in items)
        {
            bool isPrimary = i == state.TargetItems.primary;          
            _PuppetUI.AddItemPreview(item.PreviewTexture, isPrimary, item.MirrorPreview);
            i++;
        }
    }
}