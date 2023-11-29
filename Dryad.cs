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
	public static float POSITION_SPREAD = 70.0f;
	public static float FIRE_POSITION_OFFSET = 25.0f;
	public static float LOS = 240;
	public static int NUM_FIRES = 3;
	public static int HP_BAR_WIDTH = 12;
	public static int MAX_HEALTH = 100;
	public static double DRYAD_CAST_DURATION_SECS = 1.5;
	public static double DRYAD_INNER_COOLDOWN_DURATION_SECS = 3;	// After cast
	public static double DRYAD_FULL_COOLDOWN_DURATION_SECS =
		DRYAD_CAST_DURATION_SECS + DRYAD_INNER_COOLDOWN_DURATION_SECS;	// Including cast
	public static double DRYAD_FINISH_ATTACK_DURATION_SECS = 0.6;
	public static double HIDE_HP_BAR_SECS = 4;
	
	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public PlayerChar Target = null;
	
	public uint DamageId = 0;
	
	[Export]
	public bool InPlayerSwordRange = false;
	
	public double LastAffectedTimestamp = 0;
	
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
	}

	public float AiGetDirection()
	{
		return Target.Position.x - Position.x;
	}
	
	public void AiFinishCast()
	{
		Level1 levelNode = GetParent().GetParent<Level1>();
		Node dryadFiresNode = levelNode.GetNode("DryadFires");
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Animation = "attack";
		for (int i = 0; i < NUM_FIRES; ++i)
		{
			// create fire
			DryadFire fireInstance = (DryadFire) levelNode.FireScene.Instance();
			fireInstance.Target = Target;
			fireInstance.DamageId = DamageId;
			
			float randomPosition = (float)GD.RandRange(-POSITION_SPREAD, POSITION_SPREAD);
			fireInstance.Position = new Vector2(Target.Position.x + randomPosition, Target.Position.y + FIRE_POSITION_OFFSET);
			
			AnimatedSprite fireSprite = 
				fireInstance.GetNode<AnimatedSprite>("AnimatedSprite");
			fireSprite.Animation = "simmer";
			dryadFiresNode.AddChild(fireInstance);
		}
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
			
			if (LastStartCastTimestamp + DRYAD_FULL_COOLDOWN_DURATION_SECS < Time.GetUnixTimeFromSystem() )
			{
				LastStartCastTimestamp = Time.GetUnixTimeFromSystem();
				State = DryadState.Casting;
				sprite.Animation = "casting";
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
		bool visible = LastAffectedTimestamp + HIDE_HP_BAR_SECS > Time.GetUnixTimeFromSystem();
		var rectRemaining = GetNode<ColorRect>("HPRectRemaining");
		var rectBg = GetNode<ColorRect>("HPRectBg");
		rectRemaining.RectSize = new Vector2((float)Health / MAX_HEALTH * HP_BAR_WIDTH, 2);
		rectRemaining.Visible = visible;
		rectBg.Visible = visible;
	}
}
