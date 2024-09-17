using System;

public struct InventoryState
{
    public (int primary, int secondary) ActiveItems = (-1, -1); //-1 means no item
    public (int primary, int secondary) TargetItems = (-1, -1); //-1 means no item
    public enum HandState {HOLSTERING, TAKING_OUT, HOLDING, FREE, ASSISTING}
    public HandState PrimaryHandState = HandState.FREE;
    public HandState SecondaryHandState = HandState.FREE;
    public bool PrimaryBusy => PrimaryHandState == HandState.HOLDING || PrimaryHandState == HandState.FREE || PrimaryHandState == HandState.ASSISTING;
    public bool SecondaryBusy => SecondaryHandState == HandState.HOLDING || SecondaryHandState == HandState.FREE || SecondaryHandState == HandState.ASSISTING;
    
    public InventoryState()
    {}
}