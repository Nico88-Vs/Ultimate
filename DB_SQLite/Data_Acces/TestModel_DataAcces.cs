using Dapper;
using DB_SQLite.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using System.Data.SqlClient;

namespace DB_SQLite.Data_Acces
{
    public class TestModel_DataAcces
    {
        private static string GetConnectionString(bool isrelative, bool pool)
        {
            return UseSecretes.GetConnString(isrelative, pool);
        }

        public static void Save(Test_model model)
        {
            try
            {
                string x = GetConnectionString(true, true);
                using (SQLiteConnection cnn = new SQLiteConnection(x))
                {
                    cnn.Open();

                    var state = cnn.State;

                    string query = "INSERT INTO Test (Id, Name) values (@Id, @Name)";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, cnn))
                    {
                        cmd.Parameters.AddWithValue("@Id", model.Id);
                        cmd.Parameters.AddWithValue("@Name", model.Name);

                        cmd.ExecuteNonQuery();
                    }

                    cnn.Close();
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                Console.WriteLine(s);

            }

        }


        //public static void SaveP(Test_model model)
        //{
        //    try
        //    {
        //        string x = GetConnectionString(true, true);
        //        using (SQLiteConnection cnn = new SQLiteConnection(x))
        //        {
        //            cnn.Open();

        //            var state = cnn.State;

        //            string query = "INSERT INTO base (Id, Name) values (@Id, @Name)";

        //            using (SQLiteCommand cmd = new SQLiteCommand(query, cnn))
        //            {
        //                cmd.Parameters.AddWithValue("@Id", model.Id);
        //                cmd.Parameters.AddWithValue("@Name", model.Name);

        //                cmd.ExecuteNonQuery();
        //            }

        //            cnn.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string s = ex.Message;
        //        Console.WriteLine(s);

        //    }

        //}
        //public static void Save(Test_model model)
        //{
        //    try
        //    {
        //        string x = "Data Source=DESKTOP-URRF676\\MYFIRSTSQL;Initial Catalog=TestDB;Integrated Security=True";
        //        using (SqlConnection cnn = new SqlConnection(x))
        //        {
        //            cnn.Open();

        //            var state = cnn.State;

        //            string query = "INSERT INTO sTrategiTest (Id, nome) values (@Id, @nome)";

        //            using (SqlCommand cmd = new SqlCommand(query, cnn))
        //            {
        //                cmd.Parameters.AddWithValue("@Id", model.Id);
        //                cmd.Parameters.AddWithValue("@nome", model.Name);

        //                cmd.ExecuteNonQuery();
        //            }

        //            cnn.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string s = ex.Message;
        //        Console.WriteLine(s);

        //    }

        //}

        //private static List<Test_model> Load()
        //{
        //    using (IDbConnection cnn = new SQLiteConnection(GetConnectionString(true, true)))
        //    {
        //        var output = cnn.Query<Test_model>("select * from Test", new DynamicParameters());
        //        return output.ToList();
        //    }
        //}
    }
}
