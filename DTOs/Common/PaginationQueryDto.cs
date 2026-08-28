using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Common;

public class PaginationQueryDto
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}