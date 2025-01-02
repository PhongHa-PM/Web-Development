using System.Linq;
using System.Web.Mvc;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models.Admin_Md;
using System;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.Admin_ctl
{
    public class Ad_ProductController : Controller
    {
        // Kết nối với database
        DataDataContext db = new DataDataContext();

        // Hiển thị danh sách sản phẩm
        public ActionResult HienThiSP(string loaiSanPham, string thuongHieu, string searchTerm)
        {
            var sanPhams = db.san_phams.AsQueryable();

            // Lọc theo loại sản phẩm nếu có
            if (!string.IsNullOrEmpty(loaiSanPham))
            {
                sanPhams = sanPhams.Where(sp => sp.loai_san_pham == loaiSanPham);
            }

            // Lọc theo thương hiệu nếu có
            if (!string.IsNullOrEmpty(thuongHieu))
            {
                sanPhams = sanPhams.Where(sp => sp.thuong_hieu == thuongHieu);
            }

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sanPhams = sanPhams.Where(sp => sp.ten_san_pham.Contains(searchTerm));
            }

            // Truyền danh sách loại sản phẩm và thương hiệu đến ViewBag
            ViewBag.LoaiSanPham = db.san_phams.Select(sp => sp.loai_san_pham).Distinct().ToList();
            ViewBag.ThuongHieu = db.san_phams.Select(sp => sp.thuong_hieu).Distinct().ToList();
            ViewBag.SearchTerm = searchTerm;

            return View(sanPhams.ToList());
        }

        [HttpPost]
        public ActionResult XoaNhieuSP(int[] sanPhamIds)
        {
            if (sanPhamIds != null && sanPhamIds.Length > 0)
            {
                // Tìm sản phẩm cần xóa
                var productsToDelete = db.san_phams.Where(p => sanPhamIds.Contains(p.san_pham_id)).ToList();

                foreach (var product in productsToDelete)
                {
                    // Xóa các chi tiết sản phẩm liên quan
                    var chiTietSanPhams = db.chi_tiet_san_phams.Where(ct => ct.san_pham_id == product.san_pham_id).ToList();
                    foreach (var chiTiet in chiTietSanPhams)
                    {
                        db.chi_tiet_san_phams.DeleteOnSubmit(chiTiet);
                    }

                    //// Xóa hình ảnh chính nếu có
                    //if (!string.IsNullOrEmpty(product.hinh_anh_chinh))
                    //{
                    //    var oldPath = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), product.hinh_anh_chinh);
                    //    if (System.IO.File.Exists(oldPath))
                    //    {
                    //        System.IO.File.Delete(oldPath);
                    //    }
                    //}

                    //// Xóa các hình ảnh trong danh sách hình ảnh nếu có
                    //if (!string.IsNullOrEmpty(product.danh_sach_hinh_anh))
                    //{
                    //    var images = product.danh_sach_hinh_anh.Split(',');
                    //    foreach (var img in images)
                    //    {
                    //        var imagePath = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), img.Trim());
                    //        if (System.IO.File.Exists(imagePath))
                    //        {
                    //            System.IO.File.Delete(imagePath);
                    //        }
                    //    }
                    //}

                    // Xóa sản phẩm chính khỏi cơ sở dữ liệu
                    db.san_phams.DeleteOnSubmit(product);
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();
            }

            // Chuyển hướng người dùng về trang danh sách sản phẩm
            return RedirectToAction("HienThiSP");
        }



        // Hiển thị form thêm sản phẩm mới
        public ActionResult Create()
        {
            // Lấy danh sách loại sản phẩm và thương hiệu từ database
            ViewBag.LoaiSanPham = new SelectList(db.san_phams.Select(sp => sp.loai_san_pham).Distinct().ToList());
            ViewBag.ThuongHieu = new SelectList(db.san_phams.Select(sp => sp.thuong_hieu).Distinct().ToList());
            return View();
        }


        // Xử lý thêm sản phẩm mới
        [HttpPost]
        public ActionResult Create(san_pham model, HttpPostedFileBase hinh_anh_chinh, IEnumerable<HttpPostedFileBase> danh_sach_hinh_anh, string new_loai_san_pham, string new_thuong_hieu)
        {
            if (ModelState.IsValid)
            {
                // Nếu người dùng nhập loại sản phẩm mới
                if (!string.IsNullOrEmpty(new_loai_san_pham))
                {
                    model.loai_san_pham = new_loai_san_pham; // Gán loại sản phẩm mới
                }

                // Nếu người dùng nhập thương hiệu mới
                if (!string.IsNullOrEmpty(new_thuong_hieu))
                {
                    model.thuong_hieu = new_thuong_hieu; // Gán thương hiệu mới
                }
                // Xử lý hình ảnh chính
                if (hinh_anh_chinh != null && hinh_anh_chinh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(hinh_anh_chinh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), fileName);
                    hinh_anh_chinh.SaveAs(path);
                    model.hinh_anh_chinh = fileName;
                }

                // Xử lý danh sách hình ảnh
                var imageList = new List<string>();
                if (danh_sach_hinh_anh != null)
                {
                    foreach (var file in danh_sach_hinh_anh)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), fileName);
                            file.SaveAs(path);
                            imageList.Add(fileName);
                        }
                    }
                    model.danh_sach_hinh_anh = string.Join(",", imageList);
                }

                // Thêm sản phẩm vào database
                db.san_phams.InsertOnSubmit(model);
                db.SubmitChanges();

                return RedirectToAction("HienThiSP");
            }
            return View(model);
        }

        // Hiển thị form sửa sản phẩm
        public ActionResult Edit(int id)
        {
            var product = db.san_phams.SingleOrDefault(p => p.san_pham_id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // Xử lý sửa sản phẩm
        [HttpPost]
        public ActionResult Edit(san_pham model, HttpPostedFileBase hinh_anh_chinh, IEnumerable<HttpPostedFileBase> danh_sach_hinh_anh)
        {
            if (ModelState.IsValid)
            {
                var product = db.san_phams.SingleOrDefault(p => p.san_pham_id == model.san_pham_id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin sản phẩm
                product.ten_san_pham = model.ten_san_pham;
                product.loai_san_pham = model.loai_san_pham;
                product.mo_ta_chung = model.mo_ta_chung;
                product.thuong_hieu = model.thuong_hieu;

                // Xử lý hình ảnh chính
                if (hinh_anh_chinh != null && hinh_anh_chinh.ContentLength > 0)
                {
                    //// Xóa hình cũ nếu có
                    //if (!string.IsNullOrEmpty(product.hinh_anh_chinh))
                    //{
                    //    var oldPath = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), product.hinh_anh_chinh);
                    //    if (System.IO.File.Exists(oldPath))
                    //    {
                    //        System.IO.File.Delete(oldPath);
                    //    }
                    //}

                    var fileName = Path.GetFileName(hinh_anh_chinh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), fileName);
                    hinh_anh_chinh.SaveAs(path);
                    product.hinh_anh_chinh = fileName;
                }

                // Xử lý danh sách hình ảnh
                var imageList = new List<string>();
                if (danh_sach_hinh_anh != null)
                {
                    foreach (var file in danh_sach_hinh_anh)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), fileName);
                            file.SaveAs(path);
                            imageList.Add(fileName);
                        }
                    }
                    product.danh_sach_hinh_anh = string.Join(",", imageList);
                }

                // Lưu thay đổi
                db.SubmitChanges();
                return RedirectToAction("HienThiSP");
            }
            return View(model);
        }

        // Xóa sản phẩm
        public ActionResult Delete(int id)
        {
            var product = db.san_phams.SingleOrDefault(p => p.san_pham_id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var sanPham = db.san_phams.FirstOrDefault(sp => sp.san_pham_id == id);
            if (sanPham != null)
            {
                // Xóa các chi tiết sản phẩm liên quan đến sản phẩm này
                var chiTietSanPhams = db.chi_tiet_san_phams.Where(ctsp => ctsp.san_pham_id == id).ToList();
                db.chi_tiet_san_phams.DeleteAllOnSubmit(chiTietSanPhams);

                //// Xóa hình ảnh chính và danh sách hình ảnh khỏi thư mục
                //if (!string.IsNullOrEmpty(sanPham.hinh_anh_chinh))
                //{
                //    var hinhAnhChinhPath = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), sanPham.hinh_anh_chinh);
                //    if (System.IO.File.Exists(hinhAnhChinhPath))
                //    {
                //        System.IO.File.Delete(hinhAnhChinhPath);
                //    }
                //}

                //if (!string.IsNullOrEmpty(sanPham.danh_sach_hinh_anh))
                //{
                //    var images = sanPham.danh_sach_hinh_anh.Split(',');
                //    foreach (var image in images)
                //    {
                //        var imagePath = Path.Combine(Server.MapPath("~/Content/Hinh_Anh"), image.Trim());
                //        if (System.IO.File.Exists(imagePath))
                //        {
                //            System.IO.File.Delete(imagePath);
                //        }
                //    }
                //}

                // Xóa sản phẩm khỏi cơ sở dữ liệu
                db.san_phams.DeleteOnSubmit(sanPham);
                db.SubmitChanges();
            }
            return RedirectToAction("HienThiSP");
        }

        // Hiển thị chi tiết sản phẩm theo san_pham_id
        public ActionResult HienThiChiTietSanPham(int san_pham_id)
        {

            ViewBag.SanPhamId = san_pham_id;
            var lstDetailsSP = db.chi_tiet_san_phams
                .Where(ct => ct.san_pham_id == san_pham_id)
                .ToList();
            return View(lstDetailsSP);
        }

        // Hiển thị form thêm chi tiết sản phẩm
        public ActionResult CreateCTSP(int san_pham_id)
        {
            // Lấy danh sách dữ liệu từ database
            ViewBag.SanPhamId = san_pham_id;
            ViewBag.KichThuocList = db.chi_tiet_san_phams.Select(ct => ct.kich_thuoc).Distinct().ToList();
            ViewBag.MauSacList = db.chi_tiet_san_phams.Select(ct => ct.mau_sac).Distinct().ToList();
            ViewBag.ChatLieuList = db.chi_tiet_san_phams.Select(ct => ct.chat_lieu).Distinct().ToList();
            return View();
        }

        [HttpPost]
        public ActionResult CreateCTSP(chi_tiet_san_pham model, int san_pham_id, string kich_thuoc_new, string mau_sac_new, string chat_lieu_new)
        {
            if (ModelState.IsValid)
            {
                // Sử dụng giá trị mới nếu người dùng nhập
                model.kich_thuoc = !string.IsNullOrEmpty(kich_thuoc_new) ? kich_thuoc_new : model.kich_thuoc;
                model.mau_sac = !string.IsNullOrEmpty(mau_sac_new) ? mau_sac_new : model.mau_sac;
                model.chat_lieu = !string.IsNullOrEmpty(chat_lieu_new) ? chat_lieu_new : model.chat_lieu;

                // Gán san_pham_id cho model
                model.san_pham_id = san_pham_id;

                // Chèn chi tiết sản phẩm vào CSDL
                db.chi_tiet_san_phams.InsertOnSubmit(model);
                db.SubmitChanges();

                return RedirectToAction("HienThiChiTietSanPham", new { san_pham_id = san_pham_id });
            }

            // Truyền lại danh sách vào ViewBag nếu có lỗi
            ViewBag.SanPhamId = san_pham_id;
            ViewBag.KichThuocList = db.chi_tiet_san_phams.Select(ct => ct.kich_thuoc).Distinct().ToList();
            ViewBag.DoDayList = db.chi_tiet_san_phams.Select(ct => ct.do_day).Distinct().ToList();
            ViewBag.MauSacList = db.chi_tiet_san_phams.Select(ct => ct.mau_sac).Distinct().ToList();
            ViewBag.ChatLieuList = db.chi_tiet_san_phams.Select(ct => ct.chat_lieu).Distinct().ToList();

            return View(model);
        }

        public ActionResult EditCTSP(int id)
        {
            // Tìm chi tiết sản phẩm theo ID
            var chiTietSanPham = db.chi_tiet_san_phams.SingleOrDefault(ct => ct.chi_tiet_id == id);
            if (chiTietSanPham == null)
            {
                return HttpNotFound();  // Nếu không tìm thấy chi tiết sản phẩm
            }

            return View(chiTietSanPham);  // Trả về view với model chi tiết sản phẩm
        }


        // Xử lý cập nhật chi tiết sản phẩm
        [HttpPost]
        public ActionResult EditCTSP(chi_tiet_san_pham model)
        {
            if (ModelState.IsValid)
            {
                var chiTietSanPham = db.chi_tiet_san_phams.SingleOrDefault(ct => ct.chi_tiet_id == model.chi_tiet_id);
                if (chiTietSanPham == null)
                {
                    return HttpNotFound();
                }

                chiTietSanPham.kich_thuoc = model.kich_thuoc;
                chiTietSanPham.do_day = model.do_day;
                chiTietSanPham.mau_sac = model.mau_sac;
                chiTietSanPham.chat_lieu = model.chat_lieu;
                chiTietSanPham.gia = model.gia;
                chiTietSanPham.gia_khuyen_mai = model.gia_khuyen_mai;
                chiTietSanPham.so_luong_ton = model.so_luong_ton;

                db.SubmitChanges();
                return RedirectToAction("HienThiChiTietSanPham", new { san_pham_id = model.san_pham_id });
            }
            return View(model);
        }

        public ActionResult DeleteCTSP(int id)
        {
            try
            {
                // Lấy chi tiết sản phẩm theo id
                var chiTietSanPham = db.chi_tiet_san_phams.FirstOrDefault(ct => ct.chi_tiet_id == id);

                if (chiTietSanPham == null)
                {
                    // Nếu không tìm thấy, hiển thị thông báo lỗi
                    return HttpNotFound("Chi tiết sản phẩm không tồn tại.");
                }

                // Xóa chi tiết sản phẩm
                db.chi_tiet_san_phams.DeleteOnSubmit(chiTietSanPham);
                db.SubmitChanges();

                // Chuyển hướng về trang danh sách chi tiết sản phẩm
                return RedirectToAction("HienThiChiTietSanPham", new { san_pham_id = chiTietSanPham.san_pham_id });
            }
            catch (Exception ex)
            {
                // Nếu xảy ra lỗi, hiển thị thông báo lỗi
                ViewBag.ErrorMessage = "Không thể xóa chi tiết sản phẩm. Lỗi: " + ex.Message;
                return RedirectToAction("HienThiChiTietSanPham");
            }
        }





    }
}
