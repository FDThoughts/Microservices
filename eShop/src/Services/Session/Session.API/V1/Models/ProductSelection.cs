namespace eShop.Services.Session.API.V1.Models
{
    public class ProductSelection {
        public virtual long productId { get; set; }
        public virtual string name { get; set; }
        public virtual decimal price { get; set; }
        public virtual int quantity { get; set; }
    }
}