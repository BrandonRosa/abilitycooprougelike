using BrannPack.Helpers.RecourcePool;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Helpers.RecourcePool
{
    [GlobalClass]
    [GodotClassName("SpawnPoolDetails")]
    public partial class SpawnPoolDetails : Resource
    {
        [Export] public PackedScene SpawnScene;
        [Export] public string CodeName;
        [Export] public string Name;
        [Export] public SpawnType SpawnType { get; set; }

        [Export] public bool ShouldPool = true;
        [Export] public int InitialPoolSize = 8;

        [Export] public string Description;
        [Export] public string Lore;
        [Export] public Texture2D Icon;

    }
}
