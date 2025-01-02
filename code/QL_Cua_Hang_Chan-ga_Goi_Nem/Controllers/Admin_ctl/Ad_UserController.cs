using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.Admin_ctl
{
    public class Ad_UserController : Controller
    {
        DataDataContext db = new DataDataContext();

        // Hiển thị danh sách người dùng với chức năng lọc và tìm kiếm
        public ActionResult HienThiNguoiDung(string searchName, string roleFilter, string nameFilter, string sortOrder)
        {
            // Lấy danh sách người dùng từ cơ sở dữ liệu
            var users = db.nguoi_dungs.AsQueryable();

            // Áp dụng tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                users = users.Where(u => u.ten_dang_nhap.Contains(searchName));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => u.vai_tro == roleFilter);
            }

            // Lọc theo tên người dùng trong dropdown
            if (!string.IsNullOrEmpty(nameFilter))
            {
                users = users.Where(u => u.ten_dang_nhap == nameFilter);
            }

            // Sắp xếp theo tên đăng nhập
            ViewBag.NameSortParam = sortOrder == "NameAsc" ? "NameDesc" : "NameAsc";
            switch (sortOrder)
            {
                case "NameDesc":
                    users = users.OrderByDescending(u => u.ten_dang_nhap);
                    break;
                default:
                    users = users.OrderBy(u => u.ten_dang_nhap);
                    break;
            }

            return View(users.ToList());
        }

        // Xóa nhiều người dùng
        [HttpPost]
        public ActionResult DeleteSelected(int[] selectedIds)
        {
            if (selectedIds != null)
            {
                var usersToDelete = db.nguoi_dungs.Where(u => selectedIds.Contains(u.nguoi_dung_id)).ToList();
                db.nguoi_dungs.DeleteAllOnSubmit(usersToDelete);
                db.SubmitChanges();
            }
            return RedirectToAction("HienThiNguoiDung");
        }

        // Tạo người dùng mới (GET)
        public ActionResult Create()
        {
            return View();
        }

        // Tạo người dùng mới (POST)
        [HttpPost]
        public ActionResult Create(nguoi_dung model)
        {
            if (ModelState.IsValid)
            {
                model.vai_tro = "admin";
                model.ngay_tao = DateTime.Now;

                db.nguoi_dungs.InsertOnSubmit(model);
                db.SubmitChanges();
                return RedirectToAction("HienThiNguoiDung");
            }
            return View(model);
        }

        // Xóa người dùng (GET)
        public ActionResult Delete(int id)
        {
            var nguoiDung = db.nguoi_dungs.FirstOrDefault(x => x.nguoi_dung_id == id);
            if (nguoiDung == null)
            {
                return HttpNotFound();
            }
            return View(nguoiDung);
        }

        // Xóa người dùng (POST)
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var nguoiDung = db.nguoi_dungs.FirstOrDefault(x => x.nguoi_dung_id == id);
            if (nguoiDung != null)
            {
                db.nguoi_dungs.DeleteOnSubmit(nguoiDung);
                db.SubmitChanges();
            }
            return RedirectToAction("HienThiNguoiDung");
        }
    }
}
