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
using static BrannPack.ModifiableStats.CharacterStats;
using static BrannPack.ModifiableStats.AbilityStats;
using BrannPack.ModifiableStats;
using BrannPack.ItemHandling;
using BrannPack.Interactables;

namespace BrannPack.DevConsole
{
    [GlobalClass]
    public partial class DevConsole:Node
    {
        public static bool IsConsoleEnabled = false;
        public static Dictionary<(CharacterMaster,HealthType),float> additionalHealth = new Dictionary<(CharacterMaster,HealthType),float>();
        public override void _Ready()
        {
            base._Ready();
            InitConsole();
        }
        
        public void InitConsole()
        {
            Limbo.Console.Sharp.LimboConsole.RegisterCommand(new Godot.Callable(this,MethodName._DamageAllMasters), "damageall", "Damages all masters");
            Limbo.Console.Sharp.LimboConsole.RegisterCommand(new Godot.Callable(this,MethodName._AddBaseMaxHealthByTypeToAllMasters), "addmaxhealthtoall", "Adds max health to all masters");
            Limbo.Console.Sharp.LimboConsole.RegisterCommand(new Godot.Callable(this,MethodName._SpawnItemPickup), "spawn_item", "Create Item Pickup");

            StatsHolder<CharacterMaster>.GlobalRefreshAbilityStatVariable+=(master, stat,modstat) =>
            {
                
                HealthType? healthType = null;
                switch(stat)
                {
                    case ModifiableStats.Stat.MaxHealth:
                        healthType = HealthType.Health;
                        break;
                    case ModifiableStats.Stat.MaxArmor:
                        healthType = HealthType.Armor;
                        break;
                    case ModifiableStats.Stat.MaxShield:
                        healthType = HealthType.Shield;
                        break;
                    case ModifiableStats.Stat.MaxBarrier:
                        healthType = HealthType.Barrier;
                        break;
                }
                if(healthType != null)
                {
                    if (additionalHealth.TryGetValue((master, healthType.Value), out float additional))
                    {
                        
                        ((MaxHealthStat)modstat).AdditionalMaxHealth+= additional;
                        ((MaxHealthStat)modstat).CalculateTotal();
                    }
                }
            };
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

        public void _AddBaseMaxHealthByTypeToAllMasters(string healthTypeString, float amount, bool heal=true)
        {
            Enum.TryParse(healthTypeString,out HealthType healthType);
            foreach (var master in CharacterMaster.AllMasters)
            {
                if (master != null)
                {
                    if (additionalHealth.TryGetValue((master, healthType), out float additional))
                    {
                        additionalHealth[(master, healthType)] += amount;
                    }
                    else
                    {
                        additionalHealth.Add((master, healthType), amount);
                    }
                    ModifiableStats.Stat stat = Stat.MaxHealth;
                    switch (healthType)
                    {
                        case HealthType.Health:
                            stat = ModifiableStats.Stat.MaxHealth;
                            break;
                        case HealthType.Armor:
                            stat = ModifiableStats.Stat.MaxArmor;
                            break;
                        case HealthType.Shield:
                            stat = ModifiableStats.Stat.MaxShield;
                            break;
                        case HealthType.Barrier:
                            stat = ModifiableStats.Stat.MaxBarrier;
                            break;
                    }
                    
                    master.Stats.RecalculateByStatVariable(stat);
                    if (heal)
                    {
                        Enum.TryParse(healthTypeString, out HealthCategories hc);
                        
                        master.HealthBar.Heal(new HealingInfo(master,master,(-1,1,1),amount,null,hc),null);
                    }

                }
                
            }
        }

        public void _SpawnItemPickup(string itemname)
        {
            GD.Print("AAAA1");
            Item item = Item.ItemRegistry.Get(itemname);
            InventoryItemStack stack = new InventoryItemStack(item, null, 1);
            ItemPickup ip = new ItemPickup();
            ip.ItemStack = stack;
            GD.Print("ATTEMPT " + ip.ItemStack.Item.Name);
            foreach (var master in CharacterMaster.AllMasters)
            {
                if(master.IsPlayerControlled)
                {
                    var player = master.Body; // or master.GetBody(), depending on your setup
                    if (player != null)
                    {
                        ip.Position = player.GlobalPosition; // If 2D
                                                             // Add to scene
                        GetTree().Root.AddChild(ip); // or a dedicated node like GetNode("World")
                        break; // Only spawn for the first player
                    }
                }


            }
        }
    }
}
