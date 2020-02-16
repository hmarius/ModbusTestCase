using System.Threading.Tasks;
using AMWD.Modbus.Tcp.Client;
using AMWD.Modbus.Tcp.Server;
using Microsoft.Extensions.Logging;

namespace ClientServerProblem
{
    class Program
    {
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory()
#pragma warning disable 618
            .AddConsole();
#pragma warning restore 618

        public static async Task Main()
        {
            var taskMbClient = Task.Run(async () =>
            {
                var logger = LoggerFactory.CreateLogger("client");
                using (var mbClient = new ModbusClient("localhost", 502, logger))
                {
                    await mbClient.Connect();
                    await mbClient.ReadHoldingRegisters(1, 0, 1);
                }
                logger.LogInformation("Client disconnected");
            });


            var taskMbServer = await Task.Run(async () =>
            {
                var logger = LoggerFactory.CreateLogger("server");
                using (var mbServer = new ModbusServer(502, logger))
                {
                    mbServer.AddDevice(1);
                    await mbServer.Initialization;
                    logger.LogInformation("Server started");
                    while (true)
                    {
                        var register = mbServer.GetHoldingRegister(1, 0);
                        if (register.Value == 99)
                            break;
                        await Task.Delay(5000);
                    }
                    logger.LogInformation("Server stopped");
                }
                return Task.CompletedTask;
            });
            Task.WaitAll(taskMbClient, taskMbServer);
        }
    }
}