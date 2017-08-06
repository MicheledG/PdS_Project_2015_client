using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PdS_Project_2015_client_WPF.model
{
    public class KeyAndAction
    {
        private Key key;
        private int virtualCode;
        private bool isDown;

        public Key Key { get => key; set => key = value; }
        public bool IsDown { get => isDown; set => isDown = value; }
        public int VirtualCode { get => virtualCode; set => virtualCode = value; }
    }
}
