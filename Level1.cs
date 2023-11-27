using Godot;
using System;
using System.Collections.Generic;

public class Level1 : Node
{
	public const float LANTERN_DISTANCE = 30f;

	public bool ShownStory = false;
	
#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene mobScene;
	
	[Export]
	public PackedScene FireScene { get; set; }
#pragma warning restore 649

	const String STORY_TEXT_1 = "\"Pagans!\" the head priest was yelling hysterically at me.  \"Drive them from these lands!\"";
	const String STORY_TEXT_2 = "But so far I have yet to see a single human.  Instead, my mission has led me deeper into the forest.  I am no longer sure of my way.";
	
	const String LANTERN_TEXT_1_OLD = "The known gods are subject to the passage of time. The outer gods are not.";
	const String LANTERN_TEXT_1 = "Use the arrow keys or thumbstick to move";
	const String LANTERN_TEXT_2 = "To jump, press up (Cross or Xbox A)";
	const String LANTERN_TEXT_3 = "To attack, press Q or R1";
	const String LANTERN_TEXT_4 = "To use the bell, Press W (Square or Xbox X)";
	const String LANTERN_TEXT_5 = "To use the hourglass, Press E (Circle or Xbox B)";
	const String LANTERN_TEXT_6 = "To use the scales, press R (Triangle or Xbox Y)";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		var DryadNode = GetNodeOrNull<Dryad>("Dryad");
		if (DryadNode != null)
			DryadNode.Target = PlayerCharNode;
		var DryadNode2 = GetNodeOrNull<Dryad>("Dryad2");
		if (DryadNode2 != null)
			DryadNode2.Target = PlayerCharNode;
	}


//	public void GameOver()
//	{
//		GetNode<Timer>("MobTimer").Stop();
//
//		GetParent().GetNode<HUD>("HUD").ShowGameOver();
//
//		GetParent().GetNode<AudioStreamPlayer>("Music").Stop();
//		//GetNode<AudioStreamPlayer>("DeathSound").Play();
//	}

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
			GetParent().GetNode<HUD>("HUD").ShowGameMenu();
		}
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
	
		// Reset player char if it goes below a certain Y coordinate
		var startPosition = GetNode<Position2D>("StartPosition");
		if (playerCharNode.Position.y > PlayerChar.MIN_Y_COORD)
			GetParent<Main>().ProcessPlayerDeath();
		
		// Check distance to lantern
		var lantern = GetNode<AnimatedSprite>("Lantern");
		var lantern2 = GetNode<AnimatedSprite>("Lantern2");
		var lantern3 = GetNode<AnimatedSprite>("Lantern3");
		var lantern4 = GetNode<AnimatedSprite>("Lantern4");
		var lantern5 = GetNode<AnimatedSprite>("Lantern5");
		var lantern6 = GetNode<AnimatedSprite>("Lantern6");
		var hud = GetParent().GetNode<HUD>("HUD");
		
		if (Math.Abs(playerCharNode.Position.x - lantern.Position.x) < LANTERN_DISTANCE)
		{
			lantern.Animation = "on";
			hud.ShowHint(LANTERN_TEXT_1);
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern2.Position.x) < LANTERN_DISTANCE)
		{
			lantern2.Animation = "on";
			hud.ShowHint(LANTERN_TEXT_2);
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern3.Position.x) < LANTERN_DISTANCE)
		{
			lantern3.Animation = "on";
			hud.ShowHint(LANTERN_TEXT_3);
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern4.Position.x) < LANTERN_DISTANCE)
		{
			lantern4.Animation = "on";
			hud.ShowHint(LANTERN_TEXT_4);
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern5.Position.x) < LANTERN_DISTANCE)
		{
			lantern5.Animation = "on";
			hud.ShowHint(LANTERN_TEXT_5);
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern6.Position.x) < LANTERN_DISTANCE)
		{
			lantern6.Animation = "on";
			hud.ShowHint(LANTERN_TEXT_6);
		}
		else
		{
			hud.HideHint();
		}
		
//		if (!ShownStory && (playerCharNode.Position - lantern.Position).Length() < LANTERN_DISTANCE)
//		{
//			List<String> lines = new List<string>();
//			lines.Add(STORY_TEXT_1);
//			lines.Add(STORY_TEXT_2);
//			hud.ShowDialog(lines);
//			ShownStory = true;
//		}
		
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
