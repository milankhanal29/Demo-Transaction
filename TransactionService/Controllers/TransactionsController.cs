using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Services;
using TransactionService.Utils;

namespace TransactionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _service;
        private readonly IPaymentScheduler _paymentScheduler;

        public TransactionsController(ITransactionService service, IPaymentScheduler paymentScheduler)
        {
            _service = service;
            _paymentScheduler = paymentScheduler;
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Merchant,User")]
        public async Task<IActionResult> GetTransactions(int userId, [FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var txs = await _service.GetTransactionsForUserAsync(userId, paginationParams);
                return Ok(txs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
        [HttpGet("statement")]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var txs = await _service.GetAllTransactionsAsync();
                return Ok(txs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("transfer")]
        [Authorize(Roles = "Merchant,User")]
        public async Task<IActionResult> PostTransfer([FromBody] Models.TransferRequestDto dto)
        { 
            try
            {
                var result = await _service.TransferAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("upload-transfer")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> UploadTransferExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)

                return BadRequest("No file Provided");
            try
            {
                using var stream = file.OpenReadStream();
                var dto = await _service.ParseExcelAndBuildTransferDtoAsync(stream, User);
                await _service.TransferAsync(dto);
                return Ok("Transfer from Excel successful");
            }
            catch (Exception ex)
            {
                return BadRequest($"error processing excel file :{ex.Message}");
            }
        }

    }
}