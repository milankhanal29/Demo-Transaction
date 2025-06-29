namespace SharedLibrary.Transaction
{
    public class TransferRequestDto
    {
        public string MerchantAccountNumber { get; set; }
        public List<TransferUserDto> Transfers { get; set; } = new();

    }

}
