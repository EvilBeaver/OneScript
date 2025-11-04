using System.Collections.Generic;
using System.Reflection;
using OneScript.Contexts;
using OneScript.DebugServices;
using ScriptEngine;
using ScriptEngine.Machine;
using Xunit;

namespace VSCode.DebugAdapter.Tests
{
    public class VariableVisualizerSuppressionTests
    {
        private sealed class CapturingLogWriter : ISystemLogWriter
        {
            public readonly List<string> Messages = new List<string>();
            public void Write(string text)
            {
                Messages.Add(text);
            }
        }

        private sealed class DummyVariable : IVariable
        {
            private IValue _value;
            public string Name { get; }

            public DummyVariable(IValue value, string name)
            {
                _value = value;
                Name = name;
            }

            public IValue Value
            {
                get
                {
                    if (!DeprecationWarningScope.IsSuppressed)
                    {
                        SystemLogger.Write("DEPRECATED");
                    }
                    return _value;
                }
                set => _value = value;
            }

            public OneScript.Types.TypeDescriptor SystemType => _value.SystemType;
            public int CompareTo(IValue other) => _value.CompareTo(other);
            public bool Equals(IValue other) => _value.Equals(other);
            public bool Equals(IValueReference other) => ReferenceEquals(this, other);
            public override string ToString() => _value.ToString();
        }

        [Fact]
        public void Visualizer_Suppresses_Deprecation_Log_During_Variable_Read()
        {
            // Сохраним текущий логгер и восстановим его в finally
            var writerField = typeof(SystemLogger).GetField("_writer", BindingFlags.Static | BindingFlags.NonPublic);
            var previousWriter = (ISystemLogWriter)writerField.GetValue(null);

            var log = new CapturingLogWriter();
            SystemLogger.SetWriter(log);
            try
            {
                var underlying = ScriptEngine.Machine.ValueFactory.Create("text");
                var variable = new DummyVariable(underlying, "v");
                var visualizer = new DefaultVariableVisualizer();

                // Внутри метода визуализатора чтение variable.Value подавляется
                var presented = visualizer.GetVariable(variable);
                Assert.NotNull(presented);
                Assert.Empty(log.Messages);

                // При реальном обращении к значению вне визуализатора предупреждение пишется
                var _ = variable.Value;
                Assert.Single(log.Messages);
                Assert.Contains("DEPRECATED", log.Messages[0]);
            }
            finally
            {
                SystemLogger.SetWriter(previousWriter);
            }
        }
    }
}