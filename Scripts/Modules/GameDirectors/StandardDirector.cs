﻿using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrannPack.Directors
{
    [GlobalClass]
    public partial class StandardDirector : Node2D
    {
        public static StandardDirector instance { get; private set; }

        //public LayoutGenerator layoutGenerator;

        //public GlobalSpawner Spawner;

        public float RunDuration = 0f;

        public float PlayerScale = 1f;

        public int Floor = 1;


        private Dictionary<CharacterTeam, BaseCharacterBody> CharactersByTeam;

        public StandardDirector() { if (instance == null) instance = this; }

        public void GenerateFloor() { }

        public void StartRun() { }
        public void EndRun() { }

        public void SpawnEnemies() { }

        public void SpawnRoomEnemies() { }

        public override void _Ready()
        {
            
        }


    }
}
