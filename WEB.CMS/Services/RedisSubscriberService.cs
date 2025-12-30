using Entities.ViewModels.Car;
using Microsoft.AspNetCore.SignalR;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;

namespace WEB.CMS.Services
{
    public class RedisSubscriberService : BackgroundService
    {
        private readonly RedisConn _redisService;
        private readonly IHubContext<CarHub> _hubContext;
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IConfiguration _configuration;

        public RedisSubscriberService(RedisConn redisService, IHubContext<CarHub> hubContext, IVehicleInspectionRepository vehicleInspectionRepository, IConfiguration configuration)
        {
            _redisService = redisService;
            _hubContext = hubContext;
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _redisService.Connect();
            LogHelper.InsertLogTelegram("RedisSubscriberService - ExecuteAsync: đã kết nối" );
            await _redisService.SubscribeAsync("Add_ReceiveRegistration_LongAn", async (RegistrationRecord record) =>
            {

                record.CreateTime = record.RegistrationTime.ToString("HH:mm dd/MM/yyyy");
                await _hubContext.Clients.All.SendAsync("ReceiveRegistration", record);

            });
            await _redisService.Subscribed_Cam_Async("ListCartoFactory_Cam", async (CartoFactoryModel detail) =>
            {

                await _hubContext.Clients.All.SendAsync("ListCartoFactory_Da_SL", detail);

            });
            await Task.CompletedTask;
        }
    }
}
