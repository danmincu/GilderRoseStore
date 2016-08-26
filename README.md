# GilderRoseStore
Story behind - The Gilded Rose Expands
As you may know, the Gilded Rose* is a small inn in a prominent city that buys and sells only the finest items. The shopkeeper, Allison, is looking to expand by providing merchants in other cities with access to the shop's inventory via a public HTTP-accessible API.
API requirements
- Retrieve the current inventory (i.e. list of items)
- Buy an item (user must be authenticated)

Here is the definition for an item:
```xml
class Item
{
  public Guid Id { get; set;}
  public string Name { get; set; }
  public string Description { get; set; }
  public int Price { get; set; }
  public int Quantity { get; set;}
}
```
The app exposes two endpoints
```xml
  ..\api\store - retrieves the current inventory - list of items and the quantity
  ..\api\store\get\{id} - buys and item from the store and decreases the current stock. Id represents the Guid Id of the desired item.
```

My focus is on Authorization / Testability both unit test and integration test / Dependecy Injection / Ability to self host
and less focus on the business layer that would require a proper customer/orders/items/shopping cart/purchase history and a real database.

Using: Asp.net Web Api, Owin self hosting, Microsoft Identity OAuth 2.0 (DESPITE BEING REQUIRED I DIDN'T ADD HTTPS FOR SIMPLICITY)
  

