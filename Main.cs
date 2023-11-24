using Godot;

public class Main : Node
{
#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene mobScene;
#pragma warning restore 649

	[Export]
	public PackedScene FireScene { get; set; }

	public int score;

	public Vector2 screenSize; // Size of the game window.

	public override void _Ready()
	{
		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		var NymphNode = GetNode<Nymph>("Nymph");
		NymphNode.Target = PlayerCharNode;
		var NymphNode2 = GetNode<Nymph>("Nymph2");
		NymphNode2.Target = PlayerCharNode;
		screenSize = PlayerCharNode.GetViewportRect().Size;
		GD.Randomize();
	}

	public void GameOver()
	{
		GetNode<Timer>("MobTimer").Stop();
		//GetNode<Timer>("ScoreTimer").Stop();

		GetNode<HUD>("HUD").ShowGameOver();

		GetNode<AudioStreamPlayer>("Music").Stop();
		GetNode<AudioStreamPlayer>("DeathSound").Play();
	}

	public void NewGame()
	{
		// Note that for calling Godot-provided methods with strings,
		// we have to use the original Godot snake_case name.
		GetTree().CallGroup("mobs", "queue_free");
		score = 0;

		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		var startPosition = GetNode<Position2D>("StartPosition");
		PlayerCharNode.Start(startPosition.Position);

		GetNode<Timer>("StartTimer").Start();

		var hud = GetNode<HUD>("HUD");
		hud.UpdateScore(score);
		hud.ShowMessage("Get Ready!");

		// TODO
		//GetNode<AudioStreamPlayer>("Music").Play();
	}

	public void OnStartTimerTimeout()
	{
		GetNode<Timer>("MobTimer").Start();
		//GetNode<Timer>("ScoreTimer").Start();
	}

	public void OnScoreTimerTimeout()
	{
		score++;

		GetNode<HUD>("HUD").UpdateScore(score);
	}

	public void OnMobTimerTimeout()
	{
//		// Note: Normally it is best to use explicit types rather than the `var`
//		// keyword. However, var is acceptable to use here because the types are
//		// obviously PathFollow2D and Mob, since they appear later on the line.
//
//		// Choose a random location on Path2D.
//		var mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
//		mobSpawnLocation.Offset = GD.Randi();
//
//		// Create a Mob instance and add it to the scene.
//		var mob = (Mob)mobScene.Instance();
//		AddChild(mob);
//
//		// Set the mob's direction perpendicular to the path direction.
//		float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;
//
//		// Set the mob's position to a random location.
//		mob.Position = mobSpawnLocation.Position;
//
//		// Add some randomness to the direction.
//		direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
//		mob.Rotation = direction;
//
//		// Choose the velocity for the mob.
//		var velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
//		mob.LinearVelocity = velocity.Rotated(direction);
	}
	
	public void Start(Vector2 pos)
	{
		// TODO: Undo these changes when game ends
		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		//PlayerCharNode._IntegrateForces();
		PlayerCharNode.Show();
		PlayerCharNode.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		//PlayerCharNode.Sleeping = false;
	}
	
	public override void _Process(float delta)
	{
		var PlayerCharNode = GetNode<PlayerChar>("PlayerChar");
		//var velocity = Vector2.Zero; // The player's movement vector.
//
//		if (Input.IsActionPressed("move_right"))
//		{
//			velocity.x = PlayerChar.MOVE_SPEED;
//		}
//
//		if (Input.IsActionPressed("move_left"))
//		{
//			velocity.x = -PlayerChar.MOVE_SPEED;
//		}
//
////		if (Input.IsActionPressed("move_down"))
////		{
////			velocity.y = left_right_speed;
////		}
//
//		if (Input.IsActionPressed("move_up"))
//		{
//			if (PlayerCharNode.IsOnFloor())
//				velocity.y = -PlayerChar.JUMP_FORCE;
//		}

		var animatedSprite = PlayerCharNode.GetNode<AnimatedSprite>("AnimatedSprite");

		if (PlayerCharNode.Velocity.Length() > 0)
		{
			animatedSprite.Play();
		}
		else
		{
			animatedSprite.Stop();
		}
		
		//velocity.y += 900.8f * delta; // Gravity

		//PlayerCharNode.MoveAndSlide(velocity);
		//PlayerCharNode.ApplyImpulse(new Vector2(), velocity);
//		var newX = Mathf.Clamp(PlayerCharNode.Position.x, 0, screenSize.x);
//		var newY = Mathf.Clamp(PlayerCharNode.Position.y, 0, screenSize.y);
//		PlayerCharNode.Transform = new Transform2D(0, new Vector2(newX, newY));
//		PlayerCharNode.Position += velocity * delta;
//		PlayerCharNode.Position = new Vector2(
//			x: Mathf.Clamp(PlayerCharNode.Position.x, 0, screenSize.x),
//			y: Mathf.Clamp(PlayerCharNode.Position.y, 0, screenSize.y)
//		);

		if (PlayerCharNode.Velocity.x != 0)
		{
			animatedSprite.Animation = "running";
			// See the note below about boolean assignment.
			animatedSprite.FlipH = PlayerCharNode.Velocity.x < 0;
			animatedSprite.FlipV = false;
		}
		else if (PlayerCharNode.Velocity.y != 0)
		{
			animatedSprite.Animation = "standing";
			//animatedSprite.FlipV = velocity.y > 0;
		}
	
		// Reset player char if it goes below a certain Y coordinate
		var startPosition = GetNode<Position2D>("StartPosition");
		if (PlayerCharNode.Position.y > PlayerChar.MIN_Y_COORD)
		{
			PlayerCharNode.Position = startPosition.Position;
			PlayerCharNode.Velocity = new Vector2(0, 0);
			// TODO: Reset the whole level and player state
		}
	}
}
