using Godot;
using System;

public class Dryad : KinematicBody2D
{
	public static int MOVE_SPEED = 100;
	public static int MAX_RANGE = 120;
	public static int MIN_RANGE = 80;
	public static float AI_THINK_TIME = 0.8f;
	public static float CAST_TIME = 1.0f;
	public static float POSITION_SPREAD = 70.0f;
	public static float FIRE_POSITION_OFFSET = 25.0f;
	public static float LOS = 240;
	public static int NUM_FIRES = 3;
	public static int HP_BAR_WIDTH = 38;
	public static int MAX_HEALTH = 100;
	
	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public PlayerChar Target = null;
	
	[Export]
	public bool InPlayerSwordRange = false;
	
	[Export]
	public Timer AiThinkTimeTimer = null;
	
	public bool IsCasting = false;

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
		Main mainNode = GetParent<Main>();
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Animation = "attack";
		for (int i = 0; i < NUM_FIRES; ++i)
		{
			// create fire
			DryadFire fireInstance = (DryadFire)mainNode.FireScene.Instance();
			fireInstance.Target = Target;
			
			float randomPosition = (float)GD.RandRange(-POSITION_SPREAD, POSITION_SPREAD);
			fireInstance.Position = new Vector2(Target.Position.x + randomPosition, Target.Position.y + FIRE_POSITION_OFFSET);
			
			AnimatedSprite fireSprite = 
				fireInstance.GetNode<AnimatedSprite>("AnimatedSprite");
			fireSprite.Animation = "simmer";
			mainNode.AddChild(fireInstance);
		}
		Timer cooldownTimer = 
			GetNode<Timer>("CooldownTimer");
		cooldownTimer.Start();
	}
	
	public void AiFinishAttackAnimation()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
			
		if (sprite.Animation == "attack")
		{
			sprite.Animation = "idle";
			IsCasting = false;
		}
	}
	
	public void AiMove()
	{
		if (IsCasting)
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
			
			Timer cooldownTimer = 
				GetNode<Timer>("CooldownTimer");
			if (cooldownTimer.IsStopped())
			{
				IsCasting = true;
				sprite.Animation = "casting";
				
				Timer finishCastTimer =
					GetNode<Timer>("FinishCastTimer");
				finishCastTimer.Start();
			}
			else
				sprite.Animation = "idle";
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		AiMove();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		var rect = GetNode<ColorRect>("HPRectRemaining");
		rect.RectSize = new Vector2((float)Health / MAX_HEALTH * HP_BAR_WIDTH, 2);
	}
}
