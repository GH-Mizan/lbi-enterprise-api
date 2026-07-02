using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using LbI.DailyCashes.Dto;
using LbI.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.DailyCashes
{
    public class DailyCashAppService : LbIAppServiceBase, IDailyCashAppService
    {
        private readonly IRepository<DailyCash> _dailyCashRepo;
        private readonly IRepository<Voucher> _voucherRepository;
        private readonly IRepository<DailyCashIncomeDetail> _incomeDetailRepository;
        private readonly IRepository<DailyCashExpenseDetail> _expenseDetailRepository;
        private readonly IRepository<DailyCashAdvanceDetail> _advanceDetailRepository;
        private readonly IRepository<DailyCashDayEndDetail> _dayEndDetailRepository;
        public DailyCashAppService(
            IRepository<DailyCash> dailyCashRepo,
            IRepository<Voucher> voucherRepository,
            IRepository<DailyCashIncomeDetail> incomeDetailRepository,
            IRepository<DailyCashExpenseDetail> expenseDetailRepository,
            IRepository<DailyCashAdvanceDetail> advanceDetailRepository,
            IRepository<DailyCashDayEndDetail> dayEndDetailRepository)
        {
            _dailyCashRepo = dailyCashRepo;
            _voucherRepository = voucherRepository;
            _incomeDetailRepository = incomeDetailRepository;
            _expenseDetailRepository = expenseDetailRepository;
            _advanceDetailRepository = advanceDetailRepository;
            _dayEndDetailRepository = dayEndDetailRepository;
        }

        public async Task<PagedResultDto<DailyCashOutputDto>> GetPaginatedDailyCashAsync(DailyCashFilterDto filter)
        {
            var query = await _dailyCashRepo.GetAllAsync();

            if(filter.StartDate.HasValue && filter.EndDate.HasValue)
            {

            }

            var data = await query.OrderByDescending(o=> o.Date).Skip(filter.Skip).Take(filter.Take).ToListAsync();
            
            return new PagedResultDto<DailyCashOutputDto>()
            {
                Items = ObjectMapper.Map<List<DailyCashOutputDto>>(data),
                TotalCount = query.Count()
            };
        }

        public async Task<CreateOrUpdateDailyCashInput> GetAsync(int id)
        {
            var entity = await _dailyCashRepo.GetAsync(id);
            var incomes = (await _incomeDetailRepository.GetAllListAsync(x=> x.DailyCashId == id)).Select(s => new DailyCashIncomeDetailDto()
            {
                Id = s.Id,
                DailyCashId = s.DailyCashId,
                IncomeId = s.IncomeId,
                Key = s.Key,
                Head = s.Head,
                Amount = s.Amount
            }).ToList();
            var expenses = (await _expenseDetailRepository.GetAllListAsync(x => x.DailyCashId == id)).Select(s => new DailyCashExpenseDetailDto()
            {
                Id = s.Id,
                DailyCashId = s.DailyCashId,
                ExpenseId = s.ExpenseId,
                Key = s.Key,
                Head = s.Head,
                Amount = s.Amount
            }).ToList();
            var advances = (await _advanceDetailRepository.GetAllListAsync(x => x.DailyCashId == id)).Select(s => new DailyCashAdvanceDetailDto()
            {
                Id = s.Id,
                DailyCashId = s.DailyCashId,
                EmployeeId = s.EmployeeId,
                Type = s.Type,
                Head = s.Head,
                Amount = s.Amount
            }).ToList();
            var cashes = (await _dayEndDetailRepository.GetAllListAsync(x => x.DailyCashId == id)).Select(s => new DailyCashDayEndDetailDto()
            {
                Id = s.Id,
                DailyCashId = s.DailyCashId,
                EmployeeId = s.EmployeeId,
                Type = s.Type,
                Head = s.Head,
                Amount = s.Amount
            }).ToList();
            return new CreateOrUpdateDailyCashInput()
            {
               DailyCash = ObjectMapper.Map<DailyCashEntryDto>(entity),
               Incomes = incomes,
               Expenses = expenses,
               Advances = advances,
               DayEndCashes = cashes
            };
        }

        public async Task<CreateOrUpdateDailyCashInput> GetByDateAsync(DateTime date)
        {
            var output = new CreateOrUpdateDailyCashInput();
            //if (date.Day == 8 && date.Day == 8 && date.Year == 2025)
            //{
            //    var entity = await _dailyCashRepo.FirstOrDefaultAsync(x => x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year);
            //    if(entity != null)
            //    {
            //        output.DailyCashInfo = ObjectMapper.Map<DailyCashEntryDto>(entity);
            //    }
            //}
            //else
            //{
            //    var expectedCash = await _dailyCashRepo.FirstOrDefaultAsync(x => x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year);
            //    if(expectedCash != null)
            //    {
            //        output.PrevCashInfo = null;
            //        output.DailyCashInfo = ObjectMapper.Map<DailyCashEntryDto>(expectedCash);
            //    }
            //    else
            //    {
            //        var prevCash = await _dailyCashRepo.FirstOrDefaultAsync(x => x.Date.Day == date.Day - 1 && x.Date.Month == date.Month && x.Date.Year == date.Year);
            //        output.PrevCashInfo = ObjectMapper.Map<DailyCashEntryDto>(prevCash);
            //    }
            //}
            return output;
            
        }
        public async Task<bool> CheckByDate(DateTime date)
        {
            var count = await _dailyCashRepo.CountAsync(x => x.Date.Day == date.Day);
            return count > 0;
        }

        public async Task<DateTime?> GetLastEntryDateAsync()
        {
            var lastDate = (await _dailyCashRepo.GetAllAsync()).OrderByDescending(x => x.Date).FirstOrDefault()?.Date;
            return lastDate;
        }

        [UnitOfWork]
        public async Task<int> CreateOrUpdateDailyCashAsync(CreateOrUpdateDailyCashInput input)
        {
            var id = input.DailyCash.Id;
            if (id.HasValue)
            {
                var dailyCash = await _dailyCashRepo.GetAsync(id.Value);
                ObjectMapper.Map(input.DailyCash, dailyCash);

                await _incomeDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
                await _expenseDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
                await _advanceDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
                await _dayEndDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);

                await _dailyCashRepo.UpdateAsync(dailyCash);
            }
            else
            {
                var dailyCash = ObjectMapper.Map<DailyCash>(input.DailyCash);
                id = await _dailyCashRepo.InsertAndGetIdAsync(dailyCash);
            }

            await InsertDailyCashDetails(input, id.Value);
            return id.Value;

        }

        [UnitOfWork]
        public async Task<int> CreateOrUpdateVouchersAndDailyCashAsync(VouchersAndDailyCashCreateUpdateDto input)
        {
            var id = await CreateOrUpdateDailyCashAsync(input.DailyCash);
            await CreateOrUpdateVouchersAsync(id, input.Vouchers);
            return id;
        }

        private async Task InsertDailyCashDetails(CreateOrUpdateDailyCashInput input, int dailyCashId)
        {
            foreach (var inc in input.Incomes)
            {
                await _incomeDetailRepository.InsertAsync(new DailyCashIncomeDetail()
                {
                    DailyCashId = dailyCashId,
                    IncomeId = inc.IncomeId,
                    Key = inc.Key,
                    Head = inc.Head,
                    Amount = inc.Amount,
                    MtmKey = inc.MtmKey,
                    Uid = inc.Uid
                });
            }

            foreach (var exp in input.Expenses)
            {
                await _expenseDetailRepository.InsertAsync(new DailyCashExpenseDetail()
                {
                    DailyCashId = dailyCashId,
                    ExpenseId = exp.ExpenseId,
                    Key = exp.Key,
                    Head = exp.Head,
                    Amount = exp.Amount,
                    Uid = exp.Uid
                });
            }

            foreach (var adv in input.Advances)
            {
                await _advanceDetailRepository.InsertAsync(new DailyCashAdvanceDetail()
                {
                    DailyCashId = dailyCashId,
                    EmployeeId = adv.EmployeeId,
                    Type = adv.Type,
                    Head = adv.Head,
                    Amount = adv.Amount,
                    Uid = adv.Uid
                });
            }

            foreach (var de in input.DayEndCashes)
            {
                await _dayEndDetailRepository.InsertAsync(new DailyCashDayEndDetail()
                {
                    DailyCashId = dailyCashId,
                    EmployeeId = de.EmployeeId,
                    Type = de.Type,
                    Head = de.Head,
                    Amount = de.Amount,
                    MtmKey = de.MtmKey,
                    Uid = de.Uid
                });
            }
        }

        [UnitOfWork]
        public async Task CreateOrUpdateVouchersAsync(int dailyCashId, List<VoucherEntryDto> input)
        {
            await VouchersRemoveAsync(dailyCashId);

            foreach (var item in input)
            {
                await _voucherRepository.InsertAsync(
                    new Voucher()
                    {
                        Date = item.Date,
                        DailyCashId = dailyCashId,
                        VoucherNumber = item.VoucherNumber,
                        CreatorId = item.CreatorId,
                        CarNumber = item.CarNumber,
                        TotalAmount = item.TotalAmount,
                        IncomeRecords = item.IncomeRecords,
                        ExpenseRecords = item.ExpenseRecords,
                        DayEndCash = item.DayEndCash
                    });
            }
        }

        public async Task<List<VoucherOutputDto>> GetVouchersAsync(int dailyCashId)
        {
            return (await _voucherRepository.GetAllListAsync(f => f.DailyCashId == dailyCashId)).Select(s => new VoucherOutputDto()
            {
                Id = s.Id,
                Date = s.Date,
                VoucherNumber = s.VoucherNumber,
                CreatorId = s.CreatorId,
                CarNumber = s.CarNumber,
                TotalAmount = s.TotalAmount,
                IncomeRecords = s.IncomeRecords,
                ExpenseRecords = s.ExpenseRecords,
                DayEndCash = s.DayEndCash,
            }).ToList();
        }

        //public async Task<VouchersOutputDto> GetVouchersAsync(DateTime date)
        //{
        //    var output = new VouchersOutputDto()
        //    {
        //        Vouchers = (await _voucherRepository.GetAllListAsync(f => f.Date.Date == date.Date)).Select(s => new VoucherOutputDto()
        //        {
        //            Id = s.Id,
        //            Date = date,
        //            VoucherNumber = s.VoucherNumber,
        //            CreatorId = s.CreatorId,
        //            CarNumber = s.CarNumber,
        //            TotalAmount = s.TotalAmount,
        //            IncomeRecords = s.IncomeRecords,
        //            ExpenseRecords = s.ExpenseRecords,
        //            DayEndCash = s.DayEndCash,
        //        }).ToList(),
        //        IsAny = await _voucherRepository.CountAsync() > 0
        //    };
        //    if(output.Vouchers.Count > 0)
        //    {
        //        if (!(await _voucherRepository.CountAsync(x=> x.Date.Date > date.Date) > 0))
        //            output.MostRecennt = true;
        //    }
        //    if(await _dailyCashRepo.CountAsync(x=> x.Date.Date == date.Date) > 0)
        //        output.HasDailyCash = true;
        //    return output;
        //}
        public async Task<DateTime> GetStartDateAsync()
        {
            var lastCash = (await _dailyCashRepo.GetAllAsync()).OrderByDescending(s => s.Date).FirstOrDefault();
            if (lastCash == null)
                return DateTime.UtcNow;
            else
            {
                return lastCash.Completed ? lastCash.Date.AddDays(1) : lastCash.Date;
            }
        }
        public async Task<VoucherFirstLastDateDto> GetVoucherFirstLastDateAsync()
        {
            var lastDate = (await _voucherRepository.GetAllAsync()).OrderByDescending(o => o.Date).FirstOrDefault()?.Date;
            return new VoucherFirstLastDateDto()
            {
                FirstDate = (await _voucherRepository.GetAllAsync()).OrderBy(o => o.Date).FirstOrDefault()?.Date,
                CurrentDate = lastDate == null ? DateTime.UtcNow : lastDate.Value.AddDays(1)
            };
        }

        [UnitOfWork]
        public async Task DailyCashRemoveAsync(int id)
        {
            await _dailyCashRepo.DeleteAsync(id);
            await _incomeDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
            await _expenseDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
            await _advanceDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
            await _dayEndDetailRepository.BatchDeleteAsync(x => x.DailyCashId == id);
            await _voucherRepository.BatchDeleteAsync(x => x.DailyCashId == id);
        }

        public async Task VouchersRemoveAsync(int dailyCashIId)
        {
            if (await _voucherRepository.CountAsync(x => x.DailyCashId == dailyCashIId) > 0)
            {
                await _voucherRepository.BatchDeleteAsync(x => x.DailyCashId == dailyCashIId);
            }
        }

        public async Task<DailyCashOutputDto> GetPreviousDailyCashAsync(DateTime currentDate)
        {
            var entity = await _dailyCashRepo.FirstOrDefaultAsync(f => f.Date.Date == currentDate.Date);
            var isNew = false;
            var isLast = false;
            //var output = new DailyCashOutputDto();
            if (entity == null)
            {
                entity = (await _dailyCashRepo.GetAllAsync()).Where(x => x.Date.Date < currentDate.Date).OrderByDescending(o => o.Date).First();
                isNew = true;
                //if(entity.Completed)
                //    isNew = true;
            }
            else
            {  
                isLast = await _dailyCashRepo.CountAsync(f => f.Date.Date > currentDate.Date) == 0;
            }

            var incomes = (await _incomeDetailRepository.GetAllListAsync(x => x.DailyCashId == entity.Id)).Select(s => new DailyCashIncomeDetailDto()
            {
                Id = s.Id,
                DailyCashId = entity.Id,
                IncomeId = s.IncomeId,
                Key = s.Key,
                Head = s.Head,
                Amount = s.Amount,
                MtmKey = s.MtmKey,
                Uid = s.Uid
            }).ToList();
            var expenses = (await _expenseDetailRepository.GetAllListAsync(x => x.DailyCashId == entity.Id)).Select(s => new DailyCashExpenseDetailDto()
            {
                Id = s.Id,
                DailyCashId = entity.Id,
                ExpenseId = s.ExpenseId,
                Key = s.Key,
                Head = s.Head,
                Amount = s.Amount,
                Uid = s.Uid
            }).ToList();
            var advances = (await _advanceDetailRepository.GetAllListAsync(x => x.DailyCashId == entity.Id)).Select(s => new DailyCashAdvanceDetailDto()
            {
                Id = s.Id,
                DailyCashId = entity.Id,
                EmployeeId = s.EmployeeId,
                Type = s.Type,
                Head = s.Head,
                Amount = s.Amount,
                Uid = s.Uid
            }).ToList();
            var dayEndCashes = (await _dayEndDetailRepository.GetAllListAsync(x => x.DailyCashId == entity.Id)).Select(s => new DailyCashDayEndDetailDto()
            {
                Id = s.Id,
                DailyCashId = entity.Id,
                EmployeeId = s.EmployeeId,
                Type = s.Type,
                Head = s.Head,
                Amount = s.Amount,
                MtmKey = s.MtmKey,
                Uid = s.Uid
            }).ToList();
            var vouchers = isNew ? new List<VoucherOutputDto>() : await GetVouchersAsync(entity.Id);
            return new DailyCashOutputDto()
            {
                Id = isNew ? null : entity.Id,
                Date = entity.Date,
                TotalIncome = entity.TotalIncome,
                TotalExpense = entity.TotalExpense,
                TotalAdvance = entity.TotalAdvance,
                TotalDayEndCash = entity.TotalDayEndCash,
                PaperBalance = entity.PaperBalance,
                ActualBalance = entity.ActualBalance,
                Difference = entity.Difference,
                TotalDue = entity.TotalDue,
                BalanceCD = entity.BalanceCD,
                ActualDifference = entity.ActualDifference,
                Completed = entity.Completed,
                Remarks = entity.Remarks,
                IsNew = isNew,
                IsLast = isLast,
                Incomes = incomes,
                Expenses = expenses,
                Advances = advances,
                DayEndCashes = dayEndCashes,
                Vouchers = vouchers,
            };


        }
    }
}
