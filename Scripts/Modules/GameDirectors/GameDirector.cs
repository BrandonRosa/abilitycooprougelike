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
            Loading,Main,InStandardHub,InStandardRun
        }

        public readonly GameState gameState;
        public override void _Ready()
        {
            base._Ready();
        }
    }
}
