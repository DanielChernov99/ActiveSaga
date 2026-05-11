using System.Threading.Tasks;

namespace ActiveSaga.Common.Networking
{
    public interface IGameResultSubmitter
    {
        Task<ServerGameResultResponse> SubmitGameResultAsync(string jsonPayload);
    }
}