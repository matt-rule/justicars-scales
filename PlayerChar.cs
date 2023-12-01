using Godot;
using System;
using System.Collections.Generic;

public enum InputState
{
	None = 0,
	Left = 1,
	Right = 2
}

public class PlayerChar : KinematicBody2D
{
	public static int MOVE_SPEED = 250; // How fast the player will move (pixels/sec).
	public static int JUMP_FORCE = 350;
	public static int GRAVITY = 900;
	public static int MIN_Y_COORD = 480;
	public static int HP_BAR_WIDTH = 85;
	public static int HP_BAR_HEIGHT = 8;
	public static int BLEED_BAR_POS_X = 85;
	public static int BLEED_BAR_POS_Y = 0;
	public static int MAX_HEALTH = 100;
	
	public static float BLEED_SPEED = 40f;
	
	// How long it takes for the attack to connect with enemy
	// (not for the timer to finish)
	public const double ATTACK_DELAY = 0.12;
	
	// from start, not from attack connected with enemy
	public const double ATTACK_ANIMATION_FINISHED = 0.5;
	
	public const double HOURGLASS_DELAY = 0.4;
	public const double SCALES_DELAY = 0.4;
	
	public const int SWORD_DAMAGE_DRYAD = 10;
	public const int SWORD_DAMAGE_DEMON = 2;
	public const int SWORD_DAMAGE_GOBLIN = 12;
	
	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public float BleedPosition = MAX_HEALTH;
	
	[Export]
	public InputState InputState = InputState.None;
	
	[Export]
	public Vector2 Velocity = new Vector2();
	
	public bool Grounded = false;
	public double LastAttackTimestamp = 0;
	public bool PendingAttackConnected = false;
	public bool PendingHourglass = false;
	public double UsedHourglassTimestamp = 0;
	public bool PendingScales = false;
	public double UsedScalesTimestamp = 0;
	
	[Export]
	public List<DryadFire> EffectsInRange = new List<DryadFire>();

	// TODO: Undo these changes when game ends
	public void Start(Vector2 pos)
	{
		//Position = pos;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	
	// Kinematic body movement should only be done in the _physics_process() callback.
	// Things to test if this function changes:
	// 1. Floor collision / ability to jump
	// 2. Wall collision
	// 3. Walking off a platform
	// 4. Max speed
	// 5. How quickly the character slows down
	public override void _PhysicsProcess(float delta)
	{
		double now = Time.GetUnixTimeFromSystem();
		var levelNode = GetParent<Level1>();
		
		if (PendingAttackConnected && LastAttackTimestamp + ATTACK_DELAY < Time.GetUnixTimeFromSystem())
		{
			PendingAttackConnected = false;
			OnAttackProcess();
		}
		
		var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

		if (Velocity.Length() > 0)
		{
			animatedSprite.Play();
		}
		
		if (Input.IsActionPressed("attack"))
		{
			animatedSprite.Animation = "attack";
			if (LastAttackTimestamp + ATTACK_ANIMATION_FINISHED < Time.GetUnixTimeFromSystem())
			{
				LastAttackTimestamp = Time.GetUnixTimeFromSystem();
				PendingAttackConnected = true;
			}
		}
		
		if (Input.IsActionJustPressed("use_hourglass"))
		{
			if (!PendingScales && !PendingHourglass
				&& levelNode.LevelHistory.Count == Level1.HISTORY_MAX_RECORDS
				&& LastAttackTimestamp + ATTACK_ANIMATION_FINISHED < now)
			{
				PendingHourglass = true;
				UsedHourglassTimestamp = now;
			}
		}
		
		if (Input.IsActionJustPressed("use_scales"))
		{
			if (!PendingScales && !PendingHourglass
				&& UsedScalesTimestamp == 0
				&& LastAttackTimestamp + ATTACK_ANIMATION_FINISHED < now)
			{
				PendingScales = true;
				UsedScalesTimestamp = now;
			}
		}

		if (PendingHourglass
			&& UsedHourglassTimestamp + HOURGLASS_DELAY < now)
		{
			PendingHourglass = false;
			
			var main = GetParent().GetParent();
			var hud = main.GetNode<HUD>("HUD");
			var media = main.GetNode("MediaNode");
			
			var level = GetParent<Level1>();
			level.ProcessHourglass();

			var overlay = hud.GetNode<AnimatedSprite>("ItemOverlay");
			overlay.Animation = "hourglass";
			overlay.Modulate = new Color(1, 1, 1, 0);
			overlay.Show();

			var tween = GetTree().CreateTween();
			tween.TweenProperty(overlay, "modulate",
				new Color(1, 1, 1, 0.8f), 0.4f);
			tween.TweenInterval( 2 );
			tween.TweenProperty(overlay, "modulate",
				new Color(1, 1, 1, 0), 1.2f);
				
			var hourglassSound = media.GetNode<AudioStreamPlayer>("HourglassSound");
			hourglassSound.Play();
		}

		if (PendingScales
			&& UsedScalesTimestamp + SCALES_DELAY < now)
		{
			PendingScales = false;
			
			var main = GetParent().GetParent();
			var hud = main.GetNode<HUD>("HUD");
			var media = main.GetNode("MediaNode");
			
			var level = main.GetNode<Level1>("Level1");
			level.ProcessScales();

			var overlay = hud.GetNode<AnimatedSprite>("ItemOverlay");
			overlay.Animation = "scales";
			overlay.Modulate = new Color(1, 1, 1, 0);
			overlay.Show();

			var tween = GetTree().CreateTween();
			tween.TweenProperty(overlay, "modulate",
				new Color(1, 1, 1, 0.8f), 0.4f);
			tween.TweenInterval( 2 );
			tween.TweenProperty(overlay, "modulate",
				new Color(1, 1, 1, 0), 1.2f);
				
			var scalesSound = media.GetNode<AudioStreamPlayer>("ScalesSound");
			scalesSound.Play();
		}

		if (Velocity.x != 0)
		{
			animatedSprite.Animation = "running";
			// See the note below about boolean assignment.
		}
		else
		{
			if (animatedSprite.Animation != "attack")
				animatedSprite.Animation = "standing";
		}
		
		if ( LastAttackTimestamp + ATTACK_ANIMATION_FINISHED > Time.GetUnixTimeFromSystem() )
			Velocity.x = 0;
		else
		{
			var directionX = Input.GetActionStrength("move_right")
				- Input.GetActionStrength("move_left");
			Velocity.x = directionX * MOVE_SPEED;
			
			var swordCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwordCollisionShape");
			if (Velocity.x < 0)
			{
				animatedSprite.FlipH = true;
				swordCollisionShape.Position = new Vector2 (-15, -3);
			}
			else if (Velocity.x > 0)
			{
				animatedSprite.FlipH = false;
				swordCollisionShape.Position = new Vector2 (15, -3);
			}
			animatedSprite.FlipV = false;
		}

		// For horizontal collisions, turn OFF "Selected Collision On"
		// for every tile in the tile map. It has to be false.

		// Process vertical movement first to allow jumping,
		// because horizontal movement sets Grounded := false.
		bool attacking = LastAttackTimestamp + ATTACK_ANIMATION_FINISHED > Time.GetUnixTimeFromSystem();
		if (Grounded)
		{
			if (!attacking)
			{
				if (Input.IsActionPressed("move_up"))
				{
					Velocity.y = -JUMP_FORCE;
					Grounded = false;
					var jumpSound = GetNode<AudioStreamPlayer2D>("JumpSound");
					if (!jumpSound.Playing)
						jumpSound.Play();
				}
				else
					Velocity.y = 0;
			}
		}
		else
			Velocity.y += GRAVITY * delta; // Gravity

		bool wasGrounded = Grounded;
		if (Velocity.x != 0)
		{
			if (Grounded && !IsOnWall())
			{
				var grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
				if (!grassSound.Playing)
					grassSound.Play();
			}
			Grounded = false;
		}

		float oldYVelocity = Velocity.y;

		// Up vector required for IsOnFloor
		Velocity = MoveAndSlide(Velocity, Vector2.Up);
		// Note there is also MoveAndSlideWithSnap
		
		if (IsOnFloor())
		{
			Grounded = true;
			
			if (!wasGrounded)
			{
				var grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
				if (!grassSound.Playing)
					grassSound.Play();
					
				if (oldYVelocity > 360)
				{
					var landingSound =
						GetParent().GetParent().GetNode("MediaNode")
							.GetNode<AudioStreamPlayer2D>("LandingSound");
					landingSound.Position = Position;
					landingSound.Play();
				}
			}
		}
		
		if (Health < BleedPosition)
			BleedPosition -= BLEED_SPEED * delta;
		if (BleedPosition < Health)
			BleedPosition = Health;
		
		if ( BleedPosition <= 0 && BleedPosition <= Health )
			GetParent().GetParent<Main>().ProcessPlayerDeath();
	}

	private void PlayerCharAnimationFinished()
	{
		var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		if (animatedSprite.Animation == "attack")
		{
			animatedSprite.Animation = "standing";
		}
	}

	private void OnSwordCollisionBodyEntered(object body)
	{
		var dryad = body as Dryad;
		if (dryad != null)
			dryad.InPlayerSwordRange = true;
		var demon = body as LargeDemon;
		if (demon != null)
			demon.InPlayerSwordRange = true;
		var goblin = body as Goblin;
		if (goblin != null)
			goblin.InPlayerSwordRange = true;
	}

	private void OnSwordCollisionBodyExited(object body)
	{
		var dryad = body as Dryad;
		if (dryad != null)
			dryad.InPlayerSwordRange = false;
		var demon = body as LargeDemon;
		if (demon != null)
			demon.InPlayerSwordRange = false;
		var goblin = body as Goblin;
		if (goblin != null)
			goblin.InPlayerSwordRange = false;
	}

	private void ProcessAttack(Dryad dryad)
	{
		if (!dryad.IsHostile)
			return;
		
		var now = Time.GetUnixTimeFromSystem();
		var levelNode = GetParent<Level1>();
		var mainNode = levelNode.GetParent();
		var mediaNode = mainNode.GetNode("MediaNode");
		
		if (dryad.InPlayerSwordRange)
		{
			var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
			hitSound.Position = Position;
			hitSound.Play();
			int prevTargetHealth = dryad.Health;
			dryad.Health -= SWORD_DAMAGE_DRYAD;
			dryad.LastAffectedTimestamp = now;
			if (dryad.Health <= 0)
			{
				var deathSound = mediaNode
					.GetNode<AudioStreamPlayer2D>("DryadDeathSound");
				deathSound.Position = dryad.Position;
				deathSound.Play();
				
				if (dryad.IsBoss && !levelNode.ShownStory5)
				{
					var hud = mainNode.GetNode<HUD>("HUD");
					
					List<String> lines = new List<string>();
					lines.Add("Zealot. You don't know what you're doing.");
					lines.Add("Your justice is no one's justice.");
					lines.Add("Did the high priest send you?");
					hud.ShowDialog(lines);
					levelNode.ShownStory5 = true;
					levelNode.GetNode<Timer>("EndGameTimer").Start();
					levelNode.GameOver = true;
				}
				else
				{
					dryad.QueueFree();
				}
			}
			else
			{
				var onHitSound = mediaNode
					.GetNode<AudioStreamPlayer2D>("DryadAttackedSound");
				onHitSound.Position = dryad.Position;
				onHitSound.Play();
			}
			
			var damageReport = new DamageReport();
			damageReport.Who = dryad.DamageId;
			damageReport.FromPlayer = true;
			damageReport.Amount = prevTargetHealth - dryad.Health;
			damageReport.Timestamp = now;
			levelNode.DamageHistory.Enqueue(damageReport);
		}
	}
	
	private void ProcessAttack(LargeDemon demon)
	{
		var now = Time.GetUnixTimeFromSystem();
		var levelNode = GetParent<Level1>();
		var mainNode = levelNode.GetParent();
		var mediaNode = mainNode.GetNode("MediaNode");
		
		if (demon.Alive && demon.InPlayerSwordRange)
		{
			var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
			hitSound.Position = Position;
			hitSound.Play();
			int prevTargetHealth = demon.Health;
			demon.Health -= SWORD_DAMAGE_DEMON;
			demon.LastAffectedTimestamp = now;
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
				var onHitSound = mainNode.GetNode("MediaNode")
					.GetNode<AudioStreamPlayer2D>("DemonIsHurt");
				onHitSound.Position = demon.Position;
				onHitSound.Play();
			}
			
			var damageReport = new DamageReport();
			damageReport.Who = demon.DamageId;
			damageReport.FromPlayer = true;
			damageReport.Amount = prevTargetHealth - demon.Health;
			damageReport.Timestamp = now;
			levelNode.DamageHistory.Enqueue(damageReport);
		}
	}
	
	private void ProcessAttack(Goblin goblin)
	{
		var now = Time.GetUnixTimeFromSystem();
		var levelNode = GetParent<Level1>();
		var mainNode = levelNode.GetParent();
		var mediaNode = mainNode.GetNode("MediaNode");
		
		if (goblin.Alive && goblin.InPlayerSwordRange)
		{
			var hitSound = mediaNode.GetNode<AudioStreamPlayer2D>("HitSound");
			hitSound.Position = Position;
			hitSound.Play();
			int prevTargetHealth = goblin.Health;
			goblin.Health -= SWORD_DAMAGE_GOBLIN;
			goblin.LastAffectedTimestamp = now;
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
				var onHitSound = mainNode.GetNode("MediaNode")
					.GetNode<AudioStreamPlayer2D>("GoblinAttackedSound");
				onHitSound.Position = goblin.Position;
				onHitSound.Play();
			}
			
			var damageReport = new DamageReport();
			damageReport.Who = goblin.DamageId;
			damageReport.FromPlayer = true;
			damageReport.Amount = prevTargetHealth - goblin.Health;
			damageReport.Timestamp = now;
			levelNode.DamageHistory.Enqueue(damageReport);
		}
	}

	private void OnAttackProcess()
	{
		var levelNode = GetParent<Level1>();
		var mainNode = levelNode.GetParent();
		var swordCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwordCollisionShape");
		var swingSound = 
			mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer2D>("SwingSound");
		swingSound.Position = Position;
		swingSound.Play();
		
		foreach (var node in GetParent().GetNode("Dryads").GetChildren())
		{
			var dryad = node as Dryad;
			if (dryad == null)
				continue;
			ProcessAttack(dryad);
		}
		foreach (var node in GetParent().GetNode("Demons").GetChildren())
		{
			var demon = node as LargeDemon;
			if (demon == null)
				continue;
			ProcessAttack(demon);
		}
		foreach (var node in GetParent().GetNode("Goblins").GetChildren())
		{
			var goblin = node as Goblin;
			if (goblin == null)
				continue;
			ProcessAttack(goblin);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		var main = GetParent().GetParent();
		var hpBar = main.GetNode("HUD").GetNode("HPBar");
		var hpRect = hpBar.GetNode<ColorRect>("HPRectRemaining");
		hpRect.RectSize = new Vector2(Math.Max( 0, (float)Health / MAX_HEALTH * HP_BAR_WIDTH ), HP_BAR_HEIGHT);
		var bleedRect = hpBar.GetNode<ColorRect>("HPRectBleeding");
		float bleedBarXOffset = 0;
		
		if (Health <= -100f)
		{
			bleedBarXOffset = -100f / MAX_HEALTH * HP_BAR_WIDTH;	
		}	
		else if (Health < 0)
			bleedBarXOffset = (float)Health / MAX_HEALTH * HP_BAR_WIDTH;
		bleedRect.RectPosition = new Vector2(BLEED_BAR_POS_X + bleedBarXOffset, BLEED_BAR_POS_Y);
		bleedRect.RectSize = new Vector2((float)BleedPosition / MAX_HEALTH * HP_BAR_WIDTH - bleedBarXOffset, HP_BAR_HEIGHT);
	}
}
