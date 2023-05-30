using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Mysqlx.Session;
using MySqlX.XDevAPI.Common;

class Database_Manager
{
    private MySqlConnection conn;

    private static Database_Manager _DB_MANAGER = null;

    static public Database_Manager DB_MANAGER()
    {
        if (_DB_MANAGER == null)
        {
            _DB_MANAGER = new Database_Manager();
            _DB_MANAGER.StartDatabaseService();
        }
       
        return _DB_MANAGER;
    }
    

    public void StartDatabaseService() {
        //String con parametros de conexión
        const string connectionString = "Server=db4free.net;Port=3306;database=enti_test_db;Uid=ismael_rivero;password=warrior000;SSL Mode=None;connect timeout=3600;default command timeout=3600;";

        //Instancio clase MySQL
        conn = new MySqlConnection(connectionString);

    }
    public string GetLatestVersion()
    {
        string query = "SELECT * FROM versions";
        return  DataSelect(DataType.VERSION, query);
        
    }
    public string GetGameData()
    {
        string query = "SELECT * FROM races";
        return DataSelect(DataType.GAMEDATA, query);
        
    }
    public string GetPlayerCharacters(int playerID)
    {
        string query = "SELECT * FROM characters WHERE owner_id=" + playerID + ";";
        return DataSelect(DataType.CHARACTERDATA, query);
    }
    public string GetPlayerStats(string race_id)
    {
        string query = "SELECT * FROM races WHERE id_race=" + " '" + race_id + "';";
        return DataSelect(DataType.CHARACTERDATA, query);
    }
    public string GetPlayerRace(string player_id)
    {
        string query = "SELECT race_id FROM players WHERE id_player=" + player_id + ";";
        return (DataSelect(DataType.VERSION, query));
    }
    public bool ExistsPlayer(string playername)
    {
        string query = "SELECT * FROM players WHERE username= " + "'"+playername + "';";
        return DataSelect(DataType.USERDATA, query) != "";
    }

    public string DataSelect(DataType type, string query)
    {
        string result = "";
        
        try {
            conn.Open();

            MySqlDataReader reader;
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = query;

            try
            {
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    switch (type)
                    {
                        /// player id, username and password
                        case DataType.USERDATA:
                            result += reader.GetString(0) + ".";
                            result += reader.GetString(1) + ".";
                            result += reader.GetString(2) + ".";
                            break;

                        /// GAMEDATA
                        /// Retrieve data for races
                        /// 6 rows
                        case DataType.GAMEDATA:

                            result += reader.GetString(0) + ".";
                            result += reader.GetString(1) + ".";
                            result += reader.GetString(2) + ".";
                            result += reader.GetString(3) + ".";
                            result += reader.GetString(4) + ".";
                            result += reader.GetString(5) + "-";

                            break;

                        /// CHARACTERDATA
                        /// Retrieve user's characters
                        case DataType.CHARACTERDATA:
                            result += reader.GetString(0) + "/";
                            result += reader.GetString(1) + "/";
                            result += reader.GetString(2) + "/";
                            result += reader.GetString(3) + "/";
                            result += reader.GetString(4) + "/";
                            result += reader.GetString(5) + "/";
                            break;

                        /// VERSION
                        /// Check version
                        case DataType.VERSION:
                            result += reader.GetString(0).ToString();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            conn.Close();
            return result;
        }
        catch (Exception ex)
        { 
            Console.WriteLine(ex.Message); 
            
        }

        return result;


    }

    public int GetUserId(string name, string password)
    {
        conn.Open();
        string query = "SELECT id_player FROM players WHERE username='" + name + "' AND u_password='"+ password+ "';";
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = query;
        MySqlDataReader dataReader = cmd.ExecuteReader();


        while (dataReader.Read())
        {
            int id = dataReader.GetInt32(0);
            Console.WriteLine(id);

            conn.Close();
            return id;


        }
        conn.Close();
        return -1;
    }
    private int GetUserId(string name)
    {
        string query = "SELECT id_player FROM players WHERE username='" +name+"';";
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = query;
        MySqlDataReader dataReader = cmd.ExecuteReader();
        

        while (dataReader.Read())
        {
            int id = dataReader.GetInt32(0);
            Console.WriteLine(id);
            return id;
            

        }
        return -1;
    }

    public int  InsertUserData(string username, string password,string race)
    {
        string[] playerData = new string[3];
        playerData[0] = username;
        playerData[1] = password;
        playerData[2] = race;
        return DataInsert(DataType.USERDATA, playerData);
    }

    public int DataInsert(DataType dataType, string[] values)
    {
        int lastInsertID = -1;
        try
        {
            conn.Open();

            MySqlCommand cmd = conn.CreateCommand();

            string query = "INSERT INTO ";
                    switch (dataType)
                    {
                        /// USERDATA
                        /// Login
                        /// If only one row GOOD
                        case DataType.USERDATA:
                            query += "players(username, u_password, race_id) VALUES( '";
                            query += values[0] + "', '";
                            query += values[1] + "', '";
                            query += values[2] + "');";

                            cmd.CommandText = query;
                            cmd.ExecuteNonQuery();

                            lastInsertID = GetUserId(values[0]);
                            break;

                  
                        /// CHARACTERDATA
                        /// Retrieve user's characters
                        case DataType.CHARACTERDATA:
                            
                            query += " characters(character_name, chara_race, owner_id) VALUES('";
                            query += values[1] + "', ";
                            query += values[0] + ",  ";
                            query += values[2] + " );";
                            break;

                        default:
                            break;
                    }


            conn.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }
        return lastInsertID;
    }

        void DeleteExample(MySqlConnection conn)
        {
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM characters WHERE name='PEdro la Piedra';";

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }
}
