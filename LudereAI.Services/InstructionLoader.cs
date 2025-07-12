using LudereAI.Core.Interfaces;

namespace LudereAI.Services;

public class InstructionLoader : IInstructionLoader 
{
    private const string InstructionsFile = "instructions.txt";

    public async Task<string> LoadInstructions(string gameContext)
    {
        var game = string.IsNullOrWhiteSpace(gameContext) ? "Unknown" : gameContext;
        var system = await File.ReadAllTextAsync(InstructionsFile);
        return system.Replace("{GAME_NAME}", game);
    }
}