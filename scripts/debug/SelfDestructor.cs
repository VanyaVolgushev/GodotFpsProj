using Godot;
using System;

public partial class SelfDestructor : Node3D
{
	float timer;
	const float duration = 1.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta;
		if(timer > duration)
		{
			QueueFree();
		}
	}
}
