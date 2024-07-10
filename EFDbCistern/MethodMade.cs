using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

/// <summary>
/// Таблица Метод изготовления
/// </summary>
public partial class MethodMade
{
    public int MethodMadeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Cistern> CisternBottomMethodMades { get; } = new List<Cistern>();

    public virtual ICollection<Cistern> CisternWallMethodMades { get; } = new List<Cistern>();
}
