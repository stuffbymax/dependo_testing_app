using System.Net.Http.Json;

public static class GamesFetcher
{
    public static async Task<List<Game>> LoadGamesAsync()
    {
        using var client = new HttpClient();
        var url = "https://raw.githubusercontent.com/stuffbymax/game-dependencies-db/main/games.json";
        var games = await client.GetFromJsonAsync<List<Game>>(url);
        return games ?? new List<Game>();
    }
}
