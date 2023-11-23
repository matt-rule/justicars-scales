using Godot;
using System;

public class Nymph : KinematicBody2D
{
	public static int MOVE_SPEED = 100;
	public static int FOLLOW_DIST = 40;
	public static float AI_THINK_TIME = 0.8f;
	
	[Export]
	public PlayerChar Target = null;
	
	[Export]
	public Timer AiThinkTimeTimer = null;

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
	
	public void AiMove()
	{
		float direction = AiGetDirection();
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		if (Math.Abs(direction) < FOLLOW_DIST)
		{
			sprite.Animation = "idle";
		}
		else
		{
			sprite.FlipH = direction < 0;
			var Motion = Math.Sign(direction) * MOVE_SPEED;
			MoveAndSlide( new Vector2(Motion, 0), Vector2.Up );
			sprite.Animation = "walking";	
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		AiMove();
	}
}
