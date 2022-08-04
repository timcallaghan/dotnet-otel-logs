namespace WebHost.Models.Responses;

public class Cart
{
    public int Id { get; }
    public IReadOnlyList<CartItem> CartItems { get; }

    public Cart(int id, IReadOnlyList<CartItem> cartItems)
    {
        Id = id;
        CartItems = cartItems;
    }
}

public class CartItem
{
    public int Id { get; }
    public string Name { get; }

    public CartItem(int id, string name)
    {
        Id = id;
        Name = name;
    }
}