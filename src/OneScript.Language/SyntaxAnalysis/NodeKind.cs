/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

// ReSharper disable InconsistentNaming

using System.Reflection;
using System.Linq;

namespace OneScript.Language.SyntaxAnalysis
{
    public static class NodeKind
    {
        public const int Unknown = 0;
        public const int Module = 1;
        public const int VariablesSection = 2;
        public const int MethodsSection = 3;
        public const int ModuleBody = 4;
        public const int Annotation = 5;
        public const int AnnotationParameter = 6;
        public const int AnnotationParameterName = 7;
        public const int AnnotationParameterValue = 8;
        public const int VariableDefinition = 9;
        public const int ByValModifier = 10;
        public const int Identifier = 11;
        public const int ExportFlag = 12;
        public const int Procedure = 13;
        public const int Function = 14;
        public const int Method = 15;
        public const int MethodSignature = 16;
        public const int MethodParameters = 17;
        public const int MethodParameter = 18;
        public const int ParameterDefaultValue = 19;
        public const int BlockEnd = 20;
        public const int CodeBatch = 21;
        public const int GlobalCall = 22;
        public const int MethodCall = 23;
        public const int CallArgumentList = 24;
        public const int CallArgument = 25;
        public const int DereferenceOperation = 26;
        public const int Constant = 27;
        public const int IndexAccess = 28;
        public const int BinaryOperation = 29;
        public const int UnaryOperation = 30;
        public const int Assignment = 31;
        public const int TernaryOperator = 32;
        public const int NewObject = 33;
        public const int WhileLoop = 34;
        public const int ForLoop = 35;
        public const int Condition = 36;
        public const int ForEachLoop = 37;
        public const int ForEachVariable = 38;
        public const int ForEachCollection = 39;
        public const int ForInitializer = 40;
        public const int ForLimit = 41;
        public const int BreakStatement = 42;
        public const int ContinueStatement = 43;
        public const int ReturnStatement = 44;

        public static string Presentation(int kind)
        {
            var fields = typeof(NodeKind).GetFields(BindingFlags.Static | BindingFlags.Public);
            var field = fields?.FirstOrDefault(x => (int)x.GetValue(null) == kind);
            return field?.Name ?? kind.ToString();
        }
    }
}