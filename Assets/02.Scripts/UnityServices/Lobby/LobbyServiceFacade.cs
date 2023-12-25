using System.Threading.Tasks;
using System;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using T.UnityServices;
using T.Infrastructure;
using System.Net.WebSockets;

public class LobbyServiceFacade
{
    public LobbyServiceFacade(LobbyAPIInterface lobbyAPIInterface)
    {
        _interface = lobbyAPIInterface;
        _rateLimitQuery = new RateLimitCooldown(1f);
        _rateLimitJoin = new RateLimitCooldown(3f);
        _rateLimitQuickJoin = new RateLimitCooldown(10f);
        _rateLimitHost = new RateLimitCooldown(3f);
        _updateRunner = new UpdateRunner();
    }


    RateLimitCooldown _rateLimitQuery;
    RateLimitCooldown _rateLimitJoin;
    RateLimitCooldown _rateLimitQuickJoin;
    RateLimitCooldown _rateLimitHost;
    UpdateRunner _updateRunner;
    float _heartBeatTime;
    const float HEART_BEAT_PERIOD = 8f;

    LobbyAPIInterface _interface;
    LocalLobbyUser _localUser;
    LocalLobby _localLobby;
    Lobby _remoteLobby;
    LobbyEventConnectionState _lobbyEventConnectionState;

    public event Action<Lobby> onLobbyCreated;
    public event Action<Lobby> onLobbyJoined;
    public event Action<ILobbyChanges> onLobbyChanged;
    public event Action onKickedFromLobby;
    public event Action<LobbyEventConnectionState> onLobbyEventConnectionStateChanged;
    ILobbyEvents _lobbyEvents;
    bool _isTracking;

    public void CreateLobby(string lobbyName, int maxPlayer, bool isPrivate)
    {
        if (_rateLimitHost.CanCall == false)
        {
            Debug.LogWarning($"[LobbyServiceFacade] : Creating lobby hit the rate limit.");
            return;
        }


        Task.Run(async () =>
        {
            try
            {
                _remoteLobby = await _interface.CreateLobbyAsync(lobbyName, maxPlayer, isPrivate, null,
                                        AuthenticationService.Instance.PlayerId,
                                        new Dictionary<string, PlayerDataObject>()
                                        {
                                            { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _localUser.displayName) }
                                        });
                onLobbyCreated?.Invoke(_remoteLobby);

            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        });
    }

    public void JoinLobby(string lobbyID, string lobbyCode = null)
    {
        if (_rateLimitJoin.CanCall == false ||
            (string.IsNullOrEmpty(lobbyID) && string.IsNullOrEmpty(lobbyCode)))
        {
            Debug.LogWarning($"[LobbyServiceFacade] : Joining lobby hit the rate limit.");
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                if (string.IsNullOrEmpty(lobbyCode) == false)
                {
                    _remoteLobby = await _interface.JoinLobbyByCodeAsync(AuthenticationService.Instance.PlayerId, lobbyCode, _localUser.GetDataForUnityServices());
                }
                else
                {
                    _remoteLobby = await _interface.JoinLobbyByIDAsync(AuthenticationService.Instance.PlayerId, lobbyCode, _localUser.GetDataForUnityServices());
                }
                onLobbyJoined?.Invoke(_remoteLobby);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    _rateLimitJoin.PutOnCooldown();
                }
                else
                {
                    PublishError(e);
                }
            }
        });
    }

    public void QuickJoinLobby()
    {
        if (_rateLimitQuickJoin.CanCall == false)
        {
            Debug.LogWarning($"[LobbyServiceFacade] : Quick Joining lobby hit the rate limit.");
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                _remoteLobby = await _interface.QuickJoinAsync(AuthenticationService.Instance.PlayerId, _localUser.GetDataForUnityServices());
                onLobbyJoined?.Invoke(_remoteLobby);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    _rateLimitQuickJoin.PutOnCooldown();
                }
                else
                {
                    PublishError(e);
                }
            }
        });
    }

    public void KickPlayer(string playerID)
    {
        Task.Run(async () =>
        {
            if (_localUser.isHost)
            {
                try
                {
                    await _interface.RemovePlayerFromLobbyAsync(playerID, _localLobby.id);
                }
                catch (LobbyServiceException e)
                {
                    PublishError(e);
                }
            }
            else
            {
                Debug.LogError("[LobbyService] : Only the host can kick other players.");
            }
        });
    }

    /// <summary>
    /// Start track current lobby events.
    /// </summary>
    public void BeginTracking()
    {
        if (_isTracking == false)
        {
            _isTracking = true;
            SubscribeToJoinedLobbyAsync();

            if (_localUser.isHost)
            {
                // todo -> Heartbeat fastest to keep lobby alive.
                _heartBeatTime = 0;
                _updateRunner.Subscribe(HeartBeat, 1.5f);
            }
        }
    }

    public void EndTracking()
    {
        if (_isTracking)
        {
            _isTracking = false;
            UnsubscribeToJoinedLobbyAsync();

            if (_localUser.isHost)
            {
                _updateRunner.Unsubscribe(HeartBeat);
            }

            if (_remoteLobby != null)
            {
                if (_localUser.isHost)
                {
                    DeleteLobbyAsync();
                }
                else
                {
                    LeaveLobbyAsync();
                }
            }
        }
    }

    private async void SubscribeToJoinedLobbyAsync()
    {
        var lobbyEventCallbacks = new LobbyEventCallbacks();
        lobbyEventCallbacks.LobbyChanged += onLobbyChanged;
        lobbyEventCallbacks.KickedFromLobby += onKickedFromLobby;
        lobbyEventCallbacks.LobbyEventConnectionStateChanged += onLobbyEventConnectionStateChanged;
        _lobbyEvents = await _interface.SubscribeToLobby(_localLobby.id, lobbyEventCallbacks);
    }

    private async void UnsubscribeToJoinedLobbyAsync()
    {
        if (_lobbyEvents != null && _lobbyEventConnectionState != LobbyEventConnectionState.Unsubscribed)
        {
#if UNITY_EDITOR
            try
            {
                await _lobbyEvents.UnsubscribeAsync();
            }
            catch (WebSocketException e)
            {
                Debug.Log(e.Message);
            }
#else
            await _lobbyEvents.UnsubscribeAsync();
#endif
        }
    }

    private async void DeleteLobbyAsync()
    {
        if (_localUser.isHost)
        {
            try
            {
                await _interface.DeleteLobbyAsync(_localLobby.id);
            }
            catch (LobbyServiceException e)
            {
                PublishError(e);
            }
            finally
            {
                ResetLobby();
            }
        }
        else
        {
            Debug.LogError("[LobbyService] : Only the host can delete a lobby.");
        }
    }

    private async void LeaveLobbyAsync()
    {
        try
        {
            await _interface.RemovePlayerFromLobbyAsync(AuthenticationService.Instance.PlayerId, _localLobby.id);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason != LobbyExceptionReason.LobbyNotFound && _localUser.isHost == false)
            {
                PublishError(e);
            }
        }
        finally
        {
            ResetLobby();
        }
    }

    private void ResetLobby()
    {
        _remoteLobby = null;

        if (_localUser != null)
        {
            _localUser.ResetState();
        }

        if (_localLobby != null)
        {
            _localLobby.Reset(_localUser);
        }
    }

    private void HeartBeat(float deltaTime)
    {
        _heartBeatTime += deltaTime;
        if (_heartBeatTime > HEART_BEAT_PERIOD)
        {
            _heartBeatTime -= HEART_BEAT_PERIOD;
            try
            {
                _interface.SendHeartbeatPing(_remoteLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && _localUser.isHost == false)
                {
                    PublishError(e);
                }
            }
        }
    }

    public void PublishError(LobbyServiceException e)
    {

    }
}
