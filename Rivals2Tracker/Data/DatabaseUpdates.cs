using Microsoft.Data.Sqlite;
using System;
using System.Diagnostics;
using System.Windows;

namespace Slipstream.Data
{
    public static class DatabaseUpdates
    {
        private static SqliteConnection userConn = new SqliteConnection(GlobalData.MatchDataConnectionString);
        private static SqliteConnection staticConn = new SqliteConnection(GlobalData.StaticDataConnectionString);

        public static void CheckForDataSchemaUpdates()
        {
            try
            {
                GlobalData.CurrentDataVersion = GetCurrentDataVersion();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to GetCurrentDataVersion() - Falling back to -1");
            }

            StepThroughDataUpdates();
        }

        private static int GetCurrentDataVersion()
        {
            return RivalsORM.GetCurrentDataVersion();
        }

        private static void StepThroughDataUpdates()
        {
            bool isUpToDate = false;

            while (!isUpToDate)
            {
                isUpToDate = RunDatabaseUpdates(GlobalData.CurrentDataVersion);
                GlobalData.CurrentDataVersion = GetCurrentDataVersion();
            }
        }

        // This returns a bool based on whether or not it's 'done' updating the database.  A value of true means that the update is complete, and a value of false means it needs to do another cycle of updates.
        public static bool RunDatabaseUpdates(int currentDataVersion)
        {
            if (currentDataVersion == GlobalData.LatestDataVersion)
            {
                return true;
            }

            if (currentDataVersion > GlobalData.LatestDataVersion)
            {
                MessageBox.Show("There is a critical data mismatch - it appears you are using an old version of Slipstream with a new Database Scheme.  " +
                    "Please make sure you are using the latest version of Slipstream!\n\n Slipstream will close to prevent data corruption.");
                Application.Current.Shutdown();
                return false;
            }

            // We check current data version and update to the NEXT version.
            // E.g., if the case is '1', the code here updates it to version 2.  The case is always '1 behind' the version it represents.
            // A case of -1 means that the database is completely new and doesn't even have the 'Data Version' structure yet.
            switch (currentDataVersion)
            {
                case -1: // Add Metadata column 'DataVersion' and set it's value to 1.
                    userConn.Open();

                    bool columnExists = false;
                    using (SqliteCommand checkCmd = userConn.CreateCommand())
                    {
                        checkCmd.CommandText = "PRAGMA table_info(Metadata);";
                        using SqliteDataReader reader = checkCmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var columnName = reader.GetString(1);
                            if (columnName.Equals("CurrentDataVersion", StringComparison.OrdinalIgnoreCase))
                            {
                                columnExists = true;
                                break;
                            }
                        }
                    }

                    if (!columnExists)
                    {
                        using (SqliteCommand alterCmd = userConn.CreateCommand())
                        {
                            alterCmd.CommandText = "ALTER TABLE Metadata ADD COLUMN CurrentDataVersion INTEGER DEFAULT 1;";
                            alterCmd.ExecuteNonQuery();
                        }

                        using (SqliteCommand updateCmd = userConn.CreateCommand())
                        {
                            updateCmd.CommandText = "UPDATE Metadata SET CurrentDataVersion = 1 WHERE CurrentDataVersion IS NULL;";
                            updateCmd.ExecuteNonQuery();
                        }
                    }

                    userConn.Close();
                    return false;

                default:
                    MessageBox.Show($"There was an error attempting to update the database scheme.  The Latest data version is {GlobalData.LatestDataVersion}.  The Current data version is: {currentDataVersion}." +
                        $"Slipstream will close to prevent data corruption - please report this issue.");
                    Application.Current.Shutdown();
                    return false;
            }
        }
    }
}
