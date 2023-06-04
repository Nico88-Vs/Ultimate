using DB_SQLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_SQLite.Event_Execute
{
    public enum storeType { Test }
    public class Store_Event : EventArgs
    {
        public storeType Type { get; set; }
        public Test_model Model { get; set; }

        public Store_Event(storeType type, Test_model model)
        {
            this.Type = type;
            this.Model = model;
        }
    }
}
