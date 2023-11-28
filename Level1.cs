using Godot;
using System;
using System.Collections.Generic;

public class LanternHistoricalState
{
	AnimatedSprite Entity;
	bool Activated;
}

public class PlayerHistoricalState
{
	PlayerChar Entity;
	bool FacingLeft;
	int Health;
	int BleedPosition;
	Vector2 Position;
	Vector2 Velocity;
}

public class DryadHistoricalState
{
	Dryad Entity;
	bool FacingLeft;
	int Health;
	Vector2 Position;
	Vector2 Velocity;
	float CastProgressSecs;
	float CastCooldownSecs;
}

public class DryadFireHistoricalState
{
	DryadFire Entity;
	int CreatedTimestamp;
}

public class Level1 : Node
{
	public const float LANTERN_DISTANCE = 30f;
	
	public const float SPAWN_OFFSET_Y = -20f;

	public bool ShownStory = false;
	
	// Up to 10 in each queue (5 seconds, 2 per second)
	public Queue<PlayerHistoricalState> PlayerHistoricalState;
	public List<Queue<DryadHistoricalState>> DryadHistoricalStates;
	public List<Queue<DryadFireHistoricalState>> DryadFireHistoricalStates;
	public List<Queue<LanternHistoricalState>> LanternHistoricalStates;
	
#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene mobScene;
	
	[Export]
	public PackedScene FireScene { get; set; }
#pragma warning restore 649

	const String STORY_TEXT_1 = "\"Pagans!\" the head priest was yelling hysterically at me.  \"Drive them from these lands!\"";
	const String STORY_TEXT_2 = "But so far I have yet to see a single human.  Instead, my mission has led me deeper into the forest.  I am no longer sure of my way.";
	
	const String LANTERN_TEXT_1 = "Use the arrow keys or thumbstick to move.";
	const String LANTERN_TEXT_2 = "To jump, press up (Cross or Xbox A).";
	const String LANTERN_TEXT_3 = "To attack, press Q or R1.";
	const String LANTERN_TEXT_4 = "To use the bell, Press W (Square or Xbox X).";
	const String LANTERN_TEXT_5 = "To flick back in time (hourglass), Press E (Circle or Xbox B).";
	const String LANTERN_TEXT_6 = "To use Retribution (scales), press R (Triangle or Xbox Y).";
	const String LANTERN_TEXT_7 = "If your HP goes negative, quickly use the scales or hourglass to save yourself.";
	
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
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
		playerCharNode.Position = spawnPoint.Position + new Vector2( 0, SPAWN_OFFSET_Y );
		
		var lantern = GetNode<AnimatedSprite>("Lantern");
		var lantern2 = GetNode<AnimatedSprite>("Lantern2");
		var lantern3 = GetNode<AnimatedSprite>("Lantern3");
		var lantern4 = GetNode<AnimatedSprite>("Lantern4");
		var lantern5 = GetNode<AnimatedSprite>("Lantern5");
		var lantern6 = GetNode<AnimatedSprite>("Lantern6");
		var lantern7 = GetNode<AnimatedSprite>("Lantern7");
		
		if (spawnPoint.Position.x <= lantern.Position.x)
			lantern.Animation = "on";
		if (spawnPoint.Position.x <= lantern2.Position.x)
			lantern2.Animation = "on";
		if (spawnPoint.Position.x <= lantern3.Position.x)
			lantern3.Animation = "on";
		if (spawnPoint.Position.x <= lantern4.Position.x)
			lantern4.Animation = "on";
		if (spawnPoint.Position.x <= lantern5.Position.x)
			lantern5.Animation = "on";
		if (spawnPoint.Position.x <= lantern6.Position.x)
			lantern6.Animation = "on";
		if (spawnPoint.Position.x <= lantern7.Position.x)
			lantern7.Animation = "on";
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

		//var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
//		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
//		PlayerCharNode.Start(spawnPoint.Position);

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
	
//	public void Start(Vector2 pos)
//	{
//		// TODO: Undo these changes when game ends
//		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
//		//playerCharNode._IntegrateForces();
//
//		var spawnPoint = GetParent().GetNode<Position2D>("StartPosiion");
//		playerCharNode.Position = spawnPoint.Position;
//		playerCharNode.Show();
//		playerCharNode.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
//		//playerCharNode.Sleeping = false;
//	}
	
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("show_menu"))
		{
			GetTree().Paused = true;
			GetParent().GetNode<HUD>("HUD").ShowGameMenu();
		}
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
	
		// Reset player char if it goes below a certain Y coordinate
		//var startPosition = GetNode<Position2D>("StartPosition");
		if (playerCharNode.Position.y > PlayerChar.MIN_Y_COORD)
			GetParent<Main>().ProcessPlayerDeath();
		
		// Check distance to lantern
		var lantern = GetNode<AnimatedSprite>("Lantern");
		var lantern2 = GetNode<AnimatedSprite>("Lantern2");
		var lantern3 = GetNode<AnimatedSprite>("Lantern3");
		var lantern4 = GetNode<AnimatedSprite>("Lantern4");
		var lantern5 = GetNode<AnimatedSprite>("Lantern5");
		var lantern6 = GetNode<AnimatedSprite>("Lantern6");
		var lantern7 = GetNode<AnimatedSprite>("Lantern7");
		var hud = GetParent().GetNode<HUD>("HUD");
		var mediaNode = GetParent().GetNode<Node>("MediaNode");
		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
		
		if (Math.Abs(playerCharNode.Position.x - lantern.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_1);
			if (lantern.Animation != "on")
			{
				lantern.Animation = "on";
				if (spawnPoint.Position.x > lantern.Position.x)
					spawnPoint.Position = lantern.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern2.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_2);
			if (lantern2.Animation != "on")
			{
				lantern2.Animation = "on";
				if (spawnPoint.Position.x > lantern2.Position.x)
					spawnPoint.Position = lantern2.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern3.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_3);
			if (lantern3.Animation != "on")
			{
				lantern3.Animation = "on";
				if (spawnPoint.Position.x > lantern3.Position.x)
					spawnPoint.Position = lantern3.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern4.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_4);
			if (lantern4.Animation != "on")
			{
				lantern4.Animation = "on";
				if (spawnPoint.Position.x > lantern4.Position.x)
					spawnPoint.Position = lantern4.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern5.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_5);
			if (lantern5.Animation != "on")
			{
				lantern5.Animation = "on";
				if (spawnPoint.Position.x > lantern5.Position.x)
					spawnPoint.Position = lantern5.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern6.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_6);
			if (lantern6.Animation != "on")
			{
				lantern6.Animation = "on";
				if (spawnPoint.Position.x > lantern6.Position.x)
					spawnPoint.Position = lantern6.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
		}
		else if (Math.Abs(playerCharNode.Position.x - lantern7.Position.x) < LANTERN_DISTANCE)
		{
			hud.ShowHint(LANTERN_TEXT_7);
			if (lantern7.Animation != "on")
			{
				lantern7.Animation = "on";
				if (spawnPoint.Position.x > lantern7.Position.x)
					spawnPoint.Position = lantern7.Position;
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
			}
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
