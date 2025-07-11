﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Message
{
    public class MessageFromNode
    {
        public string type { get; set; }
        public Dictionary<string, object> content { get; set; }
        public string? next_node { get; set; }
    }
}
