using Godot;
using System;

public class Goblin : KinematicBody2D
{
	public static int MOVE_SPEED = 120;
	public static int MAX_RANGE = 25;
	public static int MIN_RANGE = 12;
	public static int GRAVITY = 900;
	public static float LOS = 100;
	public const int MAX_HEALTH = 36;
	public static double HIDE_HP_BAR_SECS = 4;
	public static int HP_BAR_WIDTH = 12;
	public static double ATTACK_DELAY = 0.2;
	public static double ATTACK_RESET_ANIMATION = 0.6;
	public static double ATTACK_COOLDOWN_SECS = 2; // From beginning of attack
	public static int DAMAGE_ONTO_PLAYER = 20;
	public static double DEATH_DURATION_SECS = 0.2;
	
	public bool InPlayerSwordRange = false;
	public double LastAffectedTimestamp = 0;
	public double LastAttackTimestamp = 0;
	public double DeathTimestamp = 0;
	public bool AttackPending = false;
	public bool PlayerInAttackRange = false;
	public bool Alive = true;
	
	public Vector2 Velocity = new Vector2();
	public bool Grounded = false;
	
	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public PlayerChar Target = null;
	
	[Export]
	public uint DamageId = 0;
	
	public override void _Ready()
	{
		Target = GetParent().GetParent().GetNode<PlayerChar>("PlayerChar");
	}
	
	public bool AgainstRightWall()
	{
		for (int i = 0; i < GetSlideCount(); ++i)
		{
			var collision = GetSlideCollision( i );
			if (collision.Normal.x < 0)
				return true;
		}
		return false;
	}
	
	public bool BlockedByWall(float direction)
	{
		if (!IsOnWall())
			return false;
		if (direction > 0 && AgainstRightWall())
			return true;
		if (direction < 0 && !AgainstRightWall())
			return true;
		return false;
	}
	
	public float AiGetDirection()
	{
		return Target.Position.x - Position.x;
	}
	
	public void AiMove(float delta)
	{
		var levelNode = GetParent().GetParent<Level1>();
		var mainNode = levelNode.GetParent();
		
		if (!Alive)
			return;
			
		float directionToPlayer = AiGetDirection();
		
		AnimatedSprite sprite = 
			GetNode<AnimatedSprite>("AnimatedSprite");
		float absoluteDistance = Math.Abs(directionToPlayer);
		
		if (absoluteDistance > LOS)
		{
			sprite.Animation = "idle";
			return;
		}
		
		if (absoluteDistance > MAX_RANGE)
		{
			if (BlockedByWall(directionToPlayer))
			{
				sprite.FlipH = directionToPlayer > 0;
				sprite.Animation = "idle";
			}
			else
			{
				sprite.FlipH = directionToPlayer < 0;
				sprite.Animation = "walking";
			}
			Velocity.x = Math.Sign(directionToPlayer) * MOVE_SPEED;
		}
		else
		{
			if (absoluteDistance < MIN_RANGE/* && !IsOnWall()*/)
			{
				if (BlockedByWall(-directionToPlayer))
				{
					sprite.FlipH = directionToPlayer < 0;
					sprite.Animation = "idle";
				}
				else
				{
					sprite.FlipH = directionToPlayer > 0;
					sprite.Animation = "walking";	
				}
				Velocity.x = -Math.Sign(directionToPlayer) * MOVE_SPEED;
			}
			else
			{
				sprite.FlipH = directionToPlayer < 0;
				Velocity.x = 0;
			}
			
			if (LastAttackTimestamp + ATTACK_COOLDOWN_SECS < Time.GetUnixTimeFromSystem() )
			{
				LastAttackTimestamp = Time.GetUnixTimeFromSystem();
				sprite.Animation = "attacking";
				AttackPending = true;
				
				AudioStreamPlayer2D swingSound =
					mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer2D>("SwingSound");
				swingSound.Position = Position;
				swingSound.Play();
			}
			else if (LastAttackTimestamp + ATTACK_RESET_ANIMATION > Time.GetUnixTimeFromSystem() )
			{
				if (sprite.Animation != "attacking")
					sprite.Animation = "attacking";
			}
			else
				sprite.Animation = "idle";
		}
		
		var attackCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwordCollisionShape");
		if (sprite.FlipH)
		{
			attackCollisionShape.Position = new Vector2 (-17, 5);
		}
		else
		{
			attackCollisionShape.Position = new Vector2 (17, 5);
		}
		
		if (Grounded)
			Velocity.y = 0;
		else
			Velocity.y += GRAVITY * delta; // Gravity

		bool wasGrounded = Grounded;
		if (Velocity.x != 0)
		{
//			if (Grounded && !IsOnWall())
//			{
//				var grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
//				if (!grassSound.Playing)
//					grassSound.Play();
//			}
			Grounded = false;
		}

		float oldYVelocity = Velocity.y;

		Velocity = MoveAndSlide(Velocity, Vector2.Up);
		
		if (IsOnFloor())
		{
			Grounded = true;
			
			if (!wasGrounded)
			{
//				var grassSound = GetNode<AudioStreamPlayer2D>("GrassSound");
//				if (!grassSound.Playing)
//					grassSound.Play();

				if (oldYVelocity > 360)
				{
					var landingSound =
						mainNode.GetNode("MediaNode")
							.GetNode<AudioStreamPlayer2D>("LandingSound");
					landingSound.Position = Position;
					landingSound.Play();
				}
			}
		}
	}
	
	public override void _PhysicsProcess(float delta)
	{
		var now = Time.GetUnixTimeFromSystem();
		
		if (DeathTimestamp != 0)
		{
			if (DeathTimestamp + DEATH_DURATION_SECS < now)
			{
				QueueFree();
			}
			else
			{
				AnimatedSprite sprite = 
					GetNode<AnimatedSprite>("AnimatedSprite");
				sprite.Animation = "death";
			}
			return;
		}
		
		AiMove(delta);
		
		if (AttackPending
			&& LastAttackTimestamp + ATTACK_DELAY < now )
		{
			AttackPending = false;
			ProcessAttack();
		}
	}
	
	private void ProcessAttack()
	{
		if (!Alive)
			return;
		
		var now = Time.GetUnixTimeFromSystem();
		var levelNode = GetParent().GetParent<Level1>();
		var mainNode = levelNode.GetParent();
		
		AudioStreamPlayer2D hitSound =
			mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer2D>("HitSound");
		hitSound.Position = Position;
		hitSound.Play();
		
		if (PlayerInAttackRange)
		{
			int prevTargetHealth = Target.Health;
			Target.Health -= DAMAGE_ONTO_PLAYER;
			
			if (Target.Health < -100)
				Target.Health = -100;
		
			var damageReport = new DamageReport();
			damageReport.Who = DamageId;
			damageReport.FromPlayer = false;
			damageReport.Amount = prevTargetHealth - Target.Health;
			damageReport.Timestamp = now;
			levelNode.DamageHistory.Enqueue(damageReport);
		}
	}
	
	public override void _Process(float delta)
	{
		bool visible = LastAffectedTimestamp + HIDE_HP_BAR_SECS > Time.GetUnixTimeFromSystem();
		var rectRemaining = GetNode<ColorRect>("HPRectRemaining");
		var rectBg = GetNode<ColorRect>("HPRectBg");
		rectRemaining.RectSize = new Vector2((float)Health / MAX_HEALTH * HP_BAR_WIDTH, 2);
		rectRemaining.Visible = visible;
		rectBg.Visible = visible;
	}
	
	private void OnAttackCollisionBodyEntered(object body)
	{
		var player = body as PlayerChar;
		if (player != null)
			PlayerInAttackRange = true;
	}

	private void OnAttackCollisionBodyExited(object body)
	{
		var player = body as PlayerChar;
		if (player != null)
			PlayerInAttackRange = false;
	}
}
