using DB_SQLite.Data_Acces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_SQLite.Event_Execute
{
    public class StoreExecuter
    {
        public event EventHandler<Store_Event> Store;

        public StoreExecuter()
        {
            
        }

        public virtual void OnStore(Store_Event e)
        {
            Store?.Invoke(this, e);

            TestModel_DataAcces.Save(e.Model);
        }
    }
}
