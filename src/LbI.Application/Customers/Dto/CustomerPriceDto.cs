namespace LbI.Customers.Dto
{
    public class CustomerPriceDto
    {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
}
