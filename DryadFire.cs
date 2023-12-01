using Godot;
using System;

public class DryadFire : Area2D
{
	public const int FIREBALL_DAMAGE = 80;
	public const int FIREBALL_DAMAGE_BOSS = 120;
	
	[Export]
	public PlayerChar Target = null;
	
	public uint DamageId = 0;	// Identifies Dryad
	
	public bool FromBoss = false;
	
	public bool Boomed = false;
	
	public double CreatedTimestamp = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreatedTimestamp = Time.GetUnixTimeFromSystem();
	}

	public void OnFireBoom()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Animation = "activate";
		
		Level1 levelNode = GetParent().GetParent<Level1>();
		Main mainNode = levelNode.GetParent<Main>();
		AudioStreamPlayer2D soundPlayer = mainNode
			.GetNode<Node>("MediaNode")
			.GetNode<AudioStreamPlayer2D>("FireSound");
		if (Target.EffectsInRange.Contains(this) && !levelNode.GameOver)
		{
			var prevTargetHealth = Target.Health;
			int fireBallDamage = FromBoss ? FIREBALL_DAMAGE_BOSS : FIREBALL_DAMAGE;
			Target.Health -= fireBallDamage;
			if (Target.Health < -100)
				Target.Health = -100;
		
			var damageReport = new DamageReport();
			damageReport.Who = DamageId;
			damageReport.FromPlayer = false;
			damageReport.Amount = prevTargetHealth - Target.Health;
			damageReport.Timestamp = Time.GetUnixTimeFromSystem();
			levelNode.DamageHistory.Enqueue(damageReport);
		}
		
		soundPlayer.Position = Target.Position;
		soundPlayer.Play();
	}
	
	public void OnAnimationFinished()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
			
		if (sprite.Animation == "activate")
		{
			if (Target.EffectsInRange.Contains(this))
			{
				Target.EffectsInRange.Remove(this);
			}
			QueueFree();
		}
	}
	
	private void OnFireBodyEntered(object body)
	{
		if (body == Target)
			Target.EffectsInRange.Add(this);
	}

	private void OnFireBodyExited(object body)
	{
		if (body == Target)
			if (Target.EffectsInRange.Contains(this))
				Target.EffectsInRange.Remove(this);
	}
	
	public override void _PhysicsProcess(float delta)
	{
		if (!Boomed && CreatedTimestamp + 1 < Time.GetUnixTimeFromSystem() )
		{
			Boomed = true;
			OnFireBoom();	
		}
	}
}
