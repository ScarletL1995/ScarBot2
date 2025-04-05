using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using LoggingLib;

namespace MYSQL
{
    public class MySQL
    {
        private string server = "localhost";
        private string userid = "UserID";
        private string password = "Password";
        private string database = "Database";
        private uint port = 3306;
        private MySqlConnection? connection;

        private readonly LOGGER Log = new LOGGER();

        public MySQL(string server, string userid, string password, string database, uint port = 3306u)
        {
            this.server = server;
            this.userid = userid;
            this.password = password;
            this.database = database;
            this.port = port;
        }

        public MySqlConnection? GetConnect()
        {
            return connection;
        }

        public async Task<bool> ConnectAsync()
        {
            connection = new MySqlConnection($"Server={server};Database={database};User={userid};Password={password};Port={port};");
            try
            {
                await connection.OpenAsync();
                Log.Log($"Successfully connected to the database at {server}:{port}", LOGLEVEL.INFO);
                return true;
            }
            catch (Exception ex)
            {
                connection = null;
                Log.Log($"Failed to connect to the database at {server}:{port}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return false;
            }
        }

        public bool Connect()
        {
            connection = new MySqlConnection($"Server={server};Database={database};User={userid};Password={password};Port={port};");
            try
            {
                connection.Open();
                Log.Log($"Successfully connected to the database at {server}:{port}", LOGLEVEL.INFO);
                return true;
            }
            catch (Exception ex)
            {
                connection = null;
                Log.Log($"Failed to connect to the database at {server}:{port}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return false;
            }
        }

        public async Task CloseAsync()
        {
            if (connection != null)
            {
                await connection.CloseAsync();
                Log.Log($"Connection to the database at {server}:{port} closed.", LOGLEVEL.INFO);
            }
        }

        public void Close()
        {
            connection?.Close();
            Log.Log($"Connection to the database at {server}:{port} closed.", LOGLEVEL.INFO);
        }

        public async Task<bool> ExecuteAsync(string query)
        {
            if (connection == null) return false;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            try
            {
                await cmd.ExecuteNonQueryAsync();
                Log.Log($"Executed query: {query}", LOGLEVEL.INFO);
                return true;
            }
            catch (Exception ex)
            {
                Log.Log($"Failed to execute query: {query}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return false;
            }
        }

        public bool Execute(string query)
        {
            if (connection == null) return false;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            try
            {
                cmd.ExecuteNonQuery();
                Log.Log($"Executed query: {query}", LOGLEVEL.INFO);
                return true;
            }
            catch (Exception ex)
            {
                Log.Log($"Failed to execute query: {query}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return false;
            }
        }

        public async Task<List<string>> ShowTablesAsync()
        {
            var tables = new List<string>();
            var query = "SHOW TABLES";
            var result = await QueryAsync(query);
            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    tables.Add(row[0].ToString() ?? string.Empty);
                }
            }
            return tables;
        }

        public List<string> ShowTables()
        {
            var tables = new List<string>();
            var query = "SHOW TABLES";
            var result = Query(query);
            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    tables.Add(row[0].ToString() ?? string.Empty);
                }
            }
            return tables;
        }

        public async Task<List<string>> ShowColumnsAsync(string tableName)
        {
            var columns = new List<string>();
            var query = $"SHOW COLUMNS FROM {tableName}";
            var result = await QueryAsync(query);
            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    columns.Add(row[0].ToString() ?? string.Empty);
                }
            }
            return columns;
        }

        public List<string> ShowColumns(string tableName)
        {
            var columns = new List<string>();
            var query = $"SHOW COLUMNS FROM {tableName}";
            var result = Query(query);
            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    columns.Add(row[0].ToString() ?? string.Empty);
                }
            }
            return columns;
        }

        public async Task<bool> UpdateAsync(string tableName, Dictionary<string, object> setValues, string whereClause)
        {
            if (setValues.Count == 0 || string.IsNullOrWhiteSpace(whereClause)) return false;

            string setClause = string.Join(", ", setValues.Select(kv => $"{kv.Key} = '{kv.Value}'"));
            string query = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

            try
            {
                await ExecuteAsync(query);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Update(string tableName, Dictionary<string, object> setValues, string whereClause)
        {
            if (setValues.Count == 0 || string.IsNullOrWhiteSpace(whereClause)) return false;

            string setClause = string.Join(", ", setValues.Select(kv => $"{kv.Key} = '{kv.Value}'"));
            string query = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

            try
            {
                Execute(query);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertAsync(string tableName, Dictionary<string, object> columns)
        {
            if (columns.Count == 0) return false;

            string columnNames = string.Join(", ", columns.Keys);
            string values = string.Join(", ", columns.Values.Select(v => $"'{v}'"));
            string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values})";

            try
            {
                await ExecuteAsync(query);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Insert(string tableName, Dictionary<string, object> columns)
        {
            if (columns.Count == 0) return false;

            string columnNames = string.Join(", ", columns.Keys);
            string values = string.Join(", ", columns.Values.Select(v => $"'{v}'"));
            string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values})";

            try
            {
                Execute(query);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertOrUpdateAsync(string tableName, Dictionary<string, object> columns, bool updateIfExists = false, string? uniqueKey = null)
        {
            if (columns.Count == 0) return false;

            if (updateIfExists && !string.IsNullOrEmpty(uniqueKey))
            {
                string setClause = string.Join(", ", columns.Select(kv => $"{kv.Key} = '{kv.Value}'"));
                string query = $"INSERT INTO {tableName} ({string.Join(", ", columns.Keys)}) VALUES ({string.Join(", ", columns.Values.Select(v => $"'{v}'"))}) " +
                               $"ON DUPLICATE KEY UPDATE {setClause}";

                try
                {
                    await ExecuteAsync(query);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return await InsertAsync(tableName, columns);
            }
        }

        public bool InsertOrUpdate(string tableName, Dictionary<string, object> columns, bool updateIfExists = false, string? uniqueKey = null)
        {
            if (columns.Count == 0) return false;

            if (updateIfExists && !string.IsNullOrEmpty(uniqueKey))
            {
                string setClause = string.Join(", ", columns.Select(kv => $"{kv.Key} = '{kv.Value}'"));
                string query = $"INSERT INTO {tableName} ({string.Join(", ", columns.Keys)}) VALUES ({string.Join(", ", columns.Values.Select(v => $"'{v}'"))}) " +
                               $"ON DUPLICATE KEY UPDATE {setClause}";

                try
                {
                    Execute(query);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return Insert(tableName, columns);
            }
        }

        public async Task<DataTable?> QueryAsync(string query)
        {
            if (connection == null) return null;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                await Task.Run(() => da.Fill(dt));
                return dt;
            }
            catch (Exception ex)
            {
                Log.Log($"Query failed: {query}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return null;
            }
        }

        public DataTable? Query(string query)
        {
            if (connection == null) return null;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Log.Log($"Query failed: {query}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return null;
            }
        }
    }
}
