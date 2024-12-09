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
            var user = Context.User;

            if (user != null)
            {
                // Kiểm tra và thêm nhóm cho CUSTOMER
                if (user.IsInRole("CUSTOMER"))
                {
                    var customerId = Context.UserIdentifier;
                    if (!string.IsNullOrEmpty(customerId))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
                    }
                }

                // Kiểm tra và thêm nhóm cho STAFF              
                if (user.IsInRole("STAFF"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "STAFF");
                }
            }
            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var customerId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(customerId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer-{customerId}");
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "STAFF");

            await base.OnDisconnectedAsync(exception);
        }



        // Phương thức để gửi thông báo cho khách hàng có ID tương ứng
        public async Task SendOrderStatusNotification(Guid customerId, Guid orderId, OrderStatusEnum status)
        {
            string message = status switch
            {
                OrderStatusEnum.Confirmed => $"Your order with ID {orderId} has been confirmed.",
                OrderStatusEnum.Canceled => $"Your order with ID {orderId} has been cancelled.",
                _ => $"Your order with ID {orderId} is currently in {status} status."
            };
            // Gửi thông báo tới nhóm "customer-{customerId}"
            await Clients.Group($"customer-{customerId}")
                .SendAsync("SendOrderStatusNotification", message);
        }
        public async Task NotifyStaff(Guid orderId, OrderStatusEnum status)
        {
            // Log để kiểm tra         
            string message = $"Order ID: {orderId} status updated to {status}";
            await Clients.Group("STAFF").SendAsync("NotifyStaff", message);
        }

    }
}
    