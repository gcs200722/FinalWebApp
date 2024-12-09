// Đảm bảo FinalWebApp được khởi tạo trước khi sử dụng
var FinalWebApp = FinalWebApp || {};

// Định nghĩa logic của kết nối SignalR trong FinalWebApp
FinalWebApp.OrderNotifications = {
    notificationCount: 0,

    // Hàm để thiết lập kết nối SignalR
    startConnection: function () {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/orderHub")  // Đảm bảo đường dẫn đúng với Hub trên Server
            .build();

        // Nhận thông báo từ server khi có thay đổi trạng thái đơn hàng
        connection.on("SendOrderStatusNotification", function (orderId, status) {
            const notificationMessage = `OrderID: ${orderId} status has been updated to ${status}`;

            // Tăng số lượng thông báo
            FinalWebApp.OrderNotifications.notificationCount++;

            // Cập nhật số lượng thông báo hiển thị trên biểu tượng thông báo
            document.getElementById("notificationCount").style.display = "inline-block";
            document.getElementById("notificationCount").innerText = FinalWebApp.OrderNotifications.notificationCount;

            // Tạo phần tử mới cho thông báo
            const notificationItem = document.createElement("li");
            notificationItem.classList.add("dropdown-item");
            notificationItem.innerText = notificationMessage;

            // Thêm thông báo vào danh sách
            document.getElementById("notificationList").appendChild(notificationItem);

            // Ẩn thông báo "No notifications" nếu có thông báo mới
            const noNotifications = document.getElementById("noNotifications");
            if (noNotifications) {
                noNotifications.style.display = "none";
            }
        });

        // Bắt đầu kết nối SignalR
        connection.start().catch(function (err) {
            console.error(err.toString());
        });
    }
};

// Gọi hàm để bắt đầu kết nối khi trang được tải xong
window.onload = function () {
    FinalWebApp.OrderNotifications.startConnection();
};
