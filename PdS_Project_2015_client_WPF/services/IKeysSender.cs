using PdS_Project_2015_client_WPF.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PdS_Project_2015_client_WPF.services
{
    public delegate void NewKeyInShortcutEventHandler(KeyAndAction keyAndAction);

    interface IKeysSender
    {
        void Send();
        void Clear();
        void HandleKeyDown(KeyEventArgs e);
        void HandleKeyUp(KeyEventArgs e);
        event NewKeyInShortcutEventHandler NewKeyInShortcut;
        bool IsShortcutSendable { get; }
    }
    
}
