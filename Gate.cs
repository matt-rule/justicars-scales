using Godot;
using System;

public class Gate : StaticBody2D
{
	[Export]
	public uint GuardianId = 0;
	
	public override void _Ready()
	{
		
	}

	public override void _PhysicsProcess(float delta)
	{
		var spriteFrames = GetNode<AnimatedSprite>("AnimatedSprite");
		var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		
		LargeDemon guardian = null;
		var level = GetParent();
		var demonsNode = level.GetNode("Demons");
		var children = demonsNode.GetChildren();
		foreach (var c in children)
		{
			LargeDemon d = c as LargeDemon;
			if (d == null)
				continue;
			if (d.DamageId == GuardianId)
				guardian = d;
		}
				
		if (guardian == null || !guardian.Alive)
		{
			spriteFrames.Animation = "open";
			collisionShape.Disabled = true;
		}
	}
}
