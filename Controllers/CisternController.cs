using BackendOfSite.EFDbCistern;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendOfSite.Kafka;

namespace BackendOfSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CisternController : Controller
    {
        private readonly DbCisternContext db;
        private KafkaClient kafka_client = new KafkaClient("kafka:9092", KafkaClient.KafkaClientType.Producer);

        public CisternController(DbCisternContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult GetCisternNames()
        {
            kafka_client.SendMesssage(message: "CisternController: get cistern names");

            return Ok(db.Cisterns.Select(row => new
            {
                Id = row.CisternId,
                CisternNominal = row.NominalVolumeM3.ToString()
            }));
        }

        [HttpGet("CisternCharacters")]
        public IActionResult GetCisternCharacters(int cisternId)
        {
            var foundCistern = (from cistern in db.Cisterns
                                join wallMethodMade in db.MethodMades on cistern.WallMethodMadeId equals wallMethodMade.MethodMadeId
                                join bottomMethodMade in db.MethodMades on cistern.BottomMethodMadeId equals bottomMethodMade.MethodMadeId
                                join bottomTypeSlope in db.TypeSlopes on cistern.BottomTypeSlopeId equals bottomTypeSlope.TypeSlopeId
                                join roofTypeForm in db.RoofTypeForms on cistern.RoofTypeFormId equals roofTypeForm.RoofTypeFormId
                                join roofTypeConstruction in db.RoofTypeConstructions on cistern.RoofTypeConstructionId equals roofTypeConstruction.RoofTypeConstructionId
                                join ladderTypeConstruction in db.LadderTypeConstructions on cistern.LadderTypeConstructionId equals ladderTypeConstruction.LadderTypeConstructionId
                                join cisternPrice in db.PriceCisterns on cistern.CisternId equals cisternPrice.CisternId
                                where cistern.CisternId == cisternId
                                select new
                                {
                                    cistern.CisternId,
                                    cistern.NominalVolumeM3,
                                    cistern.WallInnerDrMm,
                                    cistern.WallHeightMm,
                                    cistern.HeightFillingMm,
                                    cistern.ClassDanger,
                                    cistern.WorkingLifeYear,
                                    cistern.WallBeltUnit,
                                    cistern.WallMarginRustMm,
                                    cistern.WallUpperBeltMm,
                                    cistern.WallLowerBeltMm,
                                    cistern.BottomEdgeUnit,
                                    cistern.BottomMarginRustMm,
                                    cistern.BottomCentreMm,
                                    cistern.BottomEdgeMm,
                                    cistern.RoofBeamUnit,
                                    cistern.RoofMarginRustMm,
                                    cistern.RoofBearingElement,
                                    cistern.RoofFlooringMm,
                                    cistern.WallWeightKg,
                                    cistern.BottomWeightKg,
                                    cistern.RoofWeightKg,
                                    cistern.LadderWeightKg,
                                    cistern.RoofPlatformKg,
                                    cistern.HatchPipeKg,
                                    cistern.AccessoriesKg,
                                    cistern.CarcassPackKg,
                                    wallMethodMade,
                                    bottomMethodMade,
                                    bottomTypeSlope,
                                    roofTypeForm,
                                    roofTypeConstruction,
                                    ladderTypeConstruction,
                                    cisternPrice
                                }).FirstOrDefault();

            if(foundCistern != null)
            {
                kafka_client.SendMesssage(message: "CisternController: get cistern characters");

                return Ok(foundCistern);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
