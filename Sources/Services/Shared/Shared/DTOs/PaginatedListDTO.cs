namespace Pulsar.Services.Shared.DTOs;

public class PaginatedListDTO<TModel>
{
    public List<TModel> Page { get; set; }
    public string? Next { get; set; }

    public PaginatedListDTO(List<TModel> page, string? next)
    {
        Page = page;
        Next = next;
    }
}
