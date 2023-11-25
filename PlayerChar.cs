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
	public static int MIN_Y_COORD = 240;
	public static int HP_BAR_WIDTH = 38;
	public static int MAX_HEALTH = 100;
	
	[Export]
	public int Health = MAX_HEALTH;
	
	[Export]
	public InputState InputState = InputState.None;
	
	[Export]
	public Vector2 Velocity = new Vector2();
	
	[Export]
	public bool Grounded = false;
	
	[Export]
	public bool Attacking = false;
	
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
		var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

		if (Velocity.Length() > 0)
		{
			animatedSprite.Play();
		}
		
		if (Input.IsActionPressed("attack"))
		{
			animatedSprite.Animation = "attack";
			if (!Attacking)
				GetNode<Timer>("AttackDelay").Start();
			Attacking = true;
		}

		if (Velocity.x != 0)
		{
			animatedSprite.Animation = "running";
			// See the note below about boolean assignment.
			
			var swordCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwordCollisionShape");
			if (Velocity.x < 0)
			{
				animatedSprite.FlipH = true;
				swordCollisionShape.Position = new Vector2 (-15, -3);
			}
			else
			{
				animatedSprite.FlipH = false;
				swordCollisionShape.Position = new Vector2 (15, -3);
			}
			animatedSprite.FlipV = false;
		}
		else
		{
			if (animatedSprite.Animation != "attack")
				animatedSprite.Animation = "standing";
		}
		
//		var walk = MOVE_SPEED * (Input.GetAxis("move_left", "move_right"));
		if ( Attacking )
			Velocity.x = 0;
		else
		{
			var directionX = Input.GetActionStrength("move_right")
				- Input.GetActionStrength("move_left");
			Velocity.x = directionX * MOVE_SPEED;
		}

		// For horizontal collisions, turn OFF "Selected Collision On"
		// for every tile in the tile map. It has to be false.

		// Process vertical movement first to allow jumping,
		// because horizontal movement sets Grounded := false.
		if (Grounded && !Attacking)
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
		else
			Velocity.y += GRAVITY * delta; // Gravity

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

		// Up vector required for IsOnFloor
		Velocity = MoveAndSlide(Velocity, Vector2.Up);
// Note there is also MoveAndSlideWithSnap
		
		if (IsOnFloor())	// Note there is an IsOnWall too, but not used atm
			Grounded = true;
	}

	private void PlayerCharAnimationFinished()
	{
		var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		if (animatedSprite.Animation == "attack")
		{
			animatedSprite.Animation = "standing";
			Attacking = false;
		}
	}

	private void OnSwordCollisionBodyEntered(object body)
	{
		var dryad = body as Dryad;
		if (dryad != null)
		{
			dryad.InPlayerSwordRange = true;
		}
		
//		var swordCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwordCollisionShape");
//		swordCollisionShape.Disabled = true;
	}

	private void OnSwordCollisionBodyExited(object body)
	{
		var dryad = body as Dryad;
		if (dryad != null)
		{
			dryad.InPlayerSwordRange = false;
		}
	}

	private void ProcessAttack(Dryad dryad)
	{
		if (dryad.InPlayerSwordRange)
		{
			var hitSound = GetNode<AudioStreamPlayer2D>("HitSound");
			dryad.Health -= 10;
			if (dryad.Health <= 0)
				dryad.QueueFree();
			hitSound.Play();
		}
		//if (dyrad.GetNode<CollisionShape2D>("CollisionShape2D");
		// test intersection between sword collision shape and enemy collision shape
		// if intersect, enemy is damaged
	}

	private void OnAttackProcess()
	{
		var swordCollisionShape = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("SwordCollisionShape");
		var dryadNode = GetParent().GetNodeOrNull<Dryad>("Dryad");
		var dryadNode2 = GetParent().GetNodeOrNull<Dryad>("Dryad2");
		
		var swingSound = GetNode<AudioStreamPlayer2D>("SwingSound");
		swingSound.Play();
		if (dryadNode != null)
			ProcessAttack(dryadNode);
		if (dryadNode2 != null)
			ProcessAttack(dryadNode2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		var rect = GetNode<ColorRect>("HPRectRemaining");
		rect.RectSize = new Vector2((float)Health / MAX_HEALTH * HP_BAR_WIDTH, 2);
	}
}
