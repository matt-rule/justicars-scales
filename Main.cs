using System;
using System.Collections.Generic;
using Godot;

public class Main : Node
{
	const String STORY_TEXT_1 = "\"Pagans!\" the head priest was yelling hysterically at me.  \"Drive them from these lands!\"";
	const String STORY_TEXT_2 = "But so far I have yet to see a single human.  Instead, my mission has led me deeper into the forest.  I am no longer sure of my way.";
	
	const String LANTERN_TEXT_1 = "The known gods are subject to the passage of time. The outer gods are not.";
	
	const float LANTERN_DISTANCE = 50f;
	
	public bool ShownStory = false;
	
#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene mobScene;
#pragma warning restore 649

	[Export]
	public PackedScene FireScene { get; set; }

	public Vector2 screenSize; // Size of the game window.

	public override void _Ready()
	{
		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		var DryadNode = GetNodeOrNull<Dryad>("Dryad");
		if (DryadNode != null)
			DryadNode.Target = PlayerCharNode;
		var DryadNode2 = GetNodeOrNull<Dryad>("Dryad2");
		if (DryadNode2 != null)
			DryadNode2.Target = PlayerCharNode;
		screenSize = PlayerCharNode.GetViewportRect().Size;
		GD.Randomize();
	}

	public void GameOver()
	{
		GetNode<Timer>("MobTimer").Stop();

		GetNode<HUD>("HUD").ShowGameOver();

		GetNode<AudioStreamPlayer>("Music").Stop();
		GetNode<AudioStreamPlayer>("DeathSound").Play();
	}

	public void NewGame()
	{
		// Note that for calling Godot-provided methods with strings,
		// we have to use the original Godot snake_case name.
		GetTree().CallGroup("mobs", "queue_free");

		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		var startPosition = GetNode<Position2D>("StartPosition");
		PlayerCharNode.Start(startPosition.Position);

		GetNode<Timer>("StartTimer").Start();

		// TODO
		//GetNode<AudioStreamPlayer>("Music").Play();
	}

	public void OnStartTimerTimeout()
	{
		GetNode<Timer>("MobTimer").Start();
	}

	public void OnMobTimerTimeout()
	{
	}
	
	public void Start(Vector2 pos)
	{
		// TODO: Undo these changes when game ends
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		//playerCharNode._IntegrateForces();
		playerCharNode.Show();
		playerCharNode.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		//playerCharNode.Sleeping = false;
	}
	
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("show_menu"))
		{
			GetTree().Paused = true;
			GetNode<HUD>("HUD").ShowGameMenu();
		}
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
	
		// Reset player char if it goes below a certain Y coordinate
		var startPosition = GetNode<Position2D>("StartPosition");
		if (playerCharNode.Position.y > PlayerChar.MIN_Y_COORD)
		{
			playerCharNode.Position = startPosition.Position;
			playerCharNode.Velocity = new Vector2(0, 0);
			playerCharNode.Health = 100;
			playerCharNode.Grounded = false;
			// TODO: Reset the whole level and player state
		}
		
		// Check distance to lantern
		var lantern = GetNode<AnimatedSprite>("Lantern");
		var hud = GetNode<HUD>("HUD");
		
		if (!ShownStory && (playerCharNode.Position - lantern.Position).Length() < LANTERN_DISTANCE)
		{
			List<String> lines = new List<string>();
			lines.Add(STORY_TEXT_1);
			lines.Add(STORY_TEXT_2);
			hud.ShowDialog(lines);
			ShownStory = true;
		}
		//var infoPanel = GetNode<HUD>("HUD").GetNode<Panel>("MiniInfoPanel");
//		if ((playerCharNode.Position - lantern.Position).Length() < LANTERN_DISTANCE)
//		{
//
////			infoPanel.GetNode<MarginContainer>("MarginContainer").GetNode<Label>("InfoLabel")
////			.Text = LANTERN_TEXT_1;
////			infoPanel.Show();
//		}
//		else
//		{
////			infoPanel.Hide();
//		}
	}
}
