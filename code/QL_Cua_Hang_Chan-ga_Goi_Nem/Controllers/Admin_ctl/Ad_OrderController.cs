using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models.Admin_Md;
using System.Net.Mail;
using System.Net;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers
{
    public class Ad_OrderController : Controller
    {
        DataDataContext db = new DataDataContext();

        // Hiển thị danh sách các hóa đơn
        public ActionResult DonHang(DateTime? ngayBatDau, DateTime? ngayKetThuc, string trangThai, string tenTimKiem)
        {
            var danhSachHoaDon = db.hoa_dons.AsQueryable();

            // Mặc định lọc theo trạng thái "cho_xac_nhan" nếu không có tham số trangThai
            if (string.IsNullOrEmpty(trangThai))
            {
                trangThai = "cho_xac_nhan";
            }

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai))
            {
                danhSachHoaDon = danhSachHoaDon.Where(hd => hd.trang_thai == trangThai);
            }

            // Lọc theo ngày nếu có
            if (ngayBatDau.HasValue && ngayKetThuc.HasValue)
            {
                danhSachHoaDon = danhSachHoaDon.Where(hd => hd.ngay_lap >= ngayBatDau.Value && hd.ngay_lap <= ngayKetThuc.Value);
            }

            // Lọc theo tên người dùng nếu có
            if (!string.IsNullOrEmpty(tenTimKiem))
            {
                danhSachHoaDon = danhSachHoaDon.Where(hd => hd.nguoi_dung.ten_dang_nhap.Contains(tenTimKiem));
            }

            // Sắp xếp theo ngày lập từ cũ đến mới
            danhSachHoaDon = danhSachHoaDon.OrderBy(hd => hd.ngay_lap);

            var danhSachHoaDonView = danhSachHoaDon.Select(hd => new OrderViewModel
            {
                HoaDonId = hd.hoa_don_id,
                TenNguoiDung = hd.nguoi_dung.ten_dang_nhap,
                NgayLap = hd.ngay_lap,
                TrangThai = hd.trang_thai,
                TongTien = (decimal)hd.tong_tien
            }).ToList();

            return View(danhSachHoaDonView);
        }



        // Hiển thị chi tiết hóa đơn
        public ActionResult Details(int id)
        {
            var hoaDon = db.hoa_dons
                .Where(hd => hd.hoa_don_id == id)
                .Select(hd => new OrderDetailViewModel
                {
                    HoaDonId = hd.hoa_don_id,
                    NgayLap = hd.ngay_lap,
                    TrangThai = hd.trang_thai,
                    DiaChiGiaoHang = hd.dia_chi_giao_hang,
                    SoDienThoai = hd.so_dien_thoai,
                    TenKhachHang = hd.ten_khach_hang,
                    ChiTietHoaDon = hd.chi_tiet_hoa_dons.Select(ct => new OrderDetailItemViewModel
                    {
                        TenSanPham = ct.chi_tiet_san_pham.san_pham.ten_san_pham,
                        SoLuong = (int)ct.so_luong,
                        KichThuoc = ct.kich_thuoc,
                        DoDay = ct.do_day ?? 0, // Kiểm tra null và gán 0 nếu null
                        MauSac = ct.mau_sac,
                        Gia = (decimal)ct.gia,
                        TongTien = (decimal)ct.tong_tien
                    }).ToList()
                }).FirstOrDefault();

            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            return View(hoaDon);
        }

        [HttpPost]
        public ActionResult ConfirmOrder(int id)
        {
            // Tìm hóa đơn dựa trên ID
            var hoaDon = db.hoa_dons.FirstOrDefault(hd => hd.hoa_don_id == id);

            if (hoaDon == null)
            {
                return HttpNotFound("Không tìm thấy hóa đơn.");
            }

            // Lấy thông tin người dùng từ hóa đơn
            var nguoiDung = db.nguoi_dungs.FirstOrDefault(nd => nd.nguoi_dung_id == hoaDon.nguoi_dung_id);

            if (nguoiDung == null)
            {
                return HttpNotFound("Không tìm thấy thông tin người dùng.");
            }

            // Cập nhật trạng thái đơn hàng
            hoaDon.trang_thai = "da_giao";
            db.SubmitChanges();

            // Gửi email thông báo
            SendOrderDeliveredEmail(hoaDon.ten_khach_hang, hoaDon.so_dien_thoai, hoaDon.dia_chi_giao_hang, nguoiDung.email, hoaDon.hoa_don_id.ToString(), (decimal)hoaDon.tong_tien);

            // Quay lại danh sách đơn hàng
            TempData["SuccessMessage"] = "Đơn hàng đã được giao và email đã được gửi cho khách hàng.";
            return RedirectToAction("DonHang");
        }


        private void SendOrderDeliveredEmail(string name, string phone, string address, string email, string orderId, decimal totalAmount)
        {
            try
            {
                var fromAddress = new MailAddress("tuan12345yahoo@gmail.com", "Cửa hàng Chăn Ga Gối Nệm");
                var toAddress = new MailAddress(email);
                const string fromPassword = "amtxpbwdvxxbpvjt"; // Mật khẩu ứng dụng email

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // SMTP server của Gmail
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };

                // Nội dung email
                string emailBody = string.Format(@"
                Chào bạn,

                Chúng tôi xin thông báo rằng đơn hàng #{0} của bạn đã được giao thành công.
                Thông tin khách hàng:
                - Tên khách hàng: {3}                
                - Số điện thoại: {4}
                - Địa chỉ giao hàng: {5}

                Chi tiết đơn hàng:
                - Mã đơn hàng: {0}
                - Tổng tiền: {1:N0} đ

                Cảm ơn bạn đã tin tưởng và mua sắm tại cửa hàng Chăn Ga Gối Nệm. Hy vọng bạn hài lòng với sản phẩm của chúng tôi. Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc số điện thoại hỗ trợ.

                Trân trọng,
                Cửa hàng Chăn Ga Gối Nệm", orderId, totalAmount, name, phone, address);

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Xác nhận giao hàng - Cửa hàng Chăn Ga Gối Nệm",
                    Body = emailBody,
                    IsBodyHtml = false // Gửi email dạng văn bản thuần túy
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi gửi email
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
                // Có thể thêm log hoặc hiển thị thông báo lỗi
            }
        }


    }
}



