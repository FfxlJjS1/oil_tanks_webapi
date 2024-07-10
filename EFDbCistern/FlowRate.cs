using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

public partial class FlowRate
{
    public int FlowRateId { get; set; }

    public int WellId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal? QgPred { get; set; }

    public decimal? QnPred { get; set; }

    public decimal? QwPred { get; set; }

    public int ProductParkId { get; set; }

    public virtual ProductPark ProductPark { get; set; } = null!;

    public virtual Well Well { get; set; } = null!;
}
