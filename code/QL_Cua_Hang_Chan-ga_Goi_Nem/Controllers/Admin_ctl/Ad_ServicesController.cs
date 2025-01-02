using System;
using System.Linq;
using System.Timers;
using System.Web.Mvc;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models.Admin_Md;
using System.Collections.Generic;
using System.Dynamic;
using System.Data.Linq;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.Admin_ctl;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.Admin_ctl
{
    public class Ad_ServicesController : Controller
    {
        DataDataContext db = new DataDataContext();

        // GET: Hiển thị danh sách khuyến mãi
        public ActionResult KhuyenMaiSP(string searchString, string sortOrder, string statusFilter, DateTime? startDate, DateTime? endDate)
        {
            var khuyenMaiQuery = db.khuyen_mais.AsQueryable();
            var currentDate = DateTime.Now;

            // Lọc theo tên khuyến mãi
            if (!string.IsNullOrEmpty(searchString))
            {
                khuyenMaiQuery = khuyenMaiQuery.Where(km => km.ten_khuyen_mai.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "ongoing":
                        khuyenMaiQuery = khuyenMaiQuery.Where(km => km.ngay_bat_dau <= currentDate && km.ngay_ket_thuc >= currentDate);
                        break;
                    case "ended":
                        khuyenMaiQuery = khuyenMaiQuery.Where(km => km.ngay_ket_thuc < currentDate);
                        break;
                    case "not_started":
                        khuyenMaiQuery = khuyenMaiQuery.Where(km => km.ngay_bat_dau > currentDate);
                        break;
                }
            }

            // Lọc theo khoảng thời gian
            if (startDate.HasValue)
            {
                khuyenMaiQuery = khuyenMaiQuery.Where(km => km.ngay_bat_dau >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                khuyenMaiQuery = khuyenMaiQuery.Where(km => km.ngay_ket_thuc <= endDate.Value);
            }

            // Sắp xếp theo tiêu chí được chọn
            switch (sortOrder)
            {
                case "name_asc":
                    khuyenMaiQuery = khuyenMaiQuery.OrderBy(km => km.ten_khuyen_mai);
                    break;
                case "name_desc":
                    khuyenMaiQuery = khuyenMaiQuery.OrderByDescending(km => km.ten_khuyen_mai);
                    break;
                case "date_asc":
                    khuyenMaiQuery = khuyenMaiQuery.OrderBy(km => km.ngay_tao);
                    break;
                case "date_desc":
                    khuyenMaiQuery = khuyenMaiQuery.OrderByDescending(km => km.ngay_tao);
                    break;
                default:
                    khuyenMaiQuery = khuyenMaiQuery.OrderBy(km => km.ten_khuyen_mai);
                    break;
            }

            // Cập nhật giam_gia của sản phẩm dựa trên trạng thái khuyến mãi
            foreach (var khuyenMai in khuyenMaiQuery.ToList())
            {
                var sanPhamIds = db.chi_tiet_khuyen_mais
                    .Where(ct => ct.khuyen_mai_id == khuyenMai.khuyen_mai_id)
                    .Select(ct => ct.san_pham_id)
                    .ToList();

                foreach (var sanPhamId in sanPhamIds)
                {
                    var sanPham = db.san_phams.SingleOrDefault(sp => sp.san_pham_id == sanPhamId);
                    if (sanPham != null)
                    {
                        if (currentDate >= khuyenMai.ngay_bat_dau && currentDate <= khuyenMai.ngay_ket_thuc)
                        {
                            sanPham.giam_gia = khuyenMai.giam_gia; // Đang hoạt động
                        }
                        else
                        {
                            sanPham.giam_gia = null; // Chưa bắt đầu hoặc đã kết thúc
                        }
                    }
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SubmitChanges();

            return View(khuyenMaiQuery.ToList());
        }


        // GET: Ad_Services/Create - Hiển thị trang tạo khuyến mãi
        public ActionResult CreateKhuyenMai()
        {
            return View();
        }

        // POST: Ad_Services/Create - Xử lý khi gửi form tạo khuyến mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateKhuyenMai(khuyen_mai KhuyenMai)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra điều kiện ngày hợp lệ
                if (KhuyenMai.ngay_bat_dau > KhuyenMai.ngay_ket_thuc)
                {
                    ModelState.AddModelError("ngay_ket_thuc", "Ngày kết thúc không thể nhỏ hơn ngày bắt đầu.");
                    return View(KhuyenMai);
                }

                // Gán ngày tạo cho khuyến mãi
                KhuyenMai.ngay_tao = DateTime.Now;

                // Kiểm tra xem tên khuyến mãi có bị trùng hay không
                var existingPromo = db.khuyen_mais.FirstOrDefault(km => km.ten_khuyen_mai == KhuyenMai.ten_khuyen_mai);
                if (existingPromo != null)
                {
                    ModelState.AddModelError("ten_khuyen_mai", "Khuyến mãi này đã tồn tại.");
                    return View(KhuyenMai);
                }

                // Thêm vào cơ sở dữ liệu
                db.khuyen_mais.InsertOnSubmit(KhuyenMai);
                db.SubmitChanges();

                // Chuyển hướng về danh sách khuyến mãi
                return RedirectToAction("KhuyenMaiSP");
            }

            // Nếu ModelState không hợp lệ, trả về lại form
            return View(KhuyenMai);
        }


        // GET: Ad_Services/Edit - Hiển thị form chỉnh sửa khuyến mãi
        public ActionResult EditKhuyenMai(int id)
        {
            var khuyenMai = db.khuyen_mais.FirstOrDefault(km => km.khuyen_mai_id == id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }
            return View(khuyenMai);
        }

        // POST: Ad_Services/Edit - Xử lý khi lưu chỉnh sửa khuyến mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKhuyenMai(int id, khuyen_mai updatedKhuyenMai)
        {
            var khuyenMai = db.khuyen_mais.FirstOrDefault(km => km.khuyen_mai_id == id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin khuyến mãi
                khuyenMai.ten_khuyen_mai = updatedKhuyenMai.ten_khuyen_mai;
                khuyenMai.mo_ta = updatedKhuyenMai.mo_ta;
                khuyenMai.ngay_bat_dau = updatedKhuyenMai.ngay_bat_dau;
                khuyenMai.ngay_ket_thuc = updatedKhuyenMai.ngay_ket_thuc;
                khuyenMai.giam_gia = updatedKhuyenMai.giam_gia;

                db.SubmitChanges();

                return RedirectToAction("KhuyenMaiSP");
            }
            return View(updatedKhuyenMai);
        }


        // GET: Ad_Services/Delete/5
        public ActionResult DeleteKhuyenMai(int id)
        {
            var khuyenMai = db.khuyen_mais.FirstOrDefault(km => km.khuyen_mai_id == id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            return View(khuyenMai);  // Hiển thị thông tin khuyến mãi để người dùng xác nhận xóa
        }

        // POST: Ad_Services/Delete/5
        [HttpPost, ActionName("DeleteKhuyenMai")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var khuyenMai = db.khuyen_mais.FirstOrDefault(km => km.khuyen_mai_id == id);
            if (khuyenMai != null)
            {
                // Xóa tất cả các chi tiết khuyến mãi liên quan đến khuyến mãi này
                var chiTietKhuyenMais = db.chi_tiet_khuyen_mais.Where(ctkm => ctkm.khuyen_mai_id == id);
                db.chi_tiet_khuyen_mais.DeleteAllOnSubmit(chiTietKhuyenMais);

                // Xóa khuyến mãi
                db.khuyen_mais.DeleteOnSubmit(khuyenMai);

                db.SubmitChanges();
            }

            return RedirectToAction("KhuyenMaiSP");
        }



        // Hiển thị danh sách sản phẩm trong khuyến mãi
        public ActionResult SanPhamKhuyenMai(int khuyenMaiId)
        {
            var sanPhamsKhuyenMai = new List<dynamic>();

            foreach (var ctkm in db.chi_tiet_khuyen_mais.Where(ctkm => ctkm.khuyen_mai_id == khuyenMaiId))
            {
                dynamic expando = new ExpandoObject();
                expando.san_pham_id = ctkm.san_pham_id;
                expando.khuyen_mai_id = ctkm.khuyen_mai_id;
                expando.ten_san_pham = ctkm.san_pham.ten_san_pham;
                expando.giam_gia = ctkm.khuyen_mai.giam_gia;
                sanPhamsKhuyenMai.Add(expando);
            }

            ViewBag.KhuyenMaiId = khuyenMaiId;
            return View(sanPhamsKhuyenMai);
        }

        public ActionResult ThemSanPhamVaoKhuyenMai(int khuyenMaiId)
        {
            // Logic xử lý lấy danh sách sản phẩm cho khuyến mãi này
            var khuyenMai = db.khuyen_mais.FirstOrDefault(km => km.khuyen_mai_id == khuyenMaiId);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách sản phẩm chưa thuộc khuyến mãi trùng thời gian
            var sanPhams = db.san_phams
                .Where(sp => !db.chi_tiet_khuyen_mais.Any(ctkm =>
                    ctkm.san_pham_id == sp.san_pham_id &&
                    db.khuyen_mais.Any(km =>
                        km.khuyen_mai_id == ctkm.khuyen_mai_id &&
                        ((khuyenMai.ngay_bat_dau >= km.ngay_bat_dau && khuyenMai.ngay_bat_dau <= km.ngay_ket_thuc) ||
                         (khuyenMai.ngay_ket_thuc >= km.ngay_bat_dau && khuyenMai.ngay_ket_thuc <= km.ngay_ket_thuc))
                    )
                ))
                .ToList();

            ViewBag.KhuyenMai = khuyenMai;
            return View(sanPhams); // Trả về view với danh sách sản phẩm để chọn thêm vào khuyến mãi
        }


        [HttpPost]
        public ActionResult LuuSanPhamVaoKhuyenMai(int khuyenMaiId, int[] sanPhamsChon)
        {
            var khuyenMai = db.khuyen_mais.FirstOrDefault(km => km.khuyen_mai_id == khuyenMaiId);
            if (khuyenMai == null || sanPhamsChon == null)
            {
                return HttpNotFound();
            }

            var currentDate = DateTime.Now;
            foreach (var sanPhamId in sanPhamsChon)
            {
                // Kiểm tra nếu sản phẩm đã có khuyến mãi trùng thời gian
                var hasConflictingPromotion = db.chi_tiet_khuyen_mais.Any(ctkm =>
                    ctkm.san_pham_id == sanPhamId &&
                    db.khuyen_mais.Any(km =>
                        km.khuyen_mai_id == ctkm.khuyen_mai_id &&
                        ((khuyenMai.ngay_bat_dau >= km.ngay_bat_dau && khuyenMai.ngay_bat_dau <= km.ngay_ket_thuc) ||
                         (khuyenMai.ngay_ket_thuc >= km.ngay_bat_dau && khuyenMai.ngay_ket_thuc <= km.ngay_ket_thuc))
                    )
                );

                if (!hasConflictingPromotion)
                {
                    // Thêm sản phẩm vào chi tiết khuyến mãi
                    var chiTietKhuyenMai = new chi_tiet_khuyen_mai
                    {
                        khuyen_mai_id = khuyenMaiId,
                        san_pham_id = sanPhamId
                    };
                    db.chi_tiet_khuyen_mais.InsertOnSubmit(chiTietKhuyenMai);
                }
            }

            db.SubmitChanges();
            // Redirect về trang danh sách khuyến mãi
            return RedirectToAction("SanPhamKhuyenMai", new { khuyenMaiId = khuyenMaiId });
        }


        // Hiển thị danh sách sản phẩm trong khuyến mãi với giá khuyến mãi
        public ActionResult ChiTietSanPhamKhuyenMai(int khuyenMaiId, int sanPhamId)
        {
            var sanPhamsKhuyenMai = new List<dynamic>();

            // Tìm thông tin chi tiết khuyến mãi cho sản phẩm và khuyến mãi cụ thể
            var ctkm = db.chi_tiet_khuyen_mais.FirstOrDefault(ct => ct.khuyen_mai_id == khuyenMaiId && ct.san_pham_id == sanPhamId);

            if (ctkm != null)
            {
                // Lấy thông tin sản phẩm từ database
                var sanPham = db.san_phams.FirstOrDefault(sp => sp.san_pham_id == sanPhamId);

                if (sanPham != null)
                {
                    // Lấy tất cả các biến thể của sản phẩm
                    var chiTietSanPham = db.chi_tiet_san_phams.Where(ctsp => ctsp.san_pham_id == sanPham.san_pham_id).ToList();

                    // Lặp qua từng biến thể của sản phẩm
                    foreach (var chiTiet in chiTietSanPham)
                    {
                        // Tạo đối tượng dynamic để lưu thông tin của sản phẩm và biến thể
                        dynamic expando = new ExpandoObject();

                        expando.san_pham_id = sanPham.san_pham_id;
                        expando.ten_san_pham = sanPham.ten_san_pham;
                        expando.giam_gia = sanPham.giam_gia;
                        expando.gia = chiTiet.gia;  // Giá của biến thể sản phẩm
                        expando.gia_khuyen_mai = chiTiet.gia_khuyen_mai > 0
                                                 ? chiTiet.gia_khuyen_mai
                                                 : chiTiet.gia - (chiTiet.gia * sanPham.giam_gia / 100); // Tính giá khuyến mãi

                        // Thêm thông tin của biến thể sản phẩm
                        expando.kich_thuoc = chiTiet.kich_thuoc;
                        expando.do_day = chiTiet.do_day;
                        expando.mau_sac = chiTiet.mau_sac;
                        expando.chat_lieu = chiTiet.chat_lieu;

                        // Thêm thông tin biến thể vào danh sách kết quả
                        sanPhamsKhuyenMai.Add(expando);
                    }
                }
            }

            ViewBag.KhuyenMaiId = khuyenMaiId;
            return View(sanPhamsKhuyenMai);
        }


        // Xóa sản phẩm khỏi khuyến mãi
        public ActionResult XoaSanPhamKhoiKhuyenMai(int sanPhamId, int khuyenMaiId)
        {
            // Tìm kiếm chi tiết khuyến mãi với san_pham_id và khuyen_mai_id
            var chiTietKhuyenMai = db.chi_tiet_khuyen_mais
                .FirstOrDefault(ct => ct.san_pham_id == sanPhamId && ct.khuyen_mai_id == khuyenMaiId);

            if (chiTietKhuyenMai != null)
            {
                // Đánh dấu chi tiết khuyến mãi này để xóa
                db.chi_tiet_khuyen_mais.DeleteOnSubmit(chiTietKhuyenMai);
                db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }

            return RedirectToAction("SanPhamKhuyenMai", new { khuyenMaiId });
        }

    }


}


