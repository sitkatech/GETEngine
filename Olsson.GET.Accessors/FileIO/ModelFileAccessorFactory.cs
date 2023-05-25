using System;
using Olsson.GET.Accessors.EntityFramework;
using Model = Olsson.GET.Common.DataContracts.Models.Model;

namespace Olsson.GET.Accessors.FileIO
{
    class ModelFileAccessorFactory : IModelFileAccessorFactory
    {
        public IFileFormatter CreateFileFormatterAccessor(ModelFileAccessor modflowFileAccessor)
        {
            switch (modflowFileAccessor.FileFormat)
            {
                case FileFormat.Delimited:
                    return new DelimitedFileFormatter(modflowFileAccessor);
                case FileFormat.FixedWidth:
                    return new FixedWidthFileFormatter(modflowFileAccessor);
                case FileFormat.ModflowSixStructured:
                    return new ModflowSixStructuredFileFormatter(modflowFileAccessor);
                case FileFormat.ModflowSixUnstructured:
                    return new ModflowSixUnstructuredFileFormatter(modflowFileAccessor);
                default:
                    throw new Exception("Unknown file format");
            }
        }

        public IModelFileAccessor CreateModflowFileAccessor(Model model)
        {
            switch ((ModelEngineTypeEnum)model.ModelEngineTypeID)
            {
                case ModelEngineTypeEnum.Modpath:
                    return new StructuredModflowFileAccessor(model);
                case ModelEngineTypeEnum.Modflow:
                    if (model.ModelGridTypeID == ModelGridType.Structured.ModelGridTypeID)
                    {
                        return new StructuredModflowFileAccessor(model);
                    }
                    return new UnstructuredModflowFileAccessor(model);
                case ModelEngineTypeEnum.Modflow6:
                    if (model.ModelGridTypeID == ModelGridType.Structured.ModelGridTypeID)
                    {
                        return new StructuredModflowSixFileAccessor(model);
                    }
                    return new UnstructuredModflowSixFileAccessor(model);
                case ModelEngineTypeEnum.IWFM:
                    return new StructuredModflowFileAccessor(model); // TODO: we need IWFM version
                default:
                    throw new ArgumentOutOfRangeException($"Unknown Model Engine Type {model.ModelEngineTypeID}");
            }
        }
    }
}