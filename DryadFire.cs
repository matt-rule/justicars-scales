using Godot;
using System;

public class DryadFire : Area2D
{
	[Export]
	public PlayerChar Target = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void OnFireBoom()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Animation = "activate";
		
		Main mainNode = GetParent<Main>();
		AudioStreamPlayer2D soundPlayer = mainNode.
			GetNode<AudioStreamPlayer2D>("FireSound");
		soundPlayer.Position = Target.Position;
		soundPlayer.Play();
	}
	
	public void OnAnimationFinished()
	{
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
			
		if (sprite.Animation == "activate")
		{
			QueueFree();
		}
	}
	
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
