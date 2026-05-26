using System;
using System.Data.SqlClient;
using HVIP.Models;

namespace HVIP.Helpers
{
    public static class OrderRepository
    {
        /// <summary>
        /// Saves a completed order (header + items) to the database.
        /// Returns the new database Id, or 0 on failure.
        /// </summary>
        public static int SaveOrder(Order order, int? userId = null)
        {
            const string insertOrder = @"
                INSERT INTO Orders
                    (OrderNumber, UserId, CustomerName, Email, Phone,
                     Address, City, State, Pincode,
                     PaymentMethod, SubTotal, Shipping, GrandTotal,
                     Status, OrderDate)
                OUTPUT INSERTED.Id
                VALUES
                    (@OrderNumber, @UserId, @CustomerName, @Email, @Phone,
                     @Address, @City, @State, @Pincode,
                     @PaymentMethod, @SubTotal, @Shipping, @GrandTotal,
                     @Status, @OrderDate)";

            const string insertItem = @"
                INSERT INTO OrderItems
                    (OrderId, ProductId, ProductName, ProductSize,
                     CategoryIcon, CategoryColor, UnitPrice, Quantity, Total)
                VALUES
                    (@OrderId, @ProductId, @ProductName, @ProductSize,
                     @CategoryIcon, @CategoryColor, @UnitPrice, @Quantity, @Total)";

            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                {
                    int orderId;

                    // --- Insert header ---
                    using (var cmd = new SqlCommand(insertOrder, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderNumber",   order.OrderNumber);
                        cmd.Parameters.AddWithValue("@UserId",        (object)userId ?? System.DBNull.Value);
                        cmd.Parameters.AddWithValue("@CustomerName",  order.CustomerName);
                        cmd.Parameters.AddWithValue("@Email",         order.Email);
                        cmd.Parameters.AddWithValue("@Phone",         order.Phone);
                        cmd.Parameters.AddWithValue("@Address",       order.Address);
                        cmd.Parameters.AddWithValue("@City",          order.City);
                        cmd.Parameters.AddWithValue("@State",         order.State);
                        cmd.Parameters.AddWithValue("@Pincode",       order.Pincode);
                        cmd.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod ?? "COD");
                        cmd.Parameters.AddWithValue("@SubTotal",      order.SubTotal);
                        cmd.Parameters.AddWithValue("@Shipping",      order.Shipping);
                        cmd.Parameters.AddWithValue("@GrandTotal",    order.GrandTotal);
                        cmd.Parameters.AddWithValue("@Status",        order.Status ?? "Confirmed");
                        cmd.Parameters.AddWithValue("@OrderDate",     order.OrderDate);

                        orderId = (int)cmd.ExecuteScalar();
                    }

                    // --- Insert each line item ---
                    if (order.Items != null)
                    {
                        foreach (var item in order.Items)
                        {
                            using (var cmd = new SqlCommand(insertItem, conn))
                            {
                                cmd.Parameters.AddWithValue("@OrderId",       orderId);
                                cmd.Parameters.AddWithValue("@ProductId",     item.ProductId);
                                cmd.Parameters.AddWithValue("@ProductName",   item.ProductName ?? "");
                                cmd.Parameters.AddWithValue("@ProductSize",   (object)item.ProductSize ?? System.DBNull.Value);
                                cmd.Parameters.AddWithValue("@CategoryIcon",  (object)item.CategoryIcon ?? System.DBNull.Value);
                                cmd.Parameters.AddWithValue("@CategoryColor", (object)item.CategoryColor ?? System.DBNull.Value);
                                cmd.Parameters.AddWithValue("@UnitPrice",     item.UnitPrice);
                                cmd.Parameters.AddWithValue("@Quantity",      item.Quantity);
                                cmd.Parameters.AddWithValue("@Total",         item.Total);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    return orderId;
                }
            }
            catch
            {
                return 0;   // DB error — order still shown via TempData
            }
        }

        /// <summary>
        /// Returns a single order with its line items for the given user (security check on UserId).
        /// Returns null if not found or the order belongs to a different user.
        /// </summary>
        public static Order GetDetail(int orderId, int userId)
        {
            const string orderSql = @"
                SELECT TOP 1 * FROM Orders
                WHERE Id = @Id AND (UserId = @UserId OR @UserId = 0)";
            const string itemsSql = @"
                SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                {
                    Order order = null;

                    using (var cmd = new SqlCommand(orderSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id",     orderId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) return null;
                            order = new Order
                            {
                                Id            = (int)r["Id"],
                                OrderNumber   = r["OrderNumber"]   as string,
                                CustomerName  = r["CustomerName"]  as string,
                                Email         = r["Email"]         as string,
                                Phone         = r["Phone"]         as string,
                                Address       = r["Address"]       as string,
                                City          = r["City"]          as string,
                                State         = r["State"]         as string,
                                Pincode       = r["Pincode"]       as string,
                                PaymentMethod = r["PaymentMethod"] as string ?? "COD",
                                SubTotal      = (decimal)r["SubTotal"],
                                Shipping      = (decimal)r["Shipping"],
                                GrandTotal    = (decimal)r["GrandTotal"],
                                Status        = r["Status"]        as string,
                                OrderDate     = (DateTime)r["OrderDate"],
                                Items         = new System.Collections.Generic.List<CartItem>()
                            };
                        }
                    }

                    using (var cmd = new SqlCommand(itemsSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                order.Items.Add(new CartItem
                                {
                                    ProductId     = r["ProductId"]     == DBNull.Value ? 0 : (int)r["ProductId"],
                                    ProductName   = r["ProductName"]   as string ?? "",
                                    ProductSize   = r["ProductSize"]   as string,
                                    CategoryIcon  = r["CategoryIcon"]  as string ?? "fas fa-pills",
                                    CategoryColor = r["CategoryColor"] as string ?? "#1b5e20",
                                    UnitPrice     = (decimal)r["UnitPrice"],
                                    Quantity      = (int)r["Quantity"]
                                });
                    }

                    return order;
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// Saves a contact form message to the database.
        /// </summary>
        public static bool SaveContactMessage(ContactMessage msg)
        {
            const string sql = @"
                INSERT INTO ContactMessages (Name, Email, Phone, Subject, Message, ReceivedOn)
                VALUES (@Name, @Email, @Phone, @Subject, @Message, GETDATE())";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name",    msg.Name);
                    cmd.Parameters.AddWithValue("@Email",   msg.Email);
                    cmd.Parameters.AddWithValue("@Phone",   (object)msg.Phone   ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", (object)msg.Subject ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@Message", msg.Message);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }
    }
}
