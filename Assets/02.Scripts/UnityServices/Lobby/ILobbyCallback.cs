using Unity.Services.Lobbies.Models;

namespace T.UnityServices
{
    public interface ILobbyCallback
    {
        public void OnLobbyCreated(Lobby lobby);

        public void OnJoinedToLobby(Lobby lobby);
    }
}
