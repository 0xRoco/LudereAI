namespace LudereAI.WPF.Interfaces;

public interface IConnectivityService
{
    event EventHandler<bool> OnConnectivityChanged; 
    Task StartConnectivityCheck();
    Task CheckConnectivity();
}