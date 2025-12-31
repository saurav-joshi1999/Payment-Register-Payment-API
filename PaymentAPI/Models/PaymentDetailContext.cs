using Microsoft.EntityFrameworkCore;

namespace PaymentAPI.Models
{
    public class PaymentDetailContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
    }
}
