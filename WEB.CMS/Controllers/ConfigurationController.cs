using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ConfigurationController : Controller
    {
        private readonly IAllCodeRepository _allCodeRepository;

        private readonly IConfiguration _configuration;
        public ConfigurationController( IConfiguration configuration, IAllCodeRepository allCodeRepository)
        {
            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
        }
        public async Task<IActionResult> Index()
        {
            var TIME_RESET = await _allCodeRepository.GetListSortByName(AllCodeType.TIME_RESET);
            ViewBag.TIME_RESET = TIME_RESET;
            return View();
        }
        public async Task<IActionResult> Setup(string time,int id)
        {
            
            try
            {
                var date_time = DateUtil.StringToDateTime( DateTime.Now.ToString("dd/MM/yyyy")+" "+ time+":00");
                var update = await _allCodeRepository.UpdateResetTime(id, date_time);

                if (update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Cập nhật thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Setup - ConfigurationController: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Cập nhật thất bại!"
                });
            }
        }
    }
}
