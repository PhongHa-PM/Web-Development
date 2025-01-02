using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem.Models
{
    //public class Cart
    //{
    //    private List<CartItem> items = new List<CartItem>();

    //    public IEnumerable<CartItem> Items => items;

    //    // Thêm sản phẩm vào giỏ hàng
    //    public void AddToCart(CartItem item)
    //    {
    //        var existingItem = items.FirstOrDefault(i => i.ChiTietSanPhamId == item.ChiTietSanPhamId);
    //        if (existingItem == null)
    //        {
    //            items.Add(item);
    //        }
    //        else
    //        {
    //            existingItem.SoLuong += item.SoLuong;
    //        }
    //    }

    //    // Xóa sản phẩm khỏi giỏ hàng
    //    public void RemoveFromCart(int chiTietSanPhamId)
    //    {
    //        var item = items.FirstOrDefault(i => i.ChiTietSanPhamId == chiTietSanPhamId);
    //        if (item != null)
    //        {
    //            items.Remove(item);
    //        }
    //    }

    //    // Cập nhật số lượng sản phẩm
    //    public void UpdateQuantity(int chiTietSanPhamId, int soLuong)
    //    {
    //        var item = items.FirstOrDefault(i => i.ChiTietSanPhamId == chiTietSanPhamId);
    //        if (item != null)
    //        {
    //            item.SoLuong = soLuong;
    //        }
    //    }

    //    // Tính tổng tiền giỏ hàng
    //    public decimal GetTotalAmount()
    //    {
    //        return items.Sum(i => i.TongTien);
    //    }

    //    // Xóa toàn bộ giỏ hàng
    //    public void ClearCart()
    //    {
    //        items.Clear();
    //    }
    //}

    
}
