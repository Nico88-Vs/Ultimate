using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DB_SQLite
{
    public static class UseSecretes
    {
        private static string path = "C:\\Users\\user\\AppData\\Roaming\\Microsoft\\UserSecrets\\e2f0ce6a-aeba-4fbe-b0ea-e8aaedd172af\\secrets.json";

        public static string GetConnString(bool isrelative, bool pooling)
        {
            string output = "";

            SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

            string json = File.ReadAllText(path);

            var config = JsonConvert.DeserializeObject<Secret_Model>(json);

            if (config != null )
            {
                connectionStringBuilder.DataSource = isrelative ? config.SQlite_Db_Relative_Path : config.SQlite_Db_Absolute_Path;
                connectionStringBuilder.Pooling = pooling;
                output = connectionStringBuilder.ConnectionString;
            }

            Console.WriteLine(output);
           
            return output;
        }
    }
}
