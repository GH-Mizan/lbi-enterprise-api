using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using LbI.Purchases.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LbI.Purchases
{
    public class PurchaseAppService : LbIAppServiceBase, IPurchaseAppService
    {
        private readonly IRepository<Purchase> _purchaseRepo;
        private readonly IRepository<PurchaseDetail> _purchaseDetailsRepo;
        private readonly IRepository<DuePaymentHistory> _duePaymentHistoryRepo;
        private readonly IRepository<Inventory> _inventoryRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<StockPoint> _vehicleRepo;
        private readonly IRepository<LbiSetting> _lbiSettingsRepo;
        private readonly IRepository<Supplier> _supplierRepo;
        public PurchaseAppService(
            IRepository<Purchase> purchaseRepo,
            IRepository<PurchaseDetail> purchaseDetailsRepo,
            IRepository<Inventory> inventoryRepo,
            IRepository<Product> productRepo,
            IRepository<StockPoint> vehicleRepo,
            IRepository<LbiSetting> lbiSettingsRepo,
            IRepository<DuePaymentHistory> duePaymentHistoryRepo,
            IRepository<Supplier> supplierRepo
            )
        {
            _purchaseRepo = purchaseRepo;
            _purchaseDetailsRepo = purchaseDetailsRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _vehicleRepo = vehicleRepo;
            _lbiSettingsRepo = lbiSettingsRepo;
            _duePaymentHistoryRepo = duePaymentHistoryRepo;
            _supplierRepo = supplierRepo;
        }

        public async Task<PagedResultDto<PurchaseOutputDto>> GetPaginatedPurchasesAsync(PurchasesFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from p in await _purchaseRepo.GetAllAsync()
                         join v in await _vehicleRepo.GetAllAsync() on p.StockPointId equals v.Id
                         select new PurchaseOutputDto()
                         {
                             Id = p.Id,
                             Date = p.Date,
                             InvoiceNumber = p.InvoiceNumber,
                             SupplierId = p.SupplierId,
                             SupplierName = p.SupplierName,
                             TotalAmount = p.TotalAmount,
                             Discount = p.Discount,
                             NetAmount = p.NetAmount,
                             PaidAmount = p.PaidAmount,
                             DueAmount = p.DueAmount,
                             PaymentStatus = p.PaymentStatus,
                             StockPointId = p.StockPointId,
                             StockPointName = v.Name,
                             Remarks = p.Remarks,
                             Locked = p.Locked
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.InvoiceNumber.ToLower().Contains(searchText) ||
                x.SupplierName.ToLower().Contains(searchText) ||
                x.StockPointName.ToLower().Contains(searchText)
                );
            }

            var items = query.OrderByDescending(o => o.Id).Skip(filter.Skip).Take(filter.Take).ToList();
            foreach (var p in items)
            {
                p.PaymentStatusText = p.PaymentStatus.DisplayName();
            }

            return new PagedResultDto<PurchaseOutputDto>()
            {
                Items = items,
                TotalCount = query.Count()
            };
        }

        public async Task<List<PurchaseProductDto>> GetAllProductsAsync(int? stockPointId)
        {
            var output = new List<PurchaseProductDto>();
            output = (from p in await _productRepo.GetAllAsync()
                     //join i in await _inventoryRepo.GetAllAsync() on p.Id equals i.ProductId into inventory
                     //from i in inventory.DefaultIfEmpty()
                     where p.ActiveStatus
                     select new PurchaseProductDto()
                     {
                         ProductId = p.Id,
                         Name = p.Name,
                         Size = p.Size,
                         Type = p.Type,
                         PurchasePrice = p.PurchasePrice,
                         PurchasePriceDisabled = true,
                         QtyDisabled = true,
                         Quantity = 0,
                         TotalPrice = 0,
                         //Stock = i.StockQty,
                         TypeText = p.Type.DisplayName(),
                         SizeText = p.Size.DisplayName()
                     }).ToList();
            if (stockPointId != null) 
            { 
                var stocks = await _inventoryRepo.GetAllListAsync(x=> x.StockPointId == stockPointId);
                foreach (var item in output) 
                {
                    var stock = stocks.Where(x => x.ProductId == item.ProductId).FirstOrDefault()?.StockQty;
                    item.Stock = stock != null ? stock : 0; 
                }
            }

            return output;
        }

        public async Task<PurchaseEntryInput> GetAsync(int id)
        {
            var entity = await _purchaseRepo.GetAsync(id);
            var details = await _purchaseDetailsRepo.GetAllListAsync(x=> x.PurchaseId == id);

            return new PurchaseEntryInput()
            {
                Purchase = ObjectMapper.Map<PurchaseEntryDto>(entity),
                PurchaseDetails = ObjectMapper.Map<List<PurchaseDetailsEntryDto>>(details)
            };
        }

        public async Task<string> GetLastInvoiceNumberAsync()
        {
            var settings = await _lbiSettingsRepo.FirstOrDefaultAsync(x => x.Key == InitialSetupKey.LastPurchaseInvoiceNumber);
            if (settings != null)
            {
                return settings.Value;
            } 
            else
            {
                await _lbiSettingsRepo.InsertAsync(new LbiSetting()
                {
                    Key = InitialSetupKey.LastPurchaseInvoiceNumber,
                    Value = "1001"
                });
                return "1001";
            }
        }

        [UnitOfWork]
        public async Task CreateOrUpdateAsync(PurchaseEntryInput input)
        {
            var id = input.Purchase.Id;
            if (id.HasValue)
            {
                var purchase = await _purchaseRepo.GetAsync(id.Value);
                ObjectMapper.Map(input.Purchase, purchase);
                await _purchaseRepo.UpdateAsync(purchase);

                var prevPurchaseDetails = await _purchaseDetailsRepo.GetAllListAsync(x => x.PurchaseId == id);
                foreach (var pd in prevPurchaseDetails)
                {
                    var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == input.Purchase.StockPointId);
                    if (inventory != null)
                    {
                        inventory.StockQty -= pd.Quantity;
                        await _inventoryRepo.UpdateAsync(inventory);
                    }
                }
                await _purchaseDetailsRepo.BatchDeleteAsync(x => x.PurchaseId == id);
                await InsertPurchaseDetails(input.PurchaseDetails, id.Value, input.Purchase.StockPointId);

                await _duePaymentHistoryRepo.DeleteAsync(x => x.PurchaseId == id);
                await InsertDuePaymentAsync(input.DuePayment);
            }
            else
            {
                var purchase = ObjectMapper.Map<Purchase>(input.Purchase);
                id = await _purchaseRepo.InsertAndGetIdAsync(purchase);

                await InsertPurchaseDetails(input.PurchaseDetails, id.Value, input.Purchase.StockPointId);
                input.DuePayment.PurchaseId = id.Value;
                await InsertDuePaymentAsync(input.DuePayment);

                var invoiceSettings = await _lbiSettingsRepo.SingleAsync(x => x.Key == InitialSetupKey.LastPurchaseInvoiceNumber);
                invoiceSettings.Value = (Convert.ToInt32(invoiceSettings.Value) + 1).ToString();
                await _lbiSettingsRepo.UpdateAsync(invoiceSettings);

            }
        }

        [UnitOfWork]
        public async Task DuePaymentEntryAsync(DuePaymentEntryDto input)
        {
            var purchase = await _purchaseRepo.SingleAsync(s => s.Id == input.PurchaseId);
            purchase.Discount += input.Discount;
            purchase.NetAmount = input.NetTotal;
            purchase.PaidAmount += input.TotalPaid;
            purchase.DueAmount = input.Due;
            purchase.Remarks = input.Remarks;
            purchase.PaymentStatus = purchase.DueAmount == 0 ? PaymentStatus.Paid : purchase.TotalAmount > purchase.DueAmount ? PaymentStatus.Partialpaid : PaymentStatus.Due;
            await _purchaseRepo.UpdateAsync(purchase);
            await InsertDuePaymentAsync(input.DuePayment);
        }

        public async Task<List<DuePaymentHistoryDto>> GetDuePaymentHistoriesAsync(int purchaseId)
        {
            var histories = (await _duePaymentHistoryRepo.GetAllListAsync(x => x.PurchaseId == purchaseId)).OrderBy(o=>o.CreationTime).ToList();
            return ObjectMapper.Map<List<DuePaymentHistoryDto>>(histories);
        }

        private async Task InsertPurchaseDetails(List<PurchaseDetailsEntryDto> purchaseDetailsInput, int purchaseId, int vehicleId)
        {
            var purchaseDetails = new List<PurchaseDetail>();
            foreach (var pd in purchaseDetailsInput)
            {
                var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == vehicleId);
                if (inventory != null)
                {
                    inventory.StockQty += pd.Quantity;
                    await _inventoryRepo.UpdateAsync(inventory);
                }
                else
                {
                    inventory = new Inventory()
                    {
                        ProductId = pd.ProductId,
                        StockPointId = vehicleId,
                        ProductName = pd.ProductName,
                        StockQty = pd.Quantity,
                    };
                    await _inventoryRepo.InsertAsync(inventory);
                }

                var detail = ObjectMapper.Map<PurchaseDetail>(pd);
                detail.PurchaseId = purchaseId;
                purchaseDetails.Add(detail);
            }
            await _purchaseDetailsRepo.InsertRangeAsync(purchaseDetails);
        }

        public List<ComboboxItemDto> GetPaymentStatusSelectListAsync()
        {
            var output = ((PaymentStatus[])Enum.GetValues(typeof(PaymentStatus))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
            return output;
        }

        private async Task InsertDuePaymentAsync(DuePaymentHistoryDto input)
        {
            var dp = ObjectMapper.Map<DuePaymentHistory>(input);
            await _duePaymentHistoryRepo.InsertAsync(dp);
        }

        public async Task<DailyPurchaseReportDto> GetDailyPurchaseReportAsync(DateTime date)
        {
            var purchases = await _purchaseRepo.GetAllListAsync(f => f.Date.Date == date.Date);
            if (purchases.Count == 0)
                return new DailyPurchaseReportDto();

            var products = await _productRepo.GetAllListAsync();
            var purchaseIds = purchases.Select(s => s.Id).ToList();
            var purchaseDetails = (from pd in await _purchaseDetailsRepo.GetAllAsync()
                                join p in await _productRepo.GetAllAsync() on pd.ProductId equals p.Id
                                where purchaseIds.Contains(pd.PurchaseId)
                                select new
                                {
                                    pd.PurchaseId,
                                    pd.ProductId,
                                    ProductType = p.Type,
                                    p.Size,
                                    pd.Quantity,
                                    pd.TotalPrice
                                }).ToList();

            //var histories = await _duePaymentHistoryRepo.GetAllListAsync(x => x.PaymentDate.Date == date.Date && !x.Default);
            var histories = (from h in (await _duePaymentHistoryRepo.GetAllAsync())
                             .Where(x => x.PaymentDate.Date == date.Date && !x.Default).GroupBy(t => t.PurchaseId)
                             .Select(g => new
                             {
                                 PurchaseId = g.Key,
                                 TotalPaid = g.Sum(s => s.TotalPaid)
                             })
                             join p in await _purchaseRepo.GetAllAsync() on h.PurchaseId equals p.Id
                             join s in await _supplierRepo.GetAllAsync() on p.SupplierId equals s.Id
                             select new
                             {
                                 h.PurchaseId,
                                 p.SupplierId,
                                 SupplierName = s.Name,
                                 h.TotalPaid,
                                 p.InvoiceNumber
                             }).ToList();


            var details = new List<DailyPurchaseReportDetailsDto>();
            foreach (var p in purchases)
            {
                var item = new DailyPurchaseReportDetailsDto()
                {
                    SupplierId = p.SupplierId,
                    SupplierName = p.SupplierName,
                    InvoiceNo = p.InvoiceNumber,
                    PaymentStatus = p.PaymentStatus,
                    PaymentStatusText = p.PaymentStatus.DisplayName(),
                    NetAmount = p.NetAmount,
                    PaidAmount = p.PaidAmount,
                    DueAmount = p.DueAmount
                };

                foreach (var product in products)
                {
                    var thisProductsPurchases = purchaseDetails.Where(x => x.ProductId == product.Id && x.PurchaseId == p.Id).ToList();
                    if (thisProductsPurchases.Count > 0)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            item.MedicalOxygen9_8Qty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            item.MedicalOxygen1_36Qty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            item.MedicalAir9_8Qty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            item.MedicalAir7Qty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros30KgQty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros5KgQty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            item.Nitros3KgQty = thisProductsPurchases.Sum(s => s.Quantity);
                        }
                    }
                }
                details.Add(item);
            }

            var supplierIds = details.Select(x => x.SupplierId).ToList();
            if (supplierIds.Count > supplierIds.Distinct().Count())
            {
                details = details.GroupBy(t => t.SupplierId).Select(g => new DailyPurchaseReportDetailsDto()
                {
                    SupplierId = g.Key,
                    SupplierName = g.First().SupplierName,
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

            foreach (var h in histories)
            {
                var d = details.FirstOrDefault(f => f.SupplierId == h.SupplierId);
                if (d != null)
                {
                    d.DuePayment = h.TotalPaid;
                }
                else
                {
                    var item = new DailyPurchaseReportDetailsDto()
                    {
                        SupplierId = h.SupplierId,
                        SupplierName = h.SupplierName,
                        InvoiceNo = h.InvoiceNumber,
                        DuePayment = h.TotalPaid
                    };
                    details.Add(item);
                }
            }

            var output = new DailyPurchaseReportDto()
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
                CashPayment = details.Sum(s => s.PaidAmount),
                Due = details.Sum(s => s.DueAmount),
                DuePayment = details.Sum(s => s.DuePayment),
            };

            return output;
        }

        [UnitOfWork]
        public async Task DeleteAsync(int purchaseId, int stockPointId)
        {
            var prevPurchaseDetails = await _purchaseDetailsRepo.GetAllListAsync(x => x.PurchaseId == purchaseId);
            foreach (var pd in prevPurchaseDetails)
            {
                var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == stockPointId);
                if (inventory != null)
                {
                    inventory.StockQty -= pd.Quantity;
                    await _inventoryRepo.UpdateAsync(inventory);
                }
            }
            await _duePaymentHistoryRepo.BatchDeleteAsync(x=> x.PurchaseId == purchaseId);
            await _purchaseDetailsRepo.BatchDeleteAsync(x => x.PurchaseId == purchaseId);
            await _purchaseRepo.DeleteAsync(x=> x.Id == purchaseId);
        }
    }
}
