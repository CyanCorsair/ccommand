using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace CCommand.Scripts.Core.Models
{
    public class Nation
    {
        private NationState _state;

        public NationState State
        {
            get => _state;
        }

        public Nation()
        {
            _state = new NationState();
        }
    }
}
