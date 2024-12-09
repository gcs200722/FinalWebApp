var FinalWebApp = FinalWebApp || {};

FinalWebApp.OrderNotifications = {
    notificationCount: 0,
    MAX_NOTIFICATIONS: 10,

    startConnection: function () {
        this.connection = new signalR.HubConnectionBuilder().withUrl("/orderHub").build();

        this.connection.on("SendOrderStatusNotification", (message,orderId) => this.addNotification(message, "Customer"));
        this.connection.on("NotifyStaff", (message, orderId) => this.addNotification(message, "Staff"));

        this.connection.onclose(() => setTimeout(() => this.startConnection(), 5000));

        this.connection.start().catch((err) => console.error("Error starting SignalR: ", err));
    },
    addNotification: function (message, type) {
        let notifications = JSON.parse(localStorage.getItem("notifications")) || [];

        const notificationMessage = {
            message: message,
            type: type,
            time: new Date().toLocaleTimeString(),
            orderId: this.getOrderIdFromMessage(message)
        };

        notifications.unshift(notificationMessage);

        if (notifications.length > this.MAX_NOTIFICATIONS) {
            notifications.pop();
        }

        localStorage.setItem("notifications", JSON.stringify(notifications));

        this.updateNotificationUI(notifications);
    },

    getOrderIdFromMessage: function (message) {
        const regex = /[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}/;
        const match = message.match(regex);
        return match ? match[0] : null;
    },

    updateNotificationUI: function (notifications) {
        this.notificationCount = notifications.length;
        const notificationCountElem = document.getElementById("notificationCount");
        notificationCountElem.style.display = "inline-block";
        notificationCountElem.innerText = this.notificationCount;

        const notificationList = document.getElementById("notificationList");
        notificationList.innerHTML = "";

        notifications.forEach((notification) => {
            const icon = notification.type === "Customer" ? '<i class="fas fa-check-circle"></i>' : '<i class="fas fa-info-circle"></i>';
            const notificationItem = document.createElement("li");
            notificationItem.classList.add("dropdown-item");
            notificationItem.innerHTML = `${icon} <b>[${notification.type}]</b> ${notification.message} <small>${notification.time}</small>`;

            notificationItem.addEventListener("click", () => this.handleNotificationClick(notification.orderId));

            notificationList.appendChild(notificationItem);
        });

        const noNotifications = document.getElementById("noNotifications");
        if (notifications.length === 0 && noNotifications) {
            noNotifications.style.display = "block";
        } else if (noNotifications) {
            noNotifications.style.display = "none";
        }
    },

    getUserRole: function () {
        // Lấy vai trò người dùng từ localStorage hoặc một cơ chế khác
        return localStorage.getItem("userRole"); // "Customer" hoặc "Staff"
    },

    handleNotificationClick: function (orderId) {
        if (orderId) {
            const userRole = this.getUserRole();

            if (userRole === "STAFF") {
                window.location.href = `/Staff/OrderDetails/${orderId}`;
            } else if (userRole === "CUSTOMER") {
                window.location.href = `/Order/Details/${orderId}`;
            } else {
                console.warn("Invalid user role or no role detected.");
            }
        } else {
            console.warn("No OrderId found for the notification.");
        }
    },

    clearNotifications: function () {
        localStorage.removeItem("notifications");
        this.updateNotificationUI([]);
    }
};

window.onload = function () {
    const storedNotifications = JSON.parse(localStorage.getItem("notifications")) || [];
    FinalWebApp.OrderNotifications.updateNotificationUI(storedNotifications);

    FinalWebApp.OrderNotifications.startConnection();

    document.getElementById("clearNotifications").addEventListener("click", () => FinalWebApp.OrderNotifications.clearNotifications());
};
