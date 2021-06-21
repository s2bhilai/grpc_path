using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService.Server.Services
{
    public class ChatService: Chat.ChatBase
    {
        private ILogger<ChatService> _logger;

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
        }

        public async override Task SendMessage(
            IAsyncStreamReader<ClientToServerMessage> requestStream, 
            IServerStreamWriter<ServerToClientMessage> responseStream, 
            ServerCallContext context)
        {
            var clientToServerTask =  ClientToServerPingHandlingAsync(requestStream, context);
            var serverToClientTask =  ServerToClientPingAsync(responseStream, context);

            await Task.WhenAll(clientToServerTask, serverToClientTask);
        }

        private static async Task ServerToClientPingAsync(
            IServerStreamWriter<ServerToClientMessage> responseStream, 
            ServerCallContext context)
        {
            var pingCount = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new ServerToClientMessage
                {
                    Text = $"Server said hi {++pingCount} times",
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                });
                await Task.Delay(1000);
            }
        }

        private async Task ClientToServerPingHandlingAsync(
            IAsyncStreamReader<ClientToServerMessage> requestStream, 
            ServerCallContext context)
        {
            while (await requestStream.MoveNext() &&
                !context.CancellationToken.IsCancellationRequested)
            {
                var message = requestStream.Current;
                _logger.LogInformation("The Client said: {Message}", message.Text);
            }
        }
    }
}
