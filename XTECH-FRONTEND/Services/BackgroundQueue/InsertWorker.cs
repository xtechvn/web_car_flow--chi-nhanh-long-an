using System.Text;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services.BackgroundQueue
{
    public class InsertWorker : BackgroundService
    {
        private readonly IInsertQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _factory;

        public InsertWorker(
            IInsertQueue queue,
            IServiceProvider serviceProvider,
            IHttpClientFactory factory)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await _queue.DequeueAsync(stoppingToken);

                // chạy async tách riêng -> không block worker
                _ = Task.Run(() => ProcessJob(job, stoppingToken), stoppingToken);
            }
        }

        private async Task ProcessJob(InsertJob job, CancellationToken token)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<InsertWorker>>();

                var client = _factory.CreateClient("InsertClient");

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(job.Data);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://qc-api.cargillhanam.com/api/vehicleInspection/insert",
                    content,
                    token
                );

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("InsertWorker: Failed to call API");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertWorker error: " + ex.Message);
            }
        }
    }

}
