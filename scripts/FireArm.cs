using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class FireArm : InventoryItem
{
    protected override void UsePrimary()
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        Debugger.Instance.SetProperty("FireArmOwnerPos", _ownerCamNode3D.GlobalPosition.ToString());
    	var query = PhysicsRayQueryParameters3D.Create(_ownerCamNode3D.GlobalPosition, _ownerCamNode3D.GlobalPosition - _ownerCamNode3D.GlobalBasis.Z * 1000f);
    	var result = spaceState.IntersectRay(query);
		if (result.ContainsKey("position"))
		{
			Debugger.Instance.CreateDebugArrow((Vector3)result["position"], (Vector3)result["normal"], 0.5f, 0.3f, new Color(1, 0, 0));
		}
    }

    protected override void UseSecondary()
    {
    }
}