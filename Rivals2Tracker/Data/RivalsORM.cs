using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using Slipstream.Models;
using Slipstream.Services;

namespace Slipstream.Data
{
    // TODO:  The ObjectRecord > Object pattern could be further compressed into the generic structures of CreateCollectionFromTable<T>.
    class RivalsORM
    {
        private static SqliteConnection userConn = new SqliteConnection(GlobalData.MatchDataConnectionString);
        private static SqliteConnection staticConn = new SqliteConnection(GlobalData.StaticDataConnectionString);

        public static List<MatchResult> AllMatches = new();

        public static ObservableCollection<MatchResult> GetAllMatches(RivalsCharacter character = null)
        {
            DataTable table;
            List<MatchResult> matchRecords = new();

            if (character == null)
            {
                table = ExecuteQuery("SELECT * FROM Matches ORDER BY ID DESC");
                List<MatchResultRecord> result = CreateCollectionFromTable<MatchResultRecord>(table);

                foreach (MatchResultRecord matchRecord in result)
                {
                    matchRecords.Add(new MatchResult(matchRecord));
                }

                AllMatches = matchRecords;

                return new ObservableCollection<MatchResult>(AllMatches);
            }
            else
            {
                table = ExecuteQuery($"SELECT * FROM Matches WHERE OppChar1 = '{character.ID}' ORDER BY ID DESC");
                List<MatchResultRecord> result = CreateCollectionFromTable<MatchResultRecord>(table);

                foreach (MatchResultRecord matchRecord in result)
                {
                    matchRecords.Add(new MatchResult(matchRecord));
                }

                return new ObservableCollection<MatchResult>(matchRecords);
            }
        }

        public static ObservableCollection<MatchResult> GetAllMatchesByPlayer(string playerTag)
        {
            DataTable table;
            List<MatchResult> matchRecords = new();

            table = ExecuteQuery($"SELECT * FROM Matches WHERE Opponent = '{playerTag}' ORDER BY ID DESC");

            List<MatchResultRecord> result = CreateCollectionFromTable<MatchResultRecord>(table);

            foreach (MatchResultRecord matchRecord in result)
            {
                matchRecords.Add(new MatchResult(matchRecord));
            }

            return new ObservableCollection<MatchResult>(matchRecords);
        }

        public static Dictionary<long, ObservableCollection<GameResult>> GetAllGameResultData()
        {
            List<GameResultRecord> results = new();
            Dictionary<long, ObservableCollection<GameResult>> gameReferenceDict = new();

            string query = """       
                SELECT Matches.ID as MatchID, Games.ID AS GameID, Games.GameNumber, Games.MyCharacter, Games.OppCharacter, Games.PickedStage, Games.Result,
                    CAST(GROUP_CONCAT(BannedStages.BannedStage, ', ') AS TEXT) AS 'BannedStagesList'
                FROM Matches
                INNER JOIN Games ON Games.MatchID = Matches.ID
                LEFT JOIN BannedStages ON BannedStages.GameID = Games.ID
                    GROUP BY Matches.ID, Games.ID, Games.GameNumber, Games.MyCharacter, Games.OppCharacter, Games.PickedStage, Games.Result
                ORDER BY Matches.ID, Games.GameNumber;          
                """;

            using SqliteCommand cmd = new SqliteCommand(query, userConn);
            {
                userConn.Open();
                using SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new GameResultRecord
                    {
                        MatchID = reader.GetInt64(reader.GetOrdinal("MatchID")),
                        GameID = reader.GetInt64(reader.GetOrdinal("GameID")),
                        GameNumber = reader.GetInt32(reader.GetOrdinal("GameNumber")),
                        MyCharacter = reader["MyCharacter"].ToString(),
                        OppCharacter = reader["OppCharacter"].ToString(),
                        PickedStage = reader["PickedStage"].ToString(),
                        Result = reader.GetInt64(reader.GetOrdinal("Result")),
                        BannedStagesList = reader["BannedStagesList"] is byte[] bytes
                            ? Encoding.UTF8.GetString(bytes)
                            : reader["BannedStagesList"]?.ToString()
                    });
                }
            }

            foreach (GameResultRecord gameResultRecord in results)
            {
                GameResult game = new GameResult(gameResultRecord);

                if (gameReferenceDict.ContainsKey(game.MatchID))
                {
                    gameReferenceDict[game.MatchID].Add(game);
                }
                else
                {
                    gameReferenceDict.Add(game.MatchID, new ObservableCollection<GameResult> { game });
                }
            }

            return gameReferenceDict;
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
            ObservableCollection<RivalsCharacter> characterOC = new();

            table = ExecuteQuery($"SELECT * FROM Characters", DbConnection.StaticData);

            List<RivalsCharacterRecord> result = CreateCollectionFromTable<RivalsCharacterRecord>(table);

            foreach(RivalsCharacterRecord characterRecord in result)
            {
                characterOC.Add(new RivalsCharacter(characterRecord));
            }

            return characterOC;
        }

        public static ObservableCollection<RivalsStage> GetAllStages()
        {
            DataTable table;
            ObservableCollection<RivalsStage> stages = new();

            table = ExecuteQuery($"SELECT * FROM Stages", DbConnection.StaticData);

            List<RivalsStageRecord> result = CreateCollectionFromTable<RivalsStageRecord>(table);

            foreach (RivalsStageRecord stageRecord in result)
            {
                stages.Add(new RivalsStage(stageRecord));
            }

            stages.OrderByDescending(s => s.ID);

            return stages;
        }

        public static string AddMatch(RivalsMatch match)
        {
            RivalsCharacter oppMain;
            RivalsCharacter? oppAlt = null;

            if (match.LocalPlayerNameNotMatched())
            {
                match.Opponent.FormatManualData();
                match.Me.FormatManualData();
            }

            match.DetermineMatchCharacters();

            // Order characters in the match by played amount - get index of them to determine main or alt.
            List<KeyValuePair<RivalsCharacter, int>> orderedCharacters = match.CharactersPlayed.OrderByDescending(c => c.Value).ToList();

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
                cmd.Parameters.AddWithValue("@MyChar", match.Me.Character.ID);
                cmd.Parameters.AddWithValue("@MyElo", match.Me.EloString);
                cmd.Parameters.AddWithValue("@OppName", match.Opponent.PlayerTag);
                cmd.Parameters.AddWithValue("@OppChar1", oppMain.ID);
                cmd.Parameters.AddWithValue("@OppChar2", oppAlt is not null ? oppAlt.ID : "");
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
                    cmd.Parameters.AddWithValue("@PickedStage", game.SelectedStage.ID);
                    cmd.Parameters.AddWithValue("@MyCharacter", game.MyCharacter.ID);
                    cmd.Parameters.AddWithValue("@OppCharacter", game.OppCharacter.ID);
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
                    cmd.Parameters.AddWithValue("@BannedStage", stage.ID);

                    Task<object?> rowID = cmd.ExecuteScalarAsync();

                    if (rowID == null)
                        Debug.WriteLine("Error: New Row is Null");

                    if (rowID.Exception != null)
                        Debug.WriteLine($"**Unknown Error:** {rowID.Exception.Message}");
                }
            }
        }

        public static long GetPlayerCharacter()
        {
            return ExecuteQueryForInt("SELECT PlayerCharacter FROM Metadata LIMIT 1");
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

        public static int GetCurrentDataVersion()
        {
            return ExecuteQueryForInt("SELECT CurrentDataVersion FROM Metadata LIMIT 1");
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
                        "OppChar2 = @OppChar2, Result = @MatchResult, Patch = @Patch, Notes = @Notes WHERE ID = @ID";
                    cmd.Parameters.AddWithValue("@ID", matchResult.ID);
                    cmd.Parameters.AddWithValue("@OppName", matchResult.Opponent);
                    cmd.Parameters.AddWithValue("@OppChar1", matchResult.OppChar1.ID);
                    cmd.Parameters.AddWithValue("@OppChar2", matchResult.OppChar2.ID);
                    cmd.Parameters.AddWithValue("@OpponentElo", matchResult.OpponentElo);
                    cmd.Parameters.AddWithValue("@MyChar", matchResult.MyChar.ID);
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
                    try
                    {
                        p.SetValue(obj, row[c], null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
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
