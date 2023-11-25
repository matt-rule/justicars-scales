using Godot;
using System;
using System.Collections.Generic;

public class HUD : CanvasLayer
{
	[Signal]
	public delegate void StartGame();
	
	public List<String> Lines = new List<string>();
	public int LineNumber = 0;
	
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
		GetNode<Panel>("MenuPanel").Show();
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
			main.CallDeferred("NewGame");
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
		GetNode<Panel>("MenuPanel").Hide();
	}

	private void RestartGameButtonPressed()
	{
		GetNode<Panel>("MenuPanel").Hide();
		GetNode<Panel>("RestartDialogPanel").Show();
	}

	private void YesButtonPressed()
	{
	}

	private void NoButtonPressed()
	{
		GetNode<Panel>("RestartDialogPanel").Hide();
		GetNode<Panel>("MenuPanel").Show();
	}
}
