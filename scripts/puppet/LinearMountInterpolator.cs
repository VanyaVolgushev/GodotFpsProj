using Godot;
using System;

public partial class LinearMountInterpolator : Node3D
{
	[Export]
	public Node3D TargetNode {get; set;}

	Transform3D OldTargetTransform;
	Transform3D NewTargetTransform;
	bool Updated;
	public override void _Ready()
	{
		TopLevel = true;

		OldTargetTransform = TargetNode.GlobalTransform;
		NewTargetTransform = TargetNode.GlobalTransform;
		Transform = TargetNode.GlobalTransform;
	}
    public override void _PhysicsProcess(double delta)
    {
		OldTargetTransform = NewTargetTransform;
		NewTargetTransform = TargetNode.GlobalTransform;
		Updated = true;
    }
    public override void _Process(double delta)
	{
		//possible problem with cast?
		float f = (float)Mathf.Clamp(Engine.GetPhysicsInterpolationFraction(), 0, 1);
		GlobalTransform = OldTargetTransform.InterpolateWith(NewTargetTransform, f);
	}
}
