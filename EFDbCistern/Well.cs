using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

public partial class Well
{
    public int WellId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<FlowRate> FlowRates { get; } = new List<FlowRate>();
}
