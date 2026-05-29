using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace InnocentGuardPart2
{

    class ChatMemory
    {
        private Dictionary<string, string> memory = new Dictionary<string, string>();

        public void Remember(string key, string value)
        {
            memory[key] = value;
        }

        public string Recall(string key)
        {
            return memory.ContainsKey(key) ? memory[key] : string.Empty;
        }
            public bool Knows(string key)
            {
                return memory.ContainsKey(key) && !string.IsNullOrEmpty(memory[key]);
            }
        }
    }



           
