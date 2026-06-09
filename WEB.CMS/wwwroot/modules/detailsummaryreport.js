$(document).ready(function () {

    _detail_summary_report.init();
});
let revenuChartInstance = null;
var _detail_summary_report = {
    init: function () {
        var model = {
            FromDate: null,
            ToDate: null,
            LoadType: $('#loadType').val(),
        }
        _detail_summary_report.GetDailyStatistics(model)

    },
    Seach: function () {
        var from_text = $('#date_from').val();
        parse_value = from_text.split(' ')[0].split('-')
        var from_datetime = parse_value[2] + '/' + parse_value[1] + '/' + parse_value[0];
        var to_text = $('#date_to').val();
        parse_value = to_text.split(' ')[0].split('-')
        var to_datetime = parse_value[2] + '/' + parse_value[1] + '/' + parse_value[0];
        var model = {
            FromDate: from_datetime,
            ToDate: to_datetime,
            LoadType: $('#loadType').val(),
        }
        _detail_summary_report.GetDailyStatistics(model)
        /*    _summary_report.GetTotalWeightByHour(datetime);
            _summary_report.GetProductivityStatistics(datetime);*/
    },


    GetDailyStatistics: function (model) {
        $.ajax({
            url: "/SummaryReport/DailyStatistics",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#Grid-DailyStatistics').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    getDateRangeBySelect: function () {
        const value = document.getElementById("Time").value;
        const now = new Date();
        let fromDate, toDate;

        // Reset giờ về 0:00 để so sánh chính xác
        function startOfDay(date) {
            return new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0).toLocaleDateString("en-GB");
        }
        function endOfDay(date) {
            return new Date(date.getFullYear(), date.getMonth(), date.getDate(), 23, 59, 59).toLocaleDateString("en-GB");
        }

        switch (value) {
            case "1": // Hôm nay
                fromDate = startOfDay(now);
                toDate = endOfDay(now);
                break;
            case "2": // Hôm qua
                const yesterday = new Date(now);
                yesterday.setDate(now.getDate() - 1);
                fromDate = startOfDay(yesterday);
                toDate = endOfDay(yesterday);
                break;
            case "3": // Tuần này
                const day = now.getDay() === 0 ? 7 : now.getDay(); // Chủ nhật = 7
                fromDate = startOfDay(new Date(now.getFullYear(), now.getMonth(), now.getDate() - (day - 1)));
                toDate = endOfDay(now);
                break;
            case "4": // Tuần trước
                const lastWeek = new Date(now);
                const lastWeekDay = lastWeek.getDay() === 0 ? 7 : lastWeek.getDay();
                lastWeek.setDate(now.getDate() - lastWeekDay - 6);
                fromDate = startOfDay(lastWeek);
                const toLastWeek = new Date(fromDate);
                toLastWeek.setDate(now.getDate() - lastWeekDay - 6 + 6);
                toDate = endOfDay(toLastWeek);
                break;
            case "5": // Tháng này
                fromDate = startOfDay(new Date(now.getFullYear(), now.getMonth(), 1));
                toDate = endOfDay(new Date(now.getFullYear(), now.getMonth() + 1, 0));
                break;
            case "6": // Tháng trước
                const lastMonth = now.getMonth() - 1;
                const year = lastMonth < 0 ? now.getFullYear() - 1 : now.getFullYear();
                const month = (lastMonth + 12) % 12;
                fromDate = startOfDay(new Date(year, month, 1));
                toDate = endOfDay(new Date(year, month + 1, 0));
                break;
            default:
                fromDate = null;
                toDate = null;
        }
        $('#date_from').val(_detail_summary_report.formatDate(fromDate));
        $('#date_to').val(_detail_summary_report.formatDate(toDate));
        var model = {
            FromDate: fromDate,
            ToDate: toDate,
            LoadType: $('#loadType').val(),
        }
        _detail_summary_report.GetDailyStatistics(model)

    },
    OpenPopUpVehicleLoadTaken: function (id) {
        let title = `Cập nhật trọng lượng thực tế`;
        let url = '/SummaryReport/OpenPopUpVehicleLoadTaken';
        let param = { id: id };
        _magnific.OpenSmallPopup(title, url, param);
    },
    UpdateVehicleLoad: function () {
        var status_type = 0
        var id = $('#id').val();
        var vehicleloadtaken = $('#VehicleLoadTaken').val().replace(',', '');
        $.ajax({
            url: "/Car/UpdateVehicleLoadTaken",
            type: "post",
            data: { id: id, vehicleloadtaken: vehicleloadtaken },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    $.magnificPopup.close();
                    setTimeout(location.reload(), 1000)

                } else {
                    _msgalert.error(result.msg)
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });

    },
    Export: function () {
        var from_text = $('#date_from').val();
        parse_value = from_text.split(' ')[0].split('-')
        var from_datetime = parse_value[2] + '/' + parse_value[1] + '/' + parse_value[0];
        var to_text = $('#date_to').val();
        parse_value = to_text.split(' ')[0].split('-')
        var to_datetime = parse_value[2] + '/' + parse_value[1] + '/' + parse_value[0];
        var model = {
            FromDate: from_datetime,
            ToDate: to_datetime,
            LoadType: $('#loadType').val(),
        }
        $('#btnExport').prop('disabled', true);

        _global_function.AddLoading()
        $.ajax({
            url: "/SummaryReport/ExportExcel",
            type: "Post",
            data: { SearchModel: model },
            success: function (result) {
                _global_function.RemoveLoading()
                $('#btnExport').prop('disabled', false);
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
                $('#icon-export').removeClass('fa-spinner fa-pulse');
                $('#icon-export').addClass('fa-file-excel-o');
            }
        });
    },
    formatDate: function (date) {
        parse_value = date.split(' ')[0].split('/')
        return parse_value[2] + '-' + parse_value[1] + '-' + parse_value[0];
    },
}