using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;

namespace PaymentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentDetailsController : ControllerBase
    {
        private readonly PaymentDetailContext _context;
        private readonly ILogger<PaymentDetailsController> _logger;

        public PaymentDetailsController(PaymentDetailContext context, ILogger<PaymentDetailsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/PaymentDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDetail>>> GetPaymentDetails()
        {
            _logger.LogInformation("Fetching all payment details");
            var payments = await _context.PaymentDetails.ToListAsync();
            _logger.LogInformation("Retrieved {Count} payment details", payments.Count);
            return payments;
        }

        // GET: api/PaymentDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDetail>> GetPaymentDetails(int id)
        {
            _logger.LogInformation("Fetching payment detail with ID {PaymentId}", id);
            PaymentDetail? paymentDetails = await _context.PaymentDetails.FindAsync(id);

            if (paymentDetails == null)
            {
                _logger.LogWarning("Payment detail with ID {PaymentId} not found", id);
                return NotFound();
            }

            return paymentDetails;
        }

        // PUT: api/PaymentDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentDetails(int id, PaymentDetail paymentDetail)
        {
            if (id != paymentDetail.PaymentDetailId)
            {
                _logger.LogWarning("Mismatched ID: route ID {RouteId} != body ID {BodyId}", id, paymentDetail.PaymentDetailId);
                return BadRequest();
            }

            _context.Entry(paymentDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated payment detail with ID {PaymentId}", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PaymentDetailsExists(id))
                {
                    _logger.LogWarning("Payment detail with ID {PaymentId} not found during update", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating payment detail with ID {PaymentId}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PaymentDetails
        [HttpPost]
        public async Task<ActionResult<PaymentDetail>> PostPaymentDetails(PaymentDetail paymentDetail)
        {
            _logger.LogInformation("Creating new payment detail for {CardOwnerName}", paymentDetail.CardOwnerName);

            await _context.PaymentDetails.AddAsync(paymentDetail);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created payment detail with ID {PaymentId}", paymentDetail.PaymentDetailId);
            return CreatedAtAction("GetPaymentDetails", new { id = paymentDetail.PaymentDetailId }, paymentDetail);
        }

        // DELETE: api/PaymentDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentDetails(int id)
        {
            _logger.LogInformation("Deleting payment detail with ID {PaymentId}", id);

            var paymentDetails = await _context.PaymentDetails.FindAsync(id);
            if (paymentDetails == null)
            {
                _logger.LogWarning("Payment detail with ID {PaymentId} not found for deletion", id);
                return NotFound();
            }

            _context.PaymentDetails.Remove(paymentDetails);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted payment detail with ID {PaymentId}", id);
            return NoContent();
        }

        private bool PaymentDetailsExists(int id)
        {
            return _context.PaymentDetails.Any(e => e.PaymentDetailId == id);
        }
    }
}
