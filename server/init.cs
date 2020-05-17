using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;

namespace MafiaApp {
    
    public class Startup {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {

            var webSocketOptions = new WebSocketOptions() {
                KeepAliveInterval = TimeSpan.FromSeconds(10),
            };
            app.useDeveloperExceptionPage();
            app.UseWebSockets(webSocketOptions);
            app.Use( (context, next) => {
                GetApiRequestProcessor(context.Request.path)(context, next);
            });
        }

        private delegate void APIprocessor(HttpContext context, RequestDelegate next);

        private async APIprocessor GetApiRequestProcessor(string path) {
            var mapping = new Dictionary<string, APIprocessor>() {
                {"/", ProcessGetReqeust },
                {"/ws", ProcessWebsocketRequest }
            };
            return mapping.TryGetValue(path, ProcessInvalidRequest);
        }

        private async void ProcessWebsocketRequest(HttpContext context, RequestDelegate next) {
            if (context.WebSockets.IsWebSocketRequest) {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await Echo(context, webSocket);
            } else {
                ProcessInvalidRequest(context, next);
            }
        }

        private async void ProcessInvalidRequest(HttpContext context, RequestDelegate next) {
            context.Response.StatusCode = 400;
        }

        private async Task Echo(HttpContext context, WebSocket webSocket) {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

    }
}