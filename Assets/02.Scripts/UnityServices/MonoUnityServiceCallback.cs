using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace T.UnityServices
{
    public class MonoUnityServiceCallback : MonoBehaviour, ILobbyCallback
    {
        LobbyServiceFacade _lobbyServiceFacade;
        Lobby _currentLobby;
        protected virtual void Awake()
        {
            _lobbyServiceFacade = ServiceManager.instance.container.Resolve<LobbyServiceFacade>();
            _lobbyServiceFacade.onLobbyCreated += OnLobbyCreated;
        }

        public virtual void OnLobbyCreated(Lobby lobby)
        {
            Debug.Log($"Created Lobby {lobby.Id}.");
        }

        public virtual void OnJoinedToLobby(Lobby lobby)
        {
            if (lobby != null)
            {
                _currentLobby = lobby;
                _lobbyServiceFacade.onKickedFromLobby += OnKickedFromLobby;
            }
        }

        public virtual void OnLeaveFromLobby()
        {
            if (_currentLobby != null)
            {
                _lobbyServiceFacade.onKickedFromLobby -= OnKickedFromLobby;
                _currentLobby = null;
            }
        }

        public virtual void OnKickedFromLobby()
        {
        }
    }
}
