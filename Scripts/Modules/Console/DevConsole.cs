using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limbo.Console;
using BrannPack.Character;
using System.Runtime.CompilerServices;
using Godot;
using BrannPack.Directors;

namespace BrannPack.DevConsole
{
    [GlobalClass]
    public partial class DevConsole:Node
    {
        public static bool IsConsoleEnabled = false;
        public override void _Ready()
        {
            base._Ready();
            InitConsole();
        }
        
        public void InitConsole()
        {
            Limbo.Console.Sharp.LimboConsole.RegisterCommand(new Godot.Callable(this,MethodName._DamageAllMasters), "damageall", "Damages all masters");
           IsConsoleEnabled = true;
        }
       
        public void _DamageAllMasters(float damage)
        {
            foreach (var master in CharacterMaster.AllMasters)
            {
                if (master != null)
                {
                    master.TakeDamage(master,new DamageInfo(master,master,(-1,1,1),damage,false),null);
                }
            }
        }
    }
}
