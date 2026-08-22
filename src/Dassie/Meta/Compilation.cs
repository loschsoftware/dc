using Dassie.Configuration;
using Dassie.Data;
using Dassie.Messages;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dassie.Meta;

internal sealed class Compilation
{
    public Compilation(DassieConfig configuration = null, IEnumerable<Document> inputDocuments = null, TextWriter logOut = null)
    {
        Configuration = configuration ?? DassieConfig.Default;
        InputDocuments = inputDocuments ?? [];
        DiagnosticManager = new DiagnosticManager(logOut ?? Console.Out);
    }

    public DassieConfig Configuration { get; set; }
    public IEnumerable<Document> InputDocuments { get; set; }
    public DiagnosticManager DiagnosticManager { get; }
}