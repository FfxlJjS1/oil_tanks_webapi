using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

/// <summary>
/// Таблица Стоимость резервуара
/// </summary>
public partial class PriceCistern
{
    public int PriceCisternId { get; set; }

    public int CisternId { get; set; }

    /// <summary>
    /// Цена, руб
    /// </summary>
    public decimal? PriceRub { get; set; }

    public virtual Cistern Cistern { get; set; } = null!;
}
