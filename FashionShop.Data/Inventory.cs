namespace FashionShop.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Inventory
    {
        public int InventoryId { get; set; }

        public int VariantId { get; set; }

        public int Quantity { get; set; }

        public int MinAlert { get; set; }

        [StringLength(30)]
        public string Location { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }
    }
}
