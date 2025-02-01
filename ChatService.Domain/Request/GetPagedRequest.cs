namespace ChatService.Domain.Request
{
    public class GetPagedRequest
    {
        public int PageSize { get; set; }
        public string? StartAfterDocumentId { get; set; }
    }
}
