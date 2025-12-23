$(document).ready(function () {
    _cartofactory.init();
    var input_chua_xu_ly = document.getElementById("input_chua_xu_ly");
    input_chua_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _cartofactory.ListCartoFactory();
        }
    });
    input_chua_xu_ly.addEventListener("keyup", function (event) {
        _cartofactory.ListCartoFactory();
    });
    var input_da_xu_ly = document.getElementById("input_da_xu_ly");
    input_da_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _cartofactory.ListCartoFactory_Da_SL();
        }
    });
    input_da_xu_ly.addEventListener("keyup", function (event) {
        // Kích hoạt hàm khi giá trị thay đổi và người dùng thoát khỏi input
        _cartofactory.ListCartoFactory_Da_SL();
    });

    const container = $('<div id="dropdown-container"></div>').appendTo('body');
    let $menu = null;
    let $currentBtn = null;

    $(document).on('click', '.status-dropdown .dropdown-toggle', function (e) {
        e.stopPropagation();
        const $btn = $(this);
        const optsData = $btn.data('options'); // mảng [{text, class}]
        const options = Array.isArray(optsData) ? optsData : JSON.parse(optsData);
        const currentText = $.trim($btn.text());

        // Đóng menu cũ (nếu có)
        if ($menu) {
            $menu.remove();
            $menu = null;
        }

        $currentBtn = $btn;

        // Tạo menu + danh sách li
        $menu = $('<div class="dropdown-menu"><ul></ul></div>');
        const $ul = $menu.find('ul');

        options.forEach(opt => {
            $('<li>')
                .text(opt.text)
                .addClass('status-option')
                .attr('data-value', opt.value) // Corrected from opt.valuse
                .toggleClass('active', opt.text === currentText)
                .appendTo($ul);
        });

        const $actions = $('<div class="actions"></div>');
        $('<button class="cancel">Bỏ qua</button>').appendTo($actions);
        $('<button class="confirm">Xác nhận</button>').appendTo($actions);
        $menu.append($actions);
        container.append($menu);

        // --- 🔧 Tính toán vị trí dropdown (dùng viewport coords) ---
        const rect = $btn[0].getBoundingClientRect(); // viewport coordinates
        const btnHeight = rect.height;
        const winWidth = $(window).width();
        const winHeight = $(window).height();
        const paddingScreen = 15; // chừa khoảng 15px mỗi bên
        $menu.css({
            position: 'absolute',
            left: 0,
            top: 0,
            display: 'block',
            visibility: 'hidden'
        });

        const menuWidth = $menu.outerWidth();
        const menuHeight = $menu.outerHeight();

        // Vị trí mặc định: bên dưới button (viewport coords)
        let left = rect.left;
        let top = rect.top + btnHeight;

        // Nếu dropdown tràn phải -> dịch sang trái
        if (left + menuWidth + paddingScreen > winWidth) {
            left = winWidth - menuWidth - paddingScreen;
        }

        // Nếu tràn trái -> giữ cách paddingScreen
        if (left < paddingScreen) {
            left = paddingScreen;
        }

        // Nếu tràn dưới -> bật drop-up (hiển thị phía trên button)
        if (top + menuHeight > winHeight) {
            top = rect.top - menuHeight;
            $menu.addClass('drop-up');
        } else {
            $menu.removeClass('drop-up');
        }

        // Áp vị trí cuối cùng và hiển thị menu
        $menu.css({
            left: left,
            top: top,
            visibility: 'visible' // hiện lên
        });
    });

    // Click chọn item
    $(document).on('click', '#dropdown-container .dropdown-menu li', function (e) {
        e.stopPropagation();
        $('#dropdown-container .dropdown-menu li').removeClass('active');
        $(this).addClass('active');
    });

    // Bỏ qua
    $(document).on('click', '#dropdown-container .actions .cancel', function (e) {
        e.stopPropagation();
        closeMenu();
    });

    // ✅ Xác nhận – đổi text + class cho button
    $(document).on('click', '#dropdown-container .actions .confirm', function (e) {
        e.stopPropagation();
        if ($menu && $currentBtn) {
            const $active = $menu.find('li.active');
            if ($active.length) {
                const text = $active.text();
                const val_TT = $active.attr('data-value');
                const $row = $currentBtn.closest('tr');
                let id_row = 0;
                if ($row.length) {
                    const classAttr = $row.attr('class');
                    const match = classAttr.match(/CartoFactory_(\d+)/);
                    if (match && match[1]) {
                        id_row = match[1];
                    }
                }

                const cls = $active.attr('class').split(/\s+/)
                    .filter(c => c !== 'active')[0] || '';

                var type = $currentBtn.attr('data-type');
                if (type == '1') {
                    _cartofactory.UpdateStatus(id_row, val_TT, 10);
                    $currentBtn
                        .text(text)
                        .removeClass(function (_, old) {
                            return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                        }) // xoá các class status- cũ
                        .addClass(cls); // gắn class mới (status-arrived, status-blank…)
                } else {
                    var Status_type = _cartofactory.UpdateStatus(id_row, val_TT, 1);
                    if (Status_type == 0) {
                        $currentBtn
                            .text(text)
                            .removeClass(function (_, old) {
                                return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                            }) // xoá các class status- cũ
                            .addClass(cls); // gắn class mới (status-arrived, status-blank…)


                        if (val_TT == 1) {
                            $('#dataBody-1').find('.CartoFactory_' + id_row).remove();

                        } else {
                            $('#dataBody-0').find('.CartoFactory_' + id_row).remove();
                        }
                    }
                }
                
                
            }
        }
        closeMenu();
    });

    // Đóng menu khi click ra ngoài
    $(document).on('click', function () {
        closeMenu();
    });

    function closeMenu() {
        if ($menu) {
            $menu.remove();
            $menu = null;
            $currentBtn = null;
        }
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/CarHub", {
            
        })
        
        .build();

    let retryDelay = 2000; // 2 giây

    async function startSignalR() {
        try {
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                await connection.start();
                console.log("✅ Kết nối SignalR thành công");
            }
        } catch (err) {
            console.error("❌ SignalR connect failed. Retry in 2s...", err);
            setTimeout(startSignalR, retryDelay);
        }
    }

    // 👉 Gọi lần đầu
    startSignalR();
    const AllCode = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã đến nhà máy", CodeValue: "0" },
        // Add more objects as needed
    ];
    const AllCode2 = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã xử lý", CodeValue: "0" },
        // Add more objects as needed
    ];
    // Create a new array of objects in the desired format
    const options = AllCode.map(allcode => ({
        text: allcode.Description,
        value: allcode.CodeValue
    }));
    const options2 = AllCode2.map(allcode2 => ({
        text: allcode2.Description,
        value: allcode2.CodeValue
    }));
    const jsonString = JSON.stringify(options);
    const jsonString2 = JSON.stringify(options2);
    // Hàm render row
    function renderRow(item) {
        var date = new Date(item.registerDateOnline);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}" >
            <td>${item.recordNumber}</td>
            <td>${formatted}</td>
            <td>${item.customerName}</td>
            <td>${item.driverName}</td>
            <td>${item.phoneNumber}</td>
            <td>${item.vehicleNumber}</td>
            <td>${item.vehicleLoad}</td>
            <td>${item.licenseNumber}</td>
              <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle "data-type="1" data-options='${jsonString2}'>
                        ${item.loadingTypeName}
                    </button>
                </div>

            </td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>
                        ${item.vehicleStatusName}
                    </button>
                </div>

            </td>

        </tr>`;
    }

    function renderRow2(item) {

        return `
        <tr class="CartoFactory_${item.id}" data-queue="${item.createTime}" >
            <td>${item.queueNumber}</td>
            <td>${item.createTime}</td>
            <td>${item.name}</td>
            <td>${item.gplx}</td>
            <td>${item.phoneNumber}</td>
            <td>${item.plateNumber}</td>
            <td>${item.referee}</td>
            <td>${item.camp}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle "data-type="1" data-options='${jsonString2}'>
                        
                    </button>
                </div>

            </td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>
                       Blank
                    </button>
                </div>

            </td>

        </tr>`;
    }
    function renderRow3(item) {
        var date = new Date(item.registerDateOnline);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}" >
            <td>${item.recordNumber}</td>
            <td>${formatted}</td>
            <td>${item.customerName}</td>
            <td>${item.driverName}</td>
            <td>${item.phoneNumber}</td>
            <td>${item.vehicleNumber}</td>
            <td>${item.vehicleLoad}</td>
            <td>${item.licenseNumber}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>
                        ${item.vehicleStatusName}
                    </button>
                </div>

            </td>

        </tr>`;
    }
    // Hàm sắp xếp lại tbody theo QueueNumber tăng dần
    function sortTable_Da_SL() {
        const tbody = document.getElementById("dataBody-1");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeA - timeB;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }
    function sortTable() {
        const tbody = document.getElementById("dataBody-0");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeA - timeB;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }

    // Nhận data mới từ server
    connection.on("ListCartoFactory_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-1");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow3(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });

    connection.on("ListCartoFactory", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    connection.on("ReceiveRegistration", function (item) {
        const tbody = document.getElementById("dataBody-0");
        tbody.insertAdjacentHTML("beforeend", renderRow2(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    connection.on("ListProcessingIsLoading_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.on("ListProcessingIsLoading", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow3(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.on("ProcessingIsLoading_khoa", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    connection.onreconnecting(error => {
        console.warn("🔄 Đang reconnect...", error);
    });

    connection.onreconnected(connectionId => {
        console.log("✅ Đã reconnect. Connection ID:", connectionId);
    });

    connection.onclose(error => {
        console.error("❌ Kết nối bị đóng.", error);
    });
    function parseDateTime(str) {
        // "11:33 17/12/2025"
        const [time, date] = str.split(" ");
        const [hour, minute] = time.split(":").map(Number);
        const [day, month, year] = date.split("/").map(Number);

        return new Date(year, month - 1, day, hour, minute).getTime();
    }
});
var _cartofactory = {

    init: function () {
        _cartofactory.ListCartoFactory();
        _cartofactory.ListCartoFactory_Da_SL();
    },
    ListCartoFactory: function () {
        var model = {
            VehicleNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "",
            PhoneNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "",
            VehicleStatus: null,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            type: 0,
        }
        $.ajax({
            url: "/Car/ListCartoFactory",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_chua_xu_ly').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListCartoFactory_Da_SL: function () {
        var model = {
            VehicleNumber: $('#input_da_xu_ly').val() != undefined && $('#input_da_xu_ly').val() != "" ? $('#input_da_xu_ly').val().trim() : "",
            PhoneNumber: $('#input_da_xu_ly').val() != undefined && $('#input_da_xu_ly').val() != "" ? $('#input_da_xu_ly').val().trim() : "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            type: 1,
        }
        $.ajax({
            url: "/Car/ListCartoFactory",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_da_xu_ly').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    OpenPopup: function (id) {
        let title = 'Cập nhật trạng thái';
        let url = '/Car/OpenPopup';
        let param = {
            id: id,
            type: 1
        };

        _magnific.OpenSmallPopup(title, url, param);

    },
    UpdateStatus: function (id, status, type) {
       var status_type=0
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: status, type: type },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.msg)
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
        return status_type;
    },

}