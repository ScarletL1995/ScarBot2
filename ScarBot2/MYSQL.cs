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

        public async Task ExecuteAsync(string query)
        {
            if (connection == null) return;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            try
            {
                await cmd.ExecuteNonQueryAsync();
                Log.Log($"Executed query: {query}", LOGLEVEL.INFO);
            }
            catch (Exception ex)
            {
                Log.Log($"Failed to execute query: {query}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
            }
        }

        private void Execute(string query)
        {
            if (connection == null) return;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            try
            {
                cmd.ExecuteNonQuery();
                Log.Log($"Executed query: {query}", LOGLEVEL.INFO);
            }
            catch (Exception ex)
            {
                Log.Log($"Failed to execute query: {query}. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
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
                Log.Log($"Query execution failed. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
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
                Log.Log($"Query execution failed. Error: {ex.Message}", LOGLEVEL.ERROR, ex);
                return null;
            }
        }

        public bool CreateTable(string tableName, Dictionary<string, string> columns)
        {
            if (columns.Count == 0) return false;

            string columnDefinitions = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
            string query = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitions})";

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

        public async Task<bool> CreateTableAsync(string tableName, Dictionary<string, string> columns)
        {
            if (columns.Count == 0) return false;

            if (ShowTables().Contains(tableName)) return true;

            string columnDefinitions = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
            string query = $"CREATE TABLE {tableName} ({columnDefinitions})";

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

        public async Task<bool> CreateTableIfAsync(string tableName, Dictionary<string, string> tableSchema)
        {
            if (string.IsNullOrEmpty(tableName) || tableSchema == null || tableSchema.Count == 0) return false;

            try
            {
                await Log.LogAsync($"Checking table '{tableName}' for missing columns...");

                List<string> existing = ShowColumns(tableName);

                foreach (KeyValuePair<string, string> column in tableSchema)
                    if (!existing.Contains(column.Key))
                    {
                        await ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {column.Key} {column.Value}");
                        await Log.LogAsync($"Added missing column '{column.Key}' to '{tableName}'");
                    }

                return true;
            }
            catch (Exception ex)
            {
                await Log.LogAsync($"Failed to check columns for '{tableName}', trying to create table instead. Error: {ex.Message}");

                string schema = string.Join(", ", tableSchema.Select(kvp => $"{kvp.Key} {kvp.Value}"));
                string query = $"CREATE TABLE {tableName} ({schema})";

                try
                {
                    await ExecuteAsync(query);
                    await Log.LogAsync($"Created table '{tableName}' with full schema.");
                    return true;
                }
                catch (Exception inner)
                {
                    await Log.LogAsync($"Failed to create table '{tableName}'. Error: {inner.Message}");
                    return false;
                }
            }
        }

        public bool CreateTableIf(string tableName, Dictionary<string, string> tableSchema)
        {
            if (string.IsNullOrEmpty(tableName) || tableSchema == null || tableSchema.Count == 0) return false;

            try
            {
                Log.Log($"Checking table '{tableName}' for missing columns...");

                List<string> existing = new List<string>();
                using MySqlCommand command = new MySqlCommand($"SHOW COLUMNS FROM {tableName}", connection);
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    existing.Add(reader.GetString(0));

                reader.Close();

                foreach (KeyValuePair<string, string> column in tableSchema)
                    if (!existing.Contains(column.Key))
                    {
                        Execute($"ALTER TABLE {tableName} ADD COLUMN {column.Key} {column.Value}");
                        Log.Log($"Added missing column '{column.Key}' to '{tableName}'");
                    }

                return true;
            }
            catch (Exception ex)
            {
                Log.Log($"Failed to check columns for '{tableName}', trying to create table instead. Error: {ex.Message}");

                string schema = string.Join(", ", tableSchema.Select(kvp => $"{kvp.Key} {kvp.Value}"));
                string query = $"CREATE TABLE {tableName} ({schema})";

                try
                {
                    Execute(query);
                    Log.Log($"Created table '{tableName}' with full schema.");
                    return true;
                }
                catch (Exception inner)
                {
                    Log.Log($"Failed to create table '{tableName}'. Error: {inner.Message}");
                    return false;
                }
            }
        }

    }
}
