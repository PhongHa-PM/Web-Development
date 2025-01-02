using QL_Cua_Hang_Chan_ga_Goi_Nem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_Cua_Hang_Chan_ga_Goi_Nem.Models.User_Md;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Controllers.User_ctl
{
    public class Us_ProductController : Controller
    {
        // Khởi tạo DataContext để kết nối cơ sở dữ liệu
        DataDataContext db = new DataDataContext();

        // Action để hiển thị sản phẩm nệm
        public ActionResult HienThiSanPhamNem(string[] LoaiSanPham, string[] KichThuoc, int[] DoDay, string[] KhoangGia)
        {
            // Lấy danh sách loại sản phẩm có chứa chữ "Nệm"
            var danhSachLoaiSanPham = db.san_phams
                .Where(sp => sp.loai_san_pham.Contains("Nệm"))
                .Select(sp => sp.loai_san_pham)
                .Distinct()
                .ToList();

            var danhSachKichThuoc = db.chi_tiet_san_phams
                .Where(ctsp => ctsp.san_pham.ten_san_pham.Contains("nệm"))
                .Select(ctsp => ctsp.kich_thuoc)
                .Distinct()
                .ToList();

            // Lấy danh sách độ dày
            var danhSachDoDay = db.chi_tiet_san_phams
                .Where(ctsp => ctsp.san_pham.ten_san_pham.Contains("nệm"))
                .Select(ctsp => ctsp.do_day)
                .Distinct()
                .ToList();

            // Áp dụng bộ lọc
            var query = db.san_phams.AsQueryable();

            // Lọc theo loại sản phẩm
            if (LoaiSanPham != null && LoaiSanPham.Length > 0)
            {
                query = query.Where(sp => LoaiSanPham.Contains(sp.loai_san_pham));
            }

            // Lọc theo kích thước
            if (KichThuoc != null && KichThuoc.Length > 0)
            {
                query = query.Where(sp => db.chi_tiet_san_phams
                                              .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                              .Any(ctsp => KichThuoc.Contains(ctsp.kich_thuoc)));
            }

            // Lọc theo khoảng giá
            if (KhoangGia != null && KhoangGia.Length > 0)
            {
                foreach (var gia in KhoangGia)
                {
                    switch (gia)
                    {
                        case "1":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) < 5000000);
                            break;
                        case "2":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) >= 5000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 10000000);
                            break;
                        case "3":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) >= 10000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 15000000);
                            break;
                        case "4":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) >= 15000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 30000000);
                            break;
                        case "5":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) > 30000000);
                            break;
                    }
                }
            }

            // Lọc ra các sản phẩm có loại là "nệm"
            var sanPhams = (from sp in query
                            where sp.ten_san_pham.Contains("Nệm") || sp.loai_san_pham.Contains("nệm")
                            select new
                            {
                                sp.san_pham_id,
                                sp.ten_san_pham,
                                sp.giam_gia,
                                sp.hinh_anh_chinh,
                                sp.danh_sach_hinh_anh,
                                sp.thuong_hieu,
                                // Lấy giá và giá khuyến mãi thấp nhất từ chi_tiet_san_pham
                                Gia = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .Min(ctsp => ctsp.gia),
                                GiaKhuyenMai = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .Min(ctsp => ctsp.gia_khuyen_mai),
                                // Đếm số lượng kích thước khác nhau của sản phẩm
                                SoLuongKichThuoc = db.chi_tiet_san_phams
                                    .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                    .Select(ctsp => ctsp.kich_thuoc)
                                    .Distinct()
                                    .Count(),
                                // Tính toán đánh giá trung bình của sản phẩm
                                DanhGiaTrungBinh = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Average(dg => dg.so_sao),
                                // Đếm số lượng đánh giá của sản phẩm
                                SoLuongDanhGia = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Count()
                            }).ToList();

            // Chuyển dữ liệu từ anonymous object sang ViewModel
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
            }).ToList();

            // Truyền thêm danh sách độ dày sang View
            ViewBag.DanhSachLoaiSanPham = danhSachLoaiSanPham;
            ViewBag.DanhSachKichThuoc = danhSachKichThuoc;
            ViewBag.DanhSachDoDay = danhSachDoDay;

            return View(model);
        }

        // Action để hiển thị sản phẩm Chăn Ga
        public ActionResult HienThiSanPhamChanGa(string[] LoaiSanPham, string[] KichThuoc, string[] MauSac, string[] KhoangGia)
        {
            // Lấy danh sách loại sản phẩm có chứa chữ "Chăn Ga"
            var danhSachLoaiSanPham = db.san_phams
                .Where(sp => sp.loai_san_pham.Contains("Chăn") || sp.loai_san_pham.Contains("Ga"))
                .Select(sp => sp.loai_san_pham)
                .Distinct()
                .ToList();

            // Lấy danh sách kích thước
            var danhSachKichThuoc = db.chi_tiet_san_phams
                .Where(ctsp => ctsp.san_pham.ten_san_pham.Contains("chăn")|| ctsp.san_pham.ten_san_pham.Contains("ga"))
                .Select(ctsp => ctsp.kich_thuoc)
                .Distinct()
                .ToList();

            // Lấy danh sách màu sắc
            var danhSachMauSac = db.chi_tiet_san_phams
                .Select(ctsp => ctsp.mau_sac)
                .Distinct()
                .ToList();

            // Áp dụng bộ lọc
            var query = db.san_phams.AsQueryable();

            // Lọc theo loại sản phẩm
            if (LoaiSanPham != null && LoaiSanPham.Length > 0)
            {
                query = query.Where(sp => LoaiSanPham.Contains(sp.loai_san_pham));
            }

            // Lọc theo kích thước
            if (KichThuoc != null && KichThuoc.Length > 0)
            {
                query = query.Where(sp => db.chi_tiet_san_phams
                                              .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                              .Any(ctsp => KichThuoc.Contains(ctsp.kich_thuoc)));
            }

            // Lọc theo màu sắc
            if (MauSac != null && MauSac.Length > 0)
            {
                query = query.Where(sp => db.chi_tiet_san_phams
                                              .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                              .Any(ctsp => MauSac.Contains(ctsp.mau_sac)));
            }

            // Lọc theo khoảng giá
            if (KhoangGia != null && KhoangGia.Length > 0)
            {
                foreach (var gia in KhoangGia)
                {
                    switch (gia)
                    {
                        case "1": // Dưới 1 triệu
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) < 1000000);
                            break;
                        case "2": // 1 - 3 triệu
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) >= 1000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 3000000);
                            break;
                        case "3": // 3 - 5 triệu
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) > 3000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 5000000);
                            break;
                        case "4": // 5 - 10 triệu
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) > 5000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 10000000);
                            break;
                        case "5": // Trên 10 triệu
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) > 10000000);
                            break;
                    }
                }
            }

            // Lọc ra các sản phẩm có loại là "Chăn Ga"
            var sanPhams = (from sp in query
                            where sp.ten_san_pham.Contains("Chăn") || sp.ten_san_pham.Contains("Ga") || sp.loai_san_pham.Contains("chăn") || sp.loai_san_pham.Contains("ga")
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
                                DanhGiaTrungBinh = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Average(dg => dg.so_sao),
                                SoLuongDanhGia = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Count()
                            }).ToList();

            // Chuyển dữ liệu từ anonymous object sang ViewModel
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
            }).ToList();

            // Truyền danh sách loại sản phẩm, kích thước và độ dày sang View
            ViewBag.DanhSachLoaiSanPham = danhSachLoaiSanPham;
            ViewBag.DanhSachKichThuoc = danhSachKichThuoc;
            ViewBag.DanhSachMauSac = danhSachMauSac;

            return View(model);
        }

        // Action để hiển thị sản phẩm Gối
        public ActionResult HienThiSanPhamGoi(string[] LoaiSanPham, string[] KichThuoc, string[] ThuongHieu, string[] KhoangGia)
        {
            // Lấy danh sách loại sản phẩm có chứa chữ "Gối"
            var danhSachLoaiSanPham = db.san_phams
                .Where(sp => sp.loai_san_pham.Contains("Gối"))
                .Select(sp => sp.loai_san_pham)
                .Distinct()
                .ToList();

            // Lấy danh sách kích thước
            var danhSachKichThuoc = db.chi_tiet_san_phams
                .Where(ctsp => ctsp.san_pham.ten_san_pham.Contains("gối"))
                .Select(ctsp => ctsp.kich_thuoc)
                .Distinct()
                .ToList();

            // Lấy danh sách Thương hiệu
            var danhSachThuongHieu = db.san_phams
                .Where(sp => sp.ten_san_pham.Contains("gối"))
                .Select(sp => sp.thuong_hieu)
                .Distinct()
                .ToList();

            // Áp dụng bộ lọc
            var query = db.san_phams.AsQueryable();

            // Lọc theo loại sản phẩm
            if (LoaiSanPham != null && LoaiSanPham.Length > 0)
            {
                query = query.Where(sp => LoaiSanPham.Contains(sp.loai_san_pham));
            }

            // Lọc theo kích thước
            if (KichThuoc != null && KichThuoc.Length > 0)
            {
                query = query.Where(sp => db.chi_tiet_san_phams
                                              .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                              .Any(ctsp => KichThuoc.Contains(ctsp.kich_thuoc)));
            }

            // Lọc theo thương hiệu
            if (ThuongHieu != null && ThuongHieu.Length > 0)
            {
                query = query.Where(sp => ThuongHieu.Contains(sp.thuong_hieu));
            }

            // Lọc theo khoảng giá
            if (KhoangGia != null && KhoangGia.Length > 0)
            {
                foreach (var gia in KhoangGia)
                {
                    switch (gia)
                    {
                        case "1":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) < 1000000);
                            break;
                        case "2":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) >= 1000000 && db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) <= 5000000);
                            break;
                        case "3":
                            query = query.Where(sp => db.chi_tiet_san_phams
                                                        .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                                        .Min(ctsp => ctsp.gia) > 5000000);
                            break;
                    }
                }
            }

            // Lọc ra các sản phẩm có loại là "Gối"
            var sanPhams = (from sp in query
                            where sp.ten_san_pham.Contains("Gối") || sp.loai_san_pham.Contains("gối")
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
                                DanhGiaTrungBinh = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Average(dg => dg.so_sao),
                                SoLuongDanhGia = db.danh_gias
                                    .Where(dg => dg.san_pham_id == sp.san_pham_id)
                                    .Count()
                            }).ToList();

            // Chuyển dữ liệu từ anonymous object sang ViewModel
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
            }).ToList();

            // Truyền danh sách loại sản phẩm, kích thước và độ dày sang View
            ViewBag.DanhSachLoaiSanPham = danhSachLoaiSanPham;
            ViewBag.DanhSachKichThuoc = danhSachKichThuoc;
            ViewBag.DanhSachThuongHieu = danhSachThuongHieu;

            return View(model);
        }

        public ActionResult ChiTietSanPham(int id, string kichThuoc = null, int? doDay = null, string MauSac = null)
        {
            // Lấy thông tin sản phẩm
            var sanPham = (from sp in db.san_phams
                           where sp.san_pham_id == id
                           select new
                           {
                               sp.san_pham_id,
                               sp.ten_san_pham,
                               sp.hinh_anh_chinh,
                               sp.thuong_hieu,
                               sp.mo_ta_chung,
                               sp.danh_sach_hinh_anh,
                               KichThuoc = db.chi_tiet_san_phams
                                   .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                   .Select(ctsp => ctsp.kich_thuoc)
                                   .Distinct()
                                   .ToList(),
                               MauSac = db.chi_tiet_san_phams
                                   .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                   .Select(ctsp => ctsp.mau_sac)
                                   .Distinct()
                                   .ToList(),
                               DoDay = db.chi_tiet_san_phams
                                   .Where(ctsp => ctsp.san_pham_id == sp.san_pham_id)
                                   .Select(ctsp => (int?)ctsp.do_day)
                                   .Distinct()
                                   .ToList(),
                               KhuyenMai = db.khuyen_mais
                                   .Where(km => db.chi_tiet_khuyen_mais
                                       .Any(ckm => ckm.san_pham_id == sp.san_pham_id && ckm.khuyen_mai_id == km.khuyen_mai_id))
                                   .OrderByDescending(km => km.ngay_bat_dau)
                                   .FirstOrDefault()
                           }).FirstOrDefault();

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Lọc thông tin trong chi tiết sản phẩm
            var chiTietSanPhams = db.chi_tiet_san_phams
                .Where(ctsp => ctsp.san_pham_id == id);

            if (!string.IsNullOrEmpty(kichThuoc))
            {
                chiTietSanPhams = chiTietSanPhams.Where(ctsp => ctsp.kich_thuoc == kichThuoc);
            }

            if (!string.IsNullOrEmpty(MauSac))
            {
                chiTietSanPhams = chiTietSanPhams.Where(ctsp => ctsp.mau_sac == MauSac);
            }

            if (doDay.HasValue)
            {
                chiTietSanPhams = chiTietSanPhams.Where(ctsp => ctsp.do_day == doDay.Value);
            }

            // Lấy thông tin chi tiết ID và giá khuyến mãi
            var chiTietSanPham = chiTietSanPhams
                .Select(ctsp => new
                {
                    ctsp.chi_tiet_id,
                    Gia = ctsp.gia,
                    SoLuongTon = ctsp.so_luong_ton,
                    GiaKhuyenMai = ctsp.gia_khuyen_mai
                })
                .FirstOrDefault();

            if (chiTietSanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin chi tiết sản phẩm theo yêu cầu.";
                return RedirectToAction("ChiTietSanPham", new { id });
            }

            // Cập nhật ViewModel
            var model = new ChiTietSanPhamViewModel
            {
                SanPhamId = sanPham.san_pham_id,
                TenSanPham = sanPham.ten_san_pham,
                MoTa = sanPham.mo_ta_chung,
                HinhAnh = sanPham.hinh_anh_chinh,
                Gia = (decimal)chiTietSanPham.Gia,
                GiaKhuyenMai = (decimal)chiTietSanPham.GiaKhuyenMai,
                ThuongHieu = sanPham.thuong_hieu,
                KichThuoc = chiTietSanPhams
                    .Select(ctsp => ctsp.kich_thuoc)
                    .Distinct()
                    .ToList(),
                MauSac = chiTietSanPhams
                    .Select(ctsp => ctsp.mau_sac)
                    .Distinct()
                    .ToList(),
                DoDay = chiTietSanPhams
                    .Select(ctsp => (int?)ctsp.do_day)
                    .Distinct()
                    .Where(d => d.HasValue)
                    .Select(d => d.Value)
                    .ToList(),
                DanhSachHinhAnh = sanPham.danh_sach_hinh_anh != null
                    ? sanPham.danh_sach_hinh_anh.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>(),
                ChiTietSanPhamId = chiTietSanPham.chi_tiet_id,
                SoLuongTon = chiTietSanPham.SoLuongTon,
                NgayBatDauKhuyenMai = sanPham.KhuyenMai != null ? sanPham.KhuyenMai.ngay_bat_dau : (DateTime?)null,
                NgayKetThucKhuyenMai = sanPham.KhuyenMai != null ? sanPham.KhuyenMai.ngay_ket_thuc : (DateTime?)null,
                KichThuocHienTai = kichThuoc,
                MauSacHienTai = MauSac,
                DoDayHienTai = doDay
            };

            return View(model);
        }



        public ActionResult GioHang()
        {
            // Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem> ?? new List<CartItem>();

            return View(gioHang);
        }

        [HttpPost]
        public ActionResult ThemVaoGio(int? chiTietSanPhamId, int soLuong = 1)
        {
            // Kiểm tra nếu ID là null
            if (!chiTietSanPhamId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để thêm vào giỏ hàng.";
                return RedirectToAction("GioHang");
            }

            // Lấy thông tin sản phẩm
            var chiTietSanPham = db.chi_tiet_san_phams.FirstOrDefault(ctsp => ctsp.chi_tiet_id == chiTietSanPhamId.Value);
            if (chiTietSanPham == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("ChiTietSanPham", new { id = chiTietSanPhamId });
            }

            // Kiểm tra số lượng tồn
            if (soLuong <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng sản phẩm phải lớn hơn 0.";
                return RedirectToAction("ChiTietSanPham", new { id = chiTietSanPhamId });
            }

            if (soLuong > chiTietSanPham.so_luong_ton)
            {
                TempData["ErrorMessage"] = $"Sản phẩm '{chiTietSanPham.san_pham.ten_san_pham}' chỉ còn {chiTietSanPham.so_luong_ton} sản phẩm trong kho.";
                return RedirectToAction("ChiTietSanPham", new { id = chiTietSanPhamId });
            }

            // Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem> ?? new List<CartItem>();

            // Kiểm tra sản phẩm đã tồn tại trong giỏ hàng
            var item = gioHang.FirstOrDefault(g => g.SanPhamId == chiTietSanPhamId.Value);
            if (item != null)
            {
                // Nếu tồn tại, kiểm tra tổng số lượng
                int tongSoLuong = item.SoLuong + soLuong;
                if (tongSoLuong > chiTietSanPham.so_luong_ton)
                {
                    TempData["ErrorMessage"] = $"Không thể thêm sản phẩm vào giỏ hàng. Tổng số lượng trong giỏ hàng và số lượng thêm ({tongSoLuong}) vượt quá số lượng tồn kho ({chiTietSanPham.so_luong_ton}).";
                    return RedirectToAction("GioHang");
                }

                // Cập nhật số lượng trong giỏ
                item.SoLuong = tongSoLuong;
                TempData["SuccessMessage"] = $"Đã cập nhật số lượng sản phẩm '{chiTietSanPham.san_pham.ten_san_pham}' trong giỏ hàng là {item.SoLuong}.";
            }
            else
            {
                // Nếu chưa tồn tại, thêm sản phẩm mới vào giỏ
                gioHang.Add(new CartItem
                {
                    SanPhamId = chiTietSanPham.chi_tiet_id,
                    TenSanPham = chiTietSanPham.san_pham.ten_san_pham,
                    KichThuoc = chiTietSanPham.kich_thuoc,
                    DoDay = chiTietSanPham.do_day ?? 0,
                    MauSac = chiTietSanPham.mau_sac,
                    Gia = (decimal)chiTietSanPham.gia_khuyen_mai,
                    SoLuong = soLuong,
                    HinhAnh = chiTietSanPham.san_pham.hinh_anh_chinh
                });
                TempData["SuccessMessage"] = $"Đã thêm sản phẩm '{chiTietSanPham.san_pham.ten_san_pham}' vào giỏ hàng với số lượng {soLuong}.";
            }

            // Cập nhật giỏ hàng vào Session
            Session["GioHang"] = gioHang;

            return RedirectToAction("GioHang");
        }




        [HttpPost]
        public ActionResult CapNhatGioHang(int id, int soLuong)
        {
            // Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang != null)
            {
                // Tìm sản phẩm trong giỏ hàng
                var item = gioHang.FirstOrDefault(x => x.SanPhamId == id);
                if (item != null)
                {
                    // Lấy thông tin chi tiết sản phẩm từ DB
                    var chiTietSanPham = db.chi_tiet_san_phams.FirstOrDefault(ctsp => ctsp.chi_tiet_id == id);
                    if (chiTietSanPham == null)
                    {
                        TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                        return RedirectToAction("GioHang");
                    }

                    // Kiểm tra số lượng tồn
                    if (soLuong > chiTietSanPham.so_luong_ton)
                    {
                        TempData["ErrorMessage"] = $"Không thể cập nhật sản phẩm '{item.TenSanPham}' vì số lượng vượt quá {chiTietSanPham.so_luong_ton}.";
                        return RedirectToAction("GioHang");
                    }

                    // Cập nhật số lượng
                    item.SoLuong = soLuong > 0 ? soLuong : 1;
                    TempData["SuccessMessage"] = $"Đã cập nhật số lượng sản phẩm: {item.TenSanPham}.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Giỏ hàng không tồn tại!";
            }

            // Quay lại trang giỏ hàng
            return RedirectToAction("GioHang");
        }


        [HttpGet]
        public ActionResult XoaKhoiGio(int id)
        {
            // Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang != null)
            {
                // Tìm sản phẩm trong giỏ hàng
                var item = gioHang.FirstOrDefault(x => x.SanPhamId == id);
                if (item != null)
                {
                    // Xóa sản phẩm khỏi giỏ hàng
                    gioHang.Remove(item);
                    TempData["SuccessMessage"] = $"Đã xóa sản phẩm: {item.TenSanPham} khỏi giỏ hàng.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Sản phẩm không tồn tại trong giỏ hàng!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Giỏ hàng không tồn tại!";
            }

            // Quay lại trang giỏ hàng
            return RedirectToAction("GioHang");
        }

        // Action thanh toán
        [HttpGet]
        public ActionResult ThanhToan()
        {
            // Kiểm tra đăng nhập
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện thanh toán.";
                return RedirectToAction("DangNhap", "DangNhap");
            }

            // Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang == null || !gioHang.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("GioHang");
            }

            // Kiểm tra số lượng tồn cho từng sản phẩm trong giỏ hàng
            foreach (var item in gioHang)
            {
                var chiTietSanPham = db.chi_tiet_san_phams.FirstOrDefault(ct => ct.chi_tiet_id == item.SanPhamId);
                if (chiTietSanPham == null)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm với ID {item.SanPhamId} không tồn tại.";
                    return RedirectToAction("GioHang");
                }

                if (item.SoLuong > chiTietSanPham.so_luong_ton)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{chiTietSanPham.san_pham.ten_san_pham}' không đủ số lượng tồn. Vui lòng điều chỉnh số lượng.";
                    return RedirectToAction("GioHang");
                }
            }

            // Lấy thông tin người dùng
            var user = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == Session["UserName"].ToString());
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("GioHang");
            }

            // Chuẩn bị dữ liệu để hiển thị trong View
            var hoaDonViewModel = new HoaDonViewModel
            {
                TongTien = gioHang.Sum(i => i.ThanhTien),
                DiaChiGiaoHang = user.dia_chi,
                SoDienThoai = user.so_dien_thoai,
                TenKhachHang = user.ten_dang_nhap,
                GioHang = gioHang
            };

            return View(hoaDonViewModel);
        }


        [HttpPost]
        public ActionResult XLThanhToan(FormCollection form)
        {
            // Kiểm tra đăng nhập
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện thanh toán.";
                return RedirectToAction("DangNhap", "DangNhap");
            }

            // Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang == null || !gioHang.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("GioHang");
            }

            try
            {
                // Lấy thông tin người dùng
                var user = db.nguoi_dungs.FirstOrDefault(u => u.ten_dang_nhap == Session["UserName"].ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("GioHang");
                }

                // Tạo hóa đơn mới
                var hoaDon = new hoa_don
                {
                    nguoi_dung_id = user.nguoi_dung_id,
                    tong_tien = gioHang.Sum(i => i.ThanhTien),
                    ngay_lap = DateTime.Now,
                    dia_chi_giao_hang = form["DiaChiGiaoHang"],
                    so_dien_thoai = form["SoDienThoai"],
                    ten_khach_hang = form["TenKhachHang"],
                    trang_thai = "cho_xac_nhan"
                };

                db.hoa_dons.InsertOnSubmit(hoaDon);
                db.SubmitChanges();

                // Lưu chi tiết hóa đơn và cập nhật số lượng tồn
                foreach (var item in gioHang)
                {
                    var chiTietHoaDon = new chi_tiet_hoa_don
                    {
                        hoa_don_id = hoaDon.hoa_don_id,
                        chi_tiet_san_pham_id = item.SanPhamId,
                        kich_thuoc = item.KichThuoc,
                        do_day = item.DoDay,
                        mau_sac = item.MauSac,
                        so_luong = item.SoLuong,
                        gia = item.Gia
                    };

                    db.chi_tiet_hoa_dons.InsertOnSubmit(chiTietHoaDon);

                    // Cập nhật số lượng tồn của chi tiết sản phẩm
                    var chiTietSanPham = db.chi_tiet_san_phams.FirstOrDefault(ct => ct.chi_tiet_id == item.SanPhamId);
                    if (chiTietSanPham == null)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm ID {item.SanPhamId} không tồn tại.";
                        return RedirectToAction("GioHang");
                    }

                    if (chiTietSanPham.so_luong_ton < item.SoLuong)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm '{item.SanPhamId}' không đủ số lượng tồn.";
                        return RedirectToAction("GioHang");
                    }

                    chiTietSanPham.so_luong_ton -= item.SoLuong;
                }
                db.SubmitChanges();

                // Xóa giỏ hàng khỏi Session
                Session["GioHang"] = null;

                // Gửi email thông báo thanh toán thành công
                SendPaymentSuccessEmail(user.so_dien_thoai, user.ten_dang_nhap,user.email, hoaDon.hoa_don_id.ToString(), "Thanh toán qua ngân hàng", (decimal)hoaDon.tong_tien, user.dia_chi);

                // Thông báo thanh toán thành công
                TempData["SuccessMessage"] = "Thanh toán thành công. Vui lòng kiểm tra email để nhận thông tin đơn hàng.";
                return RedirectToAction("ThanhToanThanhCong");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("GioHang");
            }
        }


        public ActionResult ThanhToanThanhCong()
        {
            return View();
        }


        private void SendPaymentSuccessEmail(string phone, string name, string email, string orderId, string paymentMethod, decimal totalAmount, string address)
        {
            try
            {
                var fromAddress = new MailAddress("tuan12345yahoo@gmail.com", "Cửa hàng Chăn Ga Gối Nệm");
                var toAddress = new MailAddress(email);
                const string fromPassword = "amtxpbwdvxxbpvjt"; // Mật khẩu của email

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

                // Sử dụng string.Format để thay thế các biến vào email body
                string emailBody = string.Format(@"
                Chào bạn,

                Chúng tôi xin thông báo rằng thanh toán cho đơn hàng #{0} đã được thực hiện thành công.
                Thông tin khách hàng:
                - Tên khách hàng: {3}                
                - Số điện thoại: {4}
                - Địa chỉ giao hàng: {5}

                Chi tiết đơn hàng:
                - Mã đơn hàng: {0}
                - Phương thức thanh toán: {1}
                - Tổng tiền: {2} đ

                Cảm ơn bạn đã mua sắm tại cửa hàng Chăn Ga Gối Nệm. Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc số điện thoại hỗ trợ.

                Trân trọng,
                Cửa hàng Chăn Ga Gối Nệm", orderId, paymentMethod, totalAmount, name, phone, address);

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Thanh toán thành công - Cửa hàng Chăn Ga Gối Nệm",
                    Body = emailBody,
                    IsBodyHtml = false // Gửi dưới dạng văn bản thuần túy
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc xử lý khi không thể gửi email
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message); // In lỗi ra console để dễ dàng kiểm tra
            }
        }






    }
}