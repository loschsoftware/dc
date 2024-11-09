using Antlr4.Runtime.Misc;
using Dassie.Parser;
using System.Linq;

namespace Dassie.CodeAnalysis.Structure;

internal class StructureListener(ProjectStructure prevStructure, string filePath) : DassieParserBaseListener
{
    public ProjectStructure Structure { get; } = prevStructure;
    private string _namespaceId;
    private Namespace _namespace;

    public override void EnterExport_directive([NotNull] DassieParser.Export_directiveContext context)
    {
        _namespaceId = context.full_identifier().GetText();
        for (int i = _namespaceId.Split('.').Length; i > 0; i--)
        {
            string ns = string.Join(".", _namespaceId.Split('.')[0..i]);
            if (Structure.Namespaces.Any(n => n.Name == ns) && ns != _namespaceId)
            {
                _namespace = new()
                {
                    Name = _namespaceId
                };

                Structure.Namespaces.First(n => n.Name == ns).Namespaces =
                [
                    .. Structure.Namespaces.First(n => n.Name == ns).Namespaces ?? [],
                    _namespace
                ];

                return;
            }
        }

        if (!Structure.Namespaces.Any(n => n.Name == _namespaceId))
        {
            _namespace = new()
            {
                Name = _namespaceId
            };

            Structure.Namespaces =
            [
                .. Structure.Namespaces ?? [],
                _namespace
            ];
        }
        else
            _namespace = Structure.Namespaces.First(n => n.Name == _namespaceId);
    }

    public override void EnterType([NotNull] DassieParser.TypeContext context)
    {
        Type.Kind kind = Type.Kind.RefType;

        if (context.type_kind().Val() != null)
            kind = Type.Kind.ValType;
        else if (context.type_kind().Template() != null)
            kind = Type.Kind.Template;
        else if (context.type_kind().Module() != null)
            kind = Type.Kind.Module;

        if (string.IsNullOrEmpty(_namespaceId))
        {
            Structure.Types =
            [
                .. Structure.Types ?? [],
                new()
                {
                    Name = context.Identifier().GetText(),
                    Files = [filePath],
                    TypeKind = kind
                }
            ];
        }
        else
        {
            _namespace.Types =
            [
                .. _namespace.Types ?? [],
                new()
                {
                    Name = context.Identifier().GetText(),
                    Files = [filePath],
                    TypeKind = kind
                }
            ];
        }
    }
}