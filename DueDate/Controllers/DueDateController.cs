using Microsoft.AspNetCore.Mvc;

namespace DueDate.Controllers
{
    [Route("api/duedate")]
    public class DueDateController : Controller
    {
        private readonly DateTime[] _holidays;
        private readonly string[] _paySpans;
        private const int MinDueDays = 10;

        public DueDateController()
        {
            _holidays = new[]
            {
                new DateTime(2023, 1, 1),
                new DateTime(2023, 5, 1),
                new DateTime(2023, 6, 8),
            };

            _paySpans = new[]
            {
                "weekly",
                "bi-weekly",
                "monthly",
            };
        }

        [HttpGet]
        public IActionResult Get(DateTime fundDay, string paySpan, DateTime payDate, bool hasDirectDeposit)
        {
            if (!_paySpans.Contains(paySpan))
            {
                return BadRequest("paySpan must have a value: weekly, bi-weekly or monthly");
            }

            return Ok(CalculateDueDate(fundDay.Date, _holidays, paySpan, payDate.Date, hasDirectDeposit));
        }

        private static DateTime CalculateDueDate(DateTime fundDay, DateTime[] holidays, string paySpan, DateTime payDate, bool hasDirectDeposit)
        {
            bool first = true;
            DateTime dueDate = payDate;
            
            do
            {
                if (!first)
                {
                    dueDate = payDate = GetNextDate(payDate, paySpan);
                }
                
                bool foward = true;

                if (!hasDirectDeposit)
                {
                    dueDate = dueDate.AddDays(1);
                }

                while (dueDate.DayOfWeek == DayOfWeek.Saturday || dueDate.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(dueDate))
                {
                    if (holidays.Contains(dueDate))
                    {
                        foward = false;
                    }
                    dueDate = foward ? dueDate.AddDays(1) : dueDate.AddDays(-1);
                }

                if (first) first = false;

            } while ((dueDate - fundDay).TotalDays < MinDueDays);

            return dueDate;
        }

        private static DateTime GetNextDate(DateTime dueDate, string paySpan)
        {
            return paySpan switch
            {
                "weekly" => dueDate.AddDays(7),
                "bi-weekly" => dueDate.AddDays(15),
                "monthly" => dueDate.AddMonths(1),
                _ => dueDate,
            };
        }
    }
}
