using Azure.Core;
using B2B.Utilities.Common;
using DAL;
using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Repositories.IRepositories;
using Repositories.Repositories;
using Telegram.Bot.Requests.Abstractions;
using Utilities.Contants;
using Web.Cargill.Api.Services;

namespace Web.Cargill.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleInspectionController : ControllerBase
    {
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly RedisConn redisService;
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient _workQueueClient;
        private readonly IAllCodeRepository _allCodeRepository;
        public VehicleInspectionController(IVehicleInspectionRepository vehicleInspectionRepository, IConfiguration configuration, IAllCodeRepository allCodeRepository)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            redisService = new RedisConn(configuration);
            redisService.Connect();
            _configuration = configuration;
            _workQueueClient = new WorkQueueClient(configuration);
            _allCodeRepository = allCodeRepository;

        }
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] RegistrationRecord request)
        {

            try
            {
                var audio = await _vehicleInspectionRepository.GetAudioPathByVehicleNumber(request.PlateNumber);
                if (!string.IsNullOrEmpty(audio))
                {
                    request.AudioPath = audio;
                    LogHelper.InsertLogTelegram("sql:" + request.PlateNumber);
                }
                var id = _vehicleInspectionRepository.SaveVehicleInspection(request);
                if (id > 0 && (request.AudioPath == null || request.AudioPath == ""))
                {
                    request.Id = id;
                    request.Bookingid = id;
                    request.text_voice = "Mời biển số xe " + request.PlateNumber + " vào cân";
                    var TIME_RESET = await _allCodeRepository.GetListSortByName(AllCodeType.TIME_RESET);
                    var now = DateTime.Now;
                    var expireAt = new DateTime(now.Year, now.Month, now.Day, ((DateTime)TIME_RESET[0].UpdateTime).Hour, ((DateTime)TIME_RESET[0].UpdateTime).Minute, 0);
                    if (now < expireAt)
                    {
                        await redisService.PublishAsync("Add_ReceiveRegistration" + _configuration["CompanyType"], request);
                    }
                    
                    
                    LogHelper.InsertLogTelegram("Queue :" + request.PlateNumber);
                    var Queue = _workQueueClient.SyncQueue(request);
                    if (!Queue)
                    {
                        Queue = _workQueueClient.SyncQueue(request);
                    }
               
                    //string url_n8n = "https://n8n.adavigo.com/webhook/text-to-speed";
                    //await redisService.PublishAsync("Add_ReceiveRegistration", request);
                    //var client = new HttpClient();
                    //var request_n8n = new HttpRequestMessage(HttpMethod.Post, url_n8n);
                    //request_n8n.Content = new StringContent(JsonConvert.SerializeObject(request), null, "application/json");
                    //var response = await client.SendAsync(request_n8n);
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    var responseContent = await response.Content.ReadAsStringAsync();

                    //}
                    //else
                    //{
                    //    LogHelper.InsertLogTelegram("Insert - VehicleInspectionController API: Gửi n8n thất bại:  Id" + id);
                    //}
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        message = "Upload audio thành công",
                        data = id
                    });
                }
                if (id > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        message = "Thêm mới thành công",
                        data = id
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        message = "Thêm mới lỗi",

                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - VehicleInspectionController API: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    message = "đã xẩy ra lỗi vui lòng liên hệ IT",

                });
            }
        }
        [HttpGet("get-time-countdown")]
        public async Task<IActionResult> GetTimeCountdown()
        {
            try
            {
                var TIME_RESET = await _allCodeRepository.GetListSortByName(AllCodeType.TIME_RESET);
                if (TIME_RESET == null || TIME_RESET.Count == 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        message = "Chưa cấu hình thời gian đặt lại",
                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    message = "Upload audio thành công",
                    data = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : ""
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTimeCountdown - VehicleInspectionController API: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    message = "đã xẩy ra lỗi vui lòng liên hệ IT",
                });
            }
        }
        [HttpGet("update-by-plateNumber")]
        public async Task<IActionResult> UpdateByPlateNumber([FromBody] CamModel request)
        {
            try
            {
                var update = await _vehicleInspectionRepository.UpdateVehicleInspectionByVehicleNumber(request.bien_so);
                if ( update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        message = "Cập nhật thành công",
                    });

                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        message = "Cập nhật thất bại",
                    });
                }

         
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateByPlateNumber - VehicleInspectionController API: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    message = "đã xẩy ra lỗi vui lòng liên hệ IT",
                });
            }
        }
    }
}
