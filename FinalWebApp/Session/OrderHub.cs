using FinalWebApp.Data.Entities;
using FinalWebApp.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace FinalWebApp.Session
{
    public class OrderHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            // Lấy danh sách tất cả người dùng có role "customer"
            var customers = await _userManager.GetUsersInRoleAsync("customer");

            // Lấy customerId từ tất cả người dùng có role "customer"
            var customerIds = customers.Select(user => user.Id.ToString()).ToList();

            // Thêm khách hàng vào nhóm với tên là "customer-{customerId}"
            foreach (var customerId in customerIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var customerId = GetCustomerIdFromContext();

            // Xóa khách hàng khỏi nhóm khi ngắt kết nối
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer-{customerId}");

            await base.OnDisconnectedAsync(exception);
        }

        private string GetCustomerIdFromContext()
        {
            // Lấy customerId từ thông tin người dùng, ví dụ từ token JWT hoặc session
            return Context.UserIdentifier;  // Giả sử userIdentifier chứa customerId
        }

        // Phương thức để gửi thông báo cho khách hàng có ID tương ứng
        public async Task SendOrderStatusNotification(Guid orderId, OrderStatusEnum status)
        {
            // Lấy danh sách tất cả người dùng có role "customer"
            var customers = await _userManager.GetUsersInRoleAsync("customer");

            foreach (var customer in customers)
            {
                // Lấy customerId từ người dùng
                var customerId = customer.Id.ToString();

                // Tạo thông điệp thông báo
                string notificationMessage = $"Order ID: {orderId} has been updated to {status}";

                // Gửi thông báo tới nhóm tương ứng với mỗi customerId
                await Clients.Group($"customer-{customerId}").SendAsync("ReceiveOrderStatusNotification", notificationMessage);
            }
        }
    }
}
