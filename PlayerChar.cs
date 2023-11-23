using Godot;
using System;

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
	
	[Export]
	public InputState InputState = InputState.None;
	
	[Export]
	public Vector2 Velocity = new Vector2();
	
	[Export]
	public bool Grounded = false;

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
//		var walk = MOVE_SPEED * (Input.GetAxis("move_left", "move_right"));
		var directionX = Input.GetActionStrength("move_right")
			- Input.GetActionStrength("move_left");
		Velocity.x = directionX * MOVE_SPEED;

		// For horizontal collisions, turn OFF "Selected Collision On"
		// for every tile in the tile map. It has to be false.

// Process vertical movement first to allow jumping,
		// because horizontal movement sets Grounded := false.
		if (Grounded)
		{
			if (Input.IsActionPressed("move_up"))
			{
				Velocity.y = -JUMP_FORCE;
				Grounded = false;
				// TODO: Play jump sound
			}
			else
				Velocity.y = 0;
		}
		else
			Velocity.y += GRAVITY * delta; // Gravity

if (Velocity.x != 0)
			Grounded = false;

		// Up vector required for IsOnFloor
		Velocity = MoveAndSlide(Velocity, Vector2.Up);
// Note there is also MoveAndSlideWithSnap
		
		if (IsOnFloor())	// Note there is an IsOnWall too, but not used atm
			Grounded = true;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
