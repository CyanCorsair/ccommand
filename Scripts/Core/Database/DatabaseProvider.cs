using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Godot;
using Microsoft.Data.Sqlite;

namespace CCDatabase
{
    public class DatabaseProvider
    {
        private const string dbPath = "Data Source=Data/CCDatabase.db";
        private SqliteConnection dbConnection;

        public async Task ExecuteCommand(string commandString)
        {
            dbConnection = new SqliteConnection(dbPath);

            try
            {
                await dbConnection.OpenAsync();
                SqliteCommand command = GetCommand(commandString);
                await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
                GD.PrintErr($"Error: {e.Message}");
                await dbConnection.DisposeAsync();
                dbConnection = null;
                throw e;
            }
            finally
            {
                await dbConnection.CloseAsync();
                await dbConnection.DisposeAsync();

                dbConnection = null;
            }
        }

        private SqliteCommand GetCommand(string text)
        {
            if (dbConnection is null || dbConnection.State != ConnectionState.Open)
            {
                return null;
            }

            SqliteCommand command = dbConnection.CreateCommand();
            command.CommandText = text;

            return command;
        }
    }
}
