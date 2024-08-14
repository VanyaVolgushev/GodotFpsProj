using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class Puppet : RigidBody3D
{
	float Gravity {get;} = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public const float Sensitivity = 0.002f;
	const float JumpVelocity = 6.0f;
	const float Speed = 7.0f;
	const float SprintMultiplier = 3f;
	public const float ParallelismTolerance = 0; // if sin of angles between two vectors is less than this, they are considered parallel
	public const float TranslationCutoff = 1e-10f; // if velocity is less than this, puppet will not move
	[Export] public Node3D HorizontalDirAxis {get; set;}
	[Export] public Node3D VerticalDirAxis {get; set;}
	[Export] public Camera3D Camera {get; set;}
	Vector3 Velocity;
	SelfCaster3D SelfCaster;
	//	DEBUG
	[Export] public Label DebugLabel {get; set;}
	private PackedScene LinearMarker {get; set;}
	private PackedScene ArrowMarker {get; set;}
	int timesMovedAway = 0;

    public override void _Ready()
    {
	//	MaxContactsReported = 1;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Freeze = true;
        SelfCaster = new SelfCaster3D(GetChild<CollisionShape3D>(0).Shape, this as CollisionObject3D);
		AddChild(SelfCaster);
		ArrowMarker = (PackedScene)GD.Load("res://scenes/debug/MarkerStick.tscn");
		LinearMarker = (PackedScene)GD.Load("res://scenes/debug/MarkerSphere.tscn");
		// to instantiate marker:
        //	var marker = ArrowMarker.Instantiate();
		//	AddSibling(marker);
    }

    public override void _Input(InputEvent @event)
    {
		if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
			VerticalDirAxis.RotateX(-mouseEvent.Relative.Y * Sensitivity);
			HorizontalDirAxis.RotateY(-mouseEvent.Relative.X * Sensitivity);
		}
    }
	Vector3 AdjustVelocityToInput(Vector3 velocity, double delta)
	{
		if (Input.IsActionJustPressed("jump"))
			velocity.Y = JumpVelocity;
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		Vector3 direction = HorizontalDirAxis.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y);
		if (direction != Vector3.Zero)
		{
			var sprintMult = 1f;
			if(Input.IsActionPressed("sprint"))
			{
				sprintMult = SprintMultiplier;
			}
			velocity.X = direction.X * Speed * sprintMult;
			velocity.Z = direction.Z * Speed * sprintMult;
		}
		else
		{
			Vector3 newVelocity = velocity;
			newVelocity.Y = 0;
			newVelocity = newVelocity.Normalized() * Mathf.MoveToward(newVelocity.Length(), 0, Speed/20f);
			velocity.X = newVelocity.X;
			velocity.Z = newVelocity.Z;
		}
		return velocity;
	}

	Vector3 AdjustVelocityToGravity(Vector3 velocity, double delta)
	{
		return velocity + Gravity * new Vector3(0, -1, 0) * (float)delta;
	}
	// only supports convex colliders
	Vector3 CollideAndSlide(Vector3 fromGlobal, Vector3 translation)
	{
		List<GodotObject> handledColliders = new List<GodotObject>();
		List<Vector3> handledNormals = new List<Vector3>();		
		Vector3 leftOverTranslation = translation;
		Vector3 newPos = fromGlobal;
		while(SelfCaster.CastExcept(handledColliders, newPos, leftOverTranslation))
		{
			float firstCastFraction = SelfCaster.GetClosestCollisionSafeFraction();
			Vector3 firstCastNormal = SelfCaster.GetCollisionNormal(0);
			GodotObject firstCastObject = SelfCaster.GetCollider(0);
			//collision along path
			if(SelfCaster.CastExcept(handledColliders, newPos, Vector3.Zero))
			{
				// we are inside something
				List<GodotObject> overlappingObjects = new();
				List<Vector3> overlappingNormals = new();
				for(int i = 0; i < SelfCaster.GetCollisionCount(); i++)
				{
					overlappingObjects.Add(SelfCaster.GetCollider(i));
					overlappingNormals.Add(SelfCaster.GetCollisionNormal(i));
				}
				handledColliders.AddRange(overlappingObjects);
				handledNormals.AddRange(overlappingNormals);
				Vector3 sliding = CustomMath.SlideAlongMultiple(leftOverTranslation, translation, handledNormals);
				leftOverTranslation = sliding;
			}
			else
			{
				// not inside anything
				handledColliders.Add(firstCastObject);
				handledNormals.Add(firstCastNormal);
				newPos += leftOverTranslation * firstCastFraction;
				Vector3 sliding = CustomMath.SlideAlongMultiple(leftOverTranslation * (1 - firstCastFraction), translation, handledNormals);
				leftOverTranslation = sliding;
			}
		}
		return newPos + leftOverTranslation - fromGlobal;
	}
	Vector3 NudgeOutsideColliders(Vector3 position)
	{
		//TODO calculate how much to move later
		position += SelfCaster.GetCollisionNormal(0) * 0.01f;
		return position;
	}
    public override void _PhysicsProcess(double delta)
	{
		if(DebugLabel != null){DebugLabel.Text = "";}
		Velocity = AdjustVelocityToInput(Velocity, delta);
		Velocity = AdjustVelocityToGravity(Velocity, delta);
		Vector3 translation = Velocity * (float)delta;
		timesMovedAway = 0;
		translation = CollideAndSlide(GlobalPosition, translation);
		Position += translation;
		Velocity = translation / (float)delta;

		//Debug
		var VelocityString = (Mathf.Round(Velocity.Length() * 100)/100).ToString();
		if(DebugLabel != null){DebugLabel.Text += "\nSpeed = " + VelocityString.Substring(0, Math.Min(VelocityString.Length, 3)) + "m/s";}
		//int depth = 0;
		//while(SelfCaster.ZeroCast() && depth < MaxNudgesOutsideColliders)
		//{
		//	Position = NudgeOutsideColliders(Position);
		//	depth++;
		//}
	}
}