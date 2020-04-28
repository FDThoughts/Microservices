namespace eShop.Services.Product.API.V1.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ProductItem : IEquatable<ProductItem>
    {
        [Key]
        public virtual long ProductId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Category { get; set; }
        public virtual string Description { get; set; }
        [Column(TypeName="decimal(8, 2)")]
        public virtual decimal Price { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProductItem)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(ProductItem p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;
            return this.ProductId == p.ProductId &&
                this.Name == p.Name &&
                this.Category == p.Category &&
                this.Description == p.Description &&
                this.Price == p.Price;
        }
    }
}