using System;
using System.Collections.Generic;
using BackendOfSite.EFDbCistern;
using Microsoft.EntityFrameworkCore;

namespace BackendOfSite;

public partial class DbCisternContext : DbContext
{
    public DbCisternContext()
    {
    }

    public DbCisternContext(DbContextOptions<DbCisternContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cistern> Cisterns { get; set; }

    public virtual DbSet<CisternEquipment> CisternEquipments { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<FlowRate> FlowRates { get; set; }

    public virtual DbSet<LadderTypeConstruction> LadderTypeConstructions { get; set; }

    public virtual DbSet<MethodMade> MethodMades { get; set; }

    public virtual DbSet<PriceCistern> PriceCisterns { get; set; }

    public virtual DbSet<ProductPark> ProductParks { get; set; }

    public virtual DbSet<PurposeCistern> PurposeCisterns { get; set; }

    public virtual DbSet<RoofTypeConstruction> RoofTypeConstructions { get; set; }

    public virtual DbSet<RoofTypeForm> RoofTypeForms { get; set; }

    public virtual DbSet<StandartSludge> StandartSludges { get; set; }

    public virtual DbSet<TypeSlope> TypeSlopes { get; set; }

    public virtual DbSet<Well> Wells { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(@"Server=postgres.tank;Port=5432;Database=db_cistern;User Id=postgres;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cistern>(entity =>
        {
            entity.HasKey(e => e.CisternId).HasName("cistern_pk");

            entity.ToTable("cistern", tb => tb.HasComment("Таблица Резервуары"));

            entity.Property(e => e.CisternId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("cistern_id");
            entity.Property(e => e.AccessoriesKg)
                .HasComment("Комплектующие конструкции масса, кг")
                .HasColumnName("accessories_kg");
            entity.Property(e => e.BottomCentreMm)
                .HasComment("Днище Толщина центральной части, мм")
                .HasColumnName("bottom_centre_mm");
            entity.Property(e => e.BottomEdgeMm)
                .HasComment("Днище Толщина окраек, мм")
                .HasColumnName("bottom_edge_mm");
            entity.Property(e => e.BottomEdgeUnit)
                .HasComment("Днище Количество окраек, шт")
                .HasColumnName("bottom_edge_unit");
            entity.Property(e => e.BottomMarginRustMm)
                .HasComment("Днище Припуск на коррозию, мм")
                .HasColumnName("bottom_margin_rust_mm");
            entity.Property(e => e.BottomMethodMadeId)
                .HasComment("Днище Метод изготовления")
                .HasColumnName("bottom_method_made_id");
            entity.Property(e => e.BottomTypeSlopeId)
                .HasComment("Днище Тип уклона")
                .HasColumnName("bottom_type_slope_id");
            entity.Property(e => e.BottomWeightKg)
                .HasComment("Днище масса, кг")
                .HasColumnName("bottom_weight_kg");
            entity.Property(e => e.CarcassPackKg)
                .HasComment("Каркасы и упаковка масса, кг")
                .HasColumnName("carcass_pack_kg");
            entity.Property(e => e.ClassDanger)
                .HasComment("Класс опасности")
                .HasColumnName("class_danger");
            entity.Property(e => e.HatchPipeKg)
                .HasComment("Люки и патрубки масса, кг")
                .HasColumnName("hatch_pipe_kg");
            entity.Property(e => e.HeightFillingMm)
                .HasComment("Высота налива, мм")
                .HasColumnType("character varying")
                .HasColumnName("height_filling_mm");
            entity.Property(e => e.LadderTypeConstructionId)
                .HasComment("Лестница Тип конструкции")
                .HasColumnName("ladder_type_construction_id");
            entity.Property(e => e.LadderWeightKg)
                .HasComment("Лестница масса, кг")
                .HasColumnName("ladder_weight_kg");
            entity.Property(e => e.NominalVolumeM3)
                .HasComment("Номинальный объем, м3")
                .HasColumnName("nominal_volume_m3");
            entity.Property(e => e.RoofBeamUnit)
                .HasComment("Крыша Количество балок, шт")
                .HasColumnName("roof_beam_unit");
            entity.Property(e => e.RoofBearingElement)
                .HasMaxLength(10)
                .HasComment("Крыша Несущий элемент")
                .HasColumnName("roof_bearing_element");
            entity.Property(e => e.RoofFlooringMm)
                .HasComment("Крыша Толщина настила, мм")
                .HasColumnName("roof_flooring_mm");
            entity.Property(e => e.RoofMarginRustMm)
                .HasComment("Крыша Припуск на коррозию, мм")
                .HasColumnName("roof_margin_rust_mm");
            entity.Property(e => e.RoofPlatformKg)
                .HasComment("Площадки на крыше масса, кг")
                .HasColumnName("roof_platform_kg");
            entity.Property(e => e.RoofTypeConstructionId)
                .HasComment("Стационарная крыша Тип конструкции")
                .HasColumnName("roof_type_construction_id");
            entity.Property(e => e.RoofTypeFormId)
                .HasComment("Стационарная крыша Вид формы")
                .HasColumnName("roof_type_form_id");
            entity.Property(e => e.RoofWeightKg)
                .HasComment("Крыша масса, кг")
                .HasColumnName("roof_weight_kg");
            entity.Property(e => e.WallBeltUnit)
                .HasComment("Стенка количество поясов, шт")
                .HasColumnName("wall_belt_unit");
            entity.Property(e => e.WallHeightMm)
                .HasComment("Высота стенки, мм")
                .HasColumnName("wall_height_mm");
            entity.Property(e => e.WallInnerDrMm)
                .HasComment("Внутренний диаметр стенки, мм")
                .HasColumnName("wall_inner_dr_mm");
            entity.Property(e => e.WallLowerBeltMm)
                .HasComment("Стенка толщина нижнего пояса, мм")
                .HasColumnName("wall_lower_belt_mm");
            entity.Property(e => e.WallMarginRustMm)
                .HasComment("Стенка припуск коррозия, мм")
                .HasColumnName("wall_margin_rust_mm");
            entity.Property(e => e.WallMethodMadeId)
                .HasComment("Стенка Метод изготовления ")
                .HasColumnName("wall_method_made_id");
            entity.Property(e => e.WallUpperBeltMm)
                .HasComment("Стенка толщина верхнего пояса, мм")
                .HasColumnName("wall_upper_belt_mm");
            entity.Property(e => e.WallWeightKg)
                .HasComment("Стенка масса, кг")
                .HasColumnName("wall_weight_kg");
            entity.Property(e => e.WorkingLifeYear)
                .HasComment("Срок службы, лет")
                .HasColumnName("working_life_year");

            entity.HasOne(d => d.BottomMethodMade).WithMany(p => p.CisternBottomMethodMades)
                .HasForeignKey(d => d.BottomMethodMadeId)
                .HasConstraintName("cistern_fk_1");

            entity.HasOne(d => d.BottomTypeSlope).WithMany(p => p.Cisterns)
                .HasForeignKey(d => d.BottomTypeSlopeId)
                .HasConstraintName("cistern_fk_2");

            entity.HasOne(d => d.LadderTypeConstruction).WithMany(p => p.Cisterns)
                .HasForeignKey(d => d.LadderTypeConstructionId)
                .HasConstraintName("cistern_fk_5");

            entity.HasOne(d => d.RoofTypeConstruction).WithMany(p => p.Cisterns)
                .HasForeignKey(d => d.RoofTypeConstructionId)
                .HasConstraintName("cistern_fk_4");

            entity.HasOne(d => d.RoofTypeForm).WithMany(p => p.Cisterns)
                .HasForeignKey(d => d.RoofTypeFormId)
                .HasConstraintName("cistern_fk_3");

            entity.HasOne(d => d.WallMethodMade).WithMany(p => p.CisternWallMethodMades)
                .HasForeignKey(d => d.WallMethodMadeId)
                .HasConstraintName("cistern_fk");
        });

        modelBuilder.Entity<CisternEquipment>(entity =>
        {
            entity.HasKey(e => e.CisternEquipmentId).HasName("cistern_equipment_pk");

            entity.ToTable("cistern_equipment", tb => tb.HasComment("Связывает таблицы Резервуар и Оборудование"));

            entity.Property(e => e.CisternEquipmentId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("cistern_equipment_id");
            entity.Property(e => e.CisternId).HasColumnName("cistern_id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.MaxCount).HasColumnName("max_count");
            entity.Property(e => e.MinCount).HasColumnName("min_count");

            entity.HasOne(d => d.Cistern).WithMany(p => p.CisternEquipments)
                .HasForeignKey(d => d.CisternId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cistern_equipment_fk");

            entity.HasOne(d => d.Equipment).WithMany(p => p.CisternEquipments)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cistern_equipment_fk_1");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("equipment_pk");

            entity.ToTable("equipment", tb => tb.HasComment("Таблица Оборудование"));

            entity.Property(e => e.EquipmentId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("equipment_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("Название оборудования")
                .HasColumnName("name");
            entity.Property(e => e.WeightKg)
                .HasComment("Вес")
                .HasColumnName("weight_kg");
        });

        modelBuilder.Entity<FlowRate>(entity =>
        {
            entity.HasKey(e => e.FlowRateId).HasName("flow_rate_pk");

            entity.ToTable("flow_rate");

            entity.Property(e => e.FlowRateId).HasColumnName("flow_rate_id");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ProductParkId).HasColumnName("product_park_id");
            entity.Property(e => e.QgPred).HasColumnName("qg_pred");
            entity.Property(e => e.QnPred).HasColumnName("qn_pred");
            entity.Property(e => e.QwPred).HasColumnName("qw_pred");
            entity.Property(e => e.WellId).HasColumnName("well_id");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.ProductPark).WithMany(p => p.FlowRates)
                .HasForeignKey(d => d.ProductParkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flow_rate_fk_1");

            entity.HasOne(d => d.Well).WithMany(p => p.FlowRates)
                .HasForeignKey(d => d.WellId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flow_rate_fk");
        });

        modelBuilder.Entity<LadderTypeConstruction>(entity =>
        {
            entity.HasKey(e => e.LadderTypeConstructionId).HasName("ladder_type_construction_pk");

            entity.ToTable("ladder_type_construction");

            entity.Property(e => e.LadderTypeConstructionId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ladder_type_construction_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MethodMade>(entity =>
        {
            entity.HasKey(e => e.MethodMadeId).HasName("method_made_pk");

            entity.ToTable("method_made", tb => tb.HasComment("Таблица Метод изготовления"));

            entity.Property(e => e.MethodMadeId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("method_made_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PriceCistern>(entity =>
        {
            entity.HasKey(e => e.PriceCisternId).HasName("price_cistern_pk");

            entity.ToTable("price_cistern", tb => tb.HasComment("Таблица Стоимость резервуара"));

            entity.Property(e => e.PriceCisternId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("price_cistern_id");
            entity.Property(e => e.CisternId).HasColumnName("cistern_id");
            entity.Property(e => e.PriceRub)
                .HasComment("Цена, руб")
                .HasColumnType("money")
                .HasColumnName("price_rub");

            entity.HasOne(d => d.Cistern).WithMany(p => p.PriceCisterns)
                .HasForeignKey(d => d.CisternId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("price_cistern_fk");
        });

        modelBuilder.Entity<ProductPark>(entity =>
        {
            entity.HasKey(e => e.ProductParkId).HasName("product_park_pk");

            entity.ToTable("product_park");

            entity.Property(e => e.ProductParkId).HasColumnName("product_park_id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<PurposeCistern>(entity =>
        {
            entity.HasKey(e => e.PurposeCisternId).HasName("purpose_cistern_pk");

            entity.ToTable("purpose_cistern");

            entity.Property(e => e.PurposeCisternId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("purpose_cistern_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RoofTypeConstruction>(entity =>
        {
            entity.HasKey(e => e.RoofTypeConstructionId).HasName("roof_type_construction_pk");

            entity.ToTable("roof_type_construction", tb => tb.HasComment("Таблица Типы конструкций лестницы"));

            entity.Property(e => e.RoofTypeConstructionId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("roof_type_construction_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RoofTypeForm>(entity =>
        {
            entity.HasKey(e => e.RoofTypeFormId).HasName("roof_type_form_pk");

            entity.ToTable("roof_type_form");

            entity.Property(e => e.RoofTypeFormId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("roof_type_form_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<StandartSludge>(entity =>
        {
            entity.HasKey(e => e.StandartSludgeId).HasName("standart_sludge_pk");

            entity.ToTable("standart_sludge");

            entity.Property(e => e.StandartSludgeId).HasColumnName("standart_sludge_id");
            entity.Property(e => e.DevonHour)
                .HasDefaultValueSql("0")
                .HasColumnName("devon_hour");
            entity.Property(e => e.PurposeCisternId).HasColumnName("purpose_cistern_id");
            entity.Property(e => e.SulfuricHour)
                .HasDefaultValueSql("0")
                .HasColumnName("sulfuric_hour");

            entity.HasOne(d => d.PurposeCistern).WithMany(p => p.StandartSludges)
                .HasForeignKey(d => d.PurposeCisternId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("standart_sludge_fk");
        });

        modelBuilder.Entity<TypeSlope>(entity =>
        {
            entity.HasKey(e => e.TypeSlopeId).HasName("type_slope_pk");

            entity.ToTable("type_slope", tb => tb.HasComment("Таблица Виды форм стационарной крыши"));

            entity.Property(e => e.TypeSlopeId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("type_slope_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Well>(entity =>
        {
            entity.HasKey(e => e.WellId).HasName("well_pk");

            entity.ToTable("well");

            entity.Property(e => e.WellId).HasColumnName("well_id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
