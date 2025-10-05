using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using Slipstream.Models;
using Slipstream.Services;

namespace Slipstream.Data
{
    class RivalsORM
    {
        private static SqliteConnection userConn = new SqliteConnection("Data Source=rivals2results.db;");
        private static SqliteConnection staticConn = new SqliteConnection("Data Source=RivalsStatic.db;");

        public static List<MatchResult> AllMatches = new();

        public static ObservableCollection<MatchResult> GetAllMatches(string character = "None")
        {
            DataTable table;

            if (character == "None")
            {
                table = ExecuteQuery("SELECT * FROM Matches ORDER BY ID DESC");
                AllMatches = CreateCollectionFromTable<MatchResult>(table);
                return new ObservableCollection<MatchResult>(AllMatches);
            }
            else
            {
                table = ExecuteQuery($"SELECT * FROM Matches WHERE OppChar1 = '{character}' ORDER BY ID DESC");
                return new ObservableCollection<MatchResult>(CreateCollectionFromTable<MatchResult>(table));
            }
        }

        public static ObservableCollection<MatchResult> GetAllMatchesByPlayer(string playerTag)
        {
            DataTable table;

            table = ExecuteQuery($"SELECT * FROM Matches WHERE Opponent = '{playerTag}' ORDER BY ID DESC");

            List<MatchResult> result = CreateCollectionFromTable<MatchResult>(table);
            return new ObservableCollection<MatchResult>(result);
        }

        public static ObservableCollection<RivalsSeason> GetSeasons()
        {
            DataTable table;

            table = ExecuteQuery($"SELECT * FROM Seasons", DbConnection.StaticData);

            List<RivalsSeason> result = CreateCollectionFromTable<RivalsSeason>(table);
            return new ObservableCollection<RivalsSeason>(result);
        }

        public static ObservableCollection<RivalsCharacter> GetAllRivals()
        {
            DataTable table;

            table = ExecuteQuery($"SELECT * FROM Characters", DbConnection.StaticData);

            List<RivalsCharacter> result = CreateCollectionFromTable<RivalsCharacter>(table);
            return new ObservableCollection<RivalsCharacter>(result);
        }

        public static string AddMatch(RivalsMatch match)
        {
            string oppMain = string.Empty;
            string oppAlt = string.Empty;

            if (match.LocalPlayerNameNotMatched())
            {
                match.Opponent.FormatManualData();
                match.Me.FormatManualData();
            }

            match.DetermineMatchCharacters();

            // Order characters in the match by played amount - get index of them to determine main or alt.
            List<KeyValuePair<string, int>> orderedCharacters = match.CharactersPlayed.OrderByDescending(c => c.Value).ToList();

            oppMain = orderedCharacters[0].Key;

            if (orderedCharacters.Count > 1)
                oppAlt = orderedCharacters[1].Key;


            using (userConn)
            {
                userConn.Open();

                SqliteCommand cmd = new SqliteCommand();
                cmd.Connection = userConn;

                cmd.CommandText = "INSERT INTO Matches (Date, MyChar, MyElo, Opponent, OpponentElo, OppChar1, OppChar2, Result, Patch, Notes) " +
                     "VALUES (@Date, @MyChar, @MyElo, @OppName, @OpponentElo, @OppChar1, @OppChar2, @MatchResult, @Patch, @Notes); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@MyChar", match.Me.Character);
                cmd.Parameters.AddWithValue("@MyElo", match.Me.EloString);
                cmd.Parameters.AddWithValue("@OppName", match.Opponent.Name);
                cmd.Parameters.AddWithValue("@OppChar1", oppMain);
                cmd.Parameters.AddWithValue("@OppChar2", oppAlt);
                cmd.Parameters.AddWithValue("@OpponentElo", match.Opponent.Elo);
                cmd.Parameters.AddWithValue("@MatchResult", match.MatchResult);
                cmd.Parameters.AddWithValue("@Patch", match.Patch);
                cmd.Parameters.AddWithValue("@Notes", match.Notes);

                Task<object?> rowID = cmd.ExecuteScalarAsync();

                if (rowID == null)
                    return "Error: New Row is Null";

                if (rowID.Exception != null)
                    return $"**Unknown Error:** {rowID.Exception.Message}";

                AddGames(match.Games, (long)rowID.Result);

                return $"Added new Match {match.Me.Elo} vs {match.Opponent.Character} / {match.Opponent.EloString} result: {match.MatchResult}";
            }
        }

        public static void AddGames(ObservableCollection<RivalsGame> games, long matchID)
        {
            foreach (RivalsGame game in games)
            {
                if (!game.ResultIsValid())
                    continue;

                using (userConn)
                {
                    userConn.Open();

                    SqliteCommand cmd = new SqliteCommand();
                    cmd.Connection = userConn;

                    cmd.CommandText = "INSERT INTO Games (MatchID, GameNumber, PickedStage, MyCharacter, OppCharacter, Result) " +
                         "VALUES (@MatchID, @GameNumber, @PickedStage, @MyCharacter, @OppCharacter, @Result); SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@MatchID", matchID);
                    cmd.Parameters.AddWithValue("@GameNumber", game.GameNumber);
                    cmd.Parameters.AddWithValue("@PickedStage", game.SelectedStage.StageName);
                    cmd.Parameters.AddWithValue("@MyCharacter", game.MyCharacter);
                    cmd.Parameters.AddWithValue("@OppCharacter", game.OppCharacter);
                    cmd.Parameters.AddWithValue("@Result", game.Result);

                    Task<object?> rowID = cmd.ExecuteScalarAsync();

                    if (rowID == null)
                        Debug.WriteLine("Error: New Row is Null");

                    if (rowID.Exception != null)
                        Debug.WriteLine($"**Unknown Error:** {rowID.Exception.Message}");

                    AddBannedStages(game.BannedStages, (long)rowID.Result, game.GameNumber);
                }
            }
        }

        public static void AddBannedStages(List<RivalsStage> bannedStages, long gameID, int gameNumber)
        {
            foreach (RivalsStage stage in bannedStages)
            {
                if (gameNumber == 1)
                    return; // We don't mark bans for game one

                using (userConn)
                {
                    userConn.Open();

                    SqliteCommand cmd = new SqliteCommand();
                    cmd.Connection = userConn;

                    cmd.CommandText = "INSERT INTO BannedStages (GameID, BannedStage) " +
                         "VALUES (@GameID, @BannedStage); SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@GameID", gameID);
                    cmd.Parameters.AddWithValue("@BannedStage", stage.StageName);

                    Task<object?> rowID = cmd.ExecuteScalarAsync();

                    if (rowID == null)
                        Debug.WriteLine("Error: New Row is Null");

                    if (rowID.Exception != null)
                        Debug.WriteLine($"**Unknown Error:** {rowID.Exception.Message}");
                }
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

        public static int GetPlayAudioValue()
        {
            return ExecuteQueryForInt("SELECT PlayAudio FROM Metadata LIMIT 1");
        }

        public static int GetSaveCapturesValue()
        {
            return ExecuteQueryForInt("SELECT SaveCaptures FROM Metadata LIMIT 1");
        }

        public static int GetCurrentSeason()
        {
            return ExecuteQueryForInt("SELECT CurrentSeason FROM Metadata LIMIT 1");
        }

        public static void SaveHotKeyToDatabase(ModifierKeys modifiers, Key key)
        {
            SetMetaDataValue("CaptureHotKeyCode", HotKeyService.ConvertKeyCodeToUint(key).ToString());
            SetMetaDataValue("CaptureModifierCode", HotKeyService.ConvertModifierFlagsToUint(modifiers).ToString());
        }

        public static string SetMetaDataValue(string field, string newValue)
        {
            using (userConn)
            {
                userConn.Open();

                SqliteCommand cmd = new SqliteCommand();
                cmd.Connection = userConn;
                cmd.CommandText = $"UPDATE Metadata SET {field} = '{newValue}' WHERE ID = '1'";

                Task<object?> rowID = cmd.ExecuteScalarAsync();

                if (rowID == null)
                    return "Error: New Row is Null";

                return "Successfully set Player Character Value";
            }
        }

        public static string UpdateMatch(MatchResult matchResult)
        {
            using (userConn)
            {
                try
                {
                    userConn.Open();

                    SqliteCommand cmd = new SqliteCommand();
                    cmd.Connection = userConn;

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
            SqliteCommand command = userConn.CreateCommand();
            ExecuteQuery($"DELETE FROM Matches WHERE ID = {matchResult.ID}");      
        }

        public static DataTable ExecuteQuery(string query, DbConnection connectionEnum = DbConnection.UserData)
        {
            SqliteConnection connection = GetConnection(connectionEnum);

            DataTable table = new DataTable();
            using (connection)
            {
                connection.Open();
                SqliteDataReader reader = new SqliteCommand(query, connection).ExecuteReader();
                table.Load(reader);
            }

            return table;
        }

        public static DataTable ExecuteQuery(SqliteCommand command, DbConnection connectionEnum = DbConnection.UserData)
        {
            SqliteConnection connection = GetConnection(connectionEnum);

            DataTable table = new DataTable();

            using (connection)
            {
                connection.Open();
                SqliteDataReader reader = command.ExecuteReader();
                table.Load(reader);
            }

            return table;
        }

        public static uint ExecuteQueryForUint(string command, DbConnection connectionEnum = DbConnection.UserData)
        {
            SqliteConnection connection = GetConnection(connectionEnum);

            object? value = null;

            using (connection)
            {
                connection.Open();
                value = new SqliteCommand(command, connection).ExecuteScalar();
            }

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToUInt32(value);
        }

        public static int ExecuteQueryForInt(string command, DbConnection connectionEnum = DbConnection.UserData)
        {
            SqliteConnection connection = GetConnection(connectionEnum);

            object? value = null;

            using (connection)
            {
                connection.Open();
                value = new SqliteCommand(command, connection).ExecuteScalar();
            }

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToInt32(value);
        }

        public static string ExecuteQueryForValue(string command, DbConnection connectionEnum = DbConnection.UserData)
        {
            SqliteConnection connection = GetConnection(connectionEnum);

            object? value = null;

            using (connection)
            {
                connection.Open();
                value = new SqliteCommand(command, connection).ExecuteScalar();
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

        public static SqliteConnection GetConnection(DbConnection connection)
        {
            if (connection == DbConnection.StaticData)
            {
                return staticConn;
            }
            else if (connection == DbConnection.UserData)
            {
                return userConn;
            }

            return userConn;
        }
    }

    public enum DbConnection
    {
        UserData,
        StaticData
    }
}
