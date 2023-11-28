using Godot;
using System;
using System.Collections.Generic;

public enum DialogBehaviour { QuitToMainMenu, RestartGame };

//public enum ItemOverlay
//{
//	None = 0,
//	Hourglass = 1,
//	Scales = 2
//}

public class HUD : CanvasLayer
{
	[Signal]
	public delegate void StartGame();
	
	public List<String> Lines = new List<string>();
	public int LineNumber = 0;
	
	public DialogBehaviour DialogBehaviour = DialogBehaviour.RestartGame;
	
//	public ItemOverlay ItemOverlayType = ItemOverlay.None;
//	public int ItemOverlaySinceMillisec = 0;
	
	public void ShowHint(String line)
	{
		Panel infoPanel = GetNode<Panel>("MiniInfoPanel");
		
		Label infoLabel = infoPanel.GetNode("MarginContainer").GetNode<Label>("InfoLabel");
		infoLabel.Text = line;
		
		infoPanel.Show();
	}
	
	public void HideHint()
	{
		Panel infoPanel = GetNode<Panel>("MiniInfoPanel");
		infoPanel.Hide();
	}
	
	public void ShowDialog(String line)
	{
		Lines = new List<string>();
		Lines.Add(line);
		LineNumber = 0;
		
		var panel = GetNode<Panel>("InfoPanel");
		panel.Show();
		panel.GetNode<Label>("InfoLabel").Text = line;
		panel.GetNode<Button>("ProceedButton").Text = "Close";
	}

	public void ShowDialog(List<String> lines)
	{
		Lines = lines;
		LineNumber = 0;
		
		var panel = GetNode<Panel>("InfoPanel");
		panel.Show();
		panel.GetNode<Label>("InfoLabel").Text = lines[LineNumber];
		panel.GetNode<Button>("ProceedButton")
			.Text = LineNumber < Lines.Count - 1 ? "Next" : "Close";
		GetTree().Paused = true;
	}

	public async void ShowGameOver()
	{
		ShowDialog("Game Over");

		var messageTimer = GetNode<Timer>("MessageTimer");
		await ToSignal(messageTimer, "timeout");

		GetNode<Panel>("InfoPanel").Show();
	}
	
	public void ShowGameMenu()
	{
		GetNode<Panel>("PauseMenuPanel").Show();
	}

	public void OnStartButtonPressed()
	{
		++LineNumber;
		if (LineNumber >= Lines.Count)
		{
			// TODO: make sure the next lines are necessary
			GetNode<Panel>("InfoPanel").Hide();
			EmitSignal(nameof(StartGame));
			var main = GetParent();
			main.GetNode<Level1>("Level1").CallDeferred("NewGame");
			// End of TODO
			GetTree().Paused = false;
		}
		else
		{
			var panel = GetNode<Panel>("InfoPanel");
			panel.Show();
			panel.GetNode<Label>("InfoLabel").Text = Lines[LineNumber];
			panel.GetNode<Button>("ProceedButton")
				.Text = LineNumber < Lines.Count - 1 ? "Next" : "Close";
		}
	}
	
	private void ResumeButtonPressed()
	{
		GetTree().Paused = false;
		GetNode<Panel>("PauseMenuPanel").Hide();
	}

	private void RestartGameButtonPressed()
	{
		GetNode<Panel>("PauseMenuPanel").Hide();
		var restartDialogPanel = GetNode<Panel>("RestartDialogPanel");
		var vbox = restartDialogPanel.GetNode<VBoxContainer>("VBoxContainer");
		vbox.GetNode<Label>("RestartPromptLabel").Show();
		vbox.GetNode<Label>("MainMenuPromptLabel").Hide();
		restartDialogPanel.Show();
		DialogBehaviour = DialogBehaviour.RestartGame;
	}

	private void YesButtonPressed()
	{
		switch (DialogBehaviour)
		{
			case DialogBehaviour.QuitToMainMenu:
				{
					Main mainNode = GetParent<Main>();
					mainNode.GetNode<Camera2D>("MainMenuCam").Current = true;
					mainNode.Level.GetNode<PlayerChar>("PlayerChar").GetNode<Camera2D>("Camera2D").Current = false;
					mainNode.GetNode("Level1").QueueFree();
					mainNode.GetNode("MediaNode").GetNode<AudioStreamPlayer>("Music").Stop();
					GetNode<TextureRect>("MainMenuLogo").Show();
					GetNode<Label>("MainMenuLabel1").Show();
					GetNode<Label>("MainMenuLabel2").Show();
					GetNode<Panel>("MainMenuPanel").Show();
					GetNode<Panel>("RestartDialogPanel").Hide();
					GetNode<Node2D>("HPBar").Hide();
					GetTree().Paused = false;
				}
				break;
			case DialogBehaviour.RestartGame:
				{
					
				}
				break;
			default:
				break;
		}
	}

	private void NoButtonPressed()
	{
		GetNode<Panel>("RestartDialogPanel").Hide();
		GetNode<Panel>("PauseMenuPanel").Show();
	}
	
	private void OnNewGameButtonPressed()
	{
		GetParent<Main>().NewGame();
	}

	private void OnCreditsButtonPressed()
	{
		// Replace with function body.
	}
	
	private void MainMenuButtonPressed()
	{
		GetNode<Panel>("PauseMenuPanel").Hide();
		var restartDialogPanel = GetNode<Panel>("RestartDialogPanel");
		var vbox = restartDialogPanel.GetNode<VBoxContainer>("VBoxContainer");
		vbox.GetNode<Label>("MainMenuPromptLabel").Show();
		vbox.GetNode<Label>("RestartPromptLabel").Hide();
		restartDialogPanel.Show();
		DialogBehaviour = DialogBehaviour.QuitToMainMenu;
	}
	
	public override void _Process(float delta)
	{
	}
}
