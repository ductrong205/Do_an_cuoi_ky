namespace FashionShop.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class StockMovement
    {
        [Key]
        public int MoveId { get; set; }

        public int VariantId { get; set; }

        public int MoveType { get; set; }

        public int Quantity { get; set; }

        [StringLength(30)]
        public string RefCode { get; set; }

        [StringLength(200)]
        public string Note { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }
    }
}
