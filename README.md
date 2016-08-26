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

Using: Asp.net Web Api, Owin self hosting, Microsoft Identity, EF, OAuth 2.0 (despite being required for OAuth 20 I didn't include https for simplicity)

Running the code:
-  Run the unit tests;
-  Run the "against the host" integration tests. Make sure you run w/o debug the web app first (CTRL+F5)
-  Run the "self host" integration tests. Make sure you run the Visual Studio in Administrator mode otherwise it won't spawn the process.
- in the browser visit [http://localhost:19683/api/Store](http://localhost:19683/api/Store) to see the current inventory. WebApi looks in the header and returns XML or JSON. A java-scrip call will receive JSON however this is what chrome gets (the default for tests database)
```xml
<ArrayOfItem xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/GilderRoseStore.Models">
  <Item>
     <Description>finest item4</Description>
     <Id>1d66cfe1-983b-4af2-85d1-7edea5f3b755</Id>
     <Name>item4</Name>
     <Price>3</Price>
     <Quantity>15</Quantity>
  </Item>
  <Item>
     <Description>finest item3</Description>
     <Id>1c10c048-28f5-426a-a286-a673d165c930</Id>
     <Name>item3</Name>
     <Price>4</Price>
     <Quantity>5</Quantity>
  </Item>
  <Item>
     <Description>finest item2</Description>
     <Id>cb896034-0c56-4bd5-ac5d-3a26bcb41a1c</Id>
     <Name>item2</Name>
     <Price>5</Price>
     <Quantity>3</Quantity>
  </Item>
  <Item>
     <Description>finest item1</Description>
     <Id>086828a2-2945-4274-8c0c-d1b17ccad3ae</Id>
     <Name>item1</Name>
     <Price>10</Price>
     <Quantity>2</Quantity>
  </Item>
</ArrayOfItem>
```
what is happening in these tests? 
 - wicked code coverage
 - a "test@test.com" user with the password "GilderRose1@" is created & the processes are spawning LocalDb catalogs to hosts the Users.
 - auth_tokens are obtained before making authenticated BuyItem call. Proving that non-auth users are getting the boot. proving that auth users are getting the right informatiom.
 - proving that the mocked Inventory in memory database is thread safe etc..

what stuff I didn't do but it would be a nice addition?
  - paginating the call to get inventory. add some metadata to the return result with total count and a sample of items. subsequent calls will have to specify the range to get specific items.
  - auding and logging; tracing for WebApi
  - HTTPS - kind of a must to clearly secure the communication and avoid tokens beind fethced, however nobody wants to install bogus CERTs. Actually I did it a test and is not extremely difficult to add https. some classes need small changes e.g.  OAuthAuthorizationServerOptions.AllowInsecureHttp = false etc.. plenty of examples on how to do it. next iteration will have it!
  - full Customer/Orders/Items/Shopping cart. Biz objecst and everything stored into a proper database.
  - the buyItem end point should/could specify the quantity; for now calling the method buys one item if the stock is positive.
  - a web page to allow the end users to call the end-points
  - an example of using postman to get the bearer auth-token and an example of how to call buyItem with the auth-token for purchasing an item.
  - a diagram of the project and the dataflow
  
I wrote all these "needs and wants" here beause hopefully the list will shrink as I add stuff to the project.

 
