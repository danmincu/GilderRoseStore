using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace GilderRoseStore.Models
{
    public class PurchaseHistoryStub : IPurchaseHistory
    {
        public void AddEntry(string userName, Item item)
        {
            System.Diagnostics.Trace.WriteLine(
                String.Format(CultureInfo.InvariantCulture, "{0} purchased {1} for the price of {2:C} on {3}.",
                userName, item.Name, item.Price, DateTime.Now.ToShortDateString()));
        }
    }
}