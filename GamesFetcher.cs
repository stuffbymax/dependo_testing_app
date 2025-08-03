// GamesFetcher.cs

// These using statements are required for the code in this file to work.
using System;
using System.Collections.Generic; // For List<>
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;     // For Task<>
using System.Windows.Forms;       // For MessageBox

public static class GamesFetcher
{
    public static async Task<List<Game>> LoadGamesAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "DependoApp/1.0");
        var url = "https://raw.githubusercontent.com/stuffbymax/game-dependencies-db/main/games.json";
        
        try
        {
            var games = await client.GetFromJsonAsync<List<Game>>(url);
            return games ?? new List<Game>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load game data. Please check your internet connection.\n\nError: {ex.Message}", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<Game>();
        }
    }
}