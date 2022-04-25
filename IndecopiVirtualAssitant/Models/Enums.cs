using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Models.AzureTable
{

    public static class Enums
    {
        public enum DocumentType
        {
            [Description("DNI")] DNI = 1,
            [Description("CARNET DE EXTRANJERIA")] EXT = 2,
            [Description("PASAPORTE")] PASAPORTE = 4,
            [Description("RUC")] RUC = 3,
            //Description("PART. DE NACIMIENTO-IDENTIDAD")] PNAC = 5,
            //[Description("OTROS")] OTROS = 6,
            [Description("NO APORTA DOCUMENTO")] NO = 7

        }

        public static DocumentType GetDocumentType(string documentType)
        {
            switch (documentType) 
            {
                case "DNI":
                    return DocumentType.DNI;
                case "EXT":
                    return DocumentType.EXT;
                case "RUC":
                    return DocumentType.RUC;
                case "PASAPORTE":
                    return DocumentType.PASAPORTE;
                default:
                    return DocumentType.NO;
                    /*
                    case "PNAC":
                        return DocumentType.PNAC;
                    case "OTROS":
                        return DocumentType.OTROS;
                    default:
                        return DocumentType.NO;
                    */
            }
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
