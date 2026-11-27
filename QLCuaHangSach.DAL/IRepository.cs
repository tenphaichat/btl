using System.Data;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>
/// Hợp đồng CRUD chung cho mọi bảng danh mục: lấy tất cả, lấy theo mã, thêm, sửa, xóa.
/// Mỗi lớp DAL ký hợp đồng này nên BUS thao tác mọi thực thể theo cùng một cách.
/// </summary>
public interface IRepository<T> where T : EntityBase
{
    DataTable GetAll();
    T GetById(string ma);
    bool Insert(T entity);
    bool Update(T entity);
    bool Delete(string ma);
}
