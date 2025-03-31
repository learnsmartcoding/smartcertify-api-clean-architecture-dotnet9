namespace LSC.SmartCertify.Application.Interfaces.Graph
{
    public interface IGraphAuthService
    {
        Task<string> GetAccessTokenAsync();
    }
}
