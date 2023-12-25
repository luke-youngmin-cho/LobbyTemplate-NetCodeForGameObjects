using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyAPIInterface
{
    List<QueryFilter> _filters = new List<QueryFilter>
    {
        new QueryFilter(field : QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0")
    };
    List<QueryOrder> _orders = new List<QueryOrder>()
    {
        new QueryOrder(asc: false,
                       field: QueryOrder.FieldOptions.Created)
    };
    
    public async Task<Lobby> CreateLobbyAsync(string lobbyName, 
                                              int maxPlayer,
                                              bool isPrivate,
                                              Dictionary<string, DataObject> lobbyData,
                                              string hostID,
                                              Dictionary<string, PlayerDataObject> hostData)
    {
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Data = lobbyData,
            IsLocked = true,
            IsPrivate = isPrivate,
            Player = new Player(id: hostID, data: hostData),
        };

        return await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
    }

    public async Task DeleteLobbyAsync(string lobbyID)
    {
        await LobbyService.Instance.DeleteLobbyAsync(lobbyID);
    }

    public async Task<Lobby> JoinLobbyByIDAsync(string requesterID, string lobbyID, Dictionary<string, PlayerDataObject> localUserData)
    {
        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions { Player = new Player(id: requesterID, data: localUserData) };
        return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID, options);
    }

    public async Task<Lobby> JoinLobbyByCodeAsync(string requesterID, string lobbyCode, Dictionary<string, PlayerDataObject> localUserData)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions { Player = new Player(id: requesterID, data: localUserData) };
        return await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
    }

    public async Task<Lobby> QuickJoinAsync(string requesterID, Dictionary<string, PlayerDataObject> localUserData)
    {
        var request = new QuickJoinLobbyOptions
        {
            Filter = _filters,
            Player = new Player(id: requesterID, data: localUserData)
        };

        return await LobbyService.Instance.QuickJoinLobbyAsync(request);
    }

    public async Task<Lobby> ReconnectAsync(string lobbyID)
    {
        return await LobbyService.Instance.ReconnectToLobbyAsync(lobbyID);
    }

    public async Task RemovePlayerFromLobbyAsync(string requesterID, string lobbyID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyID, requesterID);
        }
        catch (LobbyServiceException e)
            when (e is { Reason : LobbyExceptionReason.PlayerNotFound})
        {
            // already left.
        }
    }

    public async Task<QueryResponse> QueryAllLobbies()
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions
        {
            Count = 16,
            Filters = _filters,
            Order = _orders
        };

        return await LobbyService.Instance.QueryLobbiesAsync(options);
    }

    public async Task<Lobby> UpdateLobby(string lobbyID, Dictionary<string, DataObject> lobbyData, bool shouldLock)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions {Data = lobbyData, IsLocked = shouldLock };
        return await LobbyService.Instance.UpdateLobbyAsync(lobbyID, options);
    }

    public async Task<Lobby> UpdatePlayer(string lobbyID, string playerID, Dictionary<string, PlayerDataObject> playerData, string allocationID, string connectionInfo)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = playerData,
            AllocationId = allocationID,
            ConnectionInfo = connectionInfo,
        };

        return await LobbyService.Instance.UpdatePlayerAsync(lobbyID, playerID, options);
    }

    public async Task<ILobbyEvents> SubscribeToLobby(string lobbyID, LobbyEventCallbacks callbacks)
    {
        return await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID, callbacks);
    }

    public async void SendHeartbeatPing(string lobbyID)
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
    }
}