namespace LudereAI.Core.Interfaces;

public interface IInstructionLoader
{
    Task<string> LoadInstructions(string gameContext);
}