using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


public partial class CarBuiltInPhysicsBall : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = 1; // ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public ShapeCast3D rc3d;
	//public MeshInstance3D mesh = new MeshInstance3D();
	public float friction = .125f/10;
	public float drag = 1 + .125f/10;
	public float groundDist = 1f;

	// https://kidscancode.org/godot_recipes/3.x/3d/3d_align_surface/
	public Transform3D AlignWithY(Transform3D xfrom, Vector3 newy)
	{
		xfrom.Basis.Y = newy;
		xfrom.Basis.X = -xfrom.Basis.Z.Cross(newy);
		xfrom.Basis = xfrom.Basis.Orthonormalized();
		return xfrom;
	}

	public void SnapToTrack()
	{
		rc3d.ForceShapecastUpdate();

		Vector3 n = new Vector3(0,0,0);
		var count = rc3d.GetCollisionCount();
		Vector3? np = new Vector3(0,0,0);
		// float dif = 0;
		// Vector3 look = new Vector3(0,0,0);

		// Get collision normal
		if (count > 0)
		{
			var index = 0;
			while (index < rc3d.GetCollisionCount())
			{
				n += rc3d.GetCollisionNormal(index);
				index++;
			}
		}

		// Snap to the track
		if (count > 0)
		{
			n /= count;
			n = n.Normalized();
			var xform = AlignWithY(GlobalTransform, n);
			GlobalTransform = GlobalTransform.InterpolateWith(xform, 0.1f);
			
			// Get total collision points
			var index = 0;
			//while (index < rc3d.GetCollisionCount())
			//{
				np = rc3d.GetCollisionPoint(index);
				//index++;
			//}

			//// Average the points
			//np /= count;

			/*foreach (var ray in rc3d)
			{
				ray.ForceRaycastUpdate();
				if (ray.IsColliding())
				{
					np = ray.GetCollisionPoint();
					dif = ray.GlobalPosition.DistanceTo(Position);
					look = ray.GlobalPosition.DirectionTo(Position);
					break;
				}
			}*/

			GlobalPosition = (Vector3)np + n*groundDist;
			
			// Velocity -= n*n.Dot(Velocity);
		}
	}

    public override void _Ready()
    {
		rc3d = GetNode<ShapeCast3D>("ShapeCast3D");
    }

	public bool IsRaycastColliding()
	{
		if (rc3d.GetCollisionCount() > 0)
			return true;
		return false;
	}

    public override void _PhysicsProcess(double delta)
	{

		// Snap the machine to the track;
		SnapToTrack();

		if (Input.IsActionPressed("Forward"))
			Velocity += GlobalTransform.Basis.Z * 2;
			//ApplyForce(GlobalTransform.Basis.Z * 60);
			
		if (Input.IsActionPressed("Left"))
			Rotate(GlobalTransform.Basis.Y, 1f/20); // ApplyTorque(GlobalTransform.Basis.Y * 10);
			
		if (Input.IsActionPressed("Right"))
			Rotate(GlobalTransform.Basis.Y, -1f/20); // ApplyTorque(GlobalTransform.Basis.Y * -10);

		if (!IsRaycastColliding())
			Velocity += new Vector3(0, -1 * gravity, 0);
		else
		{
			Velocity -= GlobalTransform.Basis.Y*GlobalTransform.Basis.Y.Dot(Velocity);
			Velocity += Velocity * (-1 * friction);
		}

		// Get updated ray cast information
		MoveAndSlide();
	}
}
