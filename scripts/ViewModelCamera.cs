using Godot;
using System;

public partial class ViewModelCamera : Camera3D
{
	private Camera3D mainCamera;
	public override void _Ready()
	{
		TopLevel = true;
	}

	public override void _Process(double delta)
	{
		mainCamera = GetTree().Root.GetCamera3D(); //BAD PERFORMANCE
		GlobalTransform = mainCamera.GlobalTransform;
	}
}
