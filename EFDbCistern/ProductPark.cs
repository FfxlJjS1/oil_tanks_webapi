using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

public partial class ProductPark
{
    public int ProductParkId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<FlowRate> FlowRates { get; } = new List<FlowRate>();
}
