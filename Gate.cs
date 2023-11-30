using Godot;
using System;
using System.Collections.Generic;

public class Gate : StaticBody2D
{
	[Export]
	public List<uint> GuardianIds = new List<uint>();
	
	public bool IsGuardianAndIsAlive(object obj)
	{
		LargeDemon demon = obj as LargeDemon;
		if (demon != null && demon.Alive && GuardianIds.Contains(demon.DamageId))
			return true;
		Dryad dryad = obj as Dryad;
		if (dryad != null && GuardianIds.Contains(dryad.DamageId))
			return true;
		Goblin goblin = obj as Goblin;
		if (goblin != null && goblin.Alive && GuardianIds.Contains(goblin.DamageId))
			return true;
		return false;
	}
	
	public void Unlock()
	{
		var spriteFrames = GetNode<AnimatedSprite>("AnimatedSprite");
		var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		spriteFrames.Animation = "open";
		collisionShape.Disabled = true;
	}

	public override void _PhysicsProcess(float delta)
	{
		var levelNode = GetParent().GetParent();
		foreach (var c in levelNode.GetNode("Demons").GetChildren())
			if (IsGuardianAndIsAlive(c))
				return;
		foreach (var c in levelNode.GetNode("Dryads").GetChildren())
			if (IsGuardianAndIsAlive(c))
				return;
		foreach (var c in levelNode.GetNode("Goblins").GetChildren())
			if (IsGuardianAndIsAlive(c))
				return;
		Unlock();
	}
}
