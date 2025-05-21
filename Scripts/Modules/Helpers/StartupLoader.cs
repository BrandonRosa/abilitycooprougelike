using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Helpers
{
    using BrannPack.Helpers.Initializers;
    using Godot;
    using System;

    public partial class StartupLoader : Node
    {
        public override void _Ready()
        {
            GD.Print("Running InitializerHelper.RegisterAll()");
            InitializerHelper.RegisterAll();
        }
    }
}
