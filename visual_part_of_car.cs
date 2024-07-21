using Godot;
using System;

public partial class visual_part_of_car : MeshInstance3D
{
	[Export]
	public CarBuiltInPhysics mycar = null;
	public float VerticalDisplacementHard = 0;
	public float VerticalDisplacementVelocity = 0;
	public float DisplacementDampen = 1/1.0625f;
	public float VerticalDisplacementSoft = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// /* old stable rotation code: 
		Transform3D xform = mycar.GlobalTransform;
		xform = xform.Rotated(xform.Basis.X, (float)(90 * (Math.PI / 180)));//Rotate(GlobalTransform.Basis.X, (float)(90 * (Math.PI / 180)));
		GlobalTransform = xform;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// /* old stable rotation code: 
		Transform3D xform = mycar.GlobalTransform;
		xform = xform.Rotated(xform.Basis.X, (float)(90 * (Math.PI / 180)));//Rotate(GlobalTransform.Basis.X, (float)(90 * (Math.PI / 180)));
		GlobalTransform = GlobalTransform.InterpolateWith(xform, 0.06f); //(float)(5 * delta)
		// */
		//GlobalTransform = GlobalTransform.Rotated(xform.Basis.Z, GlobalTransform.Basis.Y.Dot(xform.Basis.Z));
		//GlobalTransform = GlobalTransform.Rotated(xform.Basis.X, GlobalTransform.Basis.X.Dot(xform.Basis.X));

		//for (var i = 0; i < 100000000f; i++)
			//continue;

		// Set the position of the model
		GlobalPosition = mycar.GlobalPosition;

		// Suspension physics - Vertical_Linear
		// Set vertical force
		VerticalDisplacementHard += mycar.WasYVel/50;
		mycar.WasYVel = 0;
		// Apply force overtime to approach rest
		VerticalDisplacementVelocity += VerticalDisplacementHard;
		VerticalDisplacementHard += -VerticalDisplacementVelocity * 0.0166666666f; // * (float)delta
		// Dampen
		VerticalDisplacementVelocity *= DisplacementDampen;
		// Apply suspension force		
		VerticalDisplacementSoft += (VerticalDisplacementHard - VerticalDisplacementSoft) * 0.4f;
		VerticalDisplacementSoft = Math.Clamp(VerticalDisplacementSoft, -0.25f, 2f);
		GlobalPosition = GlobalPosition + VerticalDisplacementSoft * -GlobalTransform.Basis.Z;

		// Rotate model with rotation from car
		Rotate(GlobalTransform.Basis.Z.Normalized(), -mycar.rotateChange*(float)delta);
		Rotate(GlobalTransform.Basis.Y.Normalized(), mycar.rollChange*(float)delta);
		Rotate(GlobalTransform.Basis.X.Normalized(), mycar.pitchChange*(float)delta);

	}
}
