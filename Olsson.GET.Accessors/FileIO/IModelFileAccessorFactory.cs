using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IModelFileAccessorFactory
    {
        IModelFileAccessor CreateModflowFileAccessor(Model model);

        IFileFormatter CreateFileFormatterAccessor(ModelFileAccessor modflowFileAccessor);
    }
}