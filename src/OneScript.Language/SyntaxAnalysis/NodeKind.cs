/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

// ReSharper disable InconsistentNaming
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
        public const int Call = 22;
        public const int CallArgumentList = 23;
        public const int CallArgument = 24;
        public const int DereferenceOperation = 25;
        public const int Constant = 26;
        public const int IndexAccess = 27;
        public const int BinaryOperation = 28;
        public const int UnaryOperation = 29;
        public const int Assignment = 30;
        public const int TernaryOperator = 31;
        public const int NewObject = 32;
        public const int WhileLoop = 33;
        public const int ForLoop = 34;
        public const int Condition = 35;
    }
}