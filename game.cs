public class Game
{
    public string Name { get; set; }
    public string Image { get; set; }
    public string DirectX { get; set; }
    public string DotNet { get; set; }
    public string[] VCRedist { get; set; }
    public string[] DLLs { get; set; }
    public string[] Fixes { get; set; }
    public Downloads Downloads { get; set; }
}

public class Downloads
{
    public string DirectX { get; set; }
    public Dictionary<string, string> VCRedist { get; set; }
    public string DotNet { get; set; }
}
