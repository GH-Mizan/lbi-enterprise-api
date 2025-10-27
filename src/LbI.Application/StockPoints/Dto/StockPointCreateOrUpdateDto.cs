using LbI.Enums;

namespace LbI.StockPoints.Dto
{
    public class StockPointCreateOrUpdateDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public StockPointType StockPointType { get; set; }
        public string StockPointNumber { get; set; }
        public string GpsTrackerNo { get; set; }
        public bool ActiveStatus { get; set; }
        public string Remarks { get; set; }
    }
}
