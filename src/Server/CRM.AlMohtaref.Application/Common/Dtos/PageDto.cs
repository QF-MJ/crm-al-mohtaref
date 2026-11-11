namespace CRM.AlMohtaref.Application.Common.Dtos;

public sealed class PageDto
{
   

    public int PageNumber { get; set; }

    public bool HasNextPage { get; set; }

    public PageDto(int pageNumber, bool hasNextPage)
    {
        PageNumber = pageNumber;
        HasNextPage = hasNextPage;
    }

}
