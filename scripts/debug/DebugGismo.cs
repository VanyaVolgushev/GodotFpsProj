using Godot;
using System;

public partial class DebugGismo : Node3D
{
	float timer = 0.0f;
	float duration = 1.0f;
	bool wasGivenColor = false;
	StandardMaterial3D myMaterial;
	public override void _Process(double delta)
	{
		timer += (float)delta;
		if(timer > duration)
		{
			QueueFree();
		}
	}
	public void SetColor(Color color)
	{
		if(!wasGivenColor)
		{
			myMaterial = (GetChild(0) as MeshInstance3D).GetActiveMaterial(0) as StandardMaterial3D;
			myMaterial = (StandardMaterial3D)myMaterial.Duplicate();
			wasGivenColor = true;
		}
		myMaterial.AlbedoColor = color;
	}
	public void SetDuration(float seconds)
	{
		duration = seconds;
	}
	public void MultiplyScale(float scaleMult)
	{
		Scale *= scaleMult;
	}
}
