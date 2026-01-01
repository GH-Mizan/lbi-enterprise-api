using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using LbI.Entities;
using LbI.Salaries.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace LbI.Salaries
{
    public class SalaryAppService: LbIAppServiceBase, ISalaryAppService
    {
        private readonly IRepository<Salary> _salaryRepository;
        private readonly IRepository<SalaryAdvance> _salaryAdvanceRepository;
        private readonly IRepository<SalaryAdvanceHistory> _salaryAdvanceHistoryRepository;
        private readonly IRepository<Employee> _employeeRepository;
        public SalaryAppService(
            IRepository<Salary> salaryRepository,
            IRepository<SalaryAdvance> salaryAdvanceRepository,
            IRepository<SalaryAdvanceHistory> salaryAdvanceHistoryRepository,
            IRepository<Employee> employeeRepository
            )
        {
            _salaryRepository = salaryRepository;
            _salaryAdvanceRepository = salaryAdvanceRepository;
            _salaryAdvanceHistoryRepository = salaryAdvanceHistoryRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<List<SalaryAdvanceDto>> GetSalaryAdvanceListAsync()
        {
            var output = (from e in await _employeeRepository.GetAllAsync()
                          join sa in await _salaryAdvanceRepository.GetAllAsync() on e.Id equals sa.EmployeeId into advances
                          from sa in advances.DefaultIfEmpty()
                          where e.ActiveStatus
                          select new SalaryAdvanceDto()
                          {
                              Id = sa.Id,
                              EmployeeId = e.Id,
                              EmployeeName = e.Name,
                              PreviousSalary = sa == null ? 0 : sa.PreviousSalary,
                              CurrentSalary = sa == null ? 0 : sa.CurrentSalary,
                              Advance = sa == null ? 0 : sa.Advance,
                              LoanFromCompany = sa == null ? 0 : sa.LoanToCompany,
                              LoanToCompany = sa == null ? 0 : sa.LoanToCompany,
                              IncrementDate = sa.IncrementDate,
                              FromUi = true
                          }).ToList();
            return output;
        }

        public async Task<PagedResultDto<SalaryOutputDto>> GetPaginatedSalaryListAsync(SalariesFlterDto filter)
        {
            var query = (await _salaryRepository.GetAllAsync()).Where(x=> filter.EmployeeId == null || x.EmployeeId == filter.EmployeeId).GroupBy(t=> t.EmployeeId).Select(g=> new SalaryOutputDto()
            {
                EmployeeId = g.Key,
                Date = g.OrderByDescending(o => o.Date).FirstOrDefault().Date,
                Year = g.FirstOrDefault().Year,
                Month = g.FirstOrDefault().Month,
                Amount = g.Sum(s=> s.Amount),
                WorkingDays = g.FirstOrDefault().WorkingDays,
                Payable = g.FirstOrDefault().Payable,
                FullPaid= g.Any(x=> x.FullPaid)
            });
            var output = query.Skip(filter.Skip).Take(filter.Take).ToList();

            var employeeIds = output.Select(x => x.EmployeeId).ToList();

            var salaryAdvance = (from e in await _employeeRepository.GetAllAsync()
                                 join sa in await _salaryAdvanceRepository.GetAllAsync() on e.Id equals sa.EmployeeId
                                 where employeeIds.Contains(e.Id)
                                 select new
                                 {
                                     EmployeeId = e.Id,
                                     e.Name,
                                     sa.CurrentSalary
                                 }).ToList();

            foreach (var item in output)
            {
                var sa = salaryAdvance.FirstOrDefault(f => f.EmployeeId == item.EmployeeId);
                item.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                item.EmployeeName = sa.Name;
                item.CurrentSalary = sa.CurrentSalary;
            }
            return new PagedResultDto<SalaryOutputDto>()
            {
                Items = output,
                TotalCount = query.Count()
            };

            //var query = (from s in await _salaryRepository.GetAllAsync()
            //             join e in await _employeeRepository.GetAllAsync() on s.EmployeeId equals e.Id
            //             join sa in await _salaryAdvanceRepository.GetAllAsync() on e.Id equals sa.EmployeeId
            //             where filter.EmployeeId == null || s.EmployeeId == filter.EmployeeId
            //             select new SalaryOutputDto()
            //             {
            //                 Id = s.Id,
            //                 EmployeeId = e.Id,
            //                 EmployeeName = e.Name,
            //                 Date = s.Date,
            //                 Year = s.Year,
            //                 Month = s.Month,
            //                 Amount = s.Amount,
            //                 CurrentSalary = sa.CurrentSalary,
            //                 WorkingDays = s.WorkingDays,
            //                 Payable = s.Payable,
            //                 FullPaid = s.FullPaid
            //             })
            //             .OrderByDescending(o => o.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.EmployeeId)
            //             .AsQueryable();

        }

        public async Task<List<SalaryEntryOutputDto>> GetSalaryEntryInfoAsync()
        {
            //var output = (await _employeeRepository.GetAllAsync()).Where(x => x.ActiveStatus).Select(s => new SalaryEntryOutputDto()
            //{
            //    EmployeeId = s.Id,
            //    EmployeeName = s.Name
            //}).ToList();

            var salaryInfo = (await _salaryRepository.GetAllAsync()).GroupBy(t => t.EmployeeId).Select(g => new
            {
                EmployeeId = g.Key,
                g.OrderByDescending(g=> g.Year).ThenByDescending(g=> g.Month).FirstOrDefault().Month,
                g.OrderByDescending(g => g.Year).ThenByDescending(g => g.Month).FirstOrDefault().Year,
                //LastTotalAmount = g.OrderByDescending(g => g.Year).ThenByDescending(g => g.Month).Sum(s=> s.Amount), // ***
                LastTotalAmount = g.Where(x=> x.Year == g.OrderByDescending(g => g.Year).ThenByDescending(g => g.Month).FirstOrDefault().Year && x.Month == g.OrderByDescending(g => g.Year).ThenByDescending(g => g.Month).FirstOrDefault().Month).Sum(s => s.Amount),
                FullPaid = g.Any(x=> x.FullPaid),
                g.FirstOrDefault().Payable,
                g.FirstOrDefault().WorkingDays
            });

            var output = (from e in await _employeeRepository.GetAllAsync()
                     join sa in await _salaryAdvanceRepository.GetAllAsync() on e.Id equals sa.EmployeeId
                     select new SalaryEntryOutputDto()
                     {
                         EmployeeId = e.Id,
                         EmployeeName = e.Name,
                         CurrentSalary = sa.CurrentSalary,
                         Advance = sa.Advance,
                         SalaryDate = DateTime.UtcNow.Date
                     }).ToList();

            foreach (var item in output) 
            {
                var thisSalaryInfo = salaryInfo.FirstOrDefault(t => t.EmployeeId == item.EmployeeId);
                if(thisSalaryInfo != null)
                {
                    var lm = thisSalaryInfo.Month;
                    var ly = thisSalaryInfo.Year;
                    item.LastSalaryMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(lm) + " " + ly;
                    item.LastSalaryAmount = item.CurrentSalary == thisSalaryInfo.LastTotalAmount ? 0 : thisSalaryInfo.LastTotalAmount;
                    item.Paid = item.LastSalaryAmount;
                    item.FullPaid = thisSalaryInfo.FullPaid;
                    item.Payable = thisSalaryInfo.Payable;
                    item.Due = item.LastSalaryAmount == 0 ? 0 : item.Payable - item.Paid;
                    item.WorkingDays = thisSalaryInfo.WorkingDays;
                    if (item.Due == 0)
                    {
                        var lastMonthStart = new DateTime(ly, lm, 1);
                        var nextMonthStart = lastMonthStart.AddMonths(1);
                        item.Year = nextMonthStart.Year;
                        item.Month = nextMonthStart.Month;
                    }
                    else
                    {
                        item.Year = ly;
                        item.Month = lm;
                    }
                }
                else
                {
                    item.Year = DateTime.UtcNow.Year;
                    item.Month = DateTime.UtcNow.Month;
                    item.WorkingDays = DateTime.DaysInMonth(item.Year, item.Month.Value);
                    item.Payable = item.CurrentSalary;
                }
                item.TotalDays = DateTime.DaysInMonth(item.Year, item.Month.Value);
            }
            return output;
        }

        [UnitOfWork]
        public async Task CreateOrUpdateBulkSalaryAsync(List<SalaryEntryInputDto> input)
        {
            foreach (var item in input)
            {
                await CreateOrUpdateSalaryAsync(item); ;
            }
        }

        public async Task CreateOrUpdateSalaryAsync(SalaryEntryInputDto input)
        {
            if (input.Id.HasValue)
            {
                var entity = await _salaryRepository.SingleAsync(x => x.Id == input.Id);
                ObjectMapper.Map(input, entity);
                await _salaryRepository.UpdateAsync(entity);
            }
            else
            {
                var entity = ObjectMapper.Map<Salary>(input);
                await _salaryRepository.InsertAsync(entity);
            }
        }

        [UnitOfWork]
        public async Task CreateOrUpdateBulkSalaryAdvanceAsync(List<SalaryAdvanceDto> input)
        {
            foreach (var item in input) 
            { 
                await CreateOrUpdateSalaryAdvanceAsync(item); ;
            }
        }

        [UnitOfWork]
        public async Task CreateOrUpdateSalaryAdvanceAsync(SalaryAdvanceDto input)
        {
            var salaryAdvanceId = input.Id;
            if(input.PreviousSalary < input.CurrentSalary) input.IncrementDate = DateTime.UtcNow;
            if(input.Id.HasValue)
            {
                var entity = await _salaryAdvanceRepository.SingleAsync(x=> x.Id == input.Id);
                ObjectMapper.Map(input, entity);
                await _salaryAdvanceRepository.UpdateAsync(entity);
            }
            else
            {
                var entity = ObjectMapper.Map<SalaryAdvance>(input);
                salaryAdvanceId = await _salaryAdvanceRepository.InsertAndGetIdAsync(entity);
            }
            input.Id = null;
            var salaryAdvanceHistory = ObjectMapper.Map<SalaryAdvanceHistory>(input);
            salaryAdvanceHistory.SalaryAdvanceId = salaryAdvanceId.Value;
            await _salaryAdvanceHistoryRepository.InsertAsync(salaryAdvanceHistory);

        }
    }
}
