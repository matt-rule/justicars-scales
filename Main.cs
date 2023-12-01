using System;
using System.Collections.Generic;
using Godot;

public class Main : Node
{
#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene levelScene;
#pragma warning restore 649

	public Vector2 ScreenSize; // Size of the game window.
	
	public Level1 Level;
	
	public float CamXPosition = 0;

	public override void _Ready()
	{
	}
	
	public override void _Process(float delta)
	{
		CamXPosition += 20f * delta;
		if (CamXPosition > 20000f)
			CamXPosition = -20000f;
		GetNode<Camera2D>("MainMenuCam").Position =
			new Vector2(CamXPosition, (float)Math.Sin(CamXPosition / 200) * 20 + 70);
	}
	
	public void NewGame()
	{
		GD.Randomize();
		var hud = GetNode<HUD>("HUD");
		var media = GetNode("MediaNode");
		
		Level1 levelInstance = (Level1)levelScene.Instance();
		AddChild(levelInstance);
		
		Level = GetNode<Level1>("Level1");
		ScreenSize = Level.GetNode<PlayerChar>("PlayerChar").GetViewportRect().Size;
		
		GetNode("MediaNode").GetNode<AudioStreamPlayer>("MenuMusic").Stop();
		GetNode("MediaNode").GetNode<AudioStreamPlayer>("Music").Play();
					
		hud.GetNode<TextureRect>("MainMenuLogo").Hide();
		hud.GetNode<Label>("MainMenuLabel1").Hide();
		hud.GetNode<Label>("MainMenuLabel2").Hide();
		hud.GetNode<Panel>("MainMenuPanel").Hide();
		hud.GetNode<ColorRect>("DeathScreen").Hide();
		hud.GetNode<Node2D>("HPBar").Show();
		//media.GetNode<AudioStreamPlayer>("Music").Play();
		var ambient = media.GetNode<AudioStreamPlayer>("Ambient");
		if (!ambient.Playing)
			ambient.Play();
		Level.GetNode<PlayerChar>("PlayerChar").GetNode<Camera2D>("Camera2D").Current = true;
		GetNode<Camera2D>("MainMenuCam").Current = false;
	}

	public void ProcessPlayerDeath()
	{
		if (Level.GameOver)
			return;
		
		var hud = GetNode<HUD>("HUD");
		var media = GetNode("MediaNode");
		
		GetNode<Camera2D>("MainMenuCam").Current = true;
		Level.GetNode<PlayerChar>("PlayerChar").GetNode<Camera2D>("Camera2D").Current = false;
		GetNode("Level1").QueueFree();
		GetNode("MediaNode").GetNode<AudioStreamPlayer>("Music").Stop();
		media.GetNode<AudioStreamPlayer>("DeathSound").Play();
		media.GetNode<AudioStreamPlayer>("Ambient").Stop();
		hud.GetNode<ColorRect>("DeathScreen").Show();
		GetNode<Timer>("DeathTimer").Start();
	}

	private void DeathTimerFinished()
	{
		NewGame();
	}
}
