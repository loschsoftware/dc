using System.Collections.Generic;
using System.Linq;

namespace Dassie.Model;

internal record ModifiedDassieType : DassieType
{
    public ModifiedDassieType() { }
    public ModifiedDassieType(DassieType source) : base(source) { }

    public IEnumerable<DassieCustomModifier> CustomModifiers { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()}{(CustomModifiers?.Any() == true ? " " : "")}{string.Join(' ', CustomModifiers.Select(m => $"{(m.IsOptional ? "modopt" : "modreq")}({m.Type})"))}";
    }
}