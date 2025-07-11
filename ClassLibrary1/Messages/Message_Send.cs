using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Message
{
    public class Message_Send
    {
        public string type { get; set; }
        public string content { get; set; }
        public string? next_node { get; set; }
    }
}
