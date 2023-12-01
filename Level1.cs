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
	public bool IsBoss;
	public bool IsHostile;
	public int MaxHealth;
	public int Health;
	public Vector2 Position;
	public Vector2 Velocity;
	public bool Grounded;
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
	public uint DamageId;
	public bool FromBoss;
	public double CreatedTimestamp;
}

public class DemonHistoricalState
{
	public bool FacingLeft;
	public int MaxHealth;
	public int Health;
	public Vector2 Position;
	public Vector2 Velocity;
	public bool Grounded;
	public bool InPlayerSwordRange;
	public uint DamageId;
	public bool Alive;
	public double DeathTimestamp;
	public double LastAttackTimestamp;
	public bool AttackPending;
	public bool PlayerInSwipeRange;
}

public class GoblinHistoricalState
{
	public bool FacingLeft;
	public int Health;
	public Vector2 Position;
	public Vector2 Velocity;
	public bool Grounded;
	public bool InPlayerSwordRange;
	public uint DamageId;
	public bool Alive;
	public double DeathTimestamp;
	public double LastAttackTimestamp;
	public bool AttackPending;
	public bool PlayerInAttackRange;
}

public class HistoricalLevelState
{
	public double Timestamp;
	public PlayerHistoricalState Player;
	public Vector2 SpawnPoint;
	public List<DryadHistoricalState> DryadList = new List<DryadHistoricalState>();
	public List<DryadFireHistoricalState> DryadFireList = new List<DryadFireHistoricalState>();
	public List<DemonHistoricalState> DemonList = new List<DemonHistoricalState>();
	public List<GoblinHistoricalState> GoblinList = new List<GoblinHistoricalState>();
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
	public const int HISTORY_MAX_RECORDS = 10;
	public const int HISTORY_SAMPLES_PER_SECOND = 2;
	
	public const int DAMAGE_REPORT_LIFESPAN_SECS = 5;
	
	public const float LANTERN_DISTANCE = 30f;
	public const float SPAWN_OFFSET_Y = -20f;
	public bool ShownStory1 = false;
	public bool ShownStory2 = false;
	public bool ShownStory3 = false;
	public bool ShownStory4 = false;
	public bool ShownStory5 = false;
	
	public bool GameOver = false;
	
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
	
	[Export]
	public PackedScene GoblinScene { get; set; }
#pragma warning restore 649

	public bool PlayerWasTouchingLantern = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
//		List<Vector2> dryadSpawnLocations = new List<Vector2>();
//		dryadSpawnLocations.Add(new Vector2(1393, 153));
//		dryadSpawnLocations.Add(new Vector2(1459, 153));
//		dryadSpawnLocations.Add(new Vector2(1593, 153));
//		dryadSpawnLocations.Add(new Vector2(1659, 153));
//		var dryadsNode = GetNodeOrNull("Dryads");
//
//		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
//
//		foreach (Vector2 location in dryadSpawnLocations)
//		{
//			Dryad dryad = (Dryad)DryadScene.Instance();
//			dryad.Target = PlayerCharNode;
//			dryad.Position = location;
//			dryad.DamageId = GD.Randi();
//			dryadsNode.AddChild(dryad);
//		}
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
		playerCharNode.Position = spawnPoint.Position + new Vector2( 0, SPAWN_OFFSET_Y );
		
		CheckLanterns();
	}
	
	public void CheckLanterns()
	{
		var spawnPoint = GetParent().GetNode<Position2D>("StartPosition");
		
		foreach (var c in GetNode("Lanterns").GetChildren())
		{
			var lantern = c as Lantern;
			
			if (lantern == null)
				continue;
				
			lantern.Animation = (spawnPoint.Position.x >= lantern.Position.x) ? "on" : "off";
		}
	}
	
	public void HistoryTick()
	{
		var mainNode = GetParent<Main>();
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var playerSprite = playerCharNode.GetNode<AnimatedSprite>("AnimatedSprite");
		var dryadsNode = GetNode("Dryads");
		var dryadFiresNode = GetNode("DryadFires");
		var demonsNode = GetNode("Demons");
		var goblinsNode = GetNode("Goblins");
		
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
			dryadHistory.IsHostile = dryad.IsHostile;
			dryadHistory.IsBoss = dryad.IsBoss;
			dryadHistory.MaxHealth = dryad.MaxHealth;
			dryadHistory.Health = dryad.Health;
			dryadHistory.Velocity = dryad.Velocity;
			dryadHistory.Grounded = dryad.Grounded;
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
			if (dryadFire == null || dryadFire.Boomed)
				continue;
				
			var dryadFireHistory = new DryadFireHistoricalState();
			dryadFireHistory.Position = dryadFire.Position;
			dryadFireHistory.DamageId = dryadFire.DamageId;
			dryadFireHistory.FromBoss = dryadFire.FromBoss;
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
			demonHistory.MaxHealth = demon.Health;
			demonHistory.Health = demon.Health;
			demonHistory.Grounded = demon.Grounded;
			demonHistory.Velocity = demon.Velocity;
			demonHistory.Position = demon.Position;
			demonHistory.InPlayerSwordRange = demon.InPlayerSwordRange;
			demonHistory.DamageId = demon.DamageId;
			demonHistory.Alive = demon.Alive;
			demonHistory.DeathTimestamp = demon.DeathTimestamp;
			demonHistory.LastAttackTimestamp = demon.LastAttackTimestamp;
			demonHistory.AttackPending = demon.AttackPending;
			demonHistory.PlayerInSwipeRange = demon.PlayerInSwipeRange;
			
			newHistory.DemonList.Add(demonHistory);
		}
		
		foreach (var c in goblinsNode.GetChildren())
		{
			Goblin goblin = c as Goblin;
			if (goblin == null)
				continue;
				
			var sprite = goblin.GetNode<AnimatedSprite>("AnimatedSprite");
			
			var goblinHistory = new GoblinHistoricalState();
			goblinHistory.FacingLeft = sprite.FlipH;
			goblinHistory.Health = goblin.Health;
			goblinHistory.Grounded = goblin.Grounded;
			goblinHistory.Velocity = goblin.Velocity;
			goblinHistory.Position = goblin.Position;
			goblinHistory.InPlayerSwordRange = goblin.InPlayerSwordRange;
			goblinHistory.DamageId = goblin.DamageId;
			goblinHistory.Alive = goblin.Alive;
			goblinHistory.DeathTimestamp = goblin.DeathTimestamp;
			goblinHistory.LastAttackTimestamp = goblin.LastAttackTimestamp;
			goblinHistory.AttackPending = goblin.AttackPending;
			goblinHistory.PlayerInAttackRange = goblin.PlayerInAttackRange;
			
			newHistory.GoblinList.Add(goblinHistory);
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
		var now = Time.GetUnixTimeFromSystem();
		
		if (LevelHistory.Count != HISTORY_MAX_RECORDS)
			return;
		
		var mainNode = GetParent<Main>();
		var mediaNode = mainNode.GetNode("MediaNode");
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
		var playerSprite = playerCharNode.GetNode<AnimatedSprite>("AnimatedSprite");
		var dryadsNode = GetNode("Dryads");
		var dryadFiresNode = GetNode("DryadFires");
		var demonsNode = GetNode("Demons");
		var goblinsNode = GetNode("Goblins");
		
		var levelHistory = LevelHistory.Peek();
		LevelHistory.Clear();
		
		double timeDiff = now - levelHistory.Timestamp;
		
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

		foreach (var c in goblinsNode.GetChildren())
		{
			var goblin = c as Goblin;
			if (goblin != null)
			{
				goblinsNode.RemoveChild(goblin);
				goblin.QueueFree();
			}
		}
		
		var playerHistory = levelHistory.Player;
		playerSprite.FlipH = playerHistory.FacingLeft;
		playerCharNode.Health = playerHistory.Health;
		playerCharNode.BleedPosition = playerHistory.BleedPosition;
		playerCharNode.Position = playerHistory.Position;
		playerCharNode.Velocity = playerHistory.Velocity;
		playerCharNode.Grounded = playerHistory.Grounded;
		playerCharNode.LastAttackTimestamp = playerHistory.LastAttackTimestamp + timeDiff;
		playerCharNode.PendingAttackConnected = playerHistory.PendingAttackConnected;
		playerCharNode.PendingScales = playerHistory.PendingScales;
		playerCharNode.UsedScalesTimestamp = playerHistory.UsedScalesTimestamp == 0 ? 0 : playerHistory.UsedScalesTimestamp + timeDiff;
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
			newDryad.IsHostile = dryadHistory.IsHostile;
			newDryad.IsBoss = dryadHistory.IsBoss;
			newDryad.MaxHealth = dryadHistory.MaxHealth;
			newDryad.Health = dryadHistory.Health;
			newDryad.Velocity = dryadHistory.Velocity;
			newDryad.Grounded = dryadHistory.Grounded;
			newDryad.Position = dryadHistory.Position;
			newDryad.State = dryadHistory.AttackState;
			newDryad.LastStartCastTimestamp = dryadHistory.LastStartCastTimestamp + timeDiff;
			newDryad.InPlayerSwordRange = dryadHistory.InPlayerSwordRange;
			newDryad.Target = playerCharNode;
			newDryad.DamageId = dryadHistory.DamageId;
		
			foreach (var report in DivineDamageHistory)
			{
				if (!report.PlayerIsTarget && report.Who == newDryad.DamageId
					&& report.Timestamp > levelHistory.Timestamp)
				{
					newDryad.Health -= report.Amount;
					if (newDryad.Health > newDryad.MaxHealth)
						newDryad.Health = newDryad.MaxHealth;
					if (newDryad.Health <= 0)
					{
						if (newDryad.IsBoss && !ShownStory5)
						{
							var hud = mainNode.GetNode<HUD>("HUD");
							
							List<String> lines = new List<string>();
							lines.Add("Zealot. You don't know what you're doing.");
							lines.Add("Your justice is no one's justice.");
							lines.Add("Did the high priest send you?");
							hud.ShowDialog(lines);
							ShownStory5 = true;
							GetNode<Timer>("EndGameTimer").Start();
							GameOver = true;
						}
						else
						{
							newDryad.QueueFree();
						}
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
			newFire.CreatedTimestamp = fireHistory.CreatedTimestamp + timeDiff;
			newFire.DamageId = fireHistory.DamageId;
			newFire.FromBoss = fireHistory.FromBoss;
			newFire.Target = playerCharNode;
			
			dryadFiresNode.AddChild(newFire);
		}
		
		foreach (var demonHistory in levelHistory.DemonList)
		{
			LargeDemon newDemon = (LargeDemon) DemonScene.Instance();
			var sprite = newDemon.GetNode<AnimatedSprite>("AnimatedSprite");
			
			sprite.FlipH = demonHistory.FacingLeft;
			newDemon.MaxHealth = demonHistory.MaxHealth;
			newDemon.Health = demonHistory.Health;
			newDemon.Velocity = demonHistory.Velocity;
			newDemon.Grounded = demonHistory.Grounded;
			newDemon.Position = demonHistory.Position;
			newDemon.InPlayerSwordRange = demonHistory.InPlayerSwordRange;
			newDemon.Target = playerCharNode;
			newDemon.DamageId = demonHistory.DamageId;
			newDemon.Alive = demonHistory.Alive;
			newDemon.DeathTimestamp = demonHistory.DeathTimestamp == 0 ? 0 : demonHistory.DeathTimestamp + timeDiff;
			newDemon.LastAttackTimestamp = demonHistory.LastAttackTimestamp + timeDiff;
			newDemon.AttackPending = demonHistory.AttackPending;
			newDemon.PlayerInSwipeRange = demonHistory.PlayerInSwipeRange;
			
			foreach (var report in DivineDamageHistory)
			{
				if (!report.PlayerIsTarget && report.Who == newDemon.DamageId
					&& report.Timestamp > levelHistory.Timestamp)
				{
					newDemon.Health -= report.Amount;
					if (newDemon.Health > newDemon.MaxHealth)
						newDemon.Health = newDemon.MaxHealth;
					if (newDemon.Health <= 0)
					{
						var deathSound = mediaNode
							.GetNode<AudioStreamPlayer2D>("DemonDeath");
						deathSound.Position = newDemon.Position;
						deathSound.Play();
						
						newDemon.Alive = false;
						newDemon.DeathTimestamp = now;
					}
				}
			}
			
			demonsNode.AddChild(newDemon);
		}
		
		foreach (var goblinHistory in levelHistory.GoblinList)
		{
			Goblin newGoblin = (Goblin) GoblinScene.Instance();
			var sprite = newGoblin.GetNode<AnimatedSprite>("AnimatedSprite");
			
			sprite.FlipH = goblinHistory.FacingLeft;
			newGoblin.Health = goblinHistory.Health;
			newGoblin.Velocity = goblinHistory.Velocity;
			newGoblin.Grounded = goblinHistory.Grounded;
			newGoblin.Position = goblinHistory.Position;
			newGoblin.InPlayerSwordRange = goblinHistory.InPlayerSwordRange;
			newGoblin.Target = playerCharNode;
			newGoblin.DamageId = goblinHistory.DamageId;
			newGoblin.Alive = goblinHistory.Alive;
			newGoblin.DeathTimestamp = goblinHistory.DeathTimestamp == 0 ? 0 : goblinHistory.DeathTimestamp + timeDiff;
			newGoblin.LastAttackTimestamp = goblinHistory.LastAttackTimestamp + timeDiff;
			newGoblin.AttackPending = goblinHistory.AttackPending;
			newGoblin.PlayerInAttackRange = goblinHistory.PlayerInAttackRange;
			
			foreach (var report in DivineDamageHistory)
			{
				if (!report.PlayerIsTarget && report.Who == newGoblin.DamageId
					&& report.Timestamp > levelHistory.Timestamp)
				{
					newGoblin.Health -= report.Amount;
					if (newGoblin.Health > Goblin.MAX_HEALTH)
						newGoblin.Health = Goblin.MAX_HEALTH;
					if (newGoblin.Health <= 0)
					{
						var deathSound = mediaNode
							.GetNode<AudioStreamPlayer2D>("GoblinDeathSound");
						deathSound.Position = newGoblin.Position;
						deathSound.Play();
						
						newGoblin.Alive = false;
						newGoblin.DeathTimestamp = now;
					}
				}
			}
			
			goblinsNode.AddChild(newGoblin);
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
		var mainNode = GetParent();
		var mediaNode = mainNode.GetNode("MediaNode");
		var dryadsNode = GetNode("Dryads");
		var demonsNode = GetNode("Demons");
		var goblinsNode = GetNode("Goblins");
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
						if (dryad.Health > dryad.MaxHealth)
							dryad.Health = dryad.MaxHealth;
							
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
						
						var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Position = playerCharNode.Position;
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
						
						var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Position = dryad.Position;
						hitSound.Play();
						if (dryad.Health <= 0)
						{
							var deathSound = GetParent().GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("DryadDeathSound");
							deathSound.Position = dryad.Position;
							deathSound.Play();
				
							if (dryad.IsBoss && !ShownStory5)
							{
								var hud = mainNode.GetNode<HUD>("HUD");
								
								List<String> lines = new List<string>();
								lines.Add("Zealot. You don't know what you're doing.");
								lines.Add("Your justice is no one's justice.");
								lines.Add("Did the high priest send you?");
								hud.ShowDialog(lines);
								ShownStory5 = true;
								GetNode<Timer>("EndGameTimer").Start();
								GameOver = true;
							}
							else
							{
								dryad.QueueFree();
							}
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
						if (demon.Health > demon.MaxHealth)
							demon.Health = demon.MaxHealth;
							
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
						
						var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Position = playerCharNode.Position;
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
						
						var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Position = demon.Position;
						hitSound.Play();
						if (demon.Health <= 0)
						{
							var deathSound = mainNode.GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("DemonDeath");
							deathSound.Position = demon.Position;
							deathSound.Play();
					
							demon.Alive = false;
							demon.DeathTimestamp = now;
						}
						else
						{
							var onHitSound = GetParent().GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("DemonIsHurt");
							onHitSound.Position = demon.Position;
							onHitSound.Play();
						}
					}
				}
			}
			
			foreach (var d in goblinsNode.GetChildren())
			{
				var goblin = d as Goblin;
				if (d == null)
					continue;
				
				if (goblin.DamageId == damageReport.Who)
				{
					int halfAmount = damageReport.Amount / 2;
					
					goblin.LastAffectedTimestamp = now;
					if (damageReport.FromPlayer)
					{
						playerCharNode.Health -= halfAmount;
						goblin.Health += halfAmount;
						if (goblin.Health > Goblin.MAX_HEALTH)
							goblin.Health = Goblin.MAX_HEALTH;
							
						var divineReport1 = new DivineDamageReport();
						divineReport1.Who = 0;
						divineReport1.PlayerIsTarget = true;
						divineReport1.Amount = halfAmount;
						divineReport1.Timestamp = now;
						DivineDamageHistory.Add(divineReport1);

						var divineReport2 = new DivineDamageReport();
						divineReport2.Who = goblin.DamageId;
						divineReport2.PlayerIsTarget = false;
						divineReport2.Amount = -halfAmount;
						divineReport2.Timestamp = now;
						DivineDamageHistory.Add(divineReport2);
						
						var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Position = playerCharNode.Position;
						hitSound.Play();
						if (playerCharNode.Health < -100)
							playerCharNode.Health = -100;
					}
					else
					{
						goblin.Health -= halfAmount;
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
						divineReport2.Who = goblin.DamageId;
						divineReport2.PlayerIsTarget = false;
						divineReport2.Amount = halfAmount;
						divineReport2.Timestamp = now;
						DivineDamageHistory.Add(divineReport2);
						
						var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
						hitSound.Position = goblin.Position;
						hitSound.Play();
						if (goblin.Health <= 0)
						{
							var deathSound = mainNode.GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("GoblinDeathSound");
							deathSound.Position = goblin.Position;
							deathSound.Play();
					
							goblin.Alive = false;
							goblin.DeathTimestamp = now;
						}
						else
						{
							var onHitSound = GetParent().GetNode("MediaNode")
								.GetNode<AudioStreamPlayer2D>("GoblinAttackedSound");
							onHitSound.Position = goblin.Position;
							onHitSound.Play();
						}
					}
				}
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		bool checkedQueue = false;
		int loop_count = 0;
		while (loop_count < 10 && DamageHistory.Count != 0 && !checkedQueue)
		{
			loop_count++;
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
		var mainNode = GetParent<Main>();
		var now = Time.GetUnixTimeFromSystem();
		
		if (Input.IsActionJustPressed("show_menu"))
		{
			GetTree().Paused = true;
			GetParent().GetNode<HUD>("HUD").ShowGameMenu();
		}
		
		var playerCharNode = GetNode<PlayerChar>("PlayerChar");
	
		// Reset player char if it goes below a certain Y coordinate
		//var startPosition = GetNode<Position2D>("StartPosition");
		if (playerCharNode.Position.y > PlayerChar.MIN_Y_COORD)
			mainNode.ProcessPlayerDeath();
		
		// Check distance to lantern
		var hud = mainNode.GetNode<HUD>("HUD");
		var mediaNode = mainNode.GetNode<Node>("MediaNode");
		var spawnPoint = mainNode.GetNode<Position2D>("StartPosition");
		
		foreach (var c in GetNode("Dryads").GetChildren())
		{
			Dryad dryad = c as Dryad;
			if (dryad == null)
				continue;
				
			if (dryad.Position.y > PlayerChar.MIN_Y_COORD)
			{
				var deathSound = mediaNode
					.GetNode<AudioStreamPlayer2D>("DryadDeathSound");
				deathSound.Position = dryad.Position;
				deathSound.Play();
				
				if (dryad.IsBoss && !ShownStory5)
				{
					List<String> lines = new List<string>();
					lines.Add("Zealot. You don't know what you're doing.");
					lines.Add("Your justice is no one's justice.");
					lines.Add("Did the high priest send you?");
					hud.ShowDialog(lines);
					ShownStory5 = true;
					GetNode<Timer>("EndGameTimer").Start();
					GameOver = true;
				}
				else
				{
					dryad.QueueFree();
				}
			}	
		}
			
		foreach (var c in GetNode("Demons").GetChildren())
		{
			LargeDemon demon = c as LargeDemon;
			if (demon == null)
				continue;
				
			if (!demon.Alive)
				continue;
				
			if (demon.Position.y > PlayerChar.MIN_Y_COORD)
			{
				var deathSound = mainNode.GetNode("MediaNode")
					.GetNode<AudioStreamPlayer2D>("DemonDeath");
				deathSound.Position = demon.Position;
				deathSound.Play();

				demon.Alive = false;
				demon.DeathTimestamp = now;
			}
		}
			
		foreach (var c in GetNode("Goblins").GetChildren())
		{
			Goblin goblin = c as Goblin;
			if (goblin == null)
				continue;
			
			if (!goblin.Alive)
				continue;
			
			if (goblin.Position.y > PlayerChar.MIN_Y_COORD)
			{
				var deathSound = mediaNode
					.GetNode<AudioStreamPlayer2D>("GoblinDeathSound");
				deathSound.Position = goblin.Position;
				deathSound.Play();
				
				goblin.Alive = false;
				goblin.DeathTimestamp = now;
			}
		}
		
		bool showHint = false;
		bool touchingAnyLantern = false;
		foreach (var c in GetNode("Lanterns").GetChildren())
		{
			var lantern = c as Lantern;
			if (lantern == null)
				continue;
				
			if (
				Math.Abs(playerCharNode.Position.x - lantern.Position.x) > LANTERN_DISTANCE
				|| Math.Abs(playerCharNode.Position.y - lantern.Position.y) > LANTERN_DISTANCE * 3
			)
				continue;

			touchingAnyLantern = true;
			if (lantern.Text != null && lantern.Text != "")
			{
				showHint = true;
				hud.ShowHint(lantern.Text);
			}
			if (!PlayerWasTouchingLantern)
			{
				mediaNode.GetNode<AudioStreamPlayer>("LanternSound").Play();
				PlayerWasTouchingLantern = true;
				playerCharNode.Health = MAX_HEALTH;
				playerCharNode.PendingHourglass = false;
				playerCharNode.UsedHourglassTimestamp = 0;
				playerCharNode.PendingScales = false;
				playerCharNode.UsedScalesTimestamp = 0;
			}
			
			if (lantern.Animation == "on")
				continue;
			
			lantern.Animation = "on";
			if (!lantern.DisableCheckpoint && spawnPoint.Position.x < lantern.Position.x)
				spawnPoint.Position = lantern.Position;
		}
		
		if (!touchingAnyLantern)
			PlayerWasTouchingLantern = false;
		
		if (!showHint)
			hud.HideHint();
	}
	
	private void StoryText1Activated(object body)
	{
		if (!(body is PlayerChar))
			return;
		
		var mainNode = GetParent<Main>();
		var hud = mainNode.GetNode<HUD>("HUD");
		
		if (!ShownStory1)
		{
			const String STORY_TEXT_1_1 = "Why are you here? Are you lost?";
			const String STORY_TEXT_1_2 = "Or perhaps, carrying out some mission?";
	
			List<String> lines = new List<string>();
			lines.Add(STORY_TEXT_1_1);
			lines.Add(STORY_TEXT_1_2);
			hud.ShowDialog(lines);
			ShownStory1 = true;
		}
	}

	private void MoveBossTrigger1To2(object body)
	{
		if (!(body is PlayerChar))
			return;
			
		var StoryNode = GetNode<Position2D>("BossStoryLocation2");
		
		var DryadsNode = GetNode("Dryads");
		foreach (var c in DryadsNode.GetChildren())
		{
			var dryad = c as Dryad;
			if (dryad == null)
				continue;
			if (dryad.IsBoss)
			{
				dryad.Position = StoryNode.Position;
				dryad.Grounded = false;
			}
		}
	}

	private void StoryText2Activated(object body)
	{
		if (!(body is PlayerChar))
			return;
			
		var mainNode = GetParent<Main>();
		var hud = mainNode.GetNode<HUD>("HUD");
		
		if (!ShownStory2)
		{
			const String STORY_TEXT_2_1 = "Having fun killing the forest inhabitants?";
			const String STORY_TEXT_2_2 = "I don't care what mission you've been given.";
			const String STORY_TEXT_2_3 = "Your scales of \"justice\" are a hypocrisy. Your justice is yours alone.";
	
			List<String> lines = new List<string>();
			lines.Add(STORY_TEXT_2_1);
			lines.Add(STORY_TEXT_2_2);
			lines.Add(STORY_TEXT_2_3);
			hud.ShowDialog(lines);
			ShownStory2 = true;
		}
	}

	private void MoveBossTrigger2To3(object body)
	{
		if (!(body is PlayerChar))
			return;
			
		var StoryNode = GetNode<Position2D>("BossStoryLocation3");
		
		var DryadsNode = GetNode("Dryads");
		foreach (var c in DryadsNode.GetChildren())
		{
			var dryad = c as Dryad;
			if (dryad == null)
				continue;
			if (dryad.IsBoss)
			{
				dryad.Position = StoryNode.Position;
				dryad.Grounded = false;
			}
		}
	}

	private void StoryText3Activated(object body)
	{
		if (!(body is PlayerChar))
			return;
	
		var mainNode = GetParent<Main>();
		var hud = mainNode.GetNode<HUD>("HUD");
		
		if (!ShownStory3)
		{
			const String STORY_TEXT_3_1 = "Enough of your justice.";
		
			List<String> lines = new List<string>();
			lines.Add(STORY_TEXT_3_1);
			hud.ShowDialog(lines);
			ShownStory3 = true;
		
			MoveBossTrigger2To3(body);
		}
	}

	private void StoryText4Activated(object body)
	{
		if (!(body is PlayerChar))
			return;
			
		const String STORY_TEXT_4_1 = "It is time to enact my own!";
	
		var mainNode = GetParent<Main>();
		var hud = mainNode.GetNode<HUD>("HUD");
		
		if (!ShownStory4)
		{
			List<String> lines = new List<string>();
			lines.Add(STORY_TEXT_4_1);
			hud.ShowDialog(lines);
			ShownStory4 = true;
		}
		
		var DryadsNode = GetNode("Dryads");
		foreach (var c in DryadsNode.GetChildren())
		{
			var dryad = c as Dryad;
			if (dryad == null)
				continue;
			if (dryad.IsBoss)
			{
				dryad.IsHostile = true;
			}
		}
	}
	
	private void HideStory(object body)
	{
		var mainNode = GetParent<Main>();
		var hud = mainNode.GetNode<HUD>("HUD");
		
		hud.GetNode<Panel>("InfoPanel").Hide();
	}
	
	private void EndGameTimerTimeout()
	{
		Main mainNode = GetParent<Main>();
		var hud = mainNode.GetNode<HUD>("HUD");

		mainNode.GetNode<Position2D>("StartPosition").Position = new Vector2(-984, 148);
		mainNode.GetNode<Camera2D>("MainMenuCam").Current = true;
		mainNode.Level.GetNode<PlayerChar>("PlayerChar").GetNode<Camera2D>("Camera2D").Current = false;
		mainNode.GetNode("Level1").QueueFree();
		mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer>("Music").Stop();
		mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer>("MenuMusic").Play();
		hud.GetNode<Label>("VictoryLabel").Hide();
		hud.GetNode<TextureRect>("MainMenuLogo").Show();
		hud.GetNode<Label>("MainMenuLabel1").Show();
		hud.GetNode<Label>("MainMenuLabel2").Show();
		hud.GetNode<Panel>("MainMenuPanel").Show();
		hud.GetNode<Panel>("RestartDialogPanel").Hide();
		hud.GetNode<Node2D>("HPBar").Hide();
		hud.GetNode<TextureRect>("HourglassIcon").Hide();
		hud.GetNode<TextureRect>("ScalesIcon").Hide();
		hud.GetNode<AnimatedSprite>("ItemOverlay").Hide();
		hud.GetNode<Panel>("MiniInfoPanel").Hide();
		hud.GetNode<Panel>("InfoPanel").Hide();
		hud.GetNode<Panel>("PauseMenuPanel").Hide();
		hud.GetNode<ColorRect>("DeathScreen").Hide();
		GetTree().Paused = false;
		GameOver = false;
	}
}
