using System.Net;
using System.Net.Sockets;

using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => builder.Configuration.Bind("JwtSettings", options));

var app = builder.Build();

app.MapPost("/tcp/{ip}/{port}", async (IPAddress ip, int port, CancellationToken cancel) => {
    using var tcpClient = new TcpClient();
    try {
        await tcpClient.ConnectAsync(ip, port, cancel).ConfigureAwait(false);
        return Results.Ok();
    } catch (ArgumentException e) {
        return Results.BadRequest(e.Message);
    } catch (SocketException e) {
        return e.SocketErrorCode switch {
            SocketError.ConnectionRefused => Results.Forbid(),
            SocketError.TimedOut => Results.StatusCode(408), // request timeout
            _ => Results.Problem(e.Message)
        };
    }
});

app.Run();
