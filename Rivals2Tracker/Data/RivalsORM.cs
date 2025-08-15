using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Rivals2Tracker.Models;

namespace Rivals2Tracker.Data
{
    class RivalsORM
    {
        private static SqliteConnection conn = new SqliteConnection("Data Source=rivals2results.db;");

        public static List<MatchResult> AllMatches = new();

        public static ObservableCollection<MatchResult> GetAllMatches()
        {
            DataTable table = ExecuteQuery("SELECT * FROM Matches ORDER BY ID DESC");
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

                cmd.CommandText = "INSERT INTO Matches (Date, Opponent, OppChar1, OpponentElo, MyElo, OppChar2, OppChar3, Result, Patch, Notes) " +
                     "VALUES (@Date, @OppName, @OppChar1, @OpponentElo, @MyElo, @OppChar2, @OppChar3, @MatchResult, @Patch, @Notes); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@OppName", match.Opponent.Name);
                cmd.Parameters.AddWithValue("@OppChar1", match.Opponent.Character);
                cmd.Parameters.AddWithValue("@OppChar2", match.Opponent.Character2);
                cmd.Parameters.AddWithValue("@OppChar3", match.Opponent.Character3);
                cmd.Parameters.AddWithValue("@OpponentElo", match.Opponent.Elo);
                cmd.Parameters.AddWithValue("@MyElo", match.Me.EloString);
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
