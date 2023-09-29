using System.Diagnostics;
using Quik.Grpc.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Quik.Grpc;

public class GrpcServer
{
    private static readonly object _launcherLock = new();
    private static bool _isDisposed;
    private static bool _isLaunched;
    private static WebApplication? _host;

    public static void Launch()
    {
        lock (_launcherLock)
        {
            if (_isLaunched)
            {
                throw new InvalidOperationException("Grpc server is already running");
            }

            var builder = WebApplication.CreateBuilder();

            // Add services to the container.
            builder.Services.AddGrpc();

            _host = builder.Build();

            // Configure the HTTP request pipeline.
            _host.MapGrpcService<QuikService>();
            _host.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. " +
                                  "To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            _isLaunched = true;
        }

        _host.Run("http://localhost:23456");
    }

    public static async void Dispose()
    {
        lock (_launcherLock)
        {
            if (_isDisposed || _host == null)
            {
                return;
            }
            _isDisposed = true;
        }

        await _host.DisposeAsync();
        _host = null;
    }
}