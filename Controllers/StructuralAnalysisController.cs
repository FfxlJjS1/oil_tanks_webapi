using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using BackendOfSite.Kafka;

namespace BackendOfSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StructuralAnalysisController : Controller
    {
        readonly string[] formTypes = { "Все", "Цилиндрический", "Параллелепипед" };
        private KafkaClient kafka_client = new KafkaClient("kafka:9092", KafkaClient.KafkaClientType.Producer);

        class Column
        {
            public string Name { get; set; } = "";
            public string Width { get; set; } = "";
        }

        class BeltInfo
        {
            public int beltNumber { get; set; }
            public double Thickness { get; set; }
        }

        class Tooltip
        {
            public double Radius { get; set; } = -1;
            public double Width { get; set; } = -1;
            public double Length { get; set; } = -1;

            public double MetalSheetWidth { get; set; } = -1;
            public double OilDensity { get; set; } = -1;

            public List<BeltInfo> beltInfos { get; set; } = new List<BeltInfo>();

            public double BottomArea { get; set; } = -1;
            public double WallSquire { get; set; } = -1;
            public double WallSteelWeight { get; set; } = -1;
            public double BottomSteelWeight { get; set; } = -1;
            public double RoofSteelWeight { get; set; } = -1; 

            public double MetalDensityKgPerCubicMetr { get; set; } = -1;
            public decimal MetalCostPeTon { get; set; } = -1;
        }

        class Row
        {
            public long Identification { get; set; }

            public List<string> Cells { get; set; } = new List<string>();
            public Tooltip TooltipInfo { get; set; } = new Tooltip();
        }

        class EntityTable
        {
            public List<Column> Columns { get; set; } = new List<Column>();
            public List<Row> Rows { get; set; } = new List<Row>();
        }

        class EntityResponce
        {
            public EntityTable entityTable { get; set; } = new EntityTable();
        }

        [HttpGet("GetFormTypes")]
        public IActionResult GetFormTypes()
        {
            return Ok(formTypes.Select((name, index) => new { name, index }));
        }

        [HttpGet("AnalyseByFormVolume")]
        public IActionResult AnalyseByFormVolume(double volumeValue, int formTypeIndex, string limitesAsString)
        {
            EntityTable entityTable = new EntityTable();
            long[] limites = limitesAsString.Split(';').Select(x => Convert.ToInt64(x)).ToArray();


            entityTable.Columns = new List<Column>();

            if (formTypeIndex == 1 || formTypeIndex == 0)
            {
                entityTable.Columns.AddRange(new List<Column>()
                {
                    new Column(){ Name = "Радиус, м", Width = "50px"}
                });
            }
            
            if (formTypeIndex == 2 || formTypeIndex == 0)
            {
                entityTable.Columns.AddRange(new List<Column>()
                {
                    new Column(){ Name = "Ширина, м", Width = "50px"},
                    new Column(){ Name = "Длина, м", Width = "50px"},
                });
            }

            entityTable.Columns.AddRange(new List<Column>(){
                new Column(){ Name = "Высота, м", Width = "100px"},
                new Column() { Name = "Толщина нижнего пояса, м", Width = "100px" },
                new Column() { Name = "Толщина верхнего пояса, м", Width = "150px" },
                new Column() { Name = "Общая площадь материалов, м²", Width = "150px" },
                new Column() { Name = "Общий вес, кг.", Width = "1000px" },
                new Column() { Name = "Общая стоимость, руб.", Width = "1000px" }
            });

            if(formTypeIndex == 0)
            {
                entityTable.Columns.Add(new Column() { Name = "Форма, тип", Width = "150px" });
            }


            if (formTypeIndex == 0)
            {
                for (formTypeIndex = 1; formTypeIndex < formTypes.Length; formTypeIndex++)
                {
                    List<Row> rowsByForm = StructualAnalyzeByForm(formTypeIndex, volumeValue, limites, true);

                    entityTable.Rows.AddRange(rowsByForm);
                }

                entityTable.Rows = entityTable.Rows.OrderBy(row => Convert.ToDecimal(row.Cells[row.Cells.Count - 2])).ToList();
            }
            else
            {
                entityTable.Rows = StructualAnalyzeByForm(formTypeIndex, volumeValue, limites, false)
                    .OrderBy(row => Convert.ToDecimal(row.Cells.Last())).ToList();
            }

            {
                long id = 1;

                entityTable.Rows.ForEach(row => row.Identification = id++);
            }


            EntityResponce entityResponce = new EntityResponce()
            {
                entityTable = entityTable
            };

            kafka_client.SendMesssage(message: "StructuralAnalysisController: got analyse by form volume");
            
            return Ok(entityResponce);
        }

        private List<Row> StructualAnalyzeByForm(int formTypeIndex, double volumeValue, long[] limites, bool willFullTable)
        {
            List<Row> rows = new List<Row>();

            if (formTypeIndex < 1 || formTypeIndex > formTypes.Length)
            {
                return rows;
            }

            const double metalDensityKgPerCubicMetr = 7500; // кг/м^3
            const decimal metalCostPeTon = 70000; // Руб/т

            if (formTypeIndex == 1)
            {
                for (long radius = limites[0] ; radius <= limites[1]; radius++)
                {
                    Row row = StructureAnalyseForCylinderTanks(radius, volumeValue, metalDensityKgPerCubicMetr, metalCostPeTon, willFullTable);
                    
                    rows.Add(row);
                }
            }
            else if (formTypeIndex == 2)
            {
                for (long length = limites[2]; length <= limites[3]; length++)
                {
                    for(long width = limites[2]; width<= length; width++)
                    {
                        Row row = StructureAnalyseForParallepipedTanks(width, length, volumeValue, metalDensityKgPerCubicMetr, metalCostPeTon, willFullTable);

                        rows.Add(row);
                    }
                }
            }

            return rows;
        }

        // For Parallepiped
        private Row StructureAnalyseForParallepipedTanks(long width, long length, double volumeValue, double metalDensityKgPerCubicMetr, decimal  metalCostPeTon, bool willFullTable)
        {
            Row row = new Row();
            Tooltip tooltip = new Tooltip();

            double radius = RoundUp(Math.Sqrt(Math.Pow(width, 2) + Math.Pow(length, 2)), 3);

            if (willFullTable)
            {
                row.Cells.Add("-1");
            }

            row.Cells.Add(width.ToString());
            row.Cells.Add(length.ToString());

            tooltip.Width = width;
            tooltip.Length = length;


            tooltip.MetalSheetWidth = 1.5;
            tooltip.OilDensity = 871;

            tooltip.MetalDensityKgPerCubicMetr = metalDensityKgPerCubicMetr;
            tooltip.MetalCostPeTon = metalCostPeTon;

            tooltip.BottomArea = RoundUp(width * length, 3);


            double height = RoundUp(volumeValue / tooltip.BottomArea, 3);
            row.Cells.Add(height.ToString());

            tooltip.WallSquire = RoundUp(2 * 1.5 * height * (width + length), 3);

            tooltip.beltInfos = CalculateBeltsThickness(volumeValue, height, radius, tooltip.MetalSheetWidth, tooltip.OilDensity);

            double lowerBeltWeight = tooltip.beltInfos.First().Thickness;
            row.Cells.Add(lowerBeltWeight.ToString());

            double upperBeltWeight = tooltip.beltInfos.Last().Thickness;
            row.Cells.Add(upperBeltWeight.ToString());

            tooltip.WallSteelWeight = CalculateWallWeightVolume(radius, tooltip.MetalDensityKgPerCubicMetr, tooltip.beltInfos);
            tooltip.BottomSteelWeight = RoundUp(metalDensityKgPerCubicMetr * tooltip.BottomArea * lowerBeltWeight, 3);
            tooltip.RoofSteelWeight = RoundUp(metalDensityKgPerCubicMetr * tooltip.BottomArea * 0.004, 3);

            double totalMetalArea = RoundUp(tooltip.BottomArea * 2 + tooltip.WallSquire, 0);
            row.Cells.Add(totalMetalArea.ToString());

            double totalSteelWeight = RoundUp(tooltip.WallSteelWeight + tooltip.BottomSteelWeight + tooltip.RoofSteelWeight, 0);
            row.Cells.Add(totalSteelWeight.ToString());

            row.Cells.Add(CalculateCylinderTankCosts(totalSteelWeight, metalCostPeTon).ToString());


            if (willFullTable)
            {
                row.Cells.Add(formTypes[2]);
            }


            row.TooltipInfo = tooltip;

            return row;
        }

        // For Cylinder
        private Row StructureAnalyseForCylinderTanks(double radius, double volumeValue, double metalDensityKgPerCubicMetr, decimal metalCostPeTon, bool willFullTable)
        {
            Row row = new Row
            {
                Cells = {
                        radius.ToString()
                    }
            };

            if(willFullTable)
            {
                row.Cells.Add("-1");
                row.Cells.Add("-1");
            }

            Tooltip tooltip = new Tooltip();

            tooltip.Radius = radius;
            tooltip.MetalSheetWidth = 1.5;
            tooltip.OilDensity = 871;

            tooltip.MetalDensityKgPerCubicMetr = metalDensityKgPerCubicMetr;
            tooltip.MetalCostPeTon = metalCostPeTon;

            tooltip.BottomArea = RoundUp(radius * radius * Math.PI, 3);


            double height = RoundUp(volumeValue / tooltip.BottomArea, 3);
            row.Cells.Add(height.ToString());

            tooltip.WallSquire = RoundUp(2 * Math.PI * radius * 1.5 * (height / 1.5), 3);

            tooltip.beltInfos = CalculateBeltsThickness(volumeValue, height, radius, tooltip.MetalSheetWidth, tooltip.OilDensity);

            double lowerBeltWeight = tooltip.beltInfos.First().Thickness;
            row.Cells.Add(lowerBeltWeight.ToString());

            double upperBeltWeight = tooltip.beltInfos.Last().Thickness;
            row.Cells.Add(upperBeltWeight.ToString());

            tooltip.WallSteelWeight = CalculateWallWeightVolume(radius, tooltip.MetalDensityKgPerCubicMetr, tooltip.beltInfos);
            tooltip.BottomSteelWeight = RoundUp(metalDensityKgPerCubicMetr * tooltip.BottomArea * lowerBeltWeight, 3);
            tooltip.RoofSteelWeight = RoundUp(metalDensityKgPerCubicMetr * tooltip.BottomArea * 0.004, 3);

            double totalMetalArea = RoundUp(tooltip.BottomArea * 2 + tooltip.WallSquire, 0);
            row.Cells.Add(totalMetalArea.ToString());

            double totalSteelWeight = RoundUp(tooltip.WallSteelWeight + tooltip.BottomSteelWeight + tooltip.RoofSteelWeight, 0);
            row.Cells.Add(totalSteelWeight.ToString());

            row.Cells.Add(CalculateCylinderTankCosts(totalSteelWeight, metalCostPeTon).ToString());


            if (willFullTable)
            {
                row.Cells.Add(formTypes[1]);
            }


            row.TooltipInfo = tooltip;

            return row;
        }

        private decimal CalculateCylinderTankCosts(double tankWeightKilogram, decimal metalCostPerKillogram)
        {
            return (decimal)RoundUp(tankWeightKilogram / 1000 * (double)metalCostPerKillogram, 0);
        }

        private double CalculateWallWeightVolume(double radius, double metalDensityKgPerCubicMetr, List<BeltInfo> belts)
        {
            const double metalSheetWidth = 1.5;

            double squire = 2 * Math.PI * radius * metalSheetWidth,
                result = 0;

            foreach(var belt in belts)
            {
                result += RoundUp(metalDensityKgPerCubicMetr * squire * belt.Thickness, 0);
            }

            return result;
        }

        private List<BeltInfo> CalculateBeltsThickness(double volumeValue, double height, double radius, double metalSheetWidth, double oilDensity)
        {
            List<BeltInfo> belts = new List<BeltInfo>();
            double beltNumbers = RoundUp(height / metalSheetWidth, 0);

            for(int beltNumber = 1; beltNumber <= beltNumbers; beltNumber++)
            {
                BeltInfo beltInfo= new BeltInfo();

                beltInfo.beltNumber = beltNumber;
                beltInfo.Thickness = CalculateBeltThicknessByItNumber(beltNumber, volumeValue, height, radius, metalSheetWidth, oilDensity);

                belts.Add(beltInfo);
            }

            return belts;
        }

        private double CalculateBeltThicknessByItNumber(int beltNumber, double volumeValue, double height, double radius, double metalSheetWidth, double oilDensity)
        {
            const double lowerBeltWorkingConditionCoefficient = 0.7,
                upperBeltWorkingConditionCoefficient = 0.8;

            double thickness,
                reliabilityFactor, steelDesignResistance,
                workingConditionCoefficient;

            reliabilityFactor = volumeValue < 10000 ? 1.1 : 1.15;

            steelDesignResistance = 325 / (reliabilityFactor * 1.025);

            workingConditionCoefficient = beltNumber == 1
                ? lowerBeltWorkingConditionCoefficient
                : upperBeltWorkingConditionCoefficient;

            thickness = RoundUp((1.1 * oilDensity * 9.81 *
                (height - metalSheetWidth * (beltNumber - 1)) * radius)
                / (steelDesignResistance * 1000000 * workingConditionCoefficient), 3);

            thickness = thickness < 0.004 ? 0.004 : thickness;

            return thickness;
        }

        private double RoundUp(double flNumber, int numberAfterDot)
        {
            return ((long)Math.Ceiling(flNumber * Math.Pow(10, numberAfterDot))) / Math.Pow(10, numberAfterDot);
        }
    }
}
