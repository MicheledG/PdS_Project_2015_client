using PdS_Project_2015_client_WPF.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PdS_Project_2015_client_WPF.services
{
    class KeysSender : IKeysSender
    {
        private HashSet<Key> pressedKeys;
        private List<KeyAndAction> keyShortcut;
        private IConnection connection;

        public bool IsShortcutSendable
        {
            get
            {
                if(pressedKeys.Count == 0 && keyShortcut.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public event NewKeyInShortcutEventHandler NewKeyInShortcut;

        public KeysSender(IConnection connection)
        {
            this.pressedKeys = new HashSet<Key>();
            this.keyShortcut = new List<KeyAndAction>();
            this.connection = connection;
        }

        public void HandleKeyDown(KeyEventArgs e)
        {            
            e.Handled = true;
            Key pressedKey = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (!this.pressedKeys.Contains(pressedKey))
            {
                Console.WriteLine("Key pressed: " + pressedKey);                
                this.pressedKeys.Add(pressedKey);
                KeyAndAction keyAndAction = new KeyAndAction() { Key = pressedKey, IsDown = true };
                this.keyShortcut.Add(keyAndAction);
                this.NotifyNewKeyInShortcut(keyAndAction);
            }            
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            e.Handled = true;
            Key releasedKey = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (this.pressedKeys.Contains(releasedKey))
            {
                Console.WriteLine("Key released: " + releasedKey);
                this.pressedKeys.Remove(releasedKey);
                KeyAndAction keyAndAction = new KeyAndAction() { Key = releasedKey, IsDown = false };
                this.keyShortcut.Add(keyAndAction);
                this.NotifyNewKeyInShortcut(keyAndAction);
            }            
        }

        public void Send()
        {
            //it assumed that the connection is already opened and connected!!!
            //create the message
            //translate to string
            //use connection Send method
            throw new NotImplementedException();
        }

        public void Clear()
        {
            this.pressedKeys.Clear();
            this.keyShortcut.Clear();
        }

        private void NotifyNewKeyInShortcut(KeyAndAction keyAndAction)
        {
            if(this.NewKeyInShortcut != null)
            {
                this.NewKeyInShortcut(keyAndAction);
            }
        }
        
    }
}
