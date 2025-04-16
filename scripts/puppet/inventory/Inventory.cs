using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class Inventory : Node
{
    public delegate void ItemlistChangedDelegate(List<Item> items, InventoryState inventoryState);
	public event ItemlistChangedDelegate ItemlistChanged;
    public InventoryAnimationEventReporter AnimationEventInformer {get; private set;} = new();
    public List<Item> Items {get; private set;} = new();
    InventoryState _InventoryState = new();
    BoneAttachment3D _HandAttachment;
    Node3D _OwnerCamNode3D;
    public Inventory(BoneAttachment3D handAttachment, Node3D ownerCamNode3D)
    {
        _HandAttachment = handAttachment;
        _OwnerCamNode3D = ownerCamNode3D;
    }
    public void TryUsePrimary()
    {
        if(_InventoryState.ActiveItems.primary == -1)
            return;
        Items[_InventoryState.ActiveItems.primary].TryUsePrimary();
    }
    public void TryUseSecondary()
    {
        if(_InventoryState.ActiveItems.primary == -1)
            return;
        Items[_InventoryState.ActiveItems.primary].TryUseSecondary();
    }
    public void TryAdd(GodotObject item)
    {
        var invItem = item as Item;
        if(invItem is not null)
        {
            Items.Add(invItem);
            invItem.TryPickUp(_HandAttachment, _OwnerCamNode3D);
            ItemlistChanged(Items, _InventoryState);
        }
    }
    public void SetTarget(int index) 
    {
        if(index < 0 || index >= Items.Count)
        {
            return;
        }
        if(index == _InventoryState.ActiveItems.primary)
        {
            return; //fix for going back to item already being holstered, could fix 
        }
        if(Items[index].Primary)
        {
            _InventoryState.TargetItems.primary = index;
        }
        else
        {
            _InventoryState.TargetItems.secondary = index;
        }
        OnTargetChanged();
        ItemlistChanged(Items, _InventoryState);
    }

    public void TryDrop()
    {
        if(_InventoryState.ActiveItems.primary == -1)
            return;
        Items[_InventoryState.ActiveItems.primary].TryDrop();
        Items.RemoveAt(_InventoryState.ActiveItems.primary);
        _InventoryState.ActiveItems = (-1, -1);
        ItemlistChanged(Items, _InventoryState);
    }
    public override void _Process(double delta)
    {
        foreach(Item item in Items)
        {
            if(item.PosessionState == Item.PosessionStateEnum.HOLSTERED)
                item.ProcessInternal(delta);
        }
        Debugger.Instance.SetProperty("ActiveItem", _InventoryState.ActiveItems.primary);
        Debugger.Instance.SetProperty("TargetItem", _InventoryState.TargetItems.primary);
    }
    private void OnTargetChanged()
    {
        if(_InventoryState.ActiveItems.primary == -1)
        {
            OnHolsterFinished(true);
        }
        else
        {
            OnEquipFinished(true);
        }
    }
    private void OnHolsterFinished(bool primary)
    {
        // this needs to be simpler, maybe make some new vars
        if(_InventoryState.TargetItems.primary == -1)
            return;
        _InventoryState.ActiveItems.primary = _InventoryState.TargetItems.primary;
        Item newCurrentItem = Items[_InventoryState.ActiveItems.primary];
        InventoryItemAnimationEventReporter[] newCurrentTrees = 
            new InventoryItemAnimationEventReporter[] {newCurrentItem.AnimationEvents}; //type conversion, fix later
        AnimationEventInformer.ActiveItemsChanged.Invoke(newCurrentTrees);
        Items[_InventoryState.TargetItems.primary].TryEquip();
        Items[_InventoryState.TargetItems.primary].EquipFinished = null;
        Items[_InventoryState.TargetItems.primary].EquipFinished += OnEquipFinished;
    }
    private void OnEquipFinished(bool primary)
    {
        // this needs to be simpler
        if(_InventoryState.TargetItems.primary == _InventoryState.ActiveItems.primary)
            return;
        Items[_InventoryState.ActiveItems.primary].TryHolster();
        Items[_InventoryState.ActiveItems.primary].HolsterFinished = null;
        Items[_InventoryState.ActiveItems.primary].HolsterFinished += OnHolsterFinished;
    }
}