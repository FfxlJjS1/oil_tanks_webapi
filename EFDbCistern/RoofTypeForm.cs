using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

public partial class RoofTypeForm
{
    public int RoofTypeFormId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Cistern> Cisterns { get; } = new List<Cistern>();
}
