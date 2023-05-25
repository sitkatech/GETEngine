namespace Olsson.GET.Common.DataContracts.APIFunctionModels
{
    public class RetrieveResultModel
    {
        public int? RunId { get; set; }

        public int? CustomerId { get; set; }

        public string FileName { get; set; }

        public string FileDate { get; set; }

        public string SubType { get; set; }

        public string FileExtension { get; set; }
    }
}
