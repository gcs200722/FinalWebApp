using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using FinalWebApp.Enum;

public static class HtmlExtensions
{
    // Helper to render radio button list
    public static IHtmlContent RadioButtonList(this IHtmlHelper htmlHelper, string name, Array values, string selectedValue)
    {
        var builder = new HtmlContentBuilder();

        foreach (var value in values)
        {
            var isChecked = value.ToString() == selectedValue ? "checked" : "";
            var id = $"{name}-{value}";
            builder.AppendHtml($@"
                <div class='form-check form-check-inline'>
                    <input type='radio' class='form-check-input' name='{name}' value='{value}' id='{id}' {isChecked} />
                    <label class='form-check-label' for='{id}'>{value}</label>
                </div>");
        }

        // Add "All" option
        builder.AppendHtml($@"
            <div class='form-check form-check-inline'>
                <input type='radio' class='form-check-input' name='{name}' value='' id='{name}-All' {(string.IsNullOrEmpty(selectedValue) ? "checked" : "")} />
                <label class='form-check-label' for='{name}-All'>All</label>
            </div>");

        return builder;
    }

    // Helper to render order badge
    public static IHtmlContent OrderBadge(this IHtmlHelper htmlHelper, OrderStatusEnum status)
    {
        var badgeClass = status == OrderStatusEnum.Confirmed ? "bg-primary" : "bg-danger";
        return new HtmlString($"<span class='badge {badgeClass}'>{status}</span>");
    }

    // Helper to render table status badge
    public static IHtmlContent TableStatusBadge(this IHtmlHelper htmlHelper, StatusTableEnum? status)
    {
        var badgeClass = status switch
        {
            StatusTableEnum.Empty => "bg-success",
            StatusTableEnum.Booked => "bg-warning",
            StatusTableEnum.Active => "bg-info",
            _ => "bg-secondary"
        };

        return new HtmlString($"<span class='badge {badgeClass}'>{status}</span>");
    }

    // Helper to render update order form
    public static IHtmlContent UpdateOrderForm(this IHtmlHelper htmlHelper, Guid orderId, OrderStatusEnum status)
    {
        return new HtmlString($@"
            <form method='post' asp-action='UpdateOrderStatus' class='d-inline'>
                <input type='hidden' name='orderId' value='{orderId}' />
                <div class='btn-group' role='group'>
                    <input type='radio' class='btn-check' name='status' value='Confirmed' id='confirmed-{orderId}' {(status == OrderStatusEnum.Confirmed ? "checked" : "")} />
                    <label class='btn btn-outline-primary btn-sm' for='confirmed-{orderId}'>Confirmed</label>

                    <input type='radio' class='btn-check' name='status' value='Canceled' id='canceled-{orderId}' {(status == OrderStatusEnum.Canceled ? "checked" : "")} />
                    <label class='btn btn-outline-danger btn-sm' for='canceled-{orderId}'>Canceled</label>
                </div>
                <button type='submit' class='btn btn-sm btn-primary mt-2'>Update</button>
            </form>");
    }
}
