using Godot;
using System;

public class LargeDemon : KinematicBody2D
{
	public static int MOVE_SPEED = 220;
	public static int MAX_RANGE = 30;
	public static int MIN_RANGE = 20;
	public static float LOS = 100;
	public const int MAX_HEALTH = 80;
	public static double HIDE_HP_BAR_SECS = 4;
	public static int HP_BAR_WIDTH = 20;
	public static double ATTACK_DELAY = 0.2;
	public static double ATTACK_RESET_ANIMATION = 0.6;
	public static double ATTACK_COOLDOWN_SECS = 5; // From beginning of attack
	public static int DAMAGE_ONTO_PLAYER = 90;
	
	public bool InPlayerSwordRange = false;
	public double LastAffectedTimestamp = 0;
	public double LastAttackTimestamp = 0;
	public bool AttackPending = false;
	public bool PlayerInSwipeRange = false;

	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public PlayerChar Target = null;
	
	[Export]
	public uint DamageId = 0;
	
	public override void _Ready()
	{
		Target = GetParent().GetParent().GetNode<PlayerChar>("PlayerChar");
	}
	
	public float AiGetDirection()
	{
		return Target.Position.x - Position.x;
	}
	
	public void AiMove()
	{
		float direction = AiGetDirection();
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		float absoluteDistance = Math.Abs(direction);
		
		if (absoluteDistance > LOS)
		{
			sprite.Animation = "idle";
			return;
		}
		
		if (absoluteDistance > MAX_RANGE)
		{
			sprite.FlipH = direction < 0;
			var Motion = Math.Sign(direction) * MOVE_SPEED;
			MoveAndSlide( new Vector2(Motion, 0), Vector2.Up );
			if (sprite.Animation != "walking")
				sprite.Animation = "walking";
		}
		else if (absoluteDistance < MIN_RANGE)
		{
			sprite.FlipH = direction > 0;
			var Motion = -Math.Sign(direction) * MOVE_SPEED;
			MoveAndSlide( new Vector2(Motion, 0), Vector2.Up );
			if (sprite.Animation != "walking")
				sprite.Animation = "walking";
		}
		else
		{
			sprite.FlipH = direction < 0;
			
			if (LastAttackTimestamp + ATTACK_COOLDOWN_SECS < Time.GetUnixTimeFromSystem() )
			{
				LastAttackTimestamp = Time.GetUnixTimeFromSystem();
				//State = DryadState.Casting;
				sprite.Animation = "attacking";
				AttackPending = true;
			}
			else if (LastAttackTimestamp + ATTACK_RESET_ANIMATION > Time.GetUnixTimeFromSystem() )
			{
				if (sprite.Animation != "attacking")
					sprite.Animation = "attacking";
			}
			else
				sprite.Animation = "idle";
		}
		
		var swipeCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwipeCollisionShape");
		if (sprite.FlipH)
		{
			swipeCollisionShape.Position = new Vector2 (-10, -10);
		}
		else
		{
			swipeCollisionShape.Position = new Vector2 (10, -10);
		}
	}
	
	public override void _PhysicsProcess(float delta)
	{
		AiMove();
		
		if (AttackPending
			&& LastAttackTimestamp + ATTACK_DELAY < Time.GetUnixTimeFromSystem() )
		{
			AttackPending = false;
			ProcessAttack();
		}
	}
	
	private void ProcessAttack()
	{
		var now = Time.GetUnixTimeFromSystem();
		var levelNode = GetParent().GetParent<Level1>();
//		var mainNode = levelNode.GetParent();

		if (PlayerInSwipeRange)
		{
			int prevTargetHealth = Target.Health;
			Target.Health -= DAMAGE_ONTO_PLAYER;
			
			if (Target.Health < -100)
				Target.Health = -100;
		
			var damageReport = new DamageReport();
			damageReport.Who = DamageId;
			damageReport.FromPlayer = false;
			damageReport.Amount = prevTargetHealth - Target.Health;
			damageReport.Timestamp = now;
			levelNode.DamageHistory.Enqueue(damageReport);
		}

//		if (dryad.InPlayerSwordRange)
//		{
//			var hitSound = GetNode<AudioStreamPlayer2D>("HitSound");
//			hitSound.Play();
//			int prevTargetHealth = dryad.Health;
//			dryad.Health -= SWORD_DAMAGE_DRYAD;
//			dryad.LastAffectedTimestamp = now;
//			if (dryad.Health <= 0)
//			{
//				var deathSound = mainNode.GetNode("MediaNode")
//					.GetNode<AudioStreamPlayer2D>("DryadDeathSound");
//				deathSound.Position = dryad.Position;
//				deathSound.Play();
//				dryad.QueueFree();
//			}
//			else
//			{
//				var onHitSound = mainNode.GetNode("MediaNode")
//					.GetNode<AudioStreamPlayer2D>("DryadAttackedSound");
//				onHitSound.Position = dryad.Position;
//				onHitSound.Play();
//			}
//
//			var damageReport = new DamageReport();
//			damageReport.Who = dryad.DamageId;
//			damageReport.FromPlayer = true;
//			damageReport.Amount = prevTargetHealth - dryad.Health;
//			damageReport.Timestamp = now;
//			levelNode.DamageHistory.Enqueue(damageReport);
//		}
	}
		
	public override void _Process(float delta)
	{
		bool visible = LastAffectedTimestamp + HIDE_HP_BAR_SECS > Time.GetUnixTimeFromSystem();
		var rectRemaining = GetNode<ColorRect>("HPRectRemaining");
		var rectBg = GetNode<ColorRect>("HPRectBg");
		rectRemaining.RectSize = new Vector2((float)Health / MAX_HEALTH * HP_BAR_WIDTH, 2);
		rectRemaining.Visible = visible;
		rectBg.Visible = visible;
	}
	
	private void OnSwipeCollisionBodyEntered(object body)
	{
		var player = body as PlayerChar;
		if (player != null)
			PlayerInSwipeRange = true;
	}

	private void OnSwipeCollisionBodyExited(object body)
	{
		var player = body as PlayerChar;
		if (player != null)
			PlayerInSwipeRange = false;
	}
}
