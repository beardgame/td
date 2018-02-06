﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Bearded.Utilities;
using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.MasterServer
{
    class MasterServer
    {
		private const string applicationName = "Bearded.TD.Master";
		private const int masterServerPort = 24293;
        private const long staleLobbyAgeSeconds = 30;
        private const long secondsBetweenLobbyPrunes = 5;

        private readonly Logger logger;
        private readonly NetPeer peer;

        private readonly Dictionary<long, Lobby> lobbiesById = new Dictionary<long, Lobby>();
        private long lastLobbyPrune;

        public MasterServer(Logger logger)
        {
            this.logger = logger;
            var config = new NetPeerConfiguration(applicationName)
			{
				Port = masterServerPort
			};
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            peer = new NetPeer(config);
			peer.Start();
        }

        public void Run()
        {
            while (true)
            {
                while (peer.ReadMessage(out var msg))
                {
                    handleIncomingMessage(msg);
                }

                if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastLobbyPrune >= secondsBetweenLobbyPrunes)
                {
                    pruneLobbies();
                }

                Thread.Sleep(100);
            }
        }

        private void handleIncomingMessage(NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.UnconnectedData:
					var request = Proto.MasterServerMessage.Parser.ParseFrom(
				        msg.ReadBytes(msg.LengthBytes));
                    handleIncomingRequest(request, msg.SenderEndPoint);
					break;

				case NetIncomingMessageType.VerboseDebugMessage:
                    logger.Trace.Log(msg.ReadString());
                    break;
				case NetIncomingMessageType.DebugMessage:
                    logger.Debug.Log(msg.ReadString());
                    break;
                case NetIncomingMessageType.WarningMessage:
					logger.Warning.Log(msg.ReadString());
					break;
				case NetIncomingMessageType.ErrorMessage:
					logger.Error.Log(msg.ReadString());
					break;
            }
        }

        private void handleIncomingRequest(Proto.MasterServerMessage request, IPEndPoint endpoint)
        {
            switch (request.RequestCase)
            {
                case Proto.MasterServerMessage.RequestOneofCase.None:
                    logger.Warning.Log("Received incoming message without request case.");
                    break;
                case Proto.MasterServerMessage.RequestOneofCase.RegisterLobby:
                    registerLobby(request.RegisterLobby, endpoint);
                    break;
                case Proto.MasterServerMessage.RequestOneofCase.ListLobbies:
                    listLobbies(request.ListLobbies, endpoint);
                    break;
                case Proto.MasterServerMessage.RequestOneofCase.IntroduceToLobby:
                    introduceToLobby(request.IntroduceToLobby, endpoint);
                    break;
            }
        }

        private void registerLobby(Proto.RegisterLobbyRequest request, IPEndPoint endpoint)
        {
            if (lobbiesById.ContainsKey(request.Lobby.Id))
            {
                logger.Debug.Log($"Received heartbeat for lobby {request.Lobby.Id}.");
                lobbiesById[request.Lobby.Id].Heartbeat();
                return;
            }

            logger.Debug.Log($"Registered new lobby {request.Lobby.Id}.");

            var lobby = new Lobby(
                request.Lobby,
                new IPEndPoint(new IPAddress(request.Address.ToByteArray()), request.Port), // internal
                endpoint // external
            );

			lobbiesById.Add(request.Lobby.Id, lobby);
        }

        private void listLobbies(Proto.ListLobbiesRequest request, IPEndPoint endpoint)
        {
            logger.Debug.Log("Received a request for lobby list.");
            foreach (var lobby in lobbiesById.Values)
            {
                logger.Trace.Log($"Sending lobby {lobby.LobbyProto.Id}");
                var msg = peer.CreateMessage();
                msg.Write(lobby.LobbyProto.ToByteArray());
                peer.SendUnconnectedMessage(msg, endpoint);
            }
        }

        private void introduceToLobby(Proto.IntroduceToLobbyRequest request, IPEndPoint endpoint)
        {
            if (lobbiesById.TryGetValue(request.LobbyId, out var lobby))
            {
                logger.Debug.Log($"Introducing endpoint with lobby {lobby.LobbyProto.Id}.");
                var clientInternal = new IPEndPoint(new IPAddress(request.Address.ToByteArray()), request.Port);
				peer.Introduce(
                    lobby.InternalEndPoint,
                    lobby.ExternalEndPoint,
                    clientInternal,
                    endpoint,
                    request.Token
				);
            }
            else
            {
                logger.Error.Log($"Peer requested to connect to lobby with unknown ID: {request.LobbyId}");
            }
        }

        private void pruneLobbies()
        {
            var lobbiesToDelete = lobbiesById.Values.Where(lobby => lobby.AgeInSeconds >= staleLobbyAgeSeconds).ToList();
            foreach (var lobby in lobbiesToDelete)
            {
                logger.Debug.Log($"Deleting lobby {lobby.LobbyProto.Id} due to staleness.");
                lobbiesById.Remove(lobby.LobbyProto.Id);
            }
            lastLobbyPrune = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
}
