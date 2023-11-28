using Godot;
using System;

public class DryadFire : Area2D
{
	[Export]
	public PlayerChar Target = null;
	
	[Export]
	public int Health = 100;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void OnFireBoom()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Animation = "activate";
		
		Level1 levelNode = GetParent<Level1>();
		Main mainNode = levelNode.GetParent<Main>();
		AudioStreamPlayer2D soundPlayer = mainNode
			.GetNode<Node>("MediaNode")
			.GetNode<AudioStreamPlayer2D>("FireSound");
		if (Target.EffectsInRange.Contains(this))
		{
			Target.Health -= 80;
			if (Target.Health < -100)
				Target.Health = -100;
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

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
