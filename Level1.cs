using Godot;
using System;
using System.Collections.Generic;

public class PlayerHistoricalState
{
	public bool FacingLeft;
	public int Health;
	public float BleedPosition;
	public Vector2 Position;
	public Vector2 Velocity;
	public bool Grounded;
	public double LastAttackTimestamp;
	public bool PendingAttackConnected;
	public bool PendingScales;
	public double UsedScalesTimestamp;
}

public class DryadHistoricalState
{
	public bool FacingLeft;
	public int Health;
	public Vector2 Position;
	public Vector2 Velocity;
	public float CastProgressSecs;
	public float CastCooldownSecs;
	public DryadState AttackState;
	public double LastStartCastTimestamp;
	public bool InPlayerSwordRange;
	public uint DamageId;
}

public class DryadFireHistoricalState
{
	public Vector2 Position;
	public double CreatedTimestamp;
}

public class DemonHistoricalState
{
	public bool FacingLeft;
	public int Health;
	public Vector2 Position;
	public Vector2 Velocity;
	public bool InPlayerSwordRange;
	public uint DamageId;
	public double LastAttackTimestamp;
	public bool AttackPending;
	public bool PlayerInSwipeRange;
}

public class HistoricalLevelState
{
	public double Timestamp;
	public PlayerHistoricalState Player;
	public Vector2 SpawnPoint;
	public List<DryadHistoricalState> DryadList = new List<DryadHistoricalState>();
	public List<DryadFireHistoricalState> DryadFireList = new List<DryadFireHistoricalState>();
	public List<DemonHistoricalState> DemonList = new List<DemonHistoricalState>();
	public Queue<DamageReport> DamageHistory;		// The history, but at this point in history
}

public class DamageReport
{
	public uint Who;		// Damage ID
	public bool FromPlayer;
	public int Amount;
	public double Timestamp;
}

public class DivineDamageReport
{
	public bool PlayerIsTarget;
	public uint Who;	// 0 if PlayerIsTarget == true
	public int Amount;	// negative = healing
	public double Timestamp;
}

public class Level1 : Node
{
	const String STORY_TEXT_1 = "\"Pagans!\" the head priest was yelling hysterically at me.  \"Drive them from these lands!\"";
	const String STORY_TEXT_2 = "But so far I have yet to see a single human.  Instead, my mission has led me deeper into the forest.  I am no longer sure of my way.";
	
	const String LANTERN_TEXT_1 = "Use the arrow keys or thumbstick to move.";
	const String LANTERN_TEXT_2 = "To jump, press up (Cross or Xbox A).";
	const String LANTERN_TEXT_3 = "To attack, press Q or R1.";
	const String LANTERN_TEXT_4x = "To use the bell, Press W (Square or Xbox X).";
	const String LANTERN_TEXT_4 = "To flick back in time (hourglass), Press E (Circle or Xbox B).";
	const String LANTERN_TEXT_5 = "Retribution: press R (Triangle or Xbox Y). One usage per life; use it to react to attacks.";
	const String LANTERN_TEXT_6 = "Retribution balances all recent damage. Thus, it is a double-edged sword.";
	const String LANTERN_TEXT_7 = "If your health drops into the left side of the bar, you have only moments to save yourself.";
	const String LANTERN_TEXT_8 = "Retribution and the hourglass can synergise against large opponents. You can do this.";
	
	public const int HISTORY_MAX_RECORDS = 10;
	public const int HISTORY_SAMPLES_PER_SECOND = 2;
	
	public const int DAMAGE_REPORT_LIFESPAN_SECS = 5;
	
	public const float LANTERN_DISTANCE = 30f;
	public const float SPAWN_OFFSET_Y = -20f;
	public bool ShownStory = false;
	
	public const int MAX_HEALTH = 100;
	
	public Queue<DamageReport> DamageHistory = new Queue<DamageReport>();
	public Queue<HistoricalLevelState> LevelHistory = new Queue<HistoricalLevelState>();
	public List<DivineDamageReport> DivineDamageHistory = new List<DivineDamageReport>();
	
// We assign this in the editor, so we don't need the warning about not being assigned.
#pragma warning disable 649
	[Export]
	public PackedScene DryadScene { get; set; }
	
	[Export]
	public PackedScene FireScene { get; set; }
	
	[Export]
	public PackedScene DemonScene { get; set; }
#pragma warning restore 649

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		List<Vector2> dryadSpawnLocations = new List<Vector2>();
		dryadSpawnLocations.Add(new Vector2(1393, 153));
		dryadSpawnLocations.Add(new Vector2(1459, 153));
		dryadSpawnLocations.Add(new Vector2(1593, 153));
		dryadSpawnLocations.Add(new Vector2(1659, 153));
		var dryadsNode = GetNodeOrNull("Dryads");

		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		
		foreach (Vector2 location in dryadSpawnLocations)
		{
			Dryad dryad = (Dryad)DryadScene.Instance();
			dryad.Target = PlayerCharNode;
			dryad.Position = location;
			dryad.DamageId = GD.Randi();
			dryadsNode.AddChild(dryad);
		}
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
		playerCharNode.Position = spawnPoint.Position + new Vector2( 0, SPAWN_OFFSET_Y );
		
		CheckLanterns();
	}
	
	public void CheckLanterns()
	{
		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
		
		var lantern = GetNode<AnimatedSprite>("Lantern");
		var lantern2 = GetNode<AnimatedSprite>("Lantern2");
		var lantern3 = GetNode<AnimatedSprite>("Lantern3");
		var lantern4 = GetNode<AnimatedSprite>("Lantern4");
		var lantern5 = GetNode<AnimatedSprite>("Lantern5");
		var lantern6 = GetNode<AnimatedSprite>("Lantern6");
		var lantern7 = GetNode<AnimatedSprite>("Lantern7");
		
		lantern.Animation = (spawnPoint.Position.x <= lantern.Position.x) ? "on" : "off";
		lantern2.Animation = (spawnPoint.Position.x <= lantern2.Position.x) ? "on" : "off";
		lantern3.Animation = (spawnPoint.Position.x <= lantern3.Position.x) ? "on" : "off";
		lantern4.Animation = (spawnPoint.Position.x <= lantern4.Position.x) ? "on" : "off";
		lantern5.Animation = (spawnPoint.Position.x <= lantern5.Position.x) ? "on" : "off";
		lantern6.Animation = (spawnPoint.Position.x <= lantern6.Position.x) ? "on" : "off";
		lantern7.Animation = (spawnPoint.Position.x <= lantern7.Position.x) ? "on" : "off";
	}
	
	public void HistoryTick()
	{
		var mainNode = GetParent<Main>();
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var playerSprite = playerCharNode.GetNode<AnimatedSprite>("AnimatedSprite");
		var dryadsNode = GetNode("Dryads");
		var dryadFiresNode = GetNode("DryadFires");
		var demonsNode = GetNode("Demons");
		
		if (LevelHistory.Count >= HISTORY_MAX_RECORDS)
			LevelHistory.Dequeue();		// Dequeue the oldest item in the queue
			
		var newHistory = new HistoricalLevelState();
		newHistory.Timestamp = Time.GetUnixTimeFromSystem();
		
		var playerHistory = new PlayerHistoricalState();
		playerHistory.FacingLeft = playerSprite.FlipH;
		playerHistory.Health = playerCharNode.Health;
		playerHistory.BleedPosition = playerCharNode.BleedPosition;
		playerHistory.Position = playerCharNode.Position;
		playerHistory.Velocity = playerCharNode.Velocity;
		playerHistory.Grounded = playerCharNode.Grounded;
		playerHistory.LastAttackTimestamp = playerCharNode.LastAttackTimestamp;
		playerHistory.PendingAttackConnected = playerCharNode.PendingAttackConnected;
		playerHistory.PendingScales = playerCharNode.PendingScales;
		playerHistory.UsedScalesTimestamp = playerCharNode.UsedScalesTimestamp;
		newHistory.Player = playerHistory;

		foreach (var c in dryadsNode.GetChildren())
		{
			Dryad dryad = c as Dryad;
			if (dryad == null)
				continue;
				
			var sprite = dryad.GetNode<AnimatedSprite>("AnimatedSprite");
			
			var dryadHistory = new DryadHistoricalState();
			dryadHistory.FacingLeft = sprite.FlipH;
			dryadHistory.Health = dryad.Health;
			dryadHistory.Position = dryad.Position;
			dryadHistory.AttackState = dryad.State;
			dryadHistory.LastStartCastTimestamp = dryad.LastStartCastTimestamp;
			dryadHistory.InPlayerSwordRange = dryad.InPlayerSwordRange;
			dryadHistory.DamageId = dryad.DamageId;
			newHistory.DryadList.Add(dryadHistory);
		}

		foreach (var c in dryadFiresNode.GetChildren())
		{
			DryadFire dryadFire = c as DryadFire;
			if (dryadFire == null)
				continue;
				
			var dryadFireHistory = new DryadFireHistoricalState();
			dryadFireHistory.Position = dryadFire.Position;
			dryadFireHistory.CreatedTimestamp = dryadFire.CreatedTimestamp;
			newHistory.DryadFireList.Add(dryadFireHistory);
		}
		
		foreach (var c in demonsNode.GetChildren())
		{
			LargeDemon demon = c as LargeDemon;
			if (demon == null)
				continue;
				
			var sprite = demon.GetNode<AnimatedSprite>("AnimatedSprite");
			
			var demonHistory = new DemonHistoricalState();
			demonHistory.FacingLeft = sprite.FlipH;
			demonHistory.Health = demon.Health;
			demonHistory.Position = demon.Position;
			demonHistory.InPlayerSwordRange = demon.InPlayerSwordRange;
			demonHistory.DamageId = demon.DamageId;
			demonHistory.LastAttackTimestamp = demon.LastAttackTimestamp;
			demonHistory.AttackPending = demon.AttackPending;
			demonHistory.PlayerInSwipeRange = demon.PlayerInSwipeRange;
			
			newHistory.DemonList.Add(demonHistory);
		}

		newHistory.SpawnPoint = mainNode.GetNode<Position2D>("StartPosition").Position;
		newHistory.DamageHistory = new Queue<DamageReport>();
		foreach (var damageReport in DamageHistory)
		{
			var newDamageReport = new DamageReport();
			newDamageReport.Who = damageReport.Who;
			newDamageReport.FromPlayer = damageReport.FromPlayer;
			newDamageReport.Amount = damageReport.Amount;
			newDamageReport.Timestamp = damageReport.Timestamp;
			newHistory.DamageHistory.Enqueue(newDamageReport);
		}
		LevelHistory.Enqueue(newHistory);
	}
	
	public void ProcessHourglass()
	{
		if (LevelHistory.Count != HISTORY_MAX_RECORDS)
			return;
		
		var mainNode = GetParent<Main>();
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var playerSprite = playerCharNode.GetNode<AnimatedSprite>("AnimatedSprite");
		var dryadsNode = GetNode("Dryads");
		var dryadFiresNode = GetNode("DryadFires");
		var demonsNode = GetNode("Demons");
		
		var levelHistory = LevelHistory.Peek();
		LevelHistory.Clear();
		
		foreach (var c in dryadsNode.GetChildren())
		{
			var dryad = c as Dryad;
			if (dryad != null)
			{
				dryadsNode.RemoveChild(dryad);
				dryad.QueueFree();
			}
		}

		foreach (var c in dryadFiresNode.GetChildren())
		{
			var fire = c as DryadFire;
			if (fire != null)
			{
				dryadFiresNode.RemoveChild(fire);
				fire.QueueFree();
			}
		}

		foreach (var c in demonsNode.GetChildren())
		{
			var demon = c as LargeDemon;
			if (demon != null)
			{
				demonsNode.RemoveChild(demon);
				demon.QueueFree();
			}
		}
		
		var playerHistory = levelHistory.Player;
		playerSprite.FlipH = playerHistory.FacingLeft;
		playerCharNode.Health = playerHistory.Health;
		playerCharNode.BleedPosition = playerHistory.BleedPosition;
		playerCharNode.Position = playerHistory.Position;
		playerCharNode.Velocity = playerHistory.Velocity;
		playerCharNode.Grounded = playerHistory.Grounded;
		playerCharNode.LastAttackTimestamp = playerHistory.LastAttackTimestamp;
		playerCharNode.PendingAttackConnected = playerHistory.PendingAttackConnected;
		playerCharNode.PendingScales = playerHistory.PendingScales;
		playerCharNode.UsedScalesTimestamp = playerHistory.UsedScalesTimestamp;
		playerCharNode.PendingHourglass = false;
		playerCharNode.UsedHourglassTimestamp = 0;
		
		foreach (var report in DivineDamageHistory)
		{
			if (report.PlayerIsTarget && report.Timestamp > levelHistory.Timestamp)
			{
				playerCharNode.Health -= report.Amount;
				if (playerCharNode.Health > MAX_HEALTH)
					playerCharNode.Health = MAX_HEALTH;
			}
		}

		foreach (var dryadHistory in levelHistory.DryadList)
		{
			Dryad newDryad = (Dryad) DryadScene.Instance();
			var sprite = newDryad.GetNode<AnimatedSprite>("AnimatedSprite");
			
			sprite.FlipH = dryadHistory.FacingLeft;
			newDryad.Health = dryadHistory.Health;
			newDryad.Position = dryadHistory.Position;
			newDryad.State = dryadHistory.AttackState;
			newDryad.LastStartCastTimestamp = dryadHistory.LastStartCastTimestamp;
			newDryad.InPlayerSwordRange = dryadHistory.InPlayerSwordRange;
			newDryad.Target = playerCharNode;
			newDryad.DamageId = dryadHistory.DamageId;
		
			foreach (var report in DivineDamageHistory)
			{
				if (!report.PlayerIsTarget && report.Who == newDryad.DamageId
					&& report.Timestamp > levelHistory.Timestamp)
				{
					newDryad.Health -= report.Amount;
					if (newDryad.Health > Dryad.MAX_HEALTH)
						newDryad.Health = Dryad.MAX_HEALTH;
					if (newDryad.Health <= 0)
					{
						newDryad.QueueFree();
					}
				}
			}
			
			dryadsNode.AddChild(newDryad);
		}

		foreach (var fireHistory in levelHistory.DryadFireList)
		{
			DryadFire newFire = (DryadFire) FireScene.Instance();
			if (newFire == null)
				continue;
				
			newFire.Position = fireHistory.Position;
			newFire.CreatedTimestamp = fireHistory.CreatedTimestamp;
			newFire.Target = playerCharNode;
			
			dryadFiresNode.AddChild(newFire);
		}
		
		foreach (var demonHistory in levelHistory.DemonList)
		{
			LargeDemon newDemon = (LargeDemon) DemonScene.Instance();
			var sprite = newDemon.GetNode<AnimatedSprite>("AnimatedSprite");
			
			sprite.FlipH = demonHistory.FacingLeft;
			newDemon.Health = demonHistory.Health;
			newDemon.Position = demonHistory.Position;
			newDemon.InPlayerSwordRange = demonHistory.InPlayerSwordRange;
			newDemon.Target = playerCharNode;
			newDemon.DamageId = demonHistory.DamageId;
			newDemon.LastAttackTimestamp = demonHistory.LastAttackTimestamp;
			newDemon.AttackPending = demonHistory.AttackPending;
			newDemon.PlayerInSwipeRange = demonHistory.PlayerInSwipeRange;
			
			foreach (var report in DivineDamageHistory)
			{
				if (!report.PlayerIsTarget && report.Who == newDemon.DamageId
					&& report.Timestamp > levelHistory.Timestamp)
				{
					newDemon.Health -= report.Amount;
					if (newDemon.Health > LargeDemon.MAX_HEALTH)
						newDemon.Health = LargeDemon.MAX_HEALTH;
					if (newDemon.Health <= 0)
					{
						newDemon.QueueFree();
					}
				}
			}
			
			demonsNode.AddChild(newDemon);
		}

		mainNode.GetNode<Position2D>("StartPosition").Position = levelHistory.SpawnPoint;
		CheckLanterns();
		
		DamageHistory.Clear();
		foreach (var damageReport in levelHistory.DamageHistory)
		{
			var newDamageReport = new DamageReport();
			newDamageReport.Who = damageReport.Who;
			newDamageReport.FromPlayer = damageReport.FromPlayer;
			newDamageReport.Amount = damageReport.Amount;
			newDamageReport.Timestamp = damageReport.Timestamp;
			DamageHistory.Enqueue(newDamageReport);
		}
	}
	
	public void ProcessScales()
	{
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var dryadsNode = GetNode("Dryads");
		var demonsNode = GetNode("Demons");
		var now = Time.GetUnixTimeFromSystem();
		
		// assume expired ones have been pruned every timestep
		foreach (var damageReport in DamageHistory)
		{
			foreach (var d in dryadsNode.GetChildren())
			{
				var dryad = d as Dryad;
				if (d == null)
					continue;
				
				if (dryad.DamageId == damageReport.Who)
				{
					int halfAmount = damageReport.Amount / 2;
					
					dryad.LastAffectedTimestamp = now;
					if (damageReport.FromPlayer)
					{
						playerCharNode.Health -= halfAmount;
						dryad.Health += halfAmount;
						if (dryad.Health > Dryad.MAX_HEALTH)
							dryad.Health = Dryad.MAX_HEALTH;
							
						var divineReport1 = new DivineDamageReport();
						divineReport1.Who = 0;
						divineReport1.PlayerIsTarget = true;
						divineReport1.Amount = halfAmount;
						divineReport1.Timestamp = now;
						DivineDamageHistory.Add(divineReport1);

						var divineReport2 = new DivineDamageReport();
						divineReport2.Who = dryad.DamageId;
						divineReport2.PlayerIsTarget = false;
						divineReport2.Amount = -halfAmount;
						divineReport2.Timestamp = now;
						DivineDamageHistory.Add(divineReport2);
						
						var hitSound = playerCharNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Play();
						if (playerCharNode.Health < -100)
							playerCharNode.Health = -100;
					}
					else
					{
						dryad.Health -= halfAmount;
						playerCharNode.Health += halfAmount;
						if (playerCharNode.Health > MAX_HEALTH)
							playerCharNode.Health = MAX_HEALTH;
							
						var divineReport1 = new DivineDamageReport();
						divineReport1.Who = 0;
						divineReport1.PlayerIsTarget = true;
						divineReport1.Amount = -halfAmount;
						divineReport1.Timestamp = now;
						DivineDamageHistory.Add(divineReport1);

						var divineReport2 = new DivineDamageReport();
						divineReport2.Who = dryad.DamageId;
						divineReport2.PlayerIsTarget = false;
						divineReport2.Amount = halfAmount;
						divineReport2.Timestamp = now;
						DivineDamageHistory.Add(divineReport2);
						
						var hitSound = playerCharNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Play();
						if (dryad.Health <= 0)
						{
							var deathSound = GetParent().GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("DryadDeathSound");
							deathSound.Position = dryad.Position;
							deathSound.Play();
							dryad.QueueFree();
						}
						else
						{
							var onHitSound = GetParent().GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("DryadAttackedSound");
							onHitSound.Position = dryad.Position;
							onHitSound.Play();
						}
					}
				}
			}
			
			foreach (var d in demonsNode.GetChildren())
			{
				var demon = d as LargeDemon;
				if (d == null)
					continue;
				
				if (demon.DamageId == damageReport.Who)
				{
					int halfAmount = damageReport.Amount / 2;
					
					demon.LastAffectedTimestamp = now;
					if (damageReport.FromPlayer)
					{
						playerCharNode.Health -= halfAmount;
						demon.Health += halfAmount;
						if (demon.Health > LargeDemon.MAX_HEALTH)
							demon.Health = LargeDemon.MAX_HEALTH;
							
						var divineReport1 = new DivineDamageReport();
						divineReport1.Who = 0;
						divineReport1.PlayerIsTarget = true;
						divineReport1.Amount = halfAmount;
						divineReport1.Timestamp = now;
						DivineDamageHistory.Add(divineReport1);

						var divineReport2 = new DivineDamageReport();
						divineReport2.Who = demon.DamageId;
						divineReport2.PlayerIsTarget = false;
						divineReport2.Amount = -halfAmount;
						divineReport2.Timestamp = now;
						DivineDamageHistory.Add(divineReport2);
						
						var hitSound = playerCharNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Play();
						if (playerCharNode.Health < -100)
							playerCharNode.Health = -100;
					}
					else
					{
						demon.Health -= halfAmount;
						playerCharNode.Health += halfAmount;
						if (playerCharNode.Health > MAX_HEALTH)
							playerCharNode.Health = MAX_HEALTH;
							
						var divineReport1 = new DivineDamageReport();
						divineReport1.Who = 0;
						divineReport1.PlayerIsTarget = true;
						divineReport1.Amount = -halfAmount;
						divineReport1.Timestamp = now;
						DivineDamageHistory.Add(divineReport1);

						var divineReport2 = new DivineDamageReport();
						divineReport2.Who = demon.DamageId;
						divineReport2.PlayerIsTarget = false;
						divineReport2.Amount = halfAmount;
						divineReport2.Timestamp = now;
						DivineDamageHistory.Add(divineReport2);
						
						var hitSound = playerCharNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Play();
						if (demon.Health <= 0)
						{
//							var deathSound = GetParent().GetNode("MediaNode")
//								.GetNode<AudioStreamPlayer2D>("DryadDeathSound");
//							deathSound.Position = dryad.Position;
//							deathSound.Play();
							demon.QueueFree();
						}
						else
						{
//							var onHitSound = GetParent().GetNode("MediaNode")
//								.GetNode<AudioStreamPlayer2D>("DryadAttackedSound");
//							onHitSound.Position = dryad.Position;
//							onHitSound.Play();
						}
					}
				}
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		bool checkedQueue = false;
		while (DamageHistory.Count != 0 && !checkedQueue)
		{
			var report = DamageHistory.Peek();
			if (report.Timestamp + DAMAGE_REPORT_LIFESPAN_SECS < Time.GetUnixTimeFromSystem())
				DamageHistory.Dequeue();
			else
				checkedQueue = true;
		}
	}
	
	// TODO: Consider moving a lot of this code to _PhysicsProcess
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
		{
//			var mainNode = GetParent<Main>();
//			mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer>("LanternSound").Play();
			GetParent<Main>().ProcessPlayerDeath();
		}
		
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
