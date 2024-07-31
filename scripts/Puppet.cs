using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class Puppet : RigidBody3D
{
	float Gravity {get;} = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	const float JumpVelocity = 6.0f;
	const float Speed = 10.0f;
	const int MaxCollideAndSlideBounces = 10;
	const int MaxNudgesOutsideColliders = 10;
	public const float ParallelismTolerance = 0.001f;
	const float SkinWidthMultiplier = 0.015f; // when moving to the surface of an object, puppet will be put inside of it by this much 

	[Export] public Node3D HorizontalDirAxis {get; set;}
	[Export] public Node3D VerticalDirAxis {get; set;}
	[Export] public Camera3D Camera {get; set;}
	[Export] public Node3D LinearMarker {get; set;}
	[Export] public Node3D ArrowMarker {get; set;}
	[Export] public Label DebugLabel {get; set;}

	Vector3 Velocity;
	SelfCaster3D SelfCaster;

	//DEBUG
	int timesMovedAway = 0;

    public override void _Ready()
    {
	//	MaxContactsReported = 1;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Freeze = true;
        SelfCaster = new SelfCaster3D(GetChild<CollisionShape3D>(0).Shape, this as CollisionObject3D);
		AddChild(SelfCaster);
    }

    public override void _Input(InputEvent @event)
    {
		if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
			VerticalDirAxis.RotateX(-mouseEvent.Relative.Y * Settings.Sensitivity);
			HorizontalDirAxis.RotateY(-mouseEvent.Relative.X * Settings.Sensitivity);
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
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			// rn friction has the classic quake problem
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed/1.2f);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed/1.2f);
		}
		return velocity;
	}
	Vector3 AdjustVelocityToGravity(Vector3 velocity, double delta)
	{
		return velocity + Gravity * new Vector3(0, -1, 0) * (float)delta;
	}
	// only supports convex colliders
	Vector3 CollideAndSlideIteration(Vector3 fromGlobal, Vector3 translation, List<GodotObject> collisionExceptions, int depth)
	{
		if(depth > MaxCollideAndSlideBounces || translation == Vector3.Zero)
		{
			// reached max depth or we are not moving
			//GD.Print("reached end at " + Time.GetTicksMsec() + " threw out " + translation.Length() + "m with "
			//		 + timesMovedAway + " bounces away from plane");
			return translation;
		}

		if(SelfCaster.CastExcept(collisionExceptions, fromGlobal, translation))
		{
			// collision along path
			float firstCastFraction = SelfCaster.GetClosestCollisionSafeFraction();
			Vector3 firstCastNormal = SelfCaster.GetCollisionNormal(0);
			if(SelfCaster.CastExcept(collisionExceptions, fromGlobal, Vector3.Zero))
			{
				// we are inside something
				List<Vector3> intersectingObjectNormals = new();
				List<GodotObject> intersectingObjects = new();
				for(int i = 0; i < SelfCaster.GetCollisionCount(); i++)
				{
					Vector3 normal = SelfCaster.GetCollisionNormal(i);
					if(normal.Dot(translation) <= ParallelismTolerance)
					{
						intersectingObjectNormals.Add(normal);
						intersectingObjects.Add(SelfCaster.GetCollider(i));
					}
				}

				if(intersectingObjectNormals.Count == 0)
				{
					// and moving away or along it
					// ignore it and move
					(Vector3 toSurface, Vector3 projected) = CustomMath.MoveAndSlide(translation, firstCastNormal, firstCastFraction);
					return toSurface + CollideAndSlideIteration(fromGlobal + toSurface, projected, new List<GodotObject>(), depth + 1);
				}
				else
				{
					// and moving towards it
					// collide with it and recurse 

					Vector3 projected = CustomMath.SlideAlongMultiple(translation, intersectingObjectNormals, ParallelismTolerance);
					return CollideAndSlideIteration(fromGlobal, projected, new List<GodotObject>(), depth + 1);
				}
			}
			else
			{
				// we are not inside anything
				// collide with first object and recurse
				(Vector3 toSurface, Vector3 projected) = CustomMath.MoveAndSlide(translation, firstCastNormal, firstCastFraction);
				return toSurface + CollideAndSlideIteration(fromGlobal + toSurface, projected, new List<GodotObject>(), depth + 1);
			}
		}
		else
		{
			// no obstacles
			return translation;
		}
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
		translation = CollideAndSlideIteration(GlobalPosition, translation, new List<GodotObject>(), 1);
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
// SOME BULLSHIT WITH DELTATIME MULTIPLICATION: DO IT PROPERLY, MULTIPLY VELOCITY FIRST AND THEN ALTER THE SPEED.