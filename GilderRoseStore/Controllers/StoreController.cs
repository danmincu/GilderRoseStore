using GilderRoseStore.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace GilderRoseStore.Controllers
{
    [Authorize]
    public class StoreController : ApiController
    {
        private readonly IInventory inventory;
        private readonly IPurchaseHistory purchaseHistory;

        public StoreController(IInventory inventory, IPurchaseHistory purchaseHistory)
        {
            this.inventory = inventory;
            this.purchaseHistory = purchaseHistory;
        }

        [AllowAnonymous]
        public IEnumerable<Item> Get()
        {            
            return inventory.Items;
        }

        public bool Get(Guid id)
        {
            Item item;
            var result = inventory.BuyItem(id, out item);
            if (result)
                this.purchaseHistory.AddEntry(User.Identity.Name, item);
            return result;
        }

    }
}
