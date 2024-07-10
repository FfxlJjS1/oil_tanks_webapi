using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

/// <summary>
/// Таблица Виды форм стационарной крыши
/// </summary>
public partial class TypeSlope
{
    public int TypeSlopeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Cistern> Cisterns { get; } = new List<Cistern>();
}
