namespace Website_QLPT.Models
{
    /// <summary>
    /// Interface đánh dấu entity hỗ trợ Soft Delete.
    /// ApplicationDbContext sẽ tự động áp dụng global query filter
    /// để các query bình thường không thấy bản ghi đã xóa mềm.
    /// </summary>
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
