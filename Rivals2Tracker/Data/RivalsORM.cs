using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using Rivals2Tracker.Models;
using Rivals2Tracker.Services;

namespace Rivals2Tracker.Data
{
    class RivalsORM
    {
        private static SqliteConnection conn = new SqliteConnection("Data Source=rivals2results.db;");

        public static List<MatchResult> AllMatches = new();

        public static ObservableCollection<MatchResult> GetAllMatches(string characterToGet = "None")
        {
            DataTable table;

            if (characterToGet == "None")
            {
                table = ExecuteQuery("SELECT * FROM Matches ORDER BY ID DESC");
            }
            else
            {
                table = ExecuteQuery($"SELECT * FROM Matches WHERE OppChar1 = '{characterToGet}' ORDER BY ID DESC");
            }

            AllMatches = CreateCollectionFromTable<MatchResult>(table);
            return new ObservableCollection<MatchResult>(AllMatches);
        }

        public static string AddMatch(RivalsMatch match)
        {
            if (match.HasNoKad())
            {
                match.Opponent.FormatManualData();
                match.Me.FormatManualData();
            }

            using (conn)
            {
                conn.Open();

                SqliteCommand cmd = new SqliteCommand();
                cmd.Connection = conn;

                cmd.CommandText = "INSERT INTO Matches (Date, MyChar, MyElo, Opponent, OpponentElo, OppChar1, OppChar2, OppChar3, Result, Patch, Notes) " +
                     "VALUES (@Date, @MyChar, @MyElo, @OppName, @OpponentElo, @OppChar1, @OppChar2, @OppChar3, @MatchResult, @Patch, @Notes); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@MyChar", match.Me.Character);
                cmd.Parameters.AddWithValue("@MyElo", match.Me.EloString);
                cmd.Parameters.AddWithValue("@OppName", match.Opponent.Name);
                cmd.Parameters.AddWithValue("@OppChar1", match.Opponent.Character);
                cmd.Parameters.AddWithValue("@OppChar2", match.Opponent.Character2);
                cmd.Parameters.AddWithValue("@OppChar3", match.Opponent.Character3);
                cmd.Parameters.AddWithValue("@OpponentElo", match.Opponent.Elo);
                cmd.Parameters.AddWithValue("@MatchResult", match.MatchResult);
                cmd.Parameters.AddWithValue("@Patch", match.Patch);
                cmd.Parameters.AddWithValue("@Notes", match.Notes);

                Task<object?> rowID = cmd.ExecuteScalarAsync();

                if (rowID == null)
                    return "Error: New Row is Null";

                if (rowID.Exception != null)
                    return $"**Unknown Error:** {rowID.Exception.Message}";

                return $"Added new Match {match.Me.Elo} vs {match.Opponent.Character} / {match.Opponent.EloString} result: {match.MatchResult}";
            }
        }
        public static string GetPlayerCharacter()
        {
            return ExecuteQueryForValue("SELECT PlayerCharacter FROM Metadata LIMIT 1");
        }

        public static string GetPlayerName()
        {
            return ExecuteQueryForValue("SELECT PlayerName FROM Metadata LIMIT 1");
        }

        public static string GetPatchValue()
        {
            return ExecuteQueryForValue("SELECT Patch FROM Metadata LIMIT 1");
        }

        public static string GetIsFirstStart()
        {
            return ExecuteQueryForValue("SELECT IsFirstStart FROM Metadata LIMIT 1");
        }

        public static uint GetMatchHotKey()
        {
            return ExecuteQueryForUint("SELECT CaptureHotKeyCode FROM Metadata LIMIT 1");
        }

        public static uint GetMatchHotKeyModifier()
        {
            return ExecuteQueryForUint("SELECT CaptureModifierCode FROM Metadata LIMIT 1");
        }

        public static void SaveHotKeyToDatabase(ModifierKeys modifiers, Key key)
        {
            SetMetaDataValue("CaptureHotKeyCode", HotKeyService.ConvertKeyCodeToUint(key).ToString());
            SetMetaDataValue("CaptureModifierCode", HotKeyService.ConvertModifierFlagsToUint(modifiers).ToString());
        }

        public static string SetMetaDataValue(string field, string newValue)
        {
            using (conn)
            {
                conn.Open();

                SqliteCommand cmd = new SqliteCommand();
                cmd.Connection = conn;
                cmd.CommandText = $"UPDATE Metadata SET {field} = '{newValue}' WHERE ID = '1'";

                Task<object?> rowID = cmd.ExecuteScalarAsync();

                if (rowID == null)
                    return "Error: New Row is Null";

                return "Successfully set Player Character Value";
            }
        }

        public static string UpdateMatch(MatchResult matchResult)
        {
            using (conn)
            {
                try
                {
                    conn.Open();

                    SqliteCommand cmd = new SqliteCommand();
                    cmd.Connection = conn;

                    cmd.CommandText = "UPDATE Matches SET Opponent = @OppName, OppChar1 = @OppChar1, OpponentElo = @OpponentElo, MyChar = @MyChar, MyElo = @MyElo, " +
                        "OppChar2 = @OppChar2, OppChar3 = @OppChar3, Result = @MatchResult, Patch = @Patch, Notes = @Notes WHERE ID = @ID";
                    cmd.Parameters.AddWithValue("@ID", matchResult.ID);
                    cmd.Parameters.AddWithValue("@OppName", matchResult.Opponent);
                    cmd.Parameters.AddWithValue("@OppChar1", matchResult.OppChar1);
                    cmd.Parameters.AddWithValue("@OppChar2", matchResult.OppChar2);
                    cmd.Parameters.AddWithValue("@OppChar3", matchResult.OppChar3);
                    cmd.Parameters.AddWithValue("@OpponentElo", matchResult.OpponentElo);
                    cmd.Parameters.AddWithValue("@MyChar", matchResult.MyChar);
                    cmd.Parameters.AddWithValue("@MyElo", matchResult.MyElo);
                    cmd.Parameters.AddWithValue("@MatchResult", matchResult.Result);
                    cmd.Parameters.AddWithValue("@Patch", matchResult.Patch);
                    cmd.Parameters.AddWithValue("@Notes", matchResult.Notes);

                    Task<object?> rowID = cmd.ExecuteScalarAsync();

                    if (rowID == null)
                        return "Error: New Row is Null";

                    if (rowID.Exception != null)
                        return $"**Unknown Error:** {rowID.Exception.Message}";

                    return $"Updated Match with ID {matchResult.ID}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return "error";
                }
            }
        }

        public static void DeleteMatch(MatchResult matchResult)
        {
            SqliteCommand command = conn.CreateCommand();
            ExecuteQuery($"DELETE FROM Matches WHERE ID = {matchResult.ID}");      
        }

        public static DataTable ExecuteQuery(string query)
        {
            DataTable table = new DataTable();
            using (conn)
            {
                conn.Open();
                SqliteDataReader reader = new SqliteCommand(query, conn).ExecuteReader();
                table.Load(reader);
            }

            return table;
        }

        public static DataTable ExecuteQuery(SqliteCommand command)
        {
            DataTable table = new DataTable();

            using (conn)
            {
                conn.Open();
                SqliteDataReader reader = command.ExecuteReader();
                table.Load(reader);
            }

            return table;
        }

        public static uint ExecuteQueryForUint(string command)
        {
            object? value = null;

            using (conn)
            {
                conn.Open();
                value = new SqliteCommand(command, conn).ExecuteScalar();
            }

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToUInt32(value);
        }

        public static string ExecuteQueryForValue(string command)
        {
            object? value = null;

            using (conn)
            {
                conn.Open();
                value = new SqliteCommand(command, conn).ExecuteScalar();
            }

            return value as string ?? string.Empty;
        }

        public static List<T> CreateCollectionFromTable<T>(DataTable tbl) where T : new()
        {
            List<T> oc = new List<T>();

            foreach (DataRow r in tbl.Rows)
            {
                oc.Add(ConvertRow<T>(r));
            }

            return oc;
        }

        public static T ConvertRow<T>(DataRow row) where T : new()
        {
            T obj = new T();

            ConvertToObject(obj, row);

            return obj;
        }

        public static void ConvertToObject<T>(T obj, DataRow row) where T : new()
        {
            foreach (DataColumn c in row.Table.Columns)
            {
                PropertyInfo p = obj.GetType().GetProperty(c.ColumnName);

                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(obj, row[c], null);
                }
            }
        }
    }
}
