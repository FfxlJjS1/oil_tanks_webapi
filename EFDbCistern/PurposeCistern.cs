using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

public partial class PurposeCistern
{
    public int PurposeCisternId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<StandartSludge> StandartSludges { get; } = new List<StandartSludge>();
}
