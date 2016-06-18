namespace LinqTo1C.Impl.Generation
{
    public static class GeneratorTemplates
    {
        public static readonly SimpleFormat namespaceFormat = SimpleFormat.Parse(@"
namespace %namespace-name%
{
%content%
}");
        public static readonly SimpleFormat classFormat = SimpleFormat.Parse(@"
    public partial class %class-name% : Abstract1CEntity
    {
%content%
    }");

        public static readonly SimpleFormat propertyFormat = SimpleFormat.Parse(@"
        private Requisite<%type%> %field-name%;
        public %type% %property-name%
        {
            get { return Controller.GetValue(ref %field-name%, ""%property-name%""); }
            set { Controller.SetValue(ref %field-name%, ""%property-name%"", value); }
        }");

        public static readonly SimpleFormat enumFormat = SimpleFormat.Parse(@"
    public enum %name%
    {
%content%
    }");

        public static readonly SimpleFormat fileFormat = SimpleFormat.Parse(@"
using System;
using System.Collections.Generic;
using Knopka.Application._1C.Mapper;

%content%");
    }
}