using System;
using System.Collections.Generic;

namespace EFCoreEgitimi.Entities;

public partial class ProductsAboveAveragePrice
{
    public string ProductName { get; set; } = null!;

    public decimal? UnitPrice { get; set; }
}
