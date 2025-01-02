using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models.User_Md;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.User_ctl
{
    public class Us_OrderController : Controller
    {
        DataDataContext db = new DataDataContext();

        // Hiển thị danh sách hóa đơn
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem danh sách hóa đơn.";
                return RedirectToAction("DangNhap", "DangNhap");
            }

            // Lấy thông tin người dùng hiện tại từ session
            string userName = Session["UserName"].ToString();
            var currentUser = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == userName);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction("DangNhap", "DangNhap");
            }

            // Lấy danh sách hóa đơn
            IEnumerable<hoa_don> orders;
            if (currentUser.vai_tro == "admin")
            {
                // Admin có thể xem tất cả hóa đơn
                orders = db.hoa_dons.ToList();
            }
            else
            {
                // Người dùng thường chỉ có thể xem hóa đơn của chính họ
                orders = db.hoa_dons.Where(h => h.nguoi_dung_id == currentUser.nguoi_dung_id).ToList();
            }

            return View(orders);
        }

        // Hiển thị chi tiết hóa đơn
        public ActionResult ChiTietHoaDon(int id)
        {
            var hoaDon = db.hoa_dons.FirstOrDefault(hd => hd.hoa_don_id == id);
            if (hoaDon == null)
            {
                TempData["ErrorMessage"] = "Hóa đơn không tồn tại.";
                return RedirectToAction("Index");
            }

            var chiTietHoaDonItems = (from cthd in db.chi_tiet_hoa_dons
                                      join cts in db.chi_tiet_san_phams on cthd.chi_tiet_san_pham_id equals cts.chi_tiet_id
                                      join sp in db.san_phams on cts.san_pham_id equals sp.san_pham_id
                                      where cthd.hoa_don_id == id
                                      select new ChiTietHoaDonItem
                                      {
                                          TenSanPham = sp.ten_san_pham,  // Lấy tên sản phẩm
                                          KichThuoc = cthd.kich_thuoc,
                                          SoLuong = (int)cthd.so_luong,
                                          Gia = cthd.gia ?? 0,
                                          TongTien = cthd.tong_tien ?? 0,
                                          DoDay = cthd.do_day.ToString(),  // Lấy độ dày
                                          MauSac = cthd.mau_sac  // Lấy màu sắc
                                      }).ToList();

            var viewModel = new ChiTietHoaDonViewModel
            {
                HoaDonId = hoaDon.hoa_don_id,
                NgayLap = hoaDon.ngay_lap ?? DateTime.Now,
                TongTien = hoaDon.tong_tien ?? 0,
                TrangThai = hoaDon.trang_thai,
                DiaChiGiaoHang = hoaDon.dia_chi_giao_hang,
                ChiTietHoaDon = chiTietHoaDonItems
            };

            return View(viewModel);
        }


    }
}
