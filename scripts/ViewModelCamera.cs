using Godot;
using System;

public partial class ViewModelCamera : Camera3D
{
	private Camera3D mainCamera;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TopLevel = true;
		mainCamera = GetTree().Root.GetCamera3D();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Transform = mainCamera.GlobalTransform;
	}
}
