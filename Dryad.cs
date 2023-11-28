using Godot;
using System;

public enum DryadState
{
	NotAttacking,
	Casting,
	FinishingCast
}

public class Dryad : KinematicBody2D
{
	public static int MOVE_SPEED = 100;
	public static int MAX_RANGE = 120;
	public static int MIN_RANGE = 80;
	public static float AI_THINK_TIME = 0.8f;
	//public static float CAST_TIME = 1.0f;
	public static float POSITION_SPREAD = 70.0f;
	public static float FIRE_POSITION_OFFSET = 25.0f;
	public static float LOS = 240;
	public static int NUM_FIRES = 3;
	public static int HP_BAR_WIDTH = 38;
	public static int MAX_HEALTH = 100;
	public static double DRYAD_CAST_DURATION_SECS = 1.5;
	public static double DRYAD_INNER_COOLDOWN_DURATION_SECS = 3;	// After cast
	public static double DRYAD_FULL_COOLDOWN_DURATION_SECS =
		DRYAD_CAST_DURATION_SECS + DRYAD_INNER_COOLDOWN_DURATION_SECS;	// Including cast
	public static double DRYAD_FINISH_ATTACK_DURATION_SECS = 0.6;
	
	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public PlayerChar Target = null;
	
	[Export]
	public bool InPlayerSwordRange = false;
	
	[Export]
	public Timer AiThinkTimeTimer = null;
	
	[Export]
	public DryadState State = DryadState.NotAttacking;
	
	// This can be used to determine whether the dryad is casting,
	// when it will finish casting (1.5s duration),
	// and when its cooldown (3s duration) finishes.
	[Export]
	public double LastStartCastTimestamp = 0.0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
//		AiThinkTimeTimer = Timer.new()
//		AiThinkTimeTimer.SetOneShot(true)
//		AiThinkTimeTimer.SetWaitTime(AI_THINK_TIME)
//		AiThinkTimeTimer.connect("timeout", self, "AiDecision")
//		add_child(ai_think_time_timer)
	}
	
	public void AiDecision()
	{
//		if is_in_range && state == 0 && target.get_state() == 0:
//			attack()
	}

	public float AiGetDirection()
	{
		return Target.Position.x - Position.x;
	}
	
	public void AiFinishCast()
	{
		Level1 levelNode = GetParent<Level1>();
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Animation = "attack";
		for (int i = 0; i < NUM_FIRES; ++i)
		{
			// create fire
			DryadFire fireInstance = (DryadFire) GetParent<Level1>().FireScene.Instance();
			fireInstance.Target = Target;
			
			float randomPosition = (float)GD.RandRange(-POSITION_SPREAD, POSITION_SPREAD);
			fireInstance.Position = new Vector2(Target.Position.x + randomPosition, Target.Position.y + FIRE_POSITION_OFFSET);
			
			AnimatedSprite fireSprite = 
				fireInstance.GetNode<AnimatedSprite>("AnimatedSprite");
			fireSprite.Animation = "simmer";
			levelNode.AddChild(fireInstance);
		}
//		Timer cooldownTimer = 
//			GetNode<Timer>("CooldownTimer");
//		cooldownTimer.Start();
	}
	
	public void AiFinishAttackAnimation()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
			
		if (sprite.Animation == "attack")
		{
			sprite.Animation = "idle";
		}
	}
	
	public void AiMove()
	{
		if (LastStartCastTimestamp + DRYAD_CAST_DURATION_SECS + DRYAD_FINISH_ATTACK_DURATION_SECS > Time.GetUnixTimeFromSystem() )
			return;
		
		float direction = AiGetDirection();
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		float absoluteDistance = Math.Abs(direction);
		
		if (absoluteDistance > LOS)
			return;
		
		if (absoluteDistance > MAX_RANGE)
		{
			sprite.FlipH = direction < 0;
			var Motion = Math.Sign(direction) * MOVE_SPEED;
			MoveAndSlide( new Vector2(Motion, 0), Vector2.Up );
			sprite.Animation = "walking";
		}
		else if (absoluteDistance < MIN_RANGE)
		{
			sprite.FlipH = direction > 0;
			var Motion = -Math.Sign(direction) * MOVE_SPEED;
			MoveAndSlide( new Vector2(Motion, 0), Vector2.Up );
			sprite.Animation = "walking";	
		}
		else
		{
			sprite.FlipH = direction < 0;
			
//			Timer cooldownTimer = 
//				GetNode<Timer>("CooldownTimer");
//			if (cooldownTimer.IsStopped())
			if (LastStartCastTimestamp + DRYAD_FULL_COOLDOWN_DURATION_SECS < Time.GetUnixTimeFromSystem() )
			{
				LastStartCastTimestamp = Time.GetUnixTimeFromSystem();
				State = DryadState.Casting;
				//IsCasting = true;
				sprite.Animation = "casting";
				
//				Timer finishCastTimer =
//					GetNode<Timer>("FinishCastTimer");
//				finishCastTimer.Start();
			}
			else if (State == DryadState.FinishingCast)
			{
				sprite.Animation = "attack";
			}
			else
				sprite.Animation = "idle";
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		if ( State == DryadState.Casting && LastStartCastTimestamp + DRYAD_CAST_DURATION_SECS < Time.GetUnixTimeFromSystem() )
		{
			State = DryadState.FinishingCast;
			AiFinishCast();
			return;
		}
		if ( State == DryadState.FinishingCast && LastStartCastTimestamp + DRYAD_CAST_DURATION_SECS + DRYAD_FINISH_ATTACK_DURATION_SECS < Time.GetUnixTimeFromSystem() )
		{
			State = DryadState.NotAttacking;
			AiFinishAttackAnimation();
			return;
		}
		
		AiMove();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		var rect = GetNode<ColorRect>("HPRectRemaining");
		rect.RectSize = new Vector2((float)Health / MAX_HEALTH * HP_BAR_WIDTH, 2);
	}
}
