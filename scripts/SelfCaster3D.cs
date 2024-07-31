using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public partial class SelfCaster3D : ShapeCast3D
{
	private CollisionObject3D CollisionObject3D;
	public SelfCaster3D (Shape3D myShape, CollisionObject3D myCollisionObject) : base()
	{
		Shape = myShape;
		Enabled = false;
		CollisionObject3D = myCollisionObject;
		AddException(CollisionObject3D);
	}
    public bool Cast(Vector3 fromGlobal, Vector3 toLocal)
	{
		//uses local coordinates for to and global for from
		GlobalPosition = fromGlobal;
		TargetPosition = toLocal;
		ForceShapecastUpdate();
		return IsColliding();
	}
	public bool Cast(Vector3 toLocal)
	{
		Position = Vector3.Zero;
		TargetPosition = toLocal;
		ForceShapecastUpdate();
		return IsColliding();
	}
	public bool ZeroCast()
	{
		//basically an overlap check
		Position = Vector3.Zero;
		TargetPosition = Vector3.Zero;
		ForceShapecastUpdate();
		return IsColliding();
	}
	public bool CastExcept(List<GodotObject> exceptions, Vector3 fromGlobal, Vector3 toLocal)
	{
		foreach(GodotObject o in exceptions)
		{
			AddException(o as CollisionObject3D);
		}
		bool collided = Cast(fromGlobal, toLocal);
		ClearExceptions();
		AddException(CollisionObject3D);
		return collided;
	}
}