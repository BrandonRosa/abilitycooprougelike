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
		public static GameDirector instance;
		public enum GameState
		{
			Loading,Main,InStandardHub,InStandardRun,DevTest
		}
        public static class CollisionLayers
        {
            public const int WorldStatic = 1 << 0;
            public const int Character = 1 << 1;
            public const int Projectile = 1 << 2;
            public const int Interactable = 1 << 3;
            public const int Hitbox = 1 << 4;
            public const int Trigger = 1 << 5;
            public const int IgnoreRaycast = 1 << 6;
        }

        public readonly GameState gameState=GameState.DevTest;

		public PoolManager PoolManager;
		public override async void _Ready()
		{
			base._Ready();
			instance = this;
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
