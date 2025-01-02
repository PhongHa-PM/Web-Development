using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;
using System;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers
{
    public class DangNhapController : Controller
    {
        // Kết nối với database
        DataDataContext db = new DataDataContext();

        // GET: /DangNhap/
        public ActionResult DangNhap()
        {
            return View();
        }

        // POST: /DangNhap/
        [HttpPost]
        public ActionResult DangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin đăng nhập.";
                return View();
            }

            // Tìm người dùng theo tên đăng nhập
            var user = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == tenDangNhap);

            if (user != null)
            {
                if (user.vai_tro == "admin")
                {
                    // Đối với admin, kiểm tra mật khẩu không mã hóa
                    if (matKhau == user.mat_khau_hash)
                    {
                        // Lưu thông tin admin vào Session
                        Session["UserName"] = user.ten_dang_nhap;
                        Session["VaiTro"] = user.vai_tro;

                        // Chuyển đến Controller Ad_Dashboard action Index
                        return RedirectToAction("HienThiSP", "Ad_Product");
                    }
                }
                else if (user.vai_tro == "nguoi_dung")
                {
                    // Đối với người dùng bình thường, kiểm tra mật khẩu đã mã hóa
                    if (VerifyPassword(matKhau, user.mat_khau_hash))
                    {
                        // Lưu thông tin người dùng vào Session
                        Session["UserName"] = user.ten_dang_nhap;
                        Session["VaiTro"] = user.vai_tro;

                        // Chuyển đến Controller Us_Home action HienThiSanPhamNem
                        return RedirectToAction("HienThiSanPhamNem", "Us_Product");
                    }
                }
            }

            // Nếu thông tin đăng nhập không chính xác
            ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        public ActionResult DangXuat()
        {
            Session.Clear();  // Xóa tất cả Session
            return RedirectToAction("DangNhap", "DangNhap");  // Chuyển hướng về trang đăng nhập
        }


        // Hàm mã hóa mật khẩu (hashing)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return string.Concat(bytes.Select(b => b.ToString("x2")));
            }
        }

        // Hàm kiểm tra mật khẩu
        private bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            string inputPasswordHash = HashPassword(inputPassword);
            return inputPasswordHash == storedPasswordHash;
        }

        // GET: /DangNhap/DangKy
        public ActionResult DangKy()
        {
            return View();
        }

        // POST: /DangNhap/DangKy
        [HttpPost]
        public ActionResult DangKy(string tenDangNhap, string matKhau, string email, string soDienThoai, string diaChi)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            // Kiểm tra tên đăng nhập đã tồn tại
            var existingUser = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == tenDangNhap);

            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            // Hash mật khẩu
            string hashedPassword = HashPassword(matKhau);

            // Thêm người dùng mới
            var newUser = new nguoi_dung
            {
                ten_dang_nhap = tenDangNhap,
                mat_khau_hash = hashedPassword,
                email = email,
                so_dien_thoai = soDienThoai,
                dia_chi = diaChi,
                vai_tro = "nguoi_dung", // Mặc định là "nguoi_dung"
                ngay_tao = System.DateTime.Now
            };

            db.nguoi_dungs.InsertOnSubmit(newUser);
            db.SubmitChanges();

            // Gửi email thông báo
            SendRegistrationEmail(email);

            // Đăng ký thành công, chuyển hướng về trang đăng nhập
            return RedirectToAction("DangNhap");
        }

        // Hàm gửi email thông báo đăng ký thành công
        private void SendRegistrationEmail(string email)
        {
            try
            {
                var fromAddress = new MailAddress("tuan12345yahoo@gmail.com", "Cửa hàng Chăn Ga Gối Nệm");
                var toAddress = new MailAddress(email);
                const string fromPassword = "amtxpbwdvxxbpvjt"; // Mật khẩu của email (hoặc mật khẩu ứng dụng nếu bật xác thực 2 bước)

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // SMTP server cho Gmail
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Chúc mừng bạn đã đăng ký thành công",
                    Body = "Chúc mừng bạn đã đăng ký thành công tài khoản tại Website cửa hàng Chăn Ga Gối Nệm!"
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc xử lý khi không thể gửi email
                ViewBag.ErrorMessage = "Không thể gửi email. Vui lòng thử lại sau.";
                Console.WriteLine(ex.Message); // In lỗi ra console để dễ dàng kiểm tra
            }
        }

        public ActionResult ThongTinTaiKhoan()
        {
            // Kiểm tra nếu người dùng chưa đăng nhập
            if (Session["UserName"] == null)
            {
                return RedirectToAction("DangNhap"); // Chuyển hướng về trang đăng nhập
            }

            // Lấy thông tin tài khoản từ Session
            string tenDangNhap = Session["UserName"].ToString();

            // Tìm thông tin tài khoản trong cơ sở dữ liệu
            var thongtin = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == tenDangNhap);

            if (thongtin == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("DangNhap");
            }

            return View(thongtin);
        }

        [HttpGet]
        public ActionResult SuaThongTinTaiKhoan()
        {
            // Kiểm tra nếu người dùng chưa đăng nhập
            if (Session["UserName"] == null)
            {
                return RedirectToAction("DangNhap"); // Chuyển hướng về trang đăng nhập
            }

            // Lấy thông tin tài khoản từ Session
            string tenDangNhap = Session["UserName"].ToString();

            // Tìm thông tin tài khoản trong cơ sở dữ liệu
            var thongtin = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == tenDangNhap);

            if (thongtin == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("DangNhap");
            }

            // Trả về view để người dùng có thể sửa thông tin
            return View(thongtin);
        }

        [HttpPost]
        public ActionResult SuaThongTinTaiKhoan(nguoi_dung updatedUser)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            // Lấy thông tin tài khoản từ Session
            string tenDangNhap = Session["UserName"].ToString();

            // Tìm thông tin tài khoản trong cơ sở dữ liệu
            var thongtin = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == tenDangNhap);

            if (thongtin == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("DangNhap");
            }

            // Cập nhật thông tin người dùng
            thongtin.email = updatedUser.email;
            thongtin.so_dien_thoai = updatedUser.so_dien_thoai;
            thongtin.dia_chi = updatedUser.dia_chi;

            db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            ViewBag.SuccessMessage = "Thông tin đã được cập nhật thành công.";
            return RedirectToAction("ThongTinTaiKhoan"); // Hiển thị lại thông tin đã cập nhật
        }
    }
}
