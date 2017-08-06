using PdS_Project_2015_client_WPF.model;
using PdS_Project_2015_client_WPF.model.json;
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
                int virtualCode = KeyInterop.VirtualKeyFromKey(pressedKey);
                KeyAndAction keyAndAction = new KeyAndAction() { Key = pressedKey, VirtualCode=virtualCode, IsDown = true };
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
                int virtualCode = KeyInterop.VirtualKeyFromKey(releasedKey);
                KeyAndAction keyAndAction = new KeyAndAction() { Key = releasedKey, VirtualCode = virtualCode, IsDown = false };                
                this.keyShortcut.Add(keyAndAction);
                this.NotifyNewKeyInShortcut(keyAndAction);
            }            
        }

        public void Send()
        {
            //WARNING: it assumed that the connection is already opened and connected!!!

            //create the message
            List<JsonKeyShortcutAction> shortcutActions = new List<JsonKeyShortcutAction>();
            foreach (KeyAndAction keyAndAction in this.keyShortcut)
            {
                JsonKeyShortcutAction jsonShortcutAction = new JsonKeyShortcutAction()
                {
                    key_virtual_code = keyAndAction.VirtualCode,
                    is_down = keyAndAction.IsDown                    
                };
                shortcutActions.Add(jsonShortcutAction);
            }
            JsonKeyShortcutMessage jsonKeyShortcutMessage = new JsonKeyShortcutMessage()
            {
                shortcut_actions_number = shortcutActions.Count,
                shortcut_actions = shortcutActions
            };

            //translate to string                        
            string messageToSend = Newtonsoft.Json.JsonConvert.SerializeObject((object)jsonKeyShortcutMessage);

            //use connection Send method
            this.connection.SendMessage(messageToSend);
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
