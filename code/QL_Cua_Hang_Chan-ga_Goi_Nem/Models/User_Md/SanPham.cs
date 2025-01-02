using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Models.User_Md
{
    public class SanPham
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public string MoTa { get; set; }
        public decimal GiamGia { get; set; }
        public decimal Gia { get; set; }
        public decimal GiaKhuyenMai { get; set; }
        public string HinhAnh { get; set; }
        public string DanhSachHinhAnh { get; set; }
        public string ThuongHieu { get; set; }
        public int SoLuongKichThuoc { get; set; }
        public int? SoLuongDaBan { get; set; }
        public double DanhGiaTrungBinh { get; set; }
        public int SoLuongDanhGia { get; set; }
        public decimal DoDay { get; set; }
    }

    public class ChiTietSanPhamViewModel
    {
        public int SanPhamId { get; set; }
        public int ChiTietSanPhamId { get; set; }
        public int SoLuong { get; set; }
        public int? SoLuongTon { get; set; }
        public string TenSanPham { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }
        public string ThuongHieu { get; set; }
        public decimal Gia { get; set; }
        public decimal GiaKhuyenMai { get; set; }
        public double DanhGiaTrungBinh { get; set; }
        public int SoLuongDanhGia { get; set; }
        public int SoLuongDaBan { get; set; }
        public List<string> MauSac { get; set; }
        public List<string> KichThuoc { get; set; }
        public List<int> DoDay { get; set; } // Kiểu List<int> để chứa độ dày
        public List<string> DanhSachHinhAnh { get; set; }
        public DateTime? NgayBatDauKhuyenMai { get; set; }
        public DateTime? NgayKetThucKhuyenMai { get; set; }
        // Thuộc tính mới để lưu lựa chọn hiện tại
        public string KichThuocHienTai { get; set; } // Lưu kích thước đang chọn
        public string MauSacHienTai { get; set; }
        public int? DoDayHienTai { get; set; } // Lưu độ dày đang chọn (nullable để xử lý trường hợp không chọn)
    }

    public class CartItem
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public string KichThuoc { get; set; }
        public string MauSac { get; set; }
        public int DoDay { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public int? SoLuongTon { get; set; }
        public string HinhAnh { get; set; }
        public decimal ThanhTien
        {
            get { return SoLuong * Gia; }
        }

    }

    public class HoaDonViewModel
    {
        public decimal TongTien { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public List<CartItem> GioHang { get; set; }
    }

    public class HoaDonCuaToiViewModel
    {
        public hoa_don HoaDon { get; set; }
        public IEnumerable<chi_tiet_hoa_don> ChiTietHoaDon { get; set; }
    }

    public class ChiTietHoaDonViewModel
    {
        public int HoaDonId { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public string DiaChiGiaoHang { get; set; }

        public List<ChiTietHoaDonItem> ChiTietHoaDon { get; set; }
    }

    public class ChiTietHoaDonItem
    {
        public string TenSanPham { get; set; }  // Tên sản phẩm
        public string KichThuoc { get; set; }
        public string DoDay { get; set; } // Độ dày
        public string MauSac { get; set; } // Màu sắc
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public decimal TongTien { get; set; }
    }





}