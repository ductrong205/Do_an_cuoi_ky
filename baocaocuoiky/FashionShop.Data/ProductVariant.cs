namespace FashionShop.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ProductVariant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductVariant()
        {
            Inventories = new HashSet<Inventory>();
            StockMovements = new HashSet<StockMovement>();
        }

        [Key]
        public int VariantId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(20)]
        public string Size { get; set; }

        [Required]
        [StringLength(30)]
        public string Color { get; set; }

        [StringLength(50)]
        public string Barcode { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Inventory> Inventories { get; set; }

        public virtual Product Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StockMovement> StockMovements { get; set; }
    }
}
