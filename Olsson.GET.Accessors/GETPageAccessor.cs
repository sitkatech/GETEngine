using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.Utilities;
using System.Linq;

namespace Olsson.GET.Accessors.GETPage
{
    public class GETPageAccessor
    {
        private static readonly ILogger Logger = Logging.GetLogger<GETPageAccessor>();

        public EntityFramework.GETPage GetGETPageByGETPageType(GETPageType GETPageType)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return db.GETPages.Single(x => x.GETPageTypeID == GETPageType.GETPageTypeID);
            }
        }

        public bool UpdateGETPageContent(int GETPageID, string newContent)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var GETPage = db.GETPages.Single(x => x.GETPageID == GETPageID);

                GETPage.GETPageContent = newContent;

                var result = db.SaveChanges();

                return result == 0 || result == 1;
            }
        }

        public bool CreateGETPageImage(int GETPageID, int fileResourceInfoId)
        {
            var GETPageImage = new GETPageImage()
            {
                GETPageID = GETPageID,
                FileResourceInfoID = fileResourceInfoId
            };

            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                db.GETPageImages.Add(GETPageImage);
                return db.SaveChanges() == 1;
            }
        }
    }
}
