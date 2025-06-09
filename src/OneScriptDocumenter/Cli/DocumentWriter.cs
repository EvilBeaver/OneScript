/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OneScript.Localization;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Secondary;

namespace OneScriptDocumenter.Cli
{
    public class DocumentWriter : BaseModelVisitor
    {
        private readonly string _baseDir;
        private MarkdownWriter _writer;
        
        public DocumentWriter(string baseDir)
        {
            _baseDir = baseDir;
        }

        protected override void VisitDocument(IDocument document)
        {
            CreateDocumentFile(_baseDir, document);
        }
        
        private void CreateDocumentFile(string outputDir, IDocument document)
        {
            var fileName = ReferenceFactory.GetBslNameForAnnotatedObject(document.Owner) + ".md";
            using var writer = MarkdownWriter.OpenFile(Path.Combine(outputDir, fileName));

            switch (document)
            {
                case GlobalContextModel globalContext:
                    WriteGlobalContext(writer, globalContext);
                    break;
                case ClassModel classModel:
                    WriteClass(writer, classModel);
                    break;
                case EnumModel enumModel:
                    WriteEnum(writer, enumModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteEnum(MarkdownWriter writer, EnumModel model)
        {
            WriteOutlineOptions(writer);
            
            writer.Header1(BiString(model.Name));
            
            writer.Paragraph(model.Description);
            
            writer.Header2("Элементы");

            foreach (var itemModel in model.Items)
            {
                writer.Header3(BiString(itemModel.Name));
                writer.Paragraph(itemModel.Description);
            }
        }

        private void WriteClass(MarkdownWriter writer, ClassModel model)
        {
            WriteOutlineOptions(writer);
            
            writer.Header1(BiString(model.Name));
            
            writer.Paragraph(model.Description);
            
            WriteProperties(writer, model.Properties);
            WriteMethods(writer, model.Methods);
            WriteConstructors(writer, model.Constructors);
        }

        private void WriteGlobalContext(MarkdownWriter writer, GlobalContextModel model)
        {
            WriteOutlineOptions(writer);
            
            writer.Header1(model.Title);
            
            writer.Paragraph(model.Description);
            
            WriteProperties(writer, model.Properties);
            WriteMethods(writer, model.Methods);
        }

        private void WriteProperties(MarkdownWriter writer, IList<PropertyModel> propModels)
        {
            if (propModels == null || propModels.Count == 0)
                return;
            
            writer.Header2("Свойства");
            foreach (var propModel in propModels)
            {
                writer.Header3(BiString(propModel.Name));
                
                writer.BeginList(true);
                writer.ListItem("Чтение: " + (propModel.CanRead ? "Да" : "Нет"));
                writer.ListItem("Запись: " + (propModel.CanWrite ? "Да" : "Нет"));
                writer.EndList();
                
                if (!string.IsNullOrEmpty(propModel.Returns))
                    writer.Paragraph("**Тип значения:** " + propModel.Returns);

                writer.Paragraph(propModel.Description);
                
                if (!string.IsNullOrEmpty(propModel.Example))
                {
                    writer.Header4("Пример");
                    writer.WriteCode(propModel.Example);
                }
            }
        }
        
        private void WriteMethods(MarkdownWriter writer, IList<MethodModel> methModels)
        {
            if (methModels == null || methModels.Count == 0)
                return;
            
            writer.Header2("Методы");
            foreach (var model in methModels)
            {
                writer.Header3(BiString(model.Name));
                
                writer.Paragraph(model.Description);

                if (model.Parameters != null && model.Parameters.Count > 0)
                {
                    writer.Header4("Параметры");
                    
                    writer.BeginList(true);
                    foreach (var parameterModel in model.Parameters)
                    {
                        var itemBuilder = new StringBuilder();
                        itemBuilder.Append($"**{parameterModel.Name}**: {parameterModel.Description}");
                        if (parameterModel.IsOptional)
                        {
                            itemBuilder.Append(" *Необязательный*. ");
                            
                            if (!string.IsNullOrEmpty(parameterModel.DefaultValue))
                                itemBuilder.Append($"Значение по умолчанию: {parameterModel.DefaultValue}");
                        }
                        writer.ListItem(itemBuilder.ToString());
                    }
                    writer.EndList();
                }

                if (!string.IsNullOrEmpty(model.ReturnTypeDocumentation))
                {
                    writer.Header4("Возвращаемое значение");
                    writer.Paragraph(model.ReturnTypeDocumentation);
                }

                if (!string.IsNullOrEmpty(model.Example))
                {
                    writer.Header4("Пример");
                    writer.WriteCode(model.Example);
                }
            }
        }

        private void WriteConstructors(MarkdownWriter writer, IList<MethodModel> methModels)
        {
            if (methModels == null || methModels.Count == 0)
                return;
            
            writer.Header2("Конструкторы");
            foreach (var model in methModels)
            {
                writer.Header3(model.Title);
                
                writer.Paragraph(model.Description);

                if (model.Parameters != null && model.Parameters.Count > 0)
                {
                    writer.Header4("Параметры");
                    
                    writer.BeginList(true);
                    foreach (var parameterModel in model.Parameters)
                    {
                        var itemBuilder = new StringBuilder();
                        itemBuilder.Append($"**{parameterModel.Name}**: {parameterModel.Description}");
                        if (parameterModel.IsOptional)
                        {
                            itemBuilder.Append(" *Необязательный*. ");
                            if (!string.IsNullOrEmpty(parameterModel.DefaultValue))
                                itemBuilder.Append($"Значение по умолчанию: {parameterModel.DefaultValue}");
                        }
                        writer.ListItem(itemBuilder.ToString());
                    }
                    writer.EndList();
                }

                if (!string.IsNullOrEmpty(model.ReturnTypeDocumentation))
                {
                    writer.Header4("Возвращаемое значение");
                    writer.Paragraph(model.ReturnTypeDocumentation);
                }

                if (!string.IsNullOrEmpty(model.Example))
                {
                    writer.Header4("Пример");
                    writer.WriteCode(model.Example);
                }
            }
        }

        
        private static void WriteOutlineOptions(MarkdownWriter writer)
        {
            writer.Raw("---\n" +
                       "outline: [2, 3]\n" +
                       "---\n");
        }

        private static string BiString(BilingualString bi) => 
            bi.English != null ? 
                $"{bi.Russian} / {bi.English}"
                : bi.Russian;
    }
}