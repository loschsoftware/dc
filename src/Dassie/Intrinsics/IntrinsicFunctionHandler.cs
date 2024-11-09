using Dassie.Errors;
using Dassie.Parser;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Reflection.Emit;

namespace Dassie.Intrinsics;

// TODO: Improve intrinsics system
internal static class IntrinsicFunctionHandler
{
    public static bool HandleSpecialFunction(string name, DassieParser.ArglistContext args, int line, int column, int length)
    {
        if (typeof(Dassie.CompilerServices.CodeGeneration).GetMethod(name) == null)
            return false;

        //if (args == null)
        //{
        //    EmitErrorMessage(
        //        line,
        //        column,
        //        length,
        //        DS0080_ReservedIdentifier,
        //        $"The identifier '{name}' is reserved and cannot be used as a function or variable name."
        //        );

        //    return true;
        //}

        CurrentFile.Fragments.Add(new()
        {
            Line = line,
            Column = column,
            Length = length,
            Color = Color.IntrinsicFunction,
            ToolTip = TooltipGenerator.Function(typeof(Dassie.CompilerServices.CodeGeneration).GetMethod(name), true)
        });

        switch (name)
        {
            case "il":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'il'. Expected 1 argument."
                        );

                    return true;
                }

                string arg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                EmitInlineIL(arg, args.expression()[0].Start.Line, args.expression()[0].Start.Column + 1, args.expression()[0].GetText().Length);

                return true;

            case "localImport":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'localImport'. Expected 1 argument."
                        );

                    return true;
                }

                string ns = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                if (Type.GetType(ns) != null)
                {
                    CurrentFile.ImportedTypes.Add(ns);
                    return true;
                }

                CurrentFile.Imports.Add(ns);

                return true;

            case "globalImport":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'globalImport'. Expected 1 argument."
                        );

                    return true;
                }

                string _ns = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                if (Type.GetType(_ns) != null)
                {
                    Context.GlobalTypeImports.Add(_ns);
                    return true;
                }

                Context.GlobalImports.Add(_ns);

                return true;

            case "localAlias":

                if (args.expression().Length != 2)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'localAlias'. Expected 2 arguments."
                        );

                    return true;
                }

                string localAlias = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string localAliasedNS = args.expression()[1].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                CurrentFile.Aliases.Add((localAliasedNS, localAlias));

                return true;

            case "globalAlias":

                if (args.expression().Length != 2)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'globalAlias'. Expected 2 arguments."
                        );

                    return true;
                }

                string globalAlias = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string globalAliasedNS = args.expression()[1].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                CurrentFile.Aliases.Add((globalAliasedNS, globalAlias));

                return true;

            case "error":
            case "warn":
            case "msg":

                if (args.expression().Length != 2)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function '{name}'. Expected 2 arguments."
                        );

                    return true;
                }

                string code = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string err = args.expression()[1].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                ErrorInfo errInfo = new()
                {
                    CodePosition = (line, column),
                    Length = length,
                    CustomErrorCode = code,
                    ErrorCode = CustomError,
                    ErrorMessage = err,
                    File = Path.GetFileName(CurrentFile.Path),
                    Severity = name == "error" ? Severity.Error : name == "warn" ? Severity.Warning : Severity.Information
                };

                EmitGeneric(errInfo);

                return true;

            case "todo":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function '{name}'. Expected 1 argument."
                        );

                    return true;
                }

                string todoMsg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string todoStr = $"TODO ({CurrentFile.Path}, line {line}): {todoMsg}";

                CurrentMethod.IL.EmitWriteLine(todoStr);

                return true;

            case "ptodo":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function '{name}'. Expected 1 argument."
                        );

                    return true;
                }

                string ptodoMsg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string ptodoStr = $"TODO ({CurrentFile.Path}, line {line}): {ptodoMsg}";

                CurrentMethod.IL.Emit(OpCodes.Ldstr, ptodoStr);
                CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new[] { typeof(string) }));
                CurrentMethod.IL.Emit(OpCodes.Throw);

                return true;
        }

        return false;
    }
}