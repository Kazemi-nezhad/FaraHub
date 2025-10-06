namespace FaraHub.Web.Models
{
    public enum TicketStatus
    {
        // در حال رسیدگی = 0
        InProgress,
        // پاسخ داده شده = 1
        Replied,
        // در انتظار پاسخ مشتری = 2
        WaitingForCustomer,
        // نگه داشته شده = 3
        OnHold,
        // پاسخ مشتری = 4
        CustomerReplied,
        // تکمیل شده = 5
        Completed
    }
}