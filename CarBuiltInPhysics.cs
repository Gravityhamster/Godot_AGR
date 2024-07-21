using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public partial class CarBuiltInPhysics : CharacterBody3D
{
	[Export]
	public float Speed = 1.0f;
	[Export]
	public float StrafeSpeed = 0.5f;
	[Export]
	public float JumpVelocity = 4.5f;
	[Export]
	public float BrakeStrength = 1/48f;
	[Export]
	public float PitchSpeed = 3f;
	public float pitchChange = 0;
	[Export]
	public float RollSpeed = 3f;
	public float rollChange = 0;
	[Export]
	public float RotateSpeed = 3f;
	public float rotateChange = 0;
	public float WasYVel = 0;
	[Export]
	public float gravity = 1.0f/4;
	public List<RayCast3D> rc3d = new List<RayCast3D>();
	//public MeshInstance3D mesh = new MeshInstance3D();
	[Export]
	public float friction = 0.125f/10;
	[Export]
	public float groundDist = 1f;
	public MeshInstance3D mesh;

	// https://kidscancode.org/godot_recipes/3.x/3d/3d_align_surface/
	public Transform3D AlignWithY(Transform3D xfrom, Vector3 newy)
	{
		xfrom.Basis.Y = newy;
		xfrom.Basis.X = -xfrom.Basis.Z.Cross(newy);
		xfrom.Basis = xfrom.Basis.Orthonormalized();
		return(xfrom);
	}

	public void SnapToTrack()
	{
		/*sphere.GlobalPosition = rc3d[0].GlobalPosition - GlobalTransform.Basis.Y * groundDist;
		sphere = GetNode<MeshInstance3D>("MeshInstance3D3");
		sphere.GlobalPosition = rc3d[1].GlobalPosition - GlobalTransform.Basis.Y * groundDist;
		sphere = GetNode<MeshInstance3D>("MeshInstance3D4");
		sphere.GlobalPosition = rc3d[2].GlobalPosition - GlobalTransform.Basis.Y * groundDist;
		sphere = GetNode<MeshInstance3D>("MeshInstance3D5");
		sphere.GlobalPosition = rc3d[3].GlobalPosition - GlobalTransform.Basis.Y * groundDist;*/

		//var dif = rc3d[0].Position.DistanceTo(Position);
		//var look = rc3d[0].Position.DirectionTo(Position);
		Vector3 n = new Vector3(0,0,0);
		var count = 0;
		Vector3? np = null; // new Vector3(0,0,0);
		float dif = 0;
		Vector3 look = new Vector3(0,0,0);
		foreach (var ray in rc3d)
		{
			ray.ForceRaycastUpdate();
			if (ray.IsColliding())
			{
				n += ray.GetCollisionNormal();
				count += 1;
			}
			/*
				Position = new Vector3(rc3d.GetCollisionPoint().X + (rc3d.GetCollisionNormal().X * groundDist),
									   rc3d.GetCollisionPoint().Y + (rc3d.GetCollisionNormal().Y * groundDist),
									   rc3d.GetCollisionPoint().Z + (rc3d.GetCollisionNormal().Z * groundDist));
				GlobalTransform = AlignWithY(GlobalTransform, rc3d.GetCollisionNormal());
			*/
		}
		if (IsRaycastColliding())
		{
			n /= count;
			n = n.Normalized();
			var xform = AlignWithY(GlobalTransform, n);
			GlobalTransform = xform; //GlobalTransform.InterpolateWith(xform, 0.1f);
			
			foreach (var ray in rc3d)
			{
				ray.ForceRaycastUpdate();
				if (ray.IsColliding())
				{
					np = ray.GetCollisionPoint();
					dif = ray.GlobalPosition.DistanceTo(GlobalPosition);
					look = ray.GlobalPosition.DirectionTo(GlobalPosition);
					break;
				}
			}
			// sphere = GetNode<MeshInstance3D>("MeshInstance3D2");
			// sphere.GlobalPosition = (Vector3)np + look * dif + n*groundDist; // rc3d[0].GlobalPosition - GlobalTransform.Basis.Y * groundDist;

			if (np != null)
				GlobalPosition = (Vector3)np + look * dif + n*groundDist;

			// LinearVelocity -= n*n.Dot(LinearVelocity);

			/*GlobalPosition = new Vector3(((Vector3)np).X + (n.X * groundDist),
								   ((Vector3)np).Y + (n.Y * groundDist),
								   ((Vector3)np).Z + (n.Z * groundDist));
			GlobalPosition += look * dif;*/
			
		}
	}

    public override void _Ready()
    {
		foreach (var child in GetChildren().OfType<RayCast3D>())
			rc3d.Add(child);
    }

	public bool IsRaycastColliding()
	{
		foreach (var ray in rc3d)
			if (ray.IsColliding())
				return true;
		return false;
	}

    public override void _PhysicsProcess(double delta)
	{
		// Create temp velocity vector
		Vector3 vel = Velocity;

		// Get inputs
		float rotation = Input.GetActionStrength("Left") - Input.GetActionStrength("Right");
		float pitch = Input.GetActionStrength("PitchUp") - Input.GetActionStrength("PitchDown");
		float roll = Input.GetActionStrength("RollLeft") - Input.GetActionStrength("RollRight");
		float strafe = Input.GetActionStrength("StrafeLeft") - Input.GetActionStrength("StrafeRight");

		// Snap the machine to the track;
		SnapToTrack();
		
		// Get movement forces
		vel += GlobalTransform.Basis.Z * Speed *  Input.GetActionStrength("Forward") ;
		vel += GlobalTransform.Basis.X * StrafeSpeed *  strafe ;
		
		// Brakes (Only on ground)
		if (IsRaycastColliding())
		{
			//DoBrake
			vel -= vel*BrakeStrength*Input.GetActionStrength("Brake"); // GlobalTransform.Basis.Z*GlobalTransform.Basis.Z.Dot(vel)*BrakeStrength;
		}

		// Get turning
		rotateChange = 0;
		pitchChange = 0;
		rollChange = 0;
		rotateChange = RotateSpeed*Mathf.Clamp(rotation,-1,1)*(float)delta;//0;
		if (!IsRaycastColliding()) pitchChange = PitchSpeed*Mathf.Clamp(pitch,-1,1)*(float)delta;
		if (!IsRaycastColliding()) rollChange = -RollSpeed*Mathf.Clamp(roll,-1,1)*(float)delta;
		
		// Apply rotation
		Rotate(GlobalTransform.Basis.Y, rotateChange);
		Rotate(GlobalTransform.Basis.X, pitchChange);
		Rotate(GlobalTransform.Basis.Z, rollChange);
		
		// Handle passive forces
		if (!IsRaycastColliding())
			vel += new Vector3(0, -1 * gravity, 0);
		else
		{
			WasYVel = GlobalTransform.Basis.Y.Dot(vel);
			vel -= GlobalTransform.Basis.Y*GlobalTransform.Basis.Y.Dot(vel); // Cancel downward movement
			vel += vel * (-1 * friction); // Friction force
		}

		// Apply velocity
		Velocity = vel;

		// Get updated ray cast information
		MoveAndSlide();
	}
}
