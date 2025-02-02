namespace LudereAI.Application.Interfaces;

public interface IInstructionLoader
{
    Task<string> LoadInstructions(string gameContext);
}