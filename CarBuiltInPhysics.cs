using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public partial class CarBuiltInPhysics : CharacterBody3D
{
	public const float Speed = 1.0f;
	public const float StrafeSpeed = 0.5f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = 1.0f/4;
	public List<RayCast3D> rc3d = new List<RayCast3D>();
	//public MeshInstance3D mesh = new MeshInstance3D();
	public float friction = 0.125f/10;
	public float drag = 1 + 0.125f/10;
	public float groundDist = 1f;
	public float rotateChange = 0;
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
					dif = ray.GlobalPosition.DistanceTo(Position);
					look = ray.GlobalPosition.DirectionTo(Position);
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
		Vector3 vel = Velocity;
		float rotation = Input.GetActionStrength("Left") - Input.GetActionStrength("Right");
		float strafe = Input.GetActionStrength("StrafeLeft") - Input.GetActionStrength("StrafeRight");
		// Snap the machine to the track;
		SnapToTrack();
		
			vel += GlobalTransform.Basis.Z * Speed *  Input.GetActionStrength("Forward") ;
			vel += GlobalTransform.Basis.X * StrafeSpeed *  strafe ;
		//ApplyForce(GlobalTransform.Basis.Z * 60);
		
		if (Input.IsActionPressed("Brake"))
		{
			//DoBrake
				
		}

		rotateChange = 0.05f*Mathf.Clamp(rotation,-1,1);//0;


		/*if (Input.IsActionPressed("Left"))
			rotateChange = 3f * (float)delta;
			
		if (Input.IsActionPressed("Right"))
			rotateChange = -3f * (float)delta;*/
		
		Rotate(GlobalTransform.Basis.Y, rotateChange); // ApplyTorque(GlobalTransform.Basis.Y * 10);
		
		if (!IsRaycastColliding())
			vel += new Vector3(0, -1 * gravity, 0);
		else
		{
			vel -= GlobalTransform.Basis.Y*GlobalTransform.Basis.Y.Dot(vel);
			vel += vel * (-1 * friction);
		}

		Velocity = vel;
		// Get updated ray cast information
		MoveAndSlide();
	}
}
