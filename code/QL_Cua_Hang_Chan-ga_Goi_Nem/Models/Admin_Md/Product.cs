using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Models.Admin_Md
{
    public class SanPhamKhuyenMaiViewModel
    {
        public int KhuyenMaiId { get; set; }
        public int ChiTietSanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public string LoaiSanPham { get; set; }
        public string KichThuoc { get; set; }
        public int? DoDay { get; set; }  // Cập nhật là kiểu Nullable int để dễ xử lý
        public string MauSac { get; set; }
        public string ChatLieu { get; set; }
        public decimal Gia { get; set; }
        public decimal? GiaKhuyenMai { get; set; }  // Cập nhật thành Nullable decimal vì giá khuyến mãi có thể là null
        public int? SoLuongTon { get; set; }
    }

    public class OrderViewModel
    {
        public int HoaDonId { get; set; }
        public string TenNguoiDung { get; set; }
        public DateTime? NgayLap { get; set; }
        public string TrangThai { get; set; }
        public decimal TongTien { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int HoaDonId { get; set; }
        public DateTime? NgayLap { get; set; }
        public string TrangThai { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public List<OrderDetailItemViewModel> ChiTietHoaDon { get; set; }
    }
    public class OrderDetailItemViewModel
    {
        public string TenSanPham { get; set; }
        public string KichThuoc { get; set; }
        public int DoDay { get; set; }
        public string MauSac { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public decimal TongTien { get; set; }
    }

    public class DoanhThuModel
    {
        public int HoaDonId { get; set; }
        public DateTime? NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
    }







}