using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using System;

using Microsoft.EntityFrameworkCore;
using B2B.Utilities.Common;
using Web.Cargill.Api.Model;
using NAudio.Wave;
using NAudio.Lame;                 // NuGet: NAudio


namespace Web.Cargill.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleAudioController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public VehicleAudioController(AppDbContext db, IWebHostEnvironment env, IConfiguration config)
        {
            _db = db;
            _env = env;
            _config = config;
        }

        [HttpPost("upload-audio")]
        public async Task<IActionResult> UploadAudio([FromForm] int booking_id, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                LogHelper.InsertLogTelegram($"UploadAudio ❌ File rỗng hoặc không hợp lệ (booking_id={booking_id})");
                return BadRequest(new { Status = 1, Msg = "File không hợp lệ." });
            }

            try
            {
                var booking = _db.VehicleInspection.FirstOrDefault(b => b.Id == booking_id);
                if (booking == null)
                {
                    LogHelper.InsertLogTelegram($"UploadAudio ❌ Không tìm thấy booking_id={booking_id}");
                    return NotFound(new { Status = 1, Msg = $"Không tìm thấy booking_id = {booking_id}" });
                }

                // Lấy biển số xe, loại bỏ ký tự đặc biệt, khoảng trắng
                string vehicleNumber = booking.VehicleNumber ?? "unknown";
                vehicleNumber = new string(vehicleNumber
                    .Where(char.IsLetterOrDigit)
                    .ToArray())
                    .ToLower();

                int recordNumber = booking.RecordNumber ?? 1;
                string customFileName = $"audio{booking_id}_{vehicleNumber}_{recordNumber}";

                LogHelper.InsertLogTelegram($"UploadAudio ▶️ Bắt đầu upload: booking_id={booking_id}, file={file.FileName}, customFileName={customFileName}");

                // Upload file
                string audioUrl = await UpLoadHelper.UploadFileOrImage(file, booking_id, 999, customFileName);

                if (string.IsNullOrEmpty(audioUrl))
                {
                    LogHelper.InsertLogTelegram($"UploadAudio ❌ Upload thất bại từ UpLoadHelper (booking_id={booking_id}, file={file.FileName})");
                    return StatusCode(500, new { Status = 1, Msg = "Upload thất bại từ UpLoadHelper" });
                }

                // Cập nhật AudioPath
                booking.AudioPath = audioUrl;

              

                // Thêm record VehicleAudio
                var newAudio = new VehicleAudio
                {
                    PlateNumber = booking.VehicleNumber,
                    AudioPath = audioUrl,
                    CreatedAt = DateTime.Now
                };

                _db.VehicleAudio.Add(newAudio);
                await _db.SaveChangesAsync();

                LogHelper.InsertLogTelegram($"UploadAudio ✅ Lưu DB thành công:  Plate={booking.VehicleNumber} ,url={audioUrl}");

                return Ok(new { Status = 0, Url = audioUrl });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"UploadAudio ❌ Exception (booking_id={booking_id}): {ex}");
                return StatusCode(500, new { Status = 1, Msg = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách booking chưa có file Audio (AudioPath == null hoặc rỗng)
        /// </summary>
        [HttpGet("GetListNoAudio")]
        public async Task<IActionResult> GetListNoAudio()
        {
            try
            {
                // Lấy thời điểm hiện tại
                var now = DateTime.Now;

                // Mốc 17:55 hôm nay
                var todayCutoff = new DateTime(now.Year, now.Month, now.Day, 17, 55, 0);

                DateTime fromDate, toDate;

                // Nếu bây giờ sau 17:55 → lấy từ 17:55 hôm nay-1 đến 17:55 hôm nay
                // Nếu bây giờ trước 17:55 → lấy từ 17:55 hôm kia đến 17:55 hôm nay-1
                if (now >= todayCutoff)
                {
                    fromDate = todayCutoff.AddDays(-1); // 17:55 hôm qua
                    toDate = todayCutoff;                // 17:55 hôm nay
                }
                else
                {
                    fromDate = todayCutoff.AddDays(-2);  // 17:55 hôm kia
                    toDate = todayCutoff.AddDays(-1);    // 17:55 hôm qua
                }

                var list = await _db.VehicleInspection
                    .Where(v =>
                        string.IsNullOrEmpty(v.AudioPath) &&
                        v.RegisterDateOnline >= fromDate &&
                        v.RegisterDateOnline < toDate)
                    .OrderByDescending(v => v.RegisterDateOnline)
                    .Select(v => new
                    {
                        v.Id,
                        v.RecordNumber,
                        v.CustomerName,
                        v.VehicleNumber,
                        v.RegisterDateOnline,
                        v.DriverName,
                        v.PhoneNumber,
                        v.VehicleStatus,
                        v.CreatedDate,
                        v.UpdatedDate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Status = 0,
                    From = fromDate.ToString("yyyy-MM-dd HH:mm"),
                    To = toDate.ToString("yyyy-MM-dd HH:mm"),
                    Count = list.Count,
                    Data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = 1,
                    Msg = "Lỗi hệ thống: " + ex.Message
                });
            }
        }




        /// <summary>
        /// Fallback: nhận link Zalo (.wav) -> download -> convert MP3 (in-memory) -> upload -> update DB
        /// form-data: booking_id (Text), link_audio_zalo_ai (Text)
        /// </summary>
        [HttpPost("download-zalo-audio")]
        public async Task<IActionResult> DownloadZaloAudio(
     [FromForm] int booking_id,
     [FromForm] string link_audio_zalo_ai,
     CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(link_audio_zalo_ai))
            {
                LogHelper.InsertLogTelegram($"DownloadZaloAudio ❌ Link audio trống hoặc không hợp lệ (booking_id={booking_id})");
                return BadRequest(new { Status = 1, Msg = "Link audio không hợp lệ." });
            }

            try
            {
               

                // 1) Download WAV về MemoryStream
                var wavStream = await DownloadWavToStreamAsync(link_audio_zalo_ai, maxBytes: 20 * 1024 * 1024, ct);
                if (wavStream == null || wavStream.Length == 0)
                {
                    LogHelper.InsertLogTelegram($"DownloadZaloAudio ❌ Không tải được WAV từ link: {link_audio_zalo_ai}");
                    return StatusCode(502, new { Status = 1, Msg = "Không tải được file từ Zalo AI" });
                }

                // 2) Convert WAV -> MP3 (in-memory)
                LogHelper.InsertLogTelegram($"DownloadZaloAudio ▶️ Bắt đầu convert WAV → MP3 (booking_id={booking_id})");
                var mp3Stream = ConvertWavStreamToMp3Stream(wavStream);

                // 3) Lấy thông tin booking để đặt tên file
                var booking = await _db.VehicleInspection.FirstOrDefaultAsync(b => b.Id == booking_id, ct);
                if (booking == null)
                {
                    LogHelper.InsertLogTelegram($"DownloadZaloAudio ❌ Không tìm thấy booking_id={booking_id}");
                    return NotFound(new { Status = 1, Msg = $"Không tìm thấy booking_id = {booking_id}" });
                }

                string vehicleNumber = booking.VehicleNumber ?? "unknown";
                vehicleNumber = new string(vehicleNumber.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                int recordNumber = booking.RecordNumber ?? 1;
                string customFileName = $"audio{booking_id}_{vehicleNumber}_{recordNumber}";

                // 4) Upload file lên server
                mp3Stream.Position = 0;
                var formFile = new FormFile(mp3Stream, 0, mp3Stream.Length, "file", $"{customFileName}.mp3")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "audio/mpeg"
                };

               

                string? audioUrl = await UpLoadHelper.UploadFileOrImage(formFile, booking_id, 999, customFileName);
                if (string.IsNullOrEmpty(audioUrl))
                {
                    LogHelper.InsertLogTelegram($"DownloadZaloAudio ❌ Upload thất bại từ UpLoadHelper (booking_id={booking_id})");
                    return StatusCode(500, new { Status = 1, Msg = "Upload thất bại sau khi convert" });
                }

                // 5) Update DB VehicleInspection
                booking.AudioPath = audioUrl;

                // ✅ 6) Thêm record mới vào VehicleAudio
                var newAudio = new VehicleAudio
                {
                    PlateNumber = booking.VehicleNumber,
                    AudioPath = audioUrl,
                    CreatedAt = DateTime.Now
                };
                _db.VehicleAudio.Add(newAudio);

                await _db.SaveChangesAsync(ct);

                LogHelper.InsertLogTelegram($"DownloadZaloAudio ✅ Hoàn tất: booking_id={booking_id}, Plate={booking.VehicleNumber}, Url={audioUrl}");

                return Ok(new { Status = 0, Url = audioUrl });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"DownloadZaloAudio ❌ Exception (booking_id={booking_id}): {ex}");
                return StatusCode(500, new { Status = 1, Msg = ex.Message });
            }
        }



        // ===== Helpers =====

        private async Task<MemoryStream> DownloadWavToStreamAsync(string url, long maxBytes, CancellationToken ct)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            // GET file
            using var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Tải WAV thất bại: HTTP {(int)resp.StatusCode}");

            var contentLength = resp.Content.Headers.ContentLength; // có thể null
            if (contentLength.HasValue && contentLength.Value > maxBytes)
                throw new Exception($"File quá lớn (> {maxBytes / (1024 * 1024)}MB)");

            // tạo stream với capacity hợp lý
            var capacity = contentLength.HasValue ? (int)Math.Min(contentLength.Value, maxBytes) : 0;
            var ms = capacity > 0 ? new MemoryStream(capacity) : new MemoryStream();

            await using (var s = await resp.Content.ReadAsStreamAsync(ct))
            {
                await s.CopyToAsync(ms, 81920, ct);
            }
            ms.Position = 0;
            return ms;
        }

        private MemoryStream ConvertWavStreamToMp3Stream(Stream wavStream)
        {
            wavStream.Position = 0;
            using var reader = new WaveFileReader(wavStream);

            WaveStream pcmStream = reader;
            if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm &&
                reader.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
            }

            var mp3 = new MemoryStream();
            using (var lame = new LameMP3FileWriter(mp3, pcmStream.WaveFormat, LAMEPreset.VBR_90))
            {
                pcmStream.CopyTo(lame);
            }

            mp3.Position = 0;
            if (pcmStream != reader) pcmStream.Dispose();
            return mp3;
        }



    }
}
