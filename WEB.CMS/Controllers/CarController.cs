using Entities.Models;
using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Services;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class CarController : Controller
    {
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IHubContext<CarHub> _hubContext;
        public CarController(IVehicleInspectionRepository vehicleInspectionRepository, IAllCodeRepository allCodeRepository, IHubContext<CarHub> hubContext)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _allCodeRepository = allCodeRepository;
            _hubContext = hubContext;
        }
        public IActionResult CartoFactory()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CartoFactory - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                return PartialView(detail);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProcessingIsLoading - CarController: " + ex);
            }
            return View();
        }

        public async Task<IActionResult> ListCartoFactory(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;//1 đã SL
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                var LoadingType = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                ViewBag.AllCode = AllCode;
                ViewBag.LoadingType = LoadingType;
                var data = await _vehicleInspectionRepository.GetListCartoFactory(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCartoFactory - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> OpenPopup(int id, int type)
        {
            try
            {
                ViewBag.Id = id;
                ViewBag.StatusCar = 0;
                var data = new List<AllCode>();
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                switch (type)
                {
                    case 1:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                        ViewBag.StatusCar = detail.VehicleStatus;
                        break;
                    case 2:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                        ViewBag.StatusCar = detail.LoadType;
                        break;
                    case 3:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHING_TYPE);
                        ViewBag.StatusCar = detail.VehicleWeighingType;
                        break;
                    case 4:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.TROUGH_TYPE);
                        ViewBag.StatusCar = detail.TroughType;
                        break;
                    case 5:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUG_HWEIGHT);
                        ViewBag.StatusCar = detail.VehicleTroughWeight;
                        break;
                    case 6:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                        ViewBag.StatusCar = detail.VehicleTroughStatus;
                        break;
                    case 7:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHINGSTATUS);
                        ViewBag.StatusCar = detail.VehicleWeighingStatus;
                        break;
                }
                ViewBag.Status = data;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OpenPopup - CarController: " + ex);
            }
            return PartialView();
        }
        public IActionResult ProcessingIsLoading()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProcessingIsLoading - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListProcessingIsLoading(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.LOADINGSTATUS);
                ViewBag.AllCode = AllCode;
                var AllCode2 = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                ViewBag.AllCode2 = AllCode2;
                var data = await _vehicleInspectionRepository.GetListVehicleProcessingIsLoading(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListProcessingIsLoading - CarController: " + ex);
            }
            return PartialView();
        }
        public IActionResult CallTheScale()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CallTheScale - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListCallTheScale(CartoFactorySearchModel SearchModel)
        {
            try
            {
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHING_TYPE);
                ViewBag.AllCode = AllCode;
                ViewBag.type = SearchModel.type;
                ViewBag.LoadType = SearchModel.LoadType == null ? "" : SearchModel.LoadType;
                var data = await _vehicleInspectionRepository.GetListVehicleCallTheScale(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCallTheScale - CarController: " + ex);
            }
            return PartialView();
        }
        public IActionResult WeighedInput()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListWeighedInput(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHEDSTATUS);
                ViewBag.AllCode = AllCode;
                var data = await _vehicleInspectionRepository.GetListVehicleWeighedInput(SearchModel);
                if (data != null)
                {
                    data = data.OrderBy(s => s.LoadType).ToList();
                }
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListWeighedInput - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> UpdateStatus(int id, int status, int type, int weight = 0 , string Note = null)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var UpdateCar = 0;
                ViewBag.Id = id;
                ViewBag.StatusCar = 0;
                var model = new VehicleInspectionUpdateModel();
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                model.Id = detail.Id;
                model.RecordNumber = detail.RecordNumber;
                model.CustomerName = detail.CustomerName;
                model.VehicleNumber = detail.VehicleNumber;
                model.RegisterDateOnline = detail.RegisterDateOnline;
                model.DriverName = detail.DriverName;
                model.LicenseNumber = detail.LicenseNumber;
                model.PhoneNumber = detail.PhoneNumber;
                model.VehicleLoad = detail.VehicleLoad;
                model.VehicleStatus = detail.VehicleStatus;
                model.LoadType = detail.LoadType;
                model.IssueCreateDate = detail.IssueCreateDate;
                model.IssueUpdatedDate = detail.IssueUpdatedDate;
                model.VehicleWeighingType = detail.VehicleWeighingType;
                model.VehicleWeighingTimeComeIn = detail.VehicleWeighingTimeComeIn;
                model.VehicleWeighingTimeComeOut = detail.VehicleWeighingTimeComeOut;
                model.VehicleWeighingTimeComplete = detail.VehicleWeighingTimeComplete;
                model.TroughType = detail.TroughType;
                model.VehicleTroughTimeComeIn = detail.VehicleTroughTimeComeIn;
                model.VehicleTroughTimeComeOut = detail.VehicleTroughTimeComeOut;
                model.VehicleTroughWeight = detail.VehicleTroughWeight;
                model.VehicleTroughStatus = detail.VehicleTroughStatus;
                model.LoadingStatus = detail.LoadingStatus;
                model.VehicleWeighedstatus = detail.VehicleWeighedstatus;
                model.TimeCallVehicleTroughTimeComeIn = detail.TimeCallVehicleTroughTimeComeIn;
                model.LoadingType = detail.LoadingType;
              
                model.CreatedBy = _UserId;
                switch (type)
                {
                    case 1:
                        {
                            if (detail.LoadingStatus == (int)LoadingStatus.Da_HTTC)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe Đã hoàn thành thử tục"
                                });
                            }
                            if (detail.VehicleStatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.VehicleStatus = status;
                            model.VehicleArrivalDate = DateTime.Now;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleStatus);
                                detail.VehicleStatusName = allcode_detail.Description;
                                if (status == (int)VehicleStatus.Da_Den_NM)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCartoFactory_Da_SL", detail);
                                }
                                else
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCartoFactory", detail);
                                }
                            }
                        }
                        break;
                    case 2:
                        {
                            model.LoadType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                            var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == status);
                            detail.LoadTypeName = allcode_detail.Description;
                            await _hubContext.Clients.All.SendAsync("ListProcessingIsLoading", detail);
                            
                        }
                        break;
                    case 3:
                        {
                            if (detail.VehicleWeighedstatus == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe Đã cân xong"
                                });
                            }
                            if (status == (int)VehicleWeighingType.DA_Vao_Can)
                            {
                                model.VehicleWeighingTimeComeIn = DateTime.Now;
                            }
                            if (detail.VehicleWeighingType == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.VehicleWeighingType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHING_TYPE);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleWeighingType);
                                detail.VehicleWeighingTypeName = allcode_detail.Description;
                                if (status == (int)VehicleWeighingType.DA_Vao_Can)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCallTheScale_Da_SL", detail);
                                }
                                else
                                {
                                    if (detail.LoadType == (int)LoadType.Xanh)
                                    {
                                        await _hubContext.Clients.All.SendAsync("ListCallTheScale_0", detail);
                                    }
                                    else
                                    {
                                        await _hubContext.Clients.All.SendAsync("ListCallTheScale_1", detail);

                                    }

                                }
                            }



                        }
                        break;
                    case 4:
                        {
                            if(model.VehicleTroughStatus == null || model.VehicleTroughStatus == (int)VehicleTroughStatus.Blank)
                            {
                                model.VehicleTroughStatus = (int)VehicleTroughStatus.Da_goi;
                            }
                            
                            model.TimeCallVehicleTroughTimeComeIn = DateTime.Now;
                            model.TroughType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                var allcode_VehicleTrough = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                                var allcode_detail_VehicleTrough = allcode_VehicleTrough.FirstOrDefault(s => s.CodeValue == model.VehicleTroughStatus);
                                detail.VehicleTroughStatusName = allcode_detail_VehicleTrough.Description;
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.TROUGH_TYPE);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.TroughType);
                                detail.TroughTypeName = allcode_detail?.Description ?? "";
                                // ✅ bắn cả máng cũ + máng mới
                                await _hubContext.Clients.All.SendAsync("UpdateMangStatus", detail.TroughType, model.TroughType, detail.Id);
                                if (model.VehicleTroughStatus == null || detail.VehicleTroughStatus == (int)VehicleTroughStatus.Blank)
                                    await _hubContext.Clients.All.SendAsync("ListCarCall", detail);
                                LogHelper.InsertLogTelegram("Xin mời xe biển số " + detail.VehicleNumber + " của tài xế " + detail.DriverName + " di chuyển vào máng số " + status + ". Trân trọng!");

                            }

                        }
                        break;
                    case 5:
                        {

                            model.VehicleTroughWeight = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            await _hubContext.Clients.All.SendAsync("ListCartoFactory", detail);
                        }
                        break;
                    case 6:
                        {
                            if (detail.VehicleTroughStatus == status && detail.VehicleTroughWeight == weight)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            if (status == (int)VehicleTroughStatus.Boc_Hang)
                                model.VehicleTroughTimeComeIn = DateTime.Now; 
                            if (status == (int)VehicleTroughStatus.Hoan_thanh)
                                model.VehicleTroughTimeComeOut = DateTime.Now;
                            model.VehicleTroughStatus = status;
                            model.VehicleTroughWeight = weight; // ✅ lấy từ input
                            model.Note = Note; 
                            if ((model.VehicleTroughWeight == null || model.VehicleTroughWeight == 0)&& status == (int)VehicleTroughStatus.Hoan_thanh)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Chưa nhập trọng lượng "
                                });
                            }
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                detail.VehicleTroughWeight = weight;
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleTroughStatus);
                                detail.VehicleTroughStatusName = allcode_detail.Description;
                                if (status == (int)VehicleTroughStatus.Bo_Luot)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCarCall_Bo_LUOT", detail);
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.SUCCESS,
                                        msg = "cập nhật thành công"
                                    });
                                }
                                if (status == (int)VehicleTroughStatus.Hoan_thanh)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCarCall_Da_SL", detail);
                                }
                                else 
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCarCall", detail);
                                }
                            }
                        }
                        break;
                    case 7:
                        {
                            if (detail.VehicleWeighingStatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            if (status == (int)VehicleWeighingStatus.DA_Can_Ra)
                            {
                                model.VehicleWeighingTimeComplete = DateTime.Now;
                                detail.VehicleWeighingTimeComplete = DateTime.Now;
                            }
                            model.VehicleWeighingStatus = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHINGSTATUS);
                            var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleWeighingStatus);
                            detail.VehicleWeighingStatusName = allcode_detail.Description;
                            if (status == (int)VehicleWeighingStatus.DA_Can_Ra)
                            {
                                await _hubContext.Clients.All.SendAsync("ListVehicles_Da_SL", detail);
                            }
                            else
                            {
                                await _hubContext.Clients.All.SendAsync("ListVehicles", detail);
                            }


                        }
                        break;
                    case 8:
                        {
                            if (model.VehicleWeighingType == (int)VehicleWeighingType.DA_Vao_Can)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe Đã vào cân"
                                });
                            }
                            if (detail.LoadingStatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.LoadingStatus = status;
                            model.UpdatedBy = _UserId;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.LoadType);
                                detail.LoadTypeName = allcode_detail == null ? "" : allcode_detail.Description;
                                var allcode2 = await _allCodeRepository.GetListSortByName(AllCodeType.LOADINGSTATUS);
                                var allcode_detail2 = allcode2.FirstOrDefault(s => s.CodeValue == model.LoadingStatus);
                                detail.LoadingStatusName = allcode_detail2 == null ? "" : allcode_detail2.Description;
                                if (status == (int)LoadingStatus.Da_HTTC)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListProcessingIsLoading_Da_SL", detail);
                                }
                                else
                                {
                                    await _hubContext.Clients.All.SendAsync("ListProcessingIsLoading", detail);
                                }
                            }


                        }
                        break;
                    case 9:
                        {
                            if ( model.VehicleTroughStatus != (int)VehicleTroughStatus.Blank && model.VehicleTroughStatus != null)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe đang được gọi vào máng"
                                });
                            }
                            if (detail.VehicleWeighedstatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.VehicleWeighedstatus = status;
                            if (status == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao)
                            {
                                model.VehicleWeighingTimeComeOut = DateTime.Now;
                                model.VehicleTroughStatus = (int)VehicleTroughStatus.Blank;
                                detail.VehicleWeighingTimeComeOut = DateTime.Now;
                            }
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if(detail.VehicleWeighedstatus==null && model.VehicleWeighedstatus == (int)VehicleWeighedstatus.Blank)
                            {
                                break;
                            }
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHEDSTATUS);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleWeighedstatus);
                                detail.VehicleWeighedstatusName = allcode_detail == null ? "" : allcode_detail.Description;

                                if (status == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListWeighedInput_Da_SL", detail);
                                }
                                else
                                {
                                    await _hubContext.Clients.All.SendAsync("ListWeighedInput", detail);
                                }
                            }


                        }
                        break;
                      
                    case 10:
                        {
                      
                            model.LoadingType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.LoadingType);
                                detail.LoadingTypeName = allcode_detail == null ? "" : allcode_detail.Description;
                                    // ✅ bắn cả máng cũ + máng mới

                                    await _hubContext.Clients.All.SendAsync("ProcessingIsLoading_khoa", detail);

                            }
                        }
                        break;
                }
                if (UpdateCar > 0)
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OpenPopup - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
        //danh sách xe đang ký
        public IActionResult RegisteredVehicle()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListRegisteredVehicle(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;//1 đã SL
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                var LoadingType = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                ViewBag.AllCode = AllCode;
                ViewBag.LoadingType = LoadingType;
                var data = await _vehicleInspectionRepository.GetListRegisteredVehicle(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCartoFactory - CarController: " + ex);
            }
            return PartialView();
        }
    }
}
