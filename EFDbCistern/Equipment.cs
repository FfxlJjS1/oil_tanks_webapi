using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

/// <summary>
/// Таблица Оборудование
/// </summary>
public partial class Equipment
{
    public int EquipmentId { get; set; }

    /// <summary>
    /// Название оборудования
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Вес
    /// </summary>
    public decimal? WeightKg { get; set; }

    public virtual ICollection<CisternEquipment> CisternEquipments { get; } = new List<CisternEquipment>();
}
