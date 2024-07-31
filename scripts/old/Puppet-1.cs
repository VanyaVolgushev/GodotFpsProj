/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class Puppet : RigidBody3D
{
	float Gravity {get;} = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	const float JumpVelocity = 7.0f;
	const float Speed = 7.0f;
	const int MaxCollideAndSlideBounces = 20;
	const int MaxNudgesOutsideColliders = 10;
	const float SkinWidthMultiplier = 0.015f; // when moving to the surface of an object, puppet will be put inside of it by this much 

	[Export] public Node3D HorizontalDirAxis {get; set;}
	[Export] public Node3D VerticalDirAxis {get; set;}
	[Export] public Camera3D Camera {get; set;}
	[Export] public Node3D LinearMarker {get; set;}
	[Export] public Node3D ArrowMarker {get; set;}

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
	static (Vector3 toSurface, Vector3 projectedLeftOver) Slide(Vector3 translation, Vector3 collisionNormal, float collisionSafeFraction)
	{
		Vector3 leftOverTranslation = translation * (1 - collisionSafeFraction);
		Vector3 translationToSurface = translation * collisionSafeFraction;
		Vector3 projectedLeftOverTranslation = leftOverTranslation -
											   leftOverTranslation.Dot(collisionNormal) * collisionNormal;
		return (translationToSurface +  translationToSurface.Normalized() * SkinWidthMultiplier, projectedLeftOverTranslation);
	}
	// only supports convex colliders
	Vector3 CollideAndSlideTranslation(Vector3 fromGlobal, Vector3 translation, List<GodotObject> collisionExceptions, int depth)
	{
		if(depth > MaxCollideAndSlideBounces || translation == Vector3.Zero)
		{
			// reached max depth or we are not moving
			GD.Print("reached end at " + Time.GetTicksMsec() + " threw out " + translation.Length() + "m with "
					 + timesMovedAway + " bounces away from plane");
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
				List<GodotObject> overlappingObjects = new();
				//for(int i = 0; i < SelfCaster.GetCollisionCount(); i++)
				//{
					overlappingObjects.Add(SelfCaster.GetCollider(0));
				//}

				//for(int i = 0; i < SelfCaster.GetCollisionCount(); i++)
				//{

				//}
				if(SelfCaster.GetCollisionNormal(0).Dot(translation) >= -0.001f)
				{
					// but we are moving away from it
					// try again but ignore the thing we are inside of
					timesMovedAway++;
					collisionExceptions.AddRange(overlappingObjects);
					return Vector3.Zero + CollideAndSlideTranslation(fromGlobal, translation, collisionExceptions, depth + 1);
				}
				else
				{
					// and we are moving towards it
					//if()
					// collide with it and recurse 
					collisionExceptions = new List<GodotObject>();
					collisionExceptions.AddRange(overlappingObjects);
					(Vector3 toSurface, Vector3 projected) = Slide(translation, SelfCaster.GetCollisionNormal(0), 0);
					return toSurface + CollideAndSlideTranslation(fromGlobal + toSurface, projected, collisionExceptions, depth + 1);
				}
			}
			else
			{
				// we are not inside anything
				// collide with first object and recurse
				(Vector3 toSurface, Vector3 projected) = Slide(translation, firstCastNormal, firstCastFraction);
				return toSurface + CollideAndSlideTranslation(fromGlobal + toSurface, projected, new List<GodotObject>(), depth + 1);
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
		Velocity = AdjustVelocityToInput(Velocity, delta);
		Velocity = AdjustVelocityToGravity(Velocity, delta);
		Vector3 translation = Velocity * (float)delta;
		timesMovedAway = 0;
		translation = CollideAndSlideTranslation(GlobalPosition, translation, new List<GodotObject>(), 1);
		Position += translation;
		Velocity = translation / (float)delta;
		//int depth = 0;
		//while(SelfCaster.ZeroCast() && depth < MaxNudgesOutsideColliders)
		//{
		//	Position = NudgeOutsideColliders(Position);
		//	depth++;
		//}
	}
}
// SOME BULLSHIT WITH DELTATIME MULTIPLICATION: DO IT PROPERLY, MULTIPLY VELOCITY FIRST AND THEN ALTER THE SPEED. */