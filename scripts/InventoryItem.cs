using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class InventoryItem : RigidBody3D
{
    public delegate void HandDelegate(bool primary);
    public delegate void AnimationStartDelegate(AnimationTree itemAnimTree); 
    public HandDelegate HolsterFinished;
    public HandDelegate EquipFinished;
    public bool Busy => Equipping || Holstering;
    public enum PosessionStateEnum {UNPOSESSED, HOLSTERED, EQUIPPED}
    public PosessionStateEnum PosessionState {get; private set;} = PosessionStateEnum.UNPOSESSED;
    [Export] public CompressedTexture2D PreviewTexture {get; private set;}
    [Export] public bool MirrorPreview {get; private set;} = false;
    [Export] protected  double EquipTime;
    [Export] protected double HolsterTime;
    [Export] public bool Primary {get; private set;} = true;
    public bool Equipping {get; private set;} = false;
    public bool Holstering {get; private set;} = false;
    protected uint _collisionLayer;
    protected Node3D _ownerCamNode3D;
    protected BoneAttachment3D _handAttachment;
    protected bool InPosession {
        get {
            if(PosessionState != PosessionStateEnum.UNPOSESSED && _ownerCamNode3D == null)
                throw new Exception("ownerCamNode3D is null when in posession");
            return PosessionState != PosessionStateEnum.UNPOSESSED;
        }
    }
    public override void _Ready()
    {
        _collisionLayer = CollisionLayer;
    }
    public override void _Process(double delta)
    {
        ProcessInternal(delta);
    }
    public bool TryPickUp(BoneAttachment3D handAttachment, Node3D ownerCamNode3D)
    {
        if(!InPosession)
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            _handAttachment = handAttachment;
            _ownerCamNode3D = ownerCamNode3D;
            SetPosessionState(PosessionStateEnum.HOLSTERED);
            return true;
        }
        return false;
    }
    public void TryDrop()
    {
        if(InPosession)
        {
            SetPosessionState(PosessionStateEnum.UNPOSESSED);
        }
    }
    public bool TryHolster()
    {
        if(!Busy && PosessionState == PosessionStateEnum.EQUIPPED) {
            Holster();
            return true;
        }
        return false;
    }
    public bool TryEquip()
    {
        if(!Busy && PosessionState == PosessionStateEnum.HOLSTERED) {
            Equip();
            return true;
        }
        return false;
    }
    private async void Equip() {
        Equipping = true;
        //START ANIMATION
        SetPosessionState(PosessionStateEnum.EQUIPPED);
        Debugger.Instance.CreateDebugSphere(_handAttachment.GlobalPosition, 2f, 0.2f, new Color(0, 1, 0));
        await ToSignal(_handAttachment.GetTree().CreateTimer(EquipTime), "timeout");
        Debugger.Instance.CreateDebugSphere(_handAttachment.GlobalPosition, 2f, 0.2f, new Color(0, 0.4f, 0));
        EquipFinished.Invoke(Primary);
        Equipping = false;
    }
    private async void Holster() {
        Holstering = true;
        //START ANIMATION
        Debugger.Instance.CreateDebugSphere(_handAttachment.GlobalPosition, 2f, 0.2f, new Color(1f, 0, 0));
        await ToSignal(_handAttachment.GetTree().CreateTimer(HolsterTime), "timeout");
        Debugger.Instance.CreateDebugSphere(_handAttachment.GlobalPosition, 2f, 0.2f, new Color(0.4f, 0, 0));
        SetPosessionState(PosessionStateEnum.HOLSTERED);
        HolsterFinished.Invoke(Primary);
        Holstering = false;
    }
    private void SetPosessionState(PosessionStateEnum state)
    {
        PosessionState = state;
        if(PosessionState == PosessionStateEnum.UNPOSESSED)
        {
            Reparent(_handAttachment.GetTree().Root);
            Scale = CustomMath.Invert(Scale);
            _ownerCamNode3D = null;
            _handAttachment = null;
            Freeze = false;
            CollisionLayer = _collisionLayer;
        }
        else if(PosessionState == PosessionStateEnum.HOLSTERED)
        {
            GetParent().RemoveChild(this);
        }
        else if(PosessionState == PosessionStateEnum.EQUIPPED)
        {
            GetParent()?.RemoveChild(this);
            _handAttachment.AddChild(this);
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = CustomMath.Invert(_handAttachment.Scale);
            Freeze = true;
            CollisionLayer = 0;
        }
    }
    // Called every frame. Should contain all time-related logic that should happen
    // even when the item is hidden in inventory (not inside tree).
    public virtual void ProcessInternal(double delta){}
    public void TryUsePrimary(){
        if(!Busy && PosessionState == PosessionStateEnum.EQUIPPED) {
            UsePrimary();
        }
    }
    public void TryUseSecondary(){
        if(!Busy && PosessionState == PosessionStateEnum.EQUIPPED) {
            UseSecondary();
        }
    }
    protected virtual void UsePrimary(){}
    protected virtual void UseSecondary(){}
}