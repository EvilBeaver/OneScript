using System;
using OneScript.Language;

namespace OneScript.Compilation.Binding
{
    public class BindingException : ApplicationException
    {
        public CodeError CodeError { get; }

        public BindingException(CodeError codeError) : base(codeError.Description)
        {
            CodeError = codeError;
        }
    }
}