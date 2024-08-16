using Godot;
using System;

public partial class ViewModelContainer : SubViewportContainer
{
	SubViewport SubViewport; 
	AnimationTree ViewmodelAnimationTree;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SubViewport = GetChild<SubViewport>(0);
		SubViewport.Size = DisplayServer.WindowGetSize();
		//ViewmodelAnimationTree = GetNode("SubViewport/ViewModelCamera/viewmodel/AnimationTree") as AnimationTree;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
