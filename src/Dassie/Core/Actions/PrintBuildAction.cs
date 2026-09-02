using Dassie.Extensions;
using Dassie.Syntax.Helpers;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dassie.Core.Actions;

internal class PrintBuildAction : IBuildAction
{
    public string Name => "Print";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        StringBuilder sb = new(StringHelpers.UnescapeString(context.Text));

        XmlAttribute newLineAttrib = context.XmlAttributes.FirstOrDefault(a => a.Name == "NewLine");
        if (newLineAttrib == null || newLineAttrib.Value == "true")
            sb.AppendLine();
        
        LogOut.Write(sb.ToString());
        return 0;
    }
}