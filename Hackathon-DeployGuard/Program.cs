using System;
using System.Threading.Tasks;
using System.Data.SqlClient;

class Program
{
    static async Task Main(string[] args)
    {
        // Connection string for Azure SQL Database
        string connectionString = "Server=tcp:hackdeploy.database.windows.net,1433;Initial Catalog=HackDeployGuard;Persist Security Info=False;User ID=hackdeploy;Password=;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        LockStatusUpdater updater = new LockStatusUpdater(connectionString);
        // Infinite loop to run the task every minute
        while (true)
        {
            await updater.UpdateLockStatusAsync();
            
            // Wait for 1 minute before running again
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}

public class LockStatusUpdater
{
    private readonly string connectionString;

    public LockStatusUpdater(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task UpdateLockStatusAsync()
    {
        string query = @"
            UPDATE LockTable
            SET LockStatus = 0,
            LockEndTime = NULL
            WHERE LockStatus = 1 AND LockEndTime < @CurrentTime";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrentTime", DateTime.UtcNow);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    Console.WriteLine($"{rowsAffected} rows updated.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
