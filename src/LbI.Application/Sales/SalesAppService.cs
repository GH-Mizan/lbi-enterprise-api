using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using LbI.Sales.Dto;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IRepository<PurchaseDetail> _purchaseDetailsRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Employee> _employeeRepo;
        public SalesAppService(
            IRepository<Sale> salesRepo,
            IRepository<SaleDetail> salesDetailsRepo,
            IRepository<Inventory> inventoryRepo,
            IRepository<Product> productRepo,
            IRepository<StockPoint> vehicleRepo,
            IRepository<LbiSetting> lbiSettingsRepo,
            IRepository<PurchaseDetail> purchaseDetailsRepo,
            IRepository<DueReceivedHistory> dueReceivedHistoryRepo,
            IRepository<Customer> customerRepo,
            IRepository<Employee> employeeRepo
            )
        {
            _salesRepo = salesRepo;
            _salesDetailsRepo = salesDetailsRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _vehicleRepo = vehicleRepo;
            _lbiSettingsRepo = lbiSettingsRepo;
            _purchaseDetailsRepo = purchaseDetailsRepo;
            _dueReceivedHistoryRepo = dueReceivedHistoryRepo;
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
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
                             ReferenceNumber = s.ReferenceNumber,
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
                x.InvoiceNumber.ToLower().Contains(searchText) ||
                x.ReferenceNumber.ToLower().Contains(searchText) ||
                x.CustomerName.ToLower().Contains(searchText) ||
                x.StockPointName.ToLower().Contains(searchText)
                );
            }

            var items = query.OrderByDescending(o => o.Id).Skip(filter.Skip).Take(filter.Take).ToList();
            foreach (var p in items)
            {
                p.PaymentStatusText = p.PaymentStatus.DisplayName();
                if(!string.IsNullOrEmpty(p.ReferenceNumber))
                    p.InvoiceNumber = p.ReferenceNumber;
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
                    Value = "A00000"
                });
                return "A00000";
            }
        }

        [UnitOfWork]
        public async Task<int> CreateOrUpdateAsync(SalesEntryInput input)
        {
            var id = input.Sales.Id;
            if (id.HasValue)
            {
                var sales = await _salesRepo.GetAsync(id.Value);
                ObjectMapper.Map(input.Sales, sales);
                await _salesRepo.UpdateAsync(sales);

                var prevSalesDetails = await _salesDetailsRepo.GetAllListAsync(x => x.SaleId == id);
                foreach (var sd in prevSalesDetails)
                {
                    var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == sd.ProductId && f.StockPointId == input.Sales.StockPointId);
                    if (inventory != null)
                    {
                        inventory.StockQty += sd.Quantity;
                        await _inventoryRepo.UpdateAsync(inventory);
                    }
                }
                await _salesDetailsRepo.BatchDeleteAsync(x => x.SaleId == id);
                await InsertSalesDetails(input.SalesDetails, id.Value, input.Sales.StockPointId);

                await _dueReceivedHistoryRepo.DeleteAsync(x=> x.SalesId == id);
                await InsertDueReceivedAsync(input.DueReceived);

            }
            else
            {
                var sales = ObjectMapper.Map<Sale>(input.Sales);
                id = await _salesRepo.InsertAndGetIdAsync(sales);

                await InsertSalesDetails(input.SalesDetails, id.Value, input.Sales.StockPointId);
                input.DueReceived.SalesId = id.Value;
                await InsertDueReceivedAsync(input.DueReceived);

                var invoiceSettings = await _lbiSettingsRepo.SingleAsync(x => x.Key == InitialSetupKey.LastSalesInvoiceNumber);
                var lastInvoiceNumber = invoiceSettings.Value;
                string prefix = lastInvoiceNumber.Substring(0, 1);
                var parsedInvoiceNumber = Convert.ToInt32(lastInvoiceNumber.Remove(0, 1));

                invoiceSettings.Value = prefix + (parsedInvoiceNumber + 1).ToString().PadLeft(5, '0');
                await _lbiSettingsRepo.UpdateAsync(invoiceSettings);

                //var unlockedSales = await _salesRepo.GetAllListAsync(f => !f.Locked && f.Id < id);
                //if (unlockedSales != null) {
                //    foreach (var item in unlockedSales) {
                //        item.Locked = true;
                //        await _salesRepo.UpdateAsync(item);
                //    }
                //}
                //var unlockedPurchases = await _purchaseRepo.GetAllListAsync(f => !f.Locked);
                //if (unlockedPurchases != null)
                //{
                //    foreach (var item in unlockedPurchases)
                //    {
                //        item.Locked = true;
                //        await _purchaseRepo.UpdateAsync(item);
                //    }
                //}
            }

            return id.Value;
        }

        [UnitOfWork]
        public async Task DueReceivedEntryAsync(DueReceivedEntryDto input)
        {
            try
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
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task DueReceivedRemoveAsync(int id)
        {
            try
            {
                var dr = await _dueReceivedHistoryRepo.SingleAsync(x => x.Id == id);
                var sale = await _salesRepo.SingleAsync(s => s.Id == dr.SalesId);
                sale.Discount -= dr.Discount;
                //sale.NetAmount -= dr.NetTotal;
                sale.PaidAmount -= dr.TotalPaid;
                sale.DueAmount = sale.NetAmount - sale.Discount - sale.PaidAmount;
                sale.PaymentStatus = sale.DueAmount == 0 ? PaymentStatus.Paid : sale.TotalAmount > sale.DueAmount ? PaymentStatus.Partialpaid : PaymentStatus.Due;
                await _salesRepo.UpdateAsync(sale);
                await _dueReceivedHistoryRepo.DeleteAsync(id);
            }
            catch(Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<DueReceivedHistoryDto>> GetDueReceivedHistoriesAsync(int salesId)
        {
            var histories = (await _dueReceivedHistoryRepo.GetAllListAsync(x => x.SalesId == salesId)).OrderBy(o => o.CreationTime).ToList();
            return ObjectMapper.Map<List<DueReceivedHistoryDto>>(histories);
        }

        [UnitOfWork]
        private async Task InsertSalesDetails(List<SalesDetailsEntryDto> salesDetailsInput, int salesId, int stockPointId)
        {
            var salesDetails = new List<SaleDetail>();
            var purchaseDetails = await _purchaseDetailsRepo.GetAllListAsync(x=> salesDetailsInput.Select(s=> s.ProductId).ToList().Contains(x.ProductId));
            foreach (var sd in salesDetailsInput)
            {
                var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == sd.ProductId && f.StockPointId == stockPointId);
                if (inventory != null)
                {
                    inventory.StockQty -= sd.Quantity;
                    await _inventoryRepo.UpdateAsync(inventory);
                }

                var detail = ObjectMapper.Map<SaleDetail>(sd);
                detail.SaleId = salesId;

                var purchasePrice = purchaseDetails.Where(x => x.ProductId == sd.ProductId).First().UnitPrice;
                detail.TotalProfit = sd.TotalPrice - (purchasePrice * sd.Quantity);
                salesDetails.Add(detail);
            }
            await _salesDetailsRepo.InsertRangeAsync(salesDetails);
        }

        private async Task InsertDueReceivedAsync(DueReceivedHistoryDto input)
        {
            var dr = ObjectMapper.Map<DueReceivedHistory>(input);
            await _dueReceivedHistoryRepo.InsertAsync(dr);
        }

        #region Reports

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

            output = output.GroupBy(t => t.Date.Date).Select(g => new SalesCollectionDueReportDto()
            {
                Date = g.Key,
                TotalSales = g.Sum(t => t.TotalSales),
                CurrentBalance = g.Sum(t => t.CurrentBalance),
                CashCollection = g.Sum(t => t.CashCollection),
                CurrenctDue = g.Sum(t => t.CurrenctDue),
            }).ToList();

            var histories = (await _dueReceivedHistoryRepo.GetAllAsync()).Where(x => x.ReceiveDate.Date >= startDate.Date && x.ReceiveDate.Date <= endDate.Date && !x.Default).GroupBy(t => t.ReceiveDate.Date).Select(g => new
            {
                Date = g.Key,
                DueCollection = g.Sum(t => t.TotalPaid)
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
            for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                if(!output.Any(x=> x.Date.Date == day))
                {
                    output.Add(new SalesCollectionDueReportDto() { Date = day, Empty = true});
                }
            }
            return output.OrderBy(o=> o.Date.Date).ToList();
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

            var histories = (from h in (await _dueReceivedHistoryRepo.GetAllAsync())
                             .Where(x => x.ReceiveDate.Date == date.Date && !x.Default).GroupBy(t => t.SalesId)
                             .Select(g => new
                             {
                                 SaleId = g.Key,
                                 TotalPaid = g.Sum(s => s.TotalPaid)
                             })
                             join s in await _salesRepo.GetAllAsync() on h.SaleId equals s.Id
                             join c in await _customerRepo.GetAllAsync() on s.CustomerId equals c.Id
                             select new
                             {
                                 //h.SaleId,
                                 s.CustomerId,
                                 CustomerName = c.Name,
                                 h.TotalPaid,
                                 //s.InvoiceNumber,
                                 //s.ReferenceNumber,
                             }).ToList();

            var details = new List<DailySalesReportDetailsDto>();
            foreach (var s in sales)
            {
                var item = new DailySalesReportDetailsDto()
                {
                    CustomerId = s.CustomerId,
                    CustomerName = s.CustomerName,
                    InvoiceNo = !string.IsNullOrEmpty(s.ReferenceNumber) ? s.ReferenceNumber : s.InvoiceNumber,
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
                    NetAmount = g.Sum(t => t.NetAmount),
                    PaidAmount = g.Sum(t => t.PaidAmount),
                    DueAmount = g.Sum(t => t.DueAmount),
                    MedicalOxygen9_8Qty = g.Sum(t => t.MedicalOxygen9_8Qty),
                    MedicalOxygen1_36Qty = g.Sum(t => t.MedicalOxygen1_36Qty),
                    MedicalAir9_8Qty = g.Sum(t => t.MedicalAir9_8Qty),
                    MedicalAir7Qty = g.Sum(t => t.MedicalAir7Qty),
                    Nitros30KgQty = g.Sum(t => t.Nitros30KgQty),
                    Nitros5KgQty = g.Sum(t => t.Nitros5KgQty),
                    Nitros3KgQty = g.Sum(t => t.Nitros3KgQty),
                }).ToList();
            }

            histories = histories.GroupBy(t => t.CustomerId).Select(g => new
            {
                CustomerId = g.Key,
                g.First().CustomerName,
                TotalPaid = g.Sum(s=> s.TotalPaid)
            }).ToList();

            foreach (var h in histories)
            {
                var d = details.FirstOrDefault(f => f.CustomerId == h.CustomerId);
                if(d != null)
                {
                    d.DueCollection = h.TotalPaid;
                }
                else
                {
                    var item = new DailySalesReportDetailsDto()
                    {
                        CustomerId = h.CustomerId,
                        CustomerName = h.CustomerName,
                        //InvoiceNo = !string.IsNullOrEmpty(h.ReferenceNumber) ? h.ReferenceNumber : h.InvoiceNumber,
                        DueCollection = h.TotalPaid
                    };
                    details.Add(item);
                }
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
                DueCollection = details.Sum(s => s.DueCollection),
                Due = details.Sum(s => s.DueAmount),
            };

            return output;
        }

        public async Task<CustomerLedgerReportDto> GetCustomerLedgerReportAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var sales = (await _salesRepo.GetAllAsync()).Where(x => x.CustomerId == customerId && x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date).
                Select(s=> new
                {
                    s.Id,
                    s.Date.Date,
                    s.NetAmount
                }).ToList();

            if (!sales.Any())
                return new CustomerLedgerReportDto()
                {
                    Details = new List<CustomerLedgerDetailsDto>()
                };

            var dateWiseSales = sales.GroupBy(t => t.Date).Select(g => new
            {
                Date = g.Key,
                NetAmount = g.Sum(s => s.NetAmount),
                //PaidAmount = g.Sum(s => s.PaidAmount)
                SaleIds = g.Select(s=>s.Id).ToList()
            }).OrderBy(o => o.Date).ToList();

            var saleIds = sales.Select(s=> s.Id).ToList();
            var saleDetails = await _salesDetailsRepo.GetAllListAsync(x => saleIds.Contains(x.SaleId));
            var histories = await _dueReceivedHistoryRepo.GetAllListAsync(x => saleIds.Contains(x.SalesId));
            var products = await _productRepo.GetAllListAsync();

            var details = new List<CustomerLedgerDetailsDto>();
            decimal lastBalance = 0M;

            foreach (var s in dateWiseSales)
            {
                //var thisSaleIds = sales.Where(x => x.Date == s.Date).Select(s => s.Id).ToList();
                var thisSaleDetails = saleDetails.Where(x => s.SaleIds.Contains(x.SaleId)).ToList();
                var thisHistories = histories.Where(x => x.ReceiveDate.Date == s.Date).ToList();
                var ls = new CustomerLedgerDetailsDto()
                {
                    Date = s.Date,
                    CreditTotal = s.NetAmount,
                    DebitTotal = thisHistories.Sum(s=> s.TotalPaid)
                };
                ls.Balance = ls.CreditTotal - ls.DebitTotal + lastBalance;
                lastBalance = ls.Balance;

                foreach (var product in products)
                {
                    var thisProductsSales = thisSaleDetails.Where(x => x.ProductId == product.Id && saleIds.Contains(x.SaleId)).ToList();
                    var qty = thisProductsSales.Sum(s => s.Quantity);
                    if (thisProductsSales.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            ls.MedicalOxygen9_8Qty = qty;
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            ls.MedicalOxygen1_36Qty = qty;
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            ls.MedicalAir9_8Qty = qty;
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            ls.MedicalAir7Qty = qty;
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            ls.Nitros30KgQty = qty;
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            ls.Nitros5KgQty = qty;
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            ls.Nitros3KgQty = qty;
                        }
                    }
                }

                details.Add(ls);
            }




            //foreach(var s in sales)
            //{
            //    var thisSaleDetails = saleDetails.Where(x=> x.SaleId == s.Id).ToList();
            //    var thisHistories = histories.Where(x => x.SalesId == s.Id && x.ReceiveDate.Date == s.Date.Date).ToList();
            //    var ls = new CustomerLedgerDetailsDto()
            //    {
            //        Date = s.Date,
            //        CreditTotal = s.NetAmount,
            //        DebitTotal = s.PaidAmount
            //    };
            //    ls.Balance = ls.CreditTotal - ls.DebitTotal + lastBalance;
            //    lastBalance = ls.Balance;

            //    foreach (var product in products)
            //    {
            //        var thisProductsSales = thisSaleDetails.Where(x => x.ProductId == product.Id && x.SaleId == s.Id).ToList();
            //        if (thisProductsSales.Count > 0)
            //        {
            //            if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
            //            {
            //                ls.MedicalOxygen9_8Qty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //            else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
            //            {
            //                ls.MedicalOxygen1_36Qty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //            else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
            //            {
            //                ls.MedicalAir9_8Qty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //            else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
            //            {
            //                ls.MedicalAir7Qty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //            else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
            //            {
            //                ls.Nitros30KgQty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //            else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
            //            {
            //                ls.Nitros5KgQty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //            else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
            //            {
            //                ls.Nitros3KgQty = thisProductsSales.Sum(s => s.Quantity);
            //            }
            //        }
            //    }

            //    details.Add(ls);
            //}
            
            
            var actualTotal = (await _salesRepo.GetAllAsync()).Where(x => x.CustomerId == customerId).GroupBy(t => 1).Select(g => new
            {
                TotalNetAmount = g.Sum(s => s.NetAmount),
                TotalPaidAmount = g.Sum(s => s.PaidAmount)
            }).FirstOrDefault();
            var output = details.GroupBy(t => 1).Select(g => new CustomerLedgerReportDto()
            {
                MedicalOxygen9_8TotalQty = g.Sum(s=> s.MedicalOxygen9_8Qty),
                MedicalOxygen1_36TotalQty = g.Sum(s => s.MedicalOxygen1_36Qty),
                MedicalAir9_8TotalQty = g.Sum(s => s.MedicalAir9_8Qty),
                MedicalAir7TotalQty = g.Sum(s => s.MedicalAir7Qty),
                Nitros30KgTotalQty = g.Sum(s => s.Nitros30KgQty),
                Nitros5KgTotalQty = g.Sum(s => s.Nitros5KgQty),
                Nitros3KgTotalQty = g.Sum(s => s.Nitros3KgQty),
                OverallCreditTotal = g.Sum(s => s.CreditTotal),
                OverallDebitTotal = g.Sum(s => s.DebitTotal),
                //OverallBalance = g.Sum(s => s.Balance),
                Details = details
            }).First();
            
            if(actualTotal != null)
            {
                output.ActualCreditTotal = actualTotal.TotalNetAmount;
                output.ActualDebitTotal = actualTotal.TotalPaidAmount;
            }

            output.InitialDue = (await _customerRepo.SingleAsync(x => x.Id == customerId)).InitialDue;
            output.OverallBalance = output.ActualCreditTotal - output.ActualDebitTotal + output.InitialDue;

            return output;
        }

        public async Task<CustomerDueReportDto> GetCustomerDueReportAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var sales = (await _salesRepo.GetAllListAsync(x => x.CustomerId == customerId && x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date && x.DueAmount > 0)).OrderBy(x => x.Date).ToList();
            if (!sales.Any())
                return new CustomerDueReportDto()
                {
                    Details = new List<CustomerDueDetailsDto>()
                };

            var saleIds = sales.Select(s => s.Id).ToList();
            var saleDetails = await _salesDetailsRepo.GetAllListAsync(x => saleIds.Contains(x.SaleId));
            //var histories = await _dueReceivedHistoryRepo.GetAllListAsync(x => saleIds.Contains(x.SalesId));
            var products = await _productRepo.GetAllListAsync();

            var details = new List<CustomerDueDetailsDto>();
            decimal lastBalance = 0M;
            foreach (var s in sales)
            {
                var thisSaleDetails = saleDetails.Where(x => x.SaleId == s.Id).ToList();
                var due = new CustomerDueDetailsDto()
                {
                    Date = s.Date,
                    InvoiceNo = !string.IsNullOrEmpty(s.ReferenceNumber) ? s.ReferenceNumber : s.InvoiceNumber,
                    TotalDue = s.DueAmount,
                    Balance = s.DueAmount + lastBalance
                };
                lastBalance = due.Balance;

                foreach (var product in products)
                {
                    var thisProductsSales = thisSaleDetails.Where(x => x.ProductId == product.Id && x.SaleId == s.Id).ToList();
                    if (thisProductsSales.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            due.MedicalOxygen9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            due.MedicalOxygen1_36Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            due.MedicalAir9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            due.MedicalAir7Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            due.Nitros30KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            due.Nitros5KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            due.Nitros3KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                    }
                }

                details.Add(due);
            }
            var output = details.GroupBy(t => 1).Select(g => new CustomerDueReportDto()
            {
                MedicalOxygen9_8TotalQty = g.Sum(s => s.MedicalOxygen9_8Qty),
                MedicalOxygen1_36TotalQty = g.Sum(s => s.MedicalOxygen1_36Qty),
                MedicalAir9_8TotalQty = g.Sum(s => s.MedicalAir9_8Qty),
                MedicalAir7TotalQty = g.Sum(s => s.MedicalAir7Qty),
                Nitros30KgTotalQty = g.Sum(s => s.Nitros30KgQty),
                Nitros5KgTotalQty = g.Sum(s => s.Nitros5KgQty),
                Nitros3KgTotalQty = g.Sum(s => s.Nitros3KgQty),
                OverallDue = g.Sum(s => s.TotalDue),
                OverallBalance = g.Sum(s => s.Balance),
                Details = details
            }).First();

            output.InitialDue = (await _customerRepo.SingleAsync(x => x.Id == customerId)).InitialDue;
            output.ActualDue = (await _salesRepo.GetAllAsync()).Where(x=> x.CustomerId == customerId && x.DueAmount > 0).Sum(s=> s.DueAmount) + output.InitialDue;

            return output;
        }

        public async Task<List<CustomerOverallDueReportDto>> GetCustomersOverallDueReportAsync(DateTime startDate, DateTime endDate)
        {
            var output = (await _customerRepo.GetAllAsync()).Select(s => new CustomerOverallDueReportDto()
            {
                CustomerId = s.Id,
                CustomerName = s.Name,
                PreviousDue = s.InitialDue
            }).ToList();

            var prevDate = startDate.Date.AddDays(-1);
            var prevDues = await _salesRepo.GetAllListAsync(x => x.Date.Date <= prevDate.Date && x.DueAmount > 0);
            var sales = await _salesRepo.GetAllListAsync(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date);

            var count = 1;
            foreach (var item in output) 
            {
                if(output.Count > 99)
                    item.Serial = count.ToString().PadLeft(3, '0');
                else
                    item.Serial = count.ToString().PadLeft(2, '0');

                var thisPrevDue = prevDues.Where(x => x.CustomerId == item.CustomerId).Sum(x => x.DueAmount);
                var thisSales = sales.Where(x => x.CustomerId == item.CustomerId);

                item.PreviousDue = item.PreviousDue + thisPrevDue;
                item.CurrentSales = thisSales.Sum(x => x.NetAmount);
                item.CurrentPaymnet = thisSales.Sum(x => x.PaidAmount);
                item.CurrentDue = item.PreviousDue + item.CurrentSales - item.CurrentPaymnet;

                count++;
            }
            return output;
        }

        public async Task<List<MonthlySalesRankingReportDto>> GetMonthlySalesRankingReportAsync(int month, int year)
        {
            var output = (await _customerRepo.GetAllAsync()).Select(s => new MonthlySalesRankingReportDto()
            {
                CustomerId = s.Id,
                CustomerName = s.Name
            }).ToList();

            var sales = await _salesRepo.GetAllListAsync(x => x.Date.Year == year && x.Date.Month == month);
            var salesDetails = await _salesDetailsRepo.GetAllListAsync(x => sales.Select(s => s.Id).ToList().Contains(x.SaleId));
            var products = await _productRepo.GetAllListAsync();

            foreach (var item in output)
            {
                var thisSaleIds = sales.Where(x => x.CustomerId == item.CustomerId).Select(s => s.Id).ToList();
                var profits = salesDetails.Where(x => thisSaleIds.Contains(x.SaleId)).GroupBy(t => t.ProductId).Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(s => s.Quantity),
                    Profit = g.Sum(s => s.TotalProfit)
                }).ToList();

                foreach (var product in products)
                {
                    var thisProfits = profits.Where(x => x.ProductId == product.Id).ToList();
                    if (thisProfits.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            item.MedicalOxygen9_8Qty = thisProfits.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            item.MedicalOxygen1_36Qty = thisProfits.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            item.MedicalAir9_8Qty = thisProfits.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            item.MedicalAir7Qty = thisProfits.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros30KgQty = thisProfits.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros5KgQty = thisProfits.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros3KgQty = thisProfits.Sum(s => s.Quantity);
                        }
                    }
                }

                item.Revenue = profits.Sum(s=> s.Profit);
            }

            return output.OrderByDescending(o=> o.Revenue).Select((s, index) => new MonthlySalesRankingReportDto()
            {
                Rank = index + 1,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                Revenue = s.Revenue,
                MedicalOxygen9_8Qty = s.MedicalOxygen9_8Qty,
                MedicalOxygen1_36Qty = s.MedicalOxygen1_36Qty,
                MedicalAir9_8Qty = s.MedicalAir9_8Qty,
                MedicalAir7Qty = s.MedicalAir7Qty,
                Nitros30KgQty = s.Nitros30KgQty,
                Nitros5KgQty = s.Nitros5KgQty,
                Nitros3KgQty = s.Nitros3KgQty,
            }).ToList();
        }

        public async Task<SalesReceiptOutputDto> GetSalesReceiptAsync(int saleId)
        {
            var output = (from s in await _salesRepo.GetAllAsync()
                          join c in await _customerRepo.GetAllAsync() on s.CustomerId equals c.Id
                          join e in await _employeeRepo.GetAllAsync() on s.SalesBy equals e.Id
                          where s.Id == saleId
                          select new SalesReceiptOutputDto()
                          {
                              Id = saleId,
                              InvoiceDate = s.Date,
                              InvoiceNumber = s.InvoiceNumber,
                              ReferenceNumber = s.ReferenceNumber,
                              CustomerId = s.CustomerId,
                              CustomerName = c.Name,
                              Address = c.Address,
                              Saler = e.Name,
                              TotalAmount = s.NetAmount,
                              TotalPaid = s.PaidAmount,
                              TotalDue = s.DueAmount
                          }).First();
            if(!string.IsNullOrEmpty(output.ReferenceNumber))
                output.InvoiceNumber = output.ReferenceNumber;

            var salesDetails = (from sd in await _salesDetailsRepo.GetAllAsync()
                                join p in await _productRepo.GetAllAsync() on sd.ProductId equals p.Id
                                where sd.SaleId == saleId
                                select new SalesRecieptProductDto()
                                {
                                    ProductId = sd.ProductId,
                                    Product = p.Name,
                                    UnitPrice = sd.UnitPrice,
                                    Qty = sd.Quantity,
                                    Amount = sd.TotalPrice
                                }).ToList();

            output.Details = salesDetails;
            output.PreviousDue = (await _salesRepo.GetAllAsync()).Where(s => s.Id < saleId && s.DueAmount > 0 && s.CustomerId == output.CustomerId).Sum(s => s.DueAmount);
            output.OverallDue = output.PreviousDue + output.TotalDue;

            return output;
        }

        public async Task<MonthlySalesInvoiceReportDto> GetMonthlySalesInvoiceReportAsync(int month, int year, int customerId)
        {
            var output = (await _customerRepo.GetAllAsync()).Where(x => x.Id == customerId).Select(s => new MonthlySalesInvoiceReportDto()
            {
                CustomerId = customerId,
                CustomerName = s.Name,
                Address = s.Address,
                PrepareDate = DateTime.Now
            }).First();

            var sales = (await _salesRepo.GetAllAsync())
                .Where(x => x.CustomerId == customerId && x.Date.Year == year && x.Date.Month == month)
                .Select(s => new MonthlySalesInvoiceDetailsReportDto() 
                {
                    Id = s.Id,
                    Date = s.Date.Date,
                    Amount = s.NetAmount
                }).ToList();

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

            var products = await _productRepo.GetAllListAsync();
            foreach(var s in sales)
            {
                foreach (var product in products)
                {
                    var thisProductsSales = salesDetails.Where(x => x.ProductId == product.Id && x.SaleId == s.Id).ToList();
                    if (thisProductsSales.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            s.MedicalOxygen9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            s.MedicalOxygen1_36Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            s.MedicalAir9_8Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            s.MedicalAir7Qty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            s.Nitros30KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            s.Nitros5KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            s.Nitros3KgQty = thisProductsSales.Sum(s => s.Quantity);
                        }
                    }
                }
            }

            sales = sales.GroupBy(t => t.Date).Select(g => new MonthlySalesInvoiceDetailsReportDto()
            {
                Date = g.Key,
                MedicalOxygen9_8Qty = g.Sum(s => s.MedicalOxygen9_8Qty),
                MedicalOxygen1_36Qty = g.Sum(s => s.MedicalOxygen1_36Qty),
                MedicalAir9_8Qty = g.Sum(s => s.MedicalAir9_8Qty),
                MedicalAir7Qty = g.Sum(s => s.MedicalAir7Qty),
                Nitros30KgQty = g.Sum(s => s.Nitros30KgQty),
                Nitros5KgQty = g.Sum(s => s.Nitros5KgQty),
                Nitros3KgQty = g.Sum(s => s.Nitros3KgQty),
                Amount = g.Sum(s => s.Amount)
            }).ToList();

            output.Details = sales;
            output.MedicalOxygen9_8TotalQty = sales.Sum(s => s.MedicalOxygen9_8Qty);
            output.MedicalOxygen1_36TotalQty = sales.Sum(s => s.MedicalOxygen1_36Qty);
            output.MedicalAir9_8TotalQty = sales.Sum(s => s.MedicalAir9_8Qty);
            output.MedicalAir7TotalQty = sales.Sum(s => s.MedicalAir7Qty);
            output.Nitros30KgTotalQty = sales.Sum(s => s.Nitros30KgQty);
            output.Nitros5KgTotalQty = sales.Sum(s => s.Nitros5KgQty);
            output.Nitros3KgTotalQty = sales.Sum(s => s.Nitros3KgQty);
            output.TotalAmount = sales.Sum(s => s.Amount);

            return output;
        }

        
        [UnitOfWork]
        public async Task SaleRemoveAsync(int saleId, int stockPointId)
        {
            var prevSalesDetails = await _salesDetailsRepo.GetAllListAsync(x => x.SaleId == saleId);
            foreach (var sd in prevSalesDetails)
            {
                var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == sd.ProductId && f.StockPointId == stockPointId);
                if (inventory != null)
                {
                    inventory.StockQty += sd.Quantity;
                    await _inventoryRepo.UpdateAsync(inventory);
                }
            }
            await _dueReceivedHistoryRepo.BatchDeleteAsync(x => x.SalesId == saleId);
            await _salesDetailsRepo.BatchDeleteAsync(x => x.SaleId == saleId);
            await _salesRepo.DeleteAsync(x => x.Id == saleId);
        }

        public async Task<bool> CheckReferenceNumberAsync(string referenceNumber, int? saleId)
        {
            if(saleId.HasValue)
            {
                return (await _salesRepo.GetAllAsync()).Any(x => x.Id != saleId && x.ReferenceNumber == referenceNumber);
            }
            else
            {
                return (await _salesRepo.GetAllAsync()).Any(x=> x.ReferenceNumber == referenceNumber);
            }
        }

        #endregion
    }
}
