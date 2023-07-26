using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Accessors.GETPage
{
    public class ExternalMapLayerAccessor
    {
        private static readonly ILogger Logger = Logging.GetLogger<GETPageAccessor>();

        public IQueryable<ExternalMapLayer> ExternalMapLayerImpl(PrimaryDBContext dbContext)
        {
            return dbContext.ExternalMapLayers
                .Include(x => x.ExternalMapLayerCustomerModels);
        }

        public ExternalMapLayer New(ExternalMapLayer externalMapLayer, List<ExternalMapLayerCustomerModelSimpleDto> externalMapLayerCustomerModels)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                db.ExternalMapLayers.Add(externalMapLayer);
                db.SaveChanges();

                if (!externalMapLayer.IsAvailableForAllConfigurations && externalMapLayerCustomerModels != null && externalMapLayerCustomerModels.Any())
                {
                    var newExternalMapLayerCustomerModels = externalMapLayerCustomerModels.Select(x =>
                            new ExternalMapLayerCustomerModel(externalMapLayer.ExternalMapLayerID, x.CustomerID,
                                x.ModelID))
                        .ToList();
                    db.ExternalMapLayerCustomerModels.AddRange(newExternalMapLayerCustomerModels);
                }
                else
                {
                    externalMapLayer.IsAvailableForAllConfigurations = true;
                }
                db.SaveChanges();

                return externalMapLayer;
            }
        }

        public ExternalMapLayer Edit(ExternalMapLayer externalMapLayer, List<ExternalMapLayerCustomerModelSimpleDto> externalMapLayerCustomerModels, int? customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var oldExternalMapLayerCustomerModels =
                    db.ExternalMapLayerCustomerModels.Where(x =>
                        x.ExternalMapLayerID == externalMapLayer.ExternalMapLayerID);

                if (externalMapLayer.IsAvailableForAllConfigurations)
                {
                    db.ExternalMapLayerCustomerModels.RemoveRange(oldExternalMapLayerCustomerModels);
                    externalMapLayer.IsAvailableForAllConfigurations = true;
                }
                else
                {
                    db.ExternalMapLayerCustomerModels.RemoveRange(oldExternalMapLayerCustomerModels);
                    db.SaveChanges();

                    externalMapLayer.IsAvailableForAllConfigurations = false;

                    if (externalMapLayerCustomerModels != null && externalMapLayerCustomerModels.Any())
                    {
                        var newExternalMapLayerCustomerModels = externalMapLayerCustomerModels.Select(x =>
                            new ExternalMapLayerCustomerModel(externalMapLayer.ExternalMapLayerID, x.CustomerID,
                                x.ModelID))
                        .ToList();
                        db.ExternalMapLayerCustomerModels.AddRange(newExternalMapLayerCustomerModels);
                    }
                }
                db.SaveChanges();


                if (db.Entry(externalMapLayer).State == EntityState.Detached)
                {
                    db.Set<ExternalMapLayer>().Attach(externalMapLayer);
                }

                db.Entry(externalMapLayer).State = EntityState.Modified;
                db.SaveChanges();

                return externalMapLayer;
            }
        }

        public List<ExternalMapLayer> List()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return db.ExternalMapLayers.ToList();
            }
        }

        public List<ExternalMapLayer> ListByCustomerID(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return ExternalMapLayerImpl(db).Where(x => x.ExternalMapLayerCustomerModels.Any(y => y.CustomerID == customerID)).ToList();
            }
        }

        public ExternalMapLayer GetByExternalMapLayerID(int externalMapLayerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return ExternalMapLayerImpl(db).SingleOrDefault(x => x.ExternalMapLayerID == externalMapLayerID);
            }
        }

        public ExternalMapLayer GetByExternalMapLayerDisplayName(string externalMapLayerDisplayName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return ExternalMapLayerImpl(db).SingleOrDefault(x => x.ExternalMapLayerDisplayName == externalMapLayerDisplayName);
            }
        }

        public ExternalMapLayer GetByExternalMapLayerURL(string url)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return ExternalMapLayerImpl(db).SingleOrDefault(x => x.ExternalMapLayerURL == url);
            }
        }

        public List<ExternalMapLayerSimpleDto> GetByIsActiveForCustomerIDModelID(int customerID, int modelID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return db.ExternalMapLayers.Include(x =>  x.ExternalMapLayerCustomerModels).Where(x => x.IsActive && (x.IsAvailableForAllConfigurations || x.ExternalMapLayerCustomerModels.Any(y => y.ModelID == modelID && y.CustomerID == customerID))).ToList().Select(x => x.AsSimpleDto()).ToList();
            }
        }

        public void Delete(int externalMapLayerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var emlcms = db.ExternalMapLayerCustomerModels.Where(x => x.ExternalMapLayerID == externalMapLayerID);

                db.ExternalMapLayerCustomerModels.RemoveRange(emlcms);
                db.SaveChanges();

                var externalMapLayer =
                    db.ExternalMapLayers.Single(x => x.ExternalMapLayerID == externalMapLayerID);
                db.ExternalMapLayers.Remove(externalMapLayer);
                db.SaveChanges();
            }
        }

        public List<ExternalMapLayerSimpleDto> GetByIsActive(bool isActive)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return db.ExternalMapLayers.Where(x => x.IsActive).Select(x => x.AsSimpleDto()).ToList();
            }
        }
    }
}