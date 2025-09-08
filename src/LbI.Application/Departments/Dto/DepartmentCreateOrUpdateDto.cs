namespace LbI.Departments.Dto
{
    public class DepartmentCreateOrUpdateDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
