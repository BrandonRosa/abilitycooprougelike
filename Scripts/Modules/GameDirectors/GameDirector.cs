using BrannPack.Helpers.RecourcePool;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.GameDirectrs
{
	public partial class GameDirector:Node
	{
		public enum GameState
		{
			Loading,Main,InStandardHub,InStandardRun,DevTest
		}

		public readonly GameState gameState=GameState.DevTest;

		public PoolManager PoolManager;
		public override async void _Ready()
		{
			base._Ready();
			await ToSignal(GetTree(), "process_frame");
			switch (gameState)
			{
				case GameState.DevTest:
					PoolManager = new PoolManager();
					GetTree().Root.GetNode("Root/Managers").AddChild(PoolManager);
					break;

			}
		}
	}
}
