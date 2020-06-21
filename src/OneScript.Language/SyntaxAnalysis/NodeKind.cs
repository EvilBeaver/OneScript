/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.SyntaxAnalysis
{
    public enum NodeKind
    {
        Unknown,
        Module,
        StatelessModule,
        VariablesSection,
        MethodsSection,
        ModuleBody,
        Annotation,
        AnnotationParameter,
        AnnotationParameterName,
        AnnotationParameterValue,
        VariableDefinition,
        ByValModifier,
        Identifier,
        ExportFlag,
        Procedure,
        Function,
        Method,
        MethodSignature,
        MethodParameters,
        MethodParameter,
        ParameterDefaultValue,
        MethodBody,
        BlockEnd,
        CodeBatch,
        Statement,
        Call,
        CallArgumentList,
        CallArgument,
        DereferenceOperation,
        Constant,
        IndexAccess,
        BinaryOperation,
        UnaryOperation,
        Assignment,
        TernaryOperator,
        NewObject,
        Loop
    }
}