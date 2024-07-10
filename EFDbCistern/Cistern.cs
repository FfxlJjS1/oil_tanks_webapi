using System;
using System.Collections.Generic;

namespace BackendOfSite.EFDbCistern;

/// <summary>
/// Таблица Резервуары
/// </summary>
public partial class Cistern
{
    public int CisternId { get; set; }

    /// <summary>
    /// Номинальный объем, м3
    /// </summary>
    public int NominalVolumeM3 { get; set; }

    /// <summary>
    /// Внутренний диаметр стенки, мм
    /// </summary>
    public decimal WallInnerDrMm { get; set; }

    /// <summary>
    /// Высота стенки, мм
    /// </summary>
    public decimal WallHeightMm { get; set; }

    /// <summary>
    /// Высота налива, мм
    /// </summary>
    public string? HeightFillingMm { get; set; }

    /// <summary>
    /// Класс опасности
    /// </summary>
    public short ClassDanger { get; set; }

    /// <summary>
    /// Срок службы, лет
    /// </summary>
    public int? WorkingLifeYear { get; set; }

    /// <summary>
    /// Стенка количество поясов, шт
    /// </summary>
    public int? WallBeltUnit { get; set; }

    /// <summary>
    /// Стенка припуск коррозия, мм
    /// </summary>
    public decimal? WallMarginRustMm { get; set; }

    /// <summary>
    /// Стенка толщина верхнего пояса, мм
    /// </summary>
    public decimal? WallUpperBeltMm { get; set; }

    /// <summary>
    /// Стенка толщина нижнего пояса, мм
    /// </summary>
    public decimal? WallLowerBeltMm { get; set; }

    /// <summary>
    /// Днище Количество окраек, шт
    /// </summary>
    public int? BottomEdgeUnit { get; set; }

    /// <summary>
    /// Днище Припуск на коррозию, мм
    /// </summary>
    public decimal? BottomMarginRustMm { get; set; }

    /// <summary>
    /// Днище Толщина центральной части, мм
    /// </summary>
    public decimal? BottomCentreMm { get; set; }

    /// <summary>
    /// Днище Толщина окраек, мм
    /// </summary>
    public decimal? BottomEdgeMm { get; set; }

    /// <summary>
    /// Крыша Количество балок, шт
    /// </summary>
    public int? RoofBeamUnit { get; set; }

    /// <summary>
    /// Крыша Припуск на коррозию, мм
    /// </summary>
    public decimal? RoofMarginRustMm { get; set; }

    /// <summary>
    /// Крыша Несущий элемент
    /// </summary>
    public string? RoofBearingElement { get; set; }

    /// <summary>
    /// Крыша Толщина настила, мм
    /// </summary>
    public decimal? RoofFlooringMm { get; set; }

    /// <summary>
    /// Стенка масса, кг
    /// </summary>
    public decimal? WallWeightKg { get; set; }

    /// <summary>
    /// Днище масса, кг
    /// </summary>
    public decimal? BottomWeightKg { get; set; }

    /// <summary>
    /// Крыша масса, кг
    /// </summary>
    public decimal? RoofWeightKg { get; set; }

    /// <summary>
    /// Лестница масса, кг
    /// </summary>
    public decimal? LadderWeightKg { get; set; }

    /// <summary>
    /// Площадки на крыше масса, кг
    /// </summary>
    public decimal? RoofPlatformKg { get; set; }

    /// <summary>
    /// Люки и патрубки масса, кг
    /// </summary>
    public decimal? HatchPipeKg { get; set; }

    /// <summary>
    /// Комплектующие конструкции масса, кг
    /// </summary>
    public decimal? AccessoriesKg { get; set; }

    /// <summary>
    /// Каркасы и упаковка масса, кг
    /// </summary>
    public decimal? CarcassPackKg { get; set; }

    /// <summary>
    /// Стенка Метод изготовления 
    /// </summary>
    public int? WallMethodMadeId { get; set; }

    /// <summary>
    /// Днище Метод изготовления
    /// </summary>
    public int? BottomMethodMadeId { get; set; }

    /// <summary>
    /// Днище Тип уклона
    /// </summary>
    public int? BottomTypeSlopeId { get; set; }

    /// <summary>
    /// Стационарная крыша Вид формы
    /// </summary>
    public int? RoofTypeFormId { get; set; }

    /// <summary>
    /// Стационарная крыша Тип конструкции
    /// </summary>
    public int? RoofTypeConstructionId { get; set; }

    /// <summary>
    /// Лестница Тип конструкции
    /// </summary>
    public int? LadderTypeConstructionId { get; set; }

    public virtual MethodMade? BottomMethodMade { get; set; }

    public virtual TypeSlope? BottomTypeSlope { get; set; }

    public virtual ICollection<CisternEquipment> CisternEquipments { get; } = new List<CisternEquipment>();

    public virtual LadderTypeConstruction? LadderTypeConstruction { get; set; }

    public virtual ICollection<PriceCistern> PriceCisterns { get; } = new List<PriceCistern>();

    public virtual RoofTypeConstruction? RoofTypeConstruction { get; set; }

    public virtual RoofTypeForm? RoofTypeForm { get; set; }

    public virtual MethodMade? WallMethodMade { get; set; }
}
