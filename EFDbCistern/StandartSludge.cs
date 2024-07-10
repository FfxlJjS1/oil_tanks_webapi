using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

public partial class StandartSludge
{
    public int StandartSludgeId { get; set; }

    public int PurposeCisternId { get; set; }

    public int? DevonHour { get; set; }

    public int? SulfuricHour { get; set; }

    public virtual PurposeCistern PurposeCistern { get; set; } = null!;
}
