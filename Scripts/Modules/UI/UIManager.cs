using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrannPack.Character;
using Godot;

namespace BrannPack.UI
{
	[GlobalClass]
	public partial class UIManager: Node
	{
		public enum UIState
		{
			MainMenu,
			InGame,
			PauseMenu,
			GameOver
		}

		[Export]
		public UIState CurrentState { get; set; } = UIState.InGame;

		public override void _Ready()
		{
			SetState(CurrentState); // Initialize the UI state on ready
		}

		public void SetState(UIState newState)
		{
			CurrentState = newState;
			switch(newState)
			{
				case UIState.MainMenu:
					GD.Print("Main Menu");
					break;
				case UIState.InGame:
					LoadPlayerHUD();
					break;
				case UIState.PauseMenu:
					GD.Print("Pause Menu");
					break;
				case UIState.GameOver:
					GD.Print("Game Over");
					break;
			}
		}

		public void LoadPlayerHUD()
		{
			
			var scene = GD.Load<PackedScene>("res://Assets/PermanentAssets/UI/StandardPlayerBar.tscn");
			var hud = scene.Instantiate<Control>(); // Assuming it's a Control-based scene
			hud.GetNode<FixedPlayerInfoBar>($"Control/FixedPlayerInfoBar").CharacterMaster = CharacterMaster.AllMasters.FirstOrDefault(c => c.IsPlayerControlled); // Set the player character reference

			AddChild(hud); // or another UI node like GetNode("CanvasLayer")
		}


	}
}
