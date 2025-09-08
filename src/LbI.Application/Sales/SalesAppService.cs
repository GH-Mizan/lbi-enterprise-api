using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using LbI.Sales.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LbI.Sales
{
    public class SalesAppService : LbIAppServiceBase, ISalesAppService
    {
        private readonly IRepository<Sale> _salesRepo;
        private readonly IRepository<SaleDetail> _salesDetailsRepo;
        private readonly IRepository<DueReceivedHistory> _dueReceivedHistoryRepo;
        private readonly IRepository<Inventory> _inventoryRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<StockPoint> _vehicleRepo;
        private readonly IRepository<LbiSetting> _lbiSettingsRepo;
        private readonly IRepository<Purchase> _purchaseRepo;
        public SalesAppService(
            IRepository<Sale> salesRepo,
            IRepository<SaleDetail> salesDetailsRepo,
            IRepository<Inventory> inventoryRepo,
            IRepository<Product> productRepo,
            IRepository<StockPoint> vehicleRepo,
            IRepository<LbiSetting> lbiSettingsRepo,
            IRepository<Purchase> purchaseRepo,
            IRepository<DueReceivedHistory> dueReceivedHistoryRepo
            )
        {
            _salesRepo = salesRepo;
            _salesDetailsRepo = salesDetailsRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _vehicleRepo = vehicleRepo;
            _lbiSettingsRepo = lbiSettingsRepo;
            _purchaseRepo = purchaseRepo;
            _dueReceivedHistoryRepo = dueReceivedHistoryRepo;
        }

        public async Task<PagedResultDto<SalesOutputDto>> GetPaginatedSalesAsync(SalesFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from s in await _salesRepo.GetAllAsync()
                         join v in await _vehicleRepo.GetAllAsync() on s.StockPointId equals v.Id
                         select new SalesOutputDto()
                         {
                             Id = s.Id,
                             Date = s.Date,
                             InvoiceNumber = s.InvoiceNumber,
                             CustomerId = s.CustomerId,
                             CustomerName = s.CustomerName,
                             TotalAmount = s.TotalAmount,
                             Discount = s.Discount,
                             NetAmount = s.NetAmount,
                             PaidAmount = s.PaidAmount,
                             DueAmount = s.DueAmount,
                             PaymentStatus = s.PaymentStatus,
                             StockPointId = s.StockPointId,
                             StockPointName = v.Name,
                             Remarks = s.Remarks,
                             Locked = s.Locked
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.InvoiceNumber.ToLower().Contains(searchText));
            }

            var items = query.OrderByDescending(o => o.Id).Skip(filter.Skip).Take(filter.Take).ToList();
            foreach (var p in items)
            {
                p.PaymentStatusText = p.PaymentStatus.DisplayName();
            }

            return new PagedResultDto<SalesOutputDto>()
            {
                Items = items,
                TotalCount = query.Count()
            };
        }

        public async Task<List<SalesProductDto>> GetAllProductsAsync(int? stockPointId)
        {
            var output = (from p in await _productRepo.GetAllAsync()
                     //join i in await _inventoryRepo.GetAllAsync() on p.Id equals i.ProductId into inventory
                     //from i in inventory.DefaultIfEmpty()
                     where p.ActiveStatus
                     select new SalesProductDto()
                     {
                         ProductId = p.Id,
                         Name = p.Name,
                         Size = p.Size,
                         Type = p.Type,
                         SalesPrice = p.SellPrice,
                         SalesPriceDisabled = true,
                         QtyDisabled = true,
                         Quantity = 0,
                         TotalPrice = 0,
                         //Stock = i.StockQty,
                         TypeText = p.Type.DisplayName()
                     }).ToList();

            if (stockPointId != null)
            {
                var stocks = await _inventoryRepo.GetAllListAsync(x => x.StockPointId == stockPointId);
                foreach (var item in output)
                {
                    var stock = stocks.Where(x => x.ProductId == item.ProductId).FirstOrDefault()?.StockQty;
                    item.Stock = stock != null ? stock : 0;
                }
            }

            return output;
        }

        public async Task<SalesEntryInput> GetAsync(int id)
        {
            var entity = await _salesRepo.GetAsync(id);
            var details = await _salesDetailsRepo.GetAllListAsync(x=> x.SaleId == id);

            return new SalesEntryInput()
            {
                Sales = ObjectMapper.Map<SalesEntryDto>(entity),
                SalesDetails = ObjectMapper.Map<List<SalesDetailsEntryDto>>(details)
            };
        }

        public async Task<string> GetLastInvoiceNumberAsync()
        {
            var settings = await _lbiSettingsRepo.FirstOrDefaultAsync(x => x.Key == InitialSetupKey.LastSalesInvoiceNumber);
            if (settings != null)
            {
                return settings.Value;
            }
            else
            {
                await _lbiSettingsRepo.InsertAsync(new LbiSetting()
                {
                    Key = InitialSetupKey.LastSalesInvoiceNumber,
                    Value = "1001"
                });
                return "1001";
            }
        }

        [UnitOfWork]
        public async Task CreateOrUpdateAsync(SalesEntryInput input)
        {
            var id = input.Sales.Id;
            if (id.HasValue)
            {
                var sales = await _salesRepo.GetAsync(id.Value);
                ObjectMapper.Map(input.Sales, sales);
                await _salesRepo.UpdateAsync(sales);

                var prevSalesDetails = await _salesDetailsRepo.GetAllListAsync(x => x.SaleId == id);
                foreach (var pd in prevSalesDetails)
                {
                    var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == input.Sales.StockPointId);
                    if (inventory != null)
                    {
                        inventory.StockQty += pd.Quantity;
                        await _inventoryRepo.UpdateAsync(inventory);
                    }
                }
                await _salesDetailsRepo.BatchDeleteAsync(x => x.SaleId == id);
                await InsertSalesDetails(input.SalesDetails, id.Value, input.Sales.StockPointId);
            }
            else
            {
                var sales = ObjectMapper.Map<Sale>(input.Sales);
                id = await _salesRepo.InsertAndGetIdAsync(sales);

                await InsertSalesDetails(input.SalesDetails, id.Value, input.Sales.StockPointId);
                input.DueReceived.SalesId = id.Value;
                await InsertDueReceivedAsync(input.DueReceived);

                var invoiceSettings = await _lbiSettingsRepo.SingleAsync(x => x.Key == InitialSetupKey.LastSalesInvoiceNumber);
                invoiceSettings.Value = (Convert.ToInt32(invoiceSettings.Value) + 1).ToString();
                await _lbiSettingsRepo.UpdateAsync(invoiceSettings);

                var unlockedSales = await _salesRepo.GetAllListAsync(f => !f.Locked && f.Id < id);
                if (unlockedSales != null) {
                    foreach (var item in unlockedSales) {
                        item.Locked = true;
                        await _salesRepo.UpdateAsync(item);
                    }
                }
                var unlockedPurchases = await _purchaseRepo.GetAllListAsync(f => !f.Locked);
                if (unlockedPurchases != null)
                {
                    foreach (var item in unlockedPurchases)
                    {
                        item.Locked = true;
                        await _purchaseRepo.UpdateAsync(item);
                    }
                }
            }
        }

        [UnitOfWork]
        public async Task DueReceivedEntryAsync(DueReceivedEntryDto input)
        {
            var sales = await _salesRepo.SingleAsync(s => s.Id == input.SalesId);
            sales.Discount += input.Discount;
            sales.NetAmount = input.NetTotal;
            sales.PaidAmount += input.TotalPaid;
            sales.DueAmount = input.Due;
            sales.Remarks = input.Remarks;
            //sales.PaymentReceiveHistory = input.PaymentReceiveHistory;
            sales.PaymentStatus = sales.DueAmount == 0 ? PaymentStatus.Paid : sales.TotalAmount > sales.DueAmount ? PaymentStatus.Partialpaid : PaymentStatus.Due;
            await _salesRepo.UpdateAsync(sales);
            await InsertDueReceivedAsync(input.DueReceived);
        }

        public async Task<List<DueReceivedHistoryDto>> GetDueReceivedHistoriesAsync(int salesId)
        {
            var histories = (await _dueReceivedHistoryRepo.GetAllListAsync(x => x.SalesId == salesId)).OrderBy(o => o.CreationTime).ToList();
            return ObjectMapper.Map<List<DueReceivedHistoryDto>>(histories);
        }

        private async Task InsertSalesDetails(List<SalesDetailsEntryDto> salesDetailsInput, int salesId, int vehicleId)
        {
            var salesDetails = new List<SaleDetail>();
            foreach (var pd in salesDetailsInput)
            {
                var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == vehicleId);
                if (inventory != null)
                {
                    inventory.StockQty -= pd.Quantity;
                    await _inventoryRepo.UpdateAsync(inventory);
                }

                var detail = ObjectMapper.Map<SaleDetail>(pd);
                detail.SaleId = salesId;
                salesDetails.Add(detail);
            }
            await _salesDetailsRepo.InsertRangeAsync(salesDetails);
        }

        private async Task InsertDueReceivedAsync(DueReceivedHistoryDto input)
        {
            var dr = ObjectMapper.Map<DueReceivedHistory>(input);
            await _dueReceivedHistoryRepo.InsertAsync(dr);
        }

        public async Task<List<SalesCollectionDueReportDto>> GetSalesCollectionDueReportAsync(DateTime startDate, DateTime endDate)
        {
            var output = (await _salesRepo.GetAllAsync()).Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date).OrderBy(x=> x.Date).Select(s=> new SalesCollectionDueReportDto()
            {
                Date =s.Date,
                TotalSales = s.NetAmount,
                CurrentBalance = s.NetAmount,
                CashCollection = s.PaidAmount,
                //DueCollection
                //TotalCollection = 
                //CollectedBalance
                CurrenctDue = s.DueAmount,
                //DetuctedDue
                //DueBalance = s.DueAmount
            }).ToList();

            var histories = (await _dueReceivedHistoryRepo.GetAllAsync()).Where(x => x.ReceiveDate.Date >= startDate.Date && x.ReceiveDate.Date <= endDate.Date).GroupBy(t => t.ReceiveDate.Date).Select(g => new
            {
                Date = g.Key,
                Due = g.Sum(x => x.TotalPaid)
            }).ToList();

            var firstItem = true;
            decimal lastCurrentBalance = 0;
            decimal lastCollectedBalance = 0;
            decimal lastDueBalance = 0;

            foreach (var item in output)
            {
                var history = histories.Where(x => x.Date.Date == item.Date.Date).First();
                item.CurrentBalance += lastCurrentBalance;
                item.DueCollection = history.Due;
                item.TotalCollection = item.CashCollection + item.DueCollection;
                item.DetuctedDue = item.DueCollection;
                if (!firstItem)
                {
                    item.CollectedBalance = item.TotalCollection + lastCollectedBalance;
                    item.DueBalance = (item.CurrenctDue - item.DetuctedDue) + lastDueBalance;
                }
                else
                {
                    item.CollectedBalance = item.TotalCollection;
                    item.DueBalance = item.CurrenctDue - item.DetuctedDue;
                    firstItem = false;
                }
                
                lastCurrentBalance = item.CurrentBalance;
                lastCollectedBalance = item.CollectedBalance;
                lastDueBalance = item.DueBalance;
            }

            return output;
        }
    }
}
