using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GilderRoseStore.Models
{
    public interface IPurchaseHistory
    {
        void AddEntry(string user, Item item);
    }
}