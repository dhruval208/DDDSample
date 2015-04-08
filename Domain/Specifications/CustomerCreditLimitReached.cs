﻿using System.Linq;
using Domain.Infrastructure;
using Domain.Models;

namespace Domain.Specifications
{
    public class CustomerCreditLimitReached:ICustomerCreditLimitReached
    {
        readonly IAppContext _context;

        public CustomerCreditLimitReached(IAppContext context)
        {
            _context = context;
        }

        public bool IsSatisfiedBy(Order entity)
        {

            // if the customer have overdue order total >= customer limit, then the credit has been reached
            var customer = _context.Customers.Single(x => x.CustomerId == entity.CustomerId);

            // no limit was specified, unlimited credit
            if (!customer.CustomerCreditLimit.HasValue) return false;

            // limit was set to zero or less, indicating to stop orders
            if (customer.CustomerCreditLimit.Value <= 0) return true;

            // get all orders that total value >0 and has not been paid fully
            var totalOutstandingOrders = (from o in _context.Orders
                                          where o.CustomerId == entity.CustomerId && o.TotalValue > 0 && o.TotalPaid < o.TotalValue
                                          select new {TotalOutstanding = o.TotalValue - o.TotalPaid}).Sum(x => x.TotalOutstanding) ?? 0;

            // add the current order to totalOutstandingOrders
            if (entity.TotalValue.HasValue && entity.TotalPaid.HasValue)
            {
                totalOutstandingOrders += (entity.TotalValue.Value - entity.TotalPaid.Value);
            }
            

            return totalOutstandingOrders > customer.CustomerCreditLimit;

        }
    }

    public interface ICustomerCreditLimitReached : ISpecification<Order>
    {
    }
}