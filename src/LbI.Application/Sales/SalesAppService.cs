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
                         TypeText = p.Type.DisplayName(),
                         SizeText = p.Size.DisplayName()
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
                input.DueReceived.Default = true;
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
                DueCollection = g.Sum(x => x.TotalPaid)
            }).ToList();

            var firstItem = true;
            decimal lastCurrentBalance = 0;
            decimal lastCollectedBalance = 0;
            decimal lastDueBalance = 0;

            foreach (var item in output)
            {
                var history = histories.Where(x => x.Date.Date == item.Date.Date).FirstOrDefault();
                item.CurrentBalance += lastCurrentBalance;
                item.DueCollection = history == null ? 0M : history.DueCollection;
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

        public async Task<DailySalesReportDto> GetDailySalesReportAsync(DateTime date)
        {
            var sales = await _salesRepo.GetAllListAsync(f => f.Date.Date == date.Date);
            if (sales.Count == 0)
                return new DailySalesReportDto();

            var products = await _productRepo.GetAllListAsync();
            var salesIds = sales.Select(s => s.Id).ToList();
            var salesDetails = (from sd in await _salesDetailsRepo.GetAllAsync()
                                join p in await _productRepo.GetAllAsync() on sd.ProductId equals p.Id
                                where salesIds.Contains(sd.SaleId)
                                select new
                                {
                                    sd.SaleId,
                                    sd.ProductId,
                                    ProductType = p.Type,
                                    p.Size,
                                    sd.Quantity,
                                    sd.TotalPrice
                                }).ToList();

            var histories = await _dueReceivedHistoryRepo.GetAllListAsync(x => x.ReceiveDate.Date == date.Date && !x.Default);
            var dueCollections = new List<DailySalesReportDueCollectionDto>();
            var details = new List<DailySalesReportDetailsDto>();
            foreach (var s in sales)
            {
                var item = new DailySalesReportDetailsDto()
                {
                    CustomerId = s.CustomerId,
                    CustomerName = s.CustomerName,
                    InvoiceNo = s.InvoiceNumber,
                    PaymentStatus = s.PaymentStatus,
                    PaymentStatusText = s.PaymentStatus.DisplayName(),
                    NetAmount = s.NetAmount,
                    PaidAmount = s.PaidAmount,
                    DueAmount = s.DueAmount
                };

                foreach (var product in products)
                {
                    var thisProductsSales = salesDetails.Where(x => x.ProductId == product.Id && x.SaleId == s.Id).ToList();
                    if (thisProductsSales.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            item.MedicalOxygen9_8Qty = thisProductsSales.Sum(s=> s.Quantity);
                        }
                        else if(product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            item.MedicalOxygen1_36Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            item.MedicalAir9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            item.MedicalAir7Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros30KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros5KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros3KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                    }
                }
                details.Add(item);

                var thisHistories = histories.Where(x => x.SalesId == s.Id).ToList();
                if(thisHistories.Count > 0)
                {
                    var dueCollection = new DailySalesReportDueCollectionDto()
                    {
                        CustomerId = s.CustomerId,
                        CustomerName = s.CustomerName,
                        DueCollection = thisHistories.Sum(s => s.TotalPaid)
                    };
                    dueCollections.Add(dueCollection);
                }
            }

            var customerIds = details.Select(x => x.CustomerId).ToList();
            if (customerIds.Count > customerIds.Distinct().Count()) { 
                details = details.GroupBy(t => t.CustomerId).Select(g => new DailySalesReportDetailsDto()
                {
                    CustomerId = g.Key,
                    CustomerName = g.First().CustomerName,
                    InvoiceNo = g.First().InvoiceNo,
                    PaymentStatus = g.First().PaymentStatus,
                    PaymentStatusText = g.First().PaymentStatusText,
                    NetAmount = g.Sum(x => x.NetAmount),
                    PaidAmount = g.Sum(x => x.PaidAmount),
                    DueAmount = g.Sum(x => x.DueAmount),
                    MedicalOxygen9_8Qty = g.Sum(x => x.MedicalOxygen9_8Qty),
                    MedicalOxygen1_36Qty = g.Sum(x => x.MedicalOxygen1_36Qty),
                    MedicalAir9_8Qty = g.Sum(x => x.MedicalAir9_8Qty),
                    MedicalAir7Qty = g.Sum(x => x.MedicalAir7Qty),
                    Nitros30KgQty = g.Sum(x => x.Nitros30KgQty),
                    Nitros5KgQty = g.Sum(x => x.Nitros5KgQty),
                    Nitros3KgQty = g.Sum(x => x.Nitros3KgQty),
                }).ToList();
            }

            var output = new DailySalesReportDto()
            {
                Details = details,
                MedicalOxygen9_8TotalQty = details.Sum(s => s.MedicalOxygen9_8Qty),
                MedicalOxygen1_36TotalQty = details.Sum(s => s.MedicalOxygen1_36Qty),
                MedicalAir9_8TotalQty = details.Sum(s => s.MedicalAir9_8Qty),
                MedicalAir7TotalQty = details.Sum(s => s.MedicalAir7Qty),
                Nitros30KgTotalQty = details.Sum(s => s.Nitros30KgQty),
                Nitros5KgTotalQty = details.Sum(s => s.Nitros5KgQty),
                Nitros3KgTotalQty = details.Sum(s => s.Nitros3KgQty),
                NetTotal = details.Sum(s => s.NetAmount),
                CashCollection = details.Sum(s => s.PaidAmount),
                Due = details.Sum(s => s.DueAmount),
                //DueCollection
            };

            if (dueCollections.Count > 0)
            {
                var dueCollectionCustomerIds = dueCollections.Select(x => x.CustomerId).ToList();
                if (dueCollectionCustomerIds.Count > dueCollectionCustomerIds.Distinct().Count())
                {
                    dueCollections = dueCollections.GroupBy(t => t.CustomerId).Select(g => new DailySalesReportDueCollectionDto()
                    {
                        CustomerId = g.Key,
                        CustomerName = g.First().CustomerName,
                        DueCollection = g.Sum(x => x.DueCollection)
                    }).ToList();
                }
                output.DueCollections = dueCollections;
                output.DueCollection = dueCollections.Sum(s=> s.DueCollection);
            }

            return output;
        }

        public async Task<List<CustomerLedgerReportDto>> GetCustomerLedgerReportAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var sales = (await _salesRepo.GetAllListAsync(x => x.CustomerId == customerId && x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)).OrderBy(x => x.Date).ToList();
            if(!sales.Any())
                return new List<CustomerLedgerReportDto>();

            var saleIds = sales.Select(s=> s.Id).ToList();
            var saleDetails = await _salesDetailsRepo.GetAllListAsync(x => saleIds.Contains(x.SaleId));
            var histories = await _dueReceivedHistoryRepo.GetAllListAsync(x => saleIds.Contains(x.SalesId));
            var products = await _productRepo.GetAllListAsync();

            var output = new List<CustomerLedgerReportDto>();
            decimal lastBalance = 0M;
            foreach(var s in sales)
            {
                var thisSaleDetails = saleDetails.Where(x=> x.SaleId == s.Id).ToList();
                var thisHistories = histories.Where(x => x.SalesId == s.Id && x.ReceiveDate.Date == s.Date.Date).ToList();
                var ls = new CustomerLedgerReportDto()
                {
                    Date = s.Date,
                    CreditTotal = s.NetAmount,
                    DebitTotal = s.PaidAmount
                };
                ls.Balance = ls.CreditTotal - ls.DebitTotal + lastBalance;

                lastBalance = ls.Balance;

                foreach (var product in products)
                {
                    var thisProductsSales = thisSaleDetails.Where(x => x.ProductId == product.Id && x.SaleId == s.Id).ToList();
                    if (thisProductsSales.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            ls.MedicalOxygen9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            ls.MedicalOxygen1_36Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            ls.MedicalAir9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            ls.MedicalAir7Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            ls.Nitros30KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            ls.Nitros5KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            ls.Nitros3KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                    }
                }

                output.Add(ls);
            }

            return output;
        }
    }
}
