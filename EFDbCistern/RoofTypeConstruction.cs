using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

/// <summary>
/// Таблица Типы конструкций лестницы
/// </summary>
public partial class RoofTypeConstruction
{
    public int RoofTypeConstructionId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Cistern> Cisterns { get; } = new List<Cistern>();
}
