using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models.User_Md;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.User_ctl
{
    public class Us_SearchController : Controller
    {
        //
        // GET: /Home/
        DataDataContext db = new DataDataContext();
        public ActionResult Search(string TimKiem)
        {
            if (string.IsNullOrEmpty(TimKiem))
            {
                // Nếu không có từ khóa tìm kiếm, trả về danh sách rỗng
                return View(new List<SanPham>());
            }

            // Tìm kiếm tất cả sản phẩm có tên chứa từ khóa tìm kiếm
            var sanPhams = (from sp in db.san_phams
                            where sp.ten_san_pham.Contains(TimKiem)
                            select new
                            {
                                sp.san_pham_id,
                                sp.ten_san_pham,
                                sp.giam_gia,
                                sp.hinh_anh_chinh,
                                sp.danh_sach_hinh_anh,
                                sp.thuong_hieu,
                                Gia = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .Min(ctsp => ctsp.gia),
                                GiaKhuyenMai = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .Min(ctsp => ctsp.gia_khuyen_mai),
                                SoLuongKichThuoc = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .Select(ctsp => ctsp.kich_thuoc)
                                    .Distinct()
                                    .Count(),
                                SoLuongDaBan = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .SelectMany(ctsp => ctsp.chi_tiet_hoa_dons)
                                    .Distinct()
                                    .Count(),
                                DanhGiaTrungBinh = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Average(dg => dg.so_sao),
                                SoLuongDanhGia = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Count()
                            }).ToList();

            // Chuyển đổi sang ViewModel
            var model = sanPhams.Select(sp => new SanPham
            {
                SanPhamId = sp.san_pham_id,
                TenSanPham = sp.ten_san_pham,
                GiamGia = sp.giam_gia ?? 0,
                HinhAnh = sp.hinh_anh_chinh,
                Gia = sp.Gia ?? 0,
                GiaKhuyenMai = sp.GiaKhuyenMai ?? 0,
                ThuongHieu = sp.thuong_hieu,
                DanhSachHinhAnh = sp.danh_sach_hinh_anh,
                SoLuongKichThuoc = sp.SoLuongKichThuoc,
                DanhGiaTrungBinh = sp.DanhGiaTrungBinh ?? 0,
                SoLuongDanhGia = sp.SoLuongDanhGia,
                SoLuongDaBan = sp.SoLuongDaBan
            }).ToList();

            // Trả dữ liệu về view
            return View(model);
        }

    }
}
