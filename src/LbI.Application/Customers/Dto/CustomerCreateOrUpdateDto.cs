namespace LbI.Customers.Dto
{
    public class CustomerCreateOrUpdateDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public bool ActiveStatus { get; set; }
        public string Remarks { get; set; }
    }
}
