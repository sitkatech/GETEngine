//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelGridType]
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    public abstract partial class ModelGridType : IHavePrimaryKey
    {
        public static readonly ModelGridTypeStructured Structured = ModelGridTypeStructured.Instance;
        public static readonly ModelGridTypeUnstructured Unstructured = ModelGridTypeUnstructured.Instance;

        public static readonly List<ModelGridType> All;
        public static readonly ReadOnlyDictionary<int, ModelGridType> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static ModelGridType()
        {
            All = new List<ModelGridType> { Structured, Unstructured };
            AllLookupDictionary = new ReadOnlyDictionary<int, ModelGridType>(All.ToDictionary(x => x.ModelGridTypeID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected ModelGridType(int modelGridTypeID, string modelGridTypeName, string modelGridTypeDisplayName)
        {
            ModelGridTypeID = modelGridTypeID;
            ModelGridTypeName = modelGridTypeName;
            ModelGridTypeDisplayName = modelGridTypeDisplayName;
        }

        [Key]
        public int ModelGridTypeID { get; private set; }
        public string ModelGridTypeName { get; private set; }
        public string ModelGridTypeDisplayName { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelGridTypeID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(ModelGridType other)
        {
            if (other == null)
            {
                return false;
            }
            return other.ModelGridTypeID == ModelGridTypeID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as ModelGridType);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return ModelGridTypeID;
        }

        public static bool operator ==(ModelGridType left, ModelGridType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelGridType left, ModelGridType right)
        {
            return !Equals(left, right);
        }

        public ModelGridTypeEnum ToEnum { get { return (ModelGridTypeEnum)GetHashCode(); } }

        public static ModelGridType ToType(int enumValue)
        {
            return ToType((ModelGridTypeEnum)enumValue);
        }

        public static ModelGridType ToType(ModelGridTypeEnum enumValue)
        {
            switch (enumValue)
            {
                case ModelGridTypeEnum.Structured:
                    return Structured;
                case ModelGridTypeEnum.Unstructured:
                    return Unstructured;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum ModelGridTypeEnum
    {
        Structured = 1,
        Unstructured = 2
    }

    public partial class ModelGridTypeStructured : ModelGridType
    {
        private ModelGridTypeStructured(int modelGridTypeID, string modelGridTypeName, string modelGridTypeDisplayName) : base(modelGridTypeID, modelGridTypeName, modelGridTypeDisplayName) {}
        public static readonly ModelGridTypeStructured Instance = new ModelGridTypeStructured(1, @"Structured", @"Structured");
    }

    public partial class ModelGridTypeUnstructured : ModelGridType
    {
        private ModelGridTypeUnstructured(int modelGridTypeID, string modelGridTypeName, string modelGridTypeDisplayName) : base(modelGridTypeID, modelGridTypeName, modelGridTypeDisplayName) {}
        public static readonly ModelGridTypeUnstructured Instance = new ModelGridTypeUnstructured(2, @"Unstructured", @"Unstructured");
    }
}