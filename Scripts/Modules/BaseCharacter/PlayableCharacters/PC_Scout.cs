using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Character.Playable
{
    

    public static class PC_Scout
    {
        public static PackedScene Prefab { get; init; } = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
    }
}
