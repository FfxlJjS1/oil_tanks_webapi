using BackendOfSite.EFDbCistern;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static BackendOfSite.Controllers.CalculatorController;
using Microsoft.AspNetCore.Identity;
using System.Xml.Schema;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using BackendOfSite.Kafka;

namespace BackendOfSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly DbCisternContext db;

        readonly string[] oilTypes = { "Девонская", "Сернистая" };
        private List<SelectCister> selectCisterns = new List<SelectCister>();
        private KafkaClient kafka_client = new KafkaClient("kafka:9092", KafkaClient.KafkaClientType.Producer);

        public CalculatorController(DbCisternContext context)
        {
            db = context;
        }

        public class CalculatedOrder
        {
            public int SettlingTimeHour { get; set; }
            public double RequiredVolume { get; set; }
            public double UsefulVolume { get; set; }
            public List<Sample> Samples { get; set; } = new List<Sample>();
        }

        public class Sample
        {
            private decimal _totalPrice = 0;
            private double _totalVolume = 0f;

            public List<SelectCisternRecord> selectCisternRecords { get; set; } = new List<SelectCisternRecord>();

            public decimal TotalPrice => _totalPrice;

            public double TotalVolume => _totalVolume;

            public decimal TotalPriceForVolume => ((decimal)TotalVolume) / TotalPrice;

            public void UpdateTotals()
            {
                _totalPrice = selectCisternRecords.Sum(x => x.TotalPrice);
                _totalVolume = selectCisternRecords.Sum(x => x.TotalVolume);
            }

            public bool Equals(Sample other)
            {
                if (TotalVolume == other.TotalVolume && TotalPrice == other.TotalPrice && selectCisternRecords.Count == other.selectCisternRecords.Count)
                {
                    long equalRecords = 0;

                    foreach (var iteratorX in this.selectCisternRecords)
                    {
                        foreach (var iteratorY in other.selectCisternRecords)
                        {
                            if (iteratorX.cistern.NominalVolume == iteratorY.cistern.NominalVolume
                                && iteratorX.CisternsNumber == iteratorY.CisternsNumber)
                            {
                                equalRecords++;
                            }
                        }
                    }

                    return equalRecords == selectCisternRecords.Count;
                }

                return false;
            }
        }

        public class SelectCisternRecord
        {
            public SelectCisternRecord() { }

            public SelectCisternRecord(SelectCisternRecord x)
            {
                cistern = x.cistern;
                CisternsNumber = x.CisternsNumber;
            }

            public SelectCister cistern { get; set; } = new SelectCister();
            public long CisternsNumber { get; set; }

            public decimal TotalPrice => cistern.CisternPrice * CisternsNumber;

            public double TotalVolume => cistern.NominalVolume * CisternsNumber;
        }

        public class SelectCister
        {
            public float NominalVolume { get; set; }
            public decimal CisternPrice { get; set; }

            public decimal PriceForVolume => ((decimal)NominalVolume) / CisternPrice;
        }

        [HttpGet("GetProductParks")]
        public IActionResult GetProductParks()
        {
            kafka_client.SendMesssage(message: "CalculatorController: get product parks");

            return Ok(db.ProductParks);
        }

        [HttpGet("GetCisternPurposes")]
        public IActionResult GetCisternPurposes()
        {
            kafka_client.SendMesssage(message: "CalculatorController: get product purposes");

            return Ok(db.PurposeCisterns);
        }

        [HttpGet("GetCisternPurposesForOilType")]
        public IActionResult GetCisternPurposesForOilType(string oilType)
        {
            int oilTypeId = Array.IndexOf(oilTypes, oilType);
            List<PurposeCistern> resultCisternPurposes = new List<PurposeCistern>();

            if (oilTypeId == 0)
            {
                resultCisternPurposes = db.StandartSludges.Where(x => x.DevonHour > 0).Select(x => x.PurposeCistern).ToList();
            }
            else if (oilTypeId == 1)
            {
                resultCisternPurposes = db.StandartSludges.Where(x => x.SulfuricHour > 0).Select(x => x.PurposeCistern).ToList();
            }

            kafka_client.SendMesssage(message: "CalculatorController: get cistern purpose for oil type");

            return Ok(resultCisternPurposes);
        }

        [HttpGet("GetOilTypes")]
        public IActionResult GetOilTypes()
        {
            return Ok(oilTypes);
        }

        [HttpGet("CalculateByProductPark")]
        public IActionResult Calculate(int productParkId, int cisternPurposeId, int needCount, bool groupSelect)
        {
            int oilTypeId = 0;
            decimal maxOil = -1, maxWater = -1;

            maxOil = db.FlowRates.Include(p => p.ProductPark).Where(p => p.ProductParkId == productParkId)
                .GroupBy(x => new { x.Month, x.Year },
                    (key, group) => new { key.Month, key.Year, Result = group.Sum(x => x.QnPred) })
                .Max(x => x.Result) ?? -1;

            maxWater = db.FlowRates.Include(p => p.ProductPark).Where(p => p.ProductParkId == productParkId)
                .GroupBy(x => new { x.Month, x.Year },
                    (key, group) => new { key.Month, key.Year, Result = group.Sum(x => x.QwPred) })
                .Max(x => x.Result) ?? -1;

            if (maxOil != -1 && maxWater != -1)
            {
                return Ok(CalculateByNW(cisternPurposeId, oilTypeId, (double)maxOil, (double)maxWater, needCount, groupSelect));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("CalculateByValues")]
        public IActionResult Calculate(int cisternPurposeId, string oilType, double oilValue, double waterValue, int needCount, bool groupSelect)
        {
            int oilTypeId = Array.IndexOf(oilTypes, oilType);

            if (cisternPurposeId == -1 || oilTypeId == -1)
            {
                return NotFound();
            }

            CalculatedOrder data;

            data = CalculateByNW(cisternPurposeId, oilTypeId, oilValue, waterValue, needCount, groupSelect);

            return Ok(data);
        }

        private CalculatedOrder CalculateByNW(int cisternPurposeId, int oilTypeId, double oilValue, double waterValue, int needCount, bool groupSelect)
        {
            CalculatedOrder data = new CalculatedOrder();

            selectCisterns = (from cistern in db.Cisterns
                              join cisternPrice in db.PriceCisterns on cistern.CisternId equals cisternPrice.CisternId
                              select new SelectCister
                              {
                                  NominalVolume = cistern.NominalVolumeM3,
                                  CisternPrice = cisternPrice.PriceRub ?? 0
                              }).ToList().Where(x => x.CisternPrice > 0).OrderBy(x => x.PriceForVolume).ToList();

            int settlingTime = CalculateSettlingTime(cisternPurposeId, oilTypeId);
            double needVolumeM3 = CalculateNeedVolumeM3(cisternPurposeId, oilValue, waterValue, settlingTime),
                usefulVolume = CalculateUsefulVolume(cisternPurposeId);

            data.SettlingTimeHour = settlingTime;
            data.RequiredVolume = needVolumeM3;
            data.UsefulVolume = usefulVolume;

            if (oilValue > 0 && waterValue > 0 && (cisternPurposeId != 0 || oilTypeId == 0))
            {
                if (groupSelect)
                {
                    data.Samples = CalculateTanksForParametrsGroupSelect(needVolumeM3, usefulVolume, needCount);
                }
                else
                {
                    data.Samples = CalculateTanksForParametrs(needVolumeM3, usefulVolume, needCount);
                }
            }

            return data;
        }

        private int CalculateSettlingTime(int cisternPurposeId, int oilTypeId)
        {
            int settlingTime = 0;

            var standartSludgeRow = db.StandartSludges.Where(row => row.PurposeCisternId == cisternPurposeId).First();

            if (oilTypeId == 0)
            {
                settlingTime = standartSludgeRow.DevonHour ?? 0;
            }
            else if (oilTypeId == 1)
            {
                settlingTime = standartSludgeRow.SulfuricHour ?? 0;
            }

            return settlingTime;
        }

        private double CalculateNeedVolumeM3(int cisternPurposeId, double oilValue, double waterValue, int settlingTime)
        {
            double needVolume = 0;

            switch (cisternPurposeId)
            {
                case 1:
                    needVolume = (oilValue + waterValue) / 24 * settlingTime;
                    break;
                case 2:
                    needVolume = (oilValue) / 24 * settlingTime;
                    break;
                case 3:
                    needVolume = (oilValue) / 24 * settlingTime;
                    break;
                case 4:
                    needVolume = (waterValue) / 24 * settlingTime;
                    break;
                case 5:
                    needVolume = (waterValue) / 24 * settlingTime;
                    break;
            }

            return Math.Round(needVolume);
        }

        private double CalculateUsefulVolume(int cisternPurposeId)
        {
            double usefulVolume = 0;

            switch (cisternPurposeId)
            {
                case 1:
                    usefulVolume = 0.7;
                    break;
                case 2:
                    usefulVolume = 0.85;
                    break;
                case 3:
                    usefulVolume = 0.9;
                    break;
                case 4:
                    usefulVolume = 0.9;
                    break;
                case 5:
                    usefulVolume = 0.8;
                    break;
            }

            return usefulVolume;
        }

        private List<Sample> CalculateTanksForParametrs(double needVolumeM3, double usefulVolume, int needCount)
        {
            List<Sample> samples = new List<Sample>();

            List<Sample> tankRecords = new List<Sample>();

            foreach (var cisternVolumePrice in selectCisterns)
            {
                tankRecords.Add(new Sample()
                {
                    selectCisternRecords = new List<SelectCisternRecord>()
                    {
                        new SelectCisternRecord() {
                         cistern = cisternVolumePrice,
                        CisternsNumber = (long)Math.Ceiling(needVolumeM3 / (cisternVolumePrice.NominalVolume * usefulVolume))
                        }
                    }
                });
            }

            tankRecords.ForEach(x => x.UpdateTotals());
            samples = tankRecords.OrderBy(group => group.TotalPrice).ToList();

            if (needCount > 0)
            {
                samples = samples.Take(needCount).ToList();
            }

            return samples;
        }

        private List<Sample> CalculateTanksForParametrsGroupSelect(double needVolumeM3, double usefulVolume, int needCount)
        {
            List<Sample> result = CalculateTanksForParametrs(needVolumeM3, usefulVolume, 0);
            decimal priceUpperBound = result.Min(x => x.TotalPrice);
            decimal priceUpperBoundInTempResult = -1;

            List<Sample> tempResult = result.Skip(1).ToList();

            result = result.Take(1).ToList();

            List<Sample> unfinishedSamples = CalculateTanksForParametrs(needVolumeM3, usefulVolume, 0)
                .Where(x => x.selectCisternRecords.Sum(x => x.CisternsNumber) > 1).ToList();
            unfinishedSamples.ForEach(x =>
            {
                x.selectCisternRecords.First().CisternsNumber -= 1;
                x.UpdateTotals();
            });

            needVolumeM3 /= usefulVolume;

            selectCisterns = selectCisterns.Where(cistern => cistern.NominalVolume < needVolumeM3).ToList();

            List<Sample> forCheckingSamples = unfinishedSamples.Where(x => x.TotalPrice <= priceUpperBound).OrderBy(x => x.TotalPrice).ToList();

            unfinishedSamples = unfinishedSamples.Where(x => x.TotalPrice > priceUpperBound).OrderBy(x => x.TotalPrice).ToList();


            while ((result.Count < needCount || needCount == 0) && (forCheckingSamples.Count > 0 || unfinishedSamples.Count > 0 || tempResult.Count > 0))
            {
                // If checking samples is empty
                if (forCheckingSamples.Count == 0)
                {
                    // If infinished samples for moving to checking samples is empty, exit from while and send result
                    if (unfinishedSamples.Count == 0)
                    {
                        result.AddRange(tempResult);
                        tempResult.Clear();

                        break;
                    }
                    // If unfinished samples isn't empty, change price upper bound and move unfinished samples with less total price than price upper bount to checking samples
                    else
                    {
                        priceUpperBound = tempResult.Where(x => x.TotalPrice > priceUpperBound).Min(x => x.TotalPrice);

                        forCheckingSamples = unfinishedSamples
                            .Where(x => x.TotalPrice <= priceUpperBound).OrderBy(x => x.TotalPriceForVolume).ToList();
                        unfinishedSamples = unfinishedSamples
                            .Where(x => x.TotalPrice > priceUpperBound).OrderBy(x => x.TotalPriceForVolume).ToList();

                        // Move samples with less total price than price upper bound from temp results list to result list
                        result.AddRange(tempResult.Where(x => x.TotalPrice <= priceUpperBound).ToList());
                        tempResult = tempResult.Where(x => x.TotalPrice > priceUpperBound).OrderBy(x => x.TotalPrice).ToList();

                        if (needCount > 0)
                        {
                            tempResult = tempResult.Take(needCount).ToList();

                            if (tempResult.Count == needCount)
                            {
                                priceUpperBoundInTempResult = tempResult.Max(x => x.TotalPrice);
                            }
                        }

                        continue;
                    }
                }


                // Check all tree of one of the samples for cheking
                List<Sample> forCheckingSamplesBranch = new List<Sample>() { forCheckingSamples.First() };
                forCheckingSamples = forCheckingSamples.Skip(1).ToList();

                {
                    Sample sample = forCheckingSamplesBranch.First();
                    forCheckingSamplesBranch = forCheckingSamplesBranch.Skip(1).ToList();

                    // For calculate temp cistern for more effencity algorithm work
                    foreach (var selectCistern in selectCisterns)
                    {
                        Sample newSample = new Sample();

                        sample.selectCisternRecords.ForEach(forCopy => newSample.selectCisternRecords.Add(new SelectCisternRecord(forCopy)));
                        newSample.UpdateTotals();

                        var findRecord = newSample.selectCisternRecords.FirstOrDefault(x => x.cistern == selectCistern);

                        if (findRecord == null)
                        {
                            newSample.selectCisternRecords.Add(new SelectCisternRecord()
                            {
                                cistern = selectCistern,
                                CisternsNumber = (long)Math.Ceiling((needVolumeM3 - newSample.TotalVolume) / selectCistern.NominalVolume)
                            });
                        }
                        else
                        {
                            findRecord.CisternsNumber += (long)Math.Ceiling((needVolumeM3 - newSample.TotalVolume) / findRecord.cistern.NominalVolume);
                        }

                        newSample.UpdateTotals();

                        bool isExist = false;

                        foreach (var cisternRecord in newSample.selectCisternRecords)
                        {
                            if (newSample.TotalVolume - cisternRecord.cistern.NominalVolume >= needVolumeM3)
                            {
                                isExist = true;

                                break;
                            }
                        }

                        foreach (Sample tempSample in tempResult)
                            if (newSample.Equals(tempSample))
                            {
                                isExist = true;
                                break;
                            }

                        if (!isExist)
                        {
                            tempResult.Add(newSample);
                        }
                    }

                    // For next calculations
                    foreach (var selectCistern in selectCisterns)
                    {
                        Sample newSample = new Sample();

                        sample.selectCisternRecords.ForEach(forCopy => newSample.selectCisternRecords.Add(new SelectCisternRecord(forCopy)));

                        var findRecord = newSample.selectCisternRecords.FirstOrDefault(x => x.cistern == selectCistern);

                        if (findRecord == null)
                        {
                            newSample.selectCisternRecords.Add(new SelectCisternRecord() { cistern = selectCistern, CisternsNumber = 1 });
                        }
                        else
                        {
                            findRecord.CisternsNumber += 1;
                        }

                        bool isExist = false;

                        newSample.UpdateTotals();
                        if (newSample.TotalVolume >= needVolumeM3)
                        {
                            foreach (var cisternRecord in newSample.selectCisternRecords)
                            {
                                if (newSample.TotalVolume - cisternRecord.cistern.NominalVolume >= needVolumeM3)
                                {
                                    isExist = true;

                                    break;
                                }
                            }

                            foreach (Sample tempSample in tempResult)
                                if (newSample.Equals(tempSample))
                                {
                                    isExist = true;
                                    break;
                                }

                            if (!isExist && (priceUpperBoundInTempResult == -1 || newSample.TotalPrice < priceUpperBoundInTempResult))
                            {
                                tempResult.Add(newSample);
                            }
                        }
                        else
                        {
                            if (priceUpperBoundInTempResult == -1 || newSample.TotalPrice < priceUpperBoundInTempResult)
                            {
                                if (newSample.TotalPrice <= priceUpperBound)
                                {
                                    foreach (Sample forCheckingSample in forCheckingSamples)
                                        if (forCheckingSample.Equals(newSample))
                                        {
                                            isExist = true;
                                            break;
                                        }

                                    if (!isExist)
                                    {
                                        forCheckingSamples.Add(newSample);
                                    }
                                }
                                else
                                {
                                    foreach (Sample unfinishedSample in unfinishedSamples)
                                        if (unfinishedSample.Equals(newSample))
                                        {
                                            isExist = true;
                                            break;
                                        }

                                    if (!isExist)
                                    {
                                        unfinishedSamples.Add(newSample);
                                    }
                                }
                            }
                        }
                    }
                }


                // Move samples with less total price than price upper bound from temp results list to result list
                if(needCount > 0)
                {
                    int countForOptimiza = 1000 + needCount * 25;

                    forCheckingSamples = forCheckingSamples.OrderBy(x => x.TotalPriceForVolume).Take(countForOptimiza).ToList();
                    unfinishedSamples = unfinishedSamples.OrderBy(x => x.TotalPriceForVolume).Take(countForOptimiza).ToList();
                }

                forCheckingSamples = forCheckingSamples.OrderBy(x => x.TotalPrice).ToList();
                unfinishedSamples = unfinishedSamples.OrderBy(x => x.TotalPrice).ToList();

                result.AddRange(tempResult.Where(x => x.TotalPrice <= priceUpperBound).ToList());
                tempResult = tempResult.Where(x => x.TotalPrice > priceUpperBound).OrderBy(x => x.TotalPrice).ToList();

                if (needCount > 0)
                {
                    tempResult = tempResult.Take(needCount).ToList();

                    if (tempResult.Count == needCount)
                    {
                        priceUpperBoundInTempResult = tempResult.Max(x => x.TotalPrice);
                    }
                }
            }


            result = result.OrderBy(x => x.TotalPrice).ToList();

            if(needCount> 0)
            {
                result = result.Take(needCount).ToList();
            }


            return result;
        }
    }
}
