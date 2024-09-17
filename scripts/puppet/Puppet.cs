using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public partial class Puppet : RigidBody3D
{
	public Vector3 Velocity { get { return _Velocity; } }
	public event Inventory.ItemlistChangedAction OnInventoryChanged;
	const float Sensitivity = 0.0012f;
	const float JumpVelocity = 6.0f;
	const float Speed = 7.0f;
	const float SprintMultiplier = 3f;
	const float PickupRange = 3.0f;
	float _Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	[Export] Node3D HorizontalDirAxis;
	[Export] Node3D VerticalDirAxis;
	[Export] Camera3D Camera;
	Inventory _Inventory;
	PuppetAnimationHandler _AnimationHandler;
	BoneAttachment3D _HandAttachment;
	Vector3 _Velocity;
	SelfCaster3D _SelfCaster;
	ViewModel _ViewModel;
	Random _Random;
    public override void _Ready()
    {
		Camera.MakeCurrent();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_Random = new Random();
		Freeze = true;
		UIHandler.Instance.SetControlledPuppet(this);
        _SelfCaster = new SelfCaster3D(GetNode<CollisionShape3D>("CollisionShape3D").Shape, this as CollisionObject3D);
		AddChild(_SelfCaster);
		_Inventory = new Inventory(FindChild("ViewModelCamera").FindChild("RightArmAttachment3D") as BoneAttachment3D, Camera);
		AddChild(_Inventory);
		_Inventory.ItemlistChanged += OnInventoryChanged;
    }
	public override void _Process(double delta)
    {
        if (Input.IsActionPressed("fov-"))
			Camera.Fov -= (float)delta * 20f;
		if (Input.IsActionPressed("fov+"))
			Camera.Fov += (float)delta * 20f;
		if (Input.IsActionPressed("viewmodel_fov-")) {
		    //TODO
		}
		if (Input.IsActionPressed("viewmodel_fov+")) {
			//TODO
		}
		if (Input.IsActionJustPressed("drop_item")) {
			_Inventory.TryDrop();
		}
		if (Input.IsActionJustPressed("select_item_1")) {
			_Inventory.SetTarget(0);
		}
		if (Input.IsActionJustPressed("select_item_2")) {
			_Inventory.SetTarget(1);
		}
		if (Input.IsActionJustPressed("select_item_3")) {
			_Inventory.SetTarget(2);
		}
		if (Input.IsActionJustPressed("select_item_4")) {
			_Inventory.SetTarget(3);
		}
		if (Input.IsActionJustPressed("select_item_5")) {
			_Inventory.SetTarget(4);
		}
		if (Input.IsActionJustPressed("select_item_6")) {
			_Inventory.SetTarget(5);
		}
		if (Input.IsActionJustPressed("select_item_7")) {
			_Inventory.SetTarget(6);
		}
		if (Input.IsActionJustPressed("select_item_8")) {
			_Inventory.SetTarget(7);
		}
		if (Input.IsActionJustPressed("select_item_9")) {
			_Inventory.SetTarget(8);
		}
    }
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("interact"))
		{
			(CollisionObject3D collisionObject3D, Vector3 position, Vector3 normal) = RaycastFromCamera(PickupRange);
			if (collisionObject3D is not null)
			{
				_Inventory.TryAdd(collisionObject3D);
			}
		}
		if (Input.IsActionJustPressed("secondary_action"))
		{
			_Inventory.TryUseSecondary();
		}
		if (Input.IsActionJustPressed("primary_action"))
		{
			_Inventory.TryUsePrimary();
		}

		_Velocity = AdjustVelocityToInput(_Velocity, delta);
		_Velocity = AdjustVelocityToGravity(_Velocity, delta);
		Vector3 translation = _Velocity * (float)delta;
		translation = CollideAndSlide(GlobalPosition, translation);
		Position += translation;
		_Velocity = translation / (float)delta;
		Debugger.Instance.SetProperty("Speed", _Velocity.Length(), "m/s", 2);

		//while(SelfCaster.ZeroCast() && depth < MaxNudgesOutsideColliders)
		//{
		//	Position = NudgeOutsideColliders(Position);
		//	depth++;
		//}
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
		return velocity + _Gravity * new Vector3(0, -1, 0) * (float)delta;
	}
	// only supports convex colliders
	Vector3 CollideAndSlide(Vector3 fromGlobal, Vector3 translation)
	{
		List<GodotObject> handledColliders = new List<GodotObject>();
		List<Vector3> handledNormals = new List<Vector3>();		
		Vector3 leftOverTranslation = translation;
		Vector3 newPos = fromGlobal;
		while(_SelfCaster.CastExcept(handledColliders, newPos, leftOverTranslation))
		{
			float firstCastFraction = _SelfCaster.GetClosestCollisionSafeFraction();
			Vector3 firstCastNormal = _SelfCaster.GetCollisionNormal(0);
			GodotObject firstCastObject = _SelfCaster.GetCollider(0);
			//collision along path
			if(_SelfCaster.CastExcept(handledColliders, newPos, Vector3.Zero))
			{
				// we are inside something
				List<GodotObject> overlappingObjects = new();
				List<Vector3> overlappingNormals = new();
				for(int i = 0; i < _SelfCaster.GetCollisionCount(); i++)
				{
					overlappingObjects.Add(_SelfCaster.GetCollider(i));
					overlappingNormals.Add(_SelfCaster.GetCollisionNormal(i));
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
		position += _SelfCaster.GetCollisionNormal(0) * 0.01f;
		return position;
	}
	(CollisionObject3D collisionObject3D, Vector3 position, Vector3 normal) RaycastFromCamera(float distance)
	{
		var spaceState = GetWorld3D().DirectSpaceState;
    	var query = PhysicsRayQueryParameters3D.Create(Camera.GlobalPosition, Camera.GlobalPosition - Camera.GlobalBasis.Z * distance);
    	var result = spaceState.IntersectRay(query);

		if (result.ContainsKey("position"))
		{
			Debugger.Instance.CreateDebugArrow((Vector3)result["position"], (Vector3)result["normal"], 0.5f, 0.3f, new Color(1, 0, 0));
			return ((CollisionObject3D)result["collider"], (Vector3)result["position"], (Vector3)result["normal"]);
		}
		else
		{
			return (null, Vector3.Zero, Vector3.Zero);
		}
	}
}