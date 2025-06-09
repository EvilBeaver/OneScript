/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Localization;
using OneScript.Types;
using OneScript.Values;
using OneScriptDocumenter.Model;
using OneScriptDocumenter.Primary;
using ArgumentException = System.ArgumentException;

namespace OneScriptDocumenter.Secondary
{
    public class DocumentationModelBuilder
    {
        private readonly PrimaryDocumentation _primaryDocs;
        private readonly string _tocFile;
        
        private XmlDocConverter _docConverter;

        public DocumentationModelBuilder(PrimaryDocumentation primaryDocs, IReferenceResolver referenceResolver, string tocFile)
        {
            _primaryDocs = primaryDocs;
            _tocFile = tocFile;

            _docConverter = new XmlDocConverter(referenceResolver);
        }

        public DocumentationModel Build()
        {
            ConsoleLogger.Info("Collecting secondary documentation");
            
            List<TocItem> items;
            if (!string.IsNullOrEmpty(_tocFile))
            {
                items = ReadTableOfContents();
            }
            else
            {
                items = new List<TocItem>();
            }

            return BuildDocumentation(items);
        }
        
        private List<TocItem> ReadTableOfContents()
        {
            using var filestream = new FileStream(_tocFile, FileMode.Open);

            return JsonSerializer.Deserialize<TableOfContents>(filestream).Items;
        }

        private DocumentationModel BuildDocumentation(List<TocItem> items)
        {
            var model = new DocumentationModel();
            var categoryOthers = new SyntaxGroup
            {
                Title = items.Count == 0 ? "Документация" : "Прочее",
                Page = "others"
            };

            var documentsByName = new Dictionary<string, IDocument>();
            
            foreach (var primaryDoc in _primaryDocs)
            {
                IDocument document;
                switch (primaryDoc.OwnerKind)
                {
                    case OwnerKind.GlobalContext:
                    case OwnerKind.DocumentationProvider:
                    {
                        document = MakeGlobalContextNode(primaryDoc);
                        break;
                    }
                    case OwnerKind.SystemEnum:
                    {
                        document = MakeSystemEnumNode(primaryDoc);
                        break;
                    }
                    case OwnerKind.SimpleEnum:
                    {
                        document = MakeSimpleEnumNode(primaryDoc);
                        break;
                    }
                    case OwnerKind.Class:
                    {
                        document = MakeClassNode(primaryDoc);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (document != null)
                    documentsByName.Add(primaryDoc.Title, document);
            }

            var toc = BuildTableOfContents(items, documentsByName).ToList();
            foreach (var keyValuePair in documentsByName.OrderBy(kv => kv.Key))
            {
                categoryOthers.AddChildDocument(keyValuePair.Value);
            }
            
            toc.Add(categoryOthers);
            model.Items = toc;

            return model;
        }

        private IDocument MakeSystemEnumNode(PrimaryBslDocument primaryDoc)
        {
            var typeMarkup = MarkupProvider.GetEnumMarkup(primaryDoc.Owner);
            // TODO сделать модель енумов для ручных енумов
            return new EnumModel
            {
                Owner = primaryDoc.Owner,
                Name = new BilingualString(typeMarkup.Name, typeMarkup.Alias),
                Description = XmlSummary(primaryDoc.SelfDoc),
                Items = LoadSystemEnumItems(primaryDoc.Owner),
                Example = XmlTextBlock(primaryDoc.SelfDoc, "example"),
                SeeAlso = XmlSeeAlso(primaryDoc.SelfDoc)
            };
        }

        private List<EnumItemModel> LoadSystemEnumItems(Type enumType)
        {
            var items = new List<EnumItemModel>();
            var instantiator = enumType.GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public);
            if (instantiator == null)
                return items;

            var enumInstance = (IEnumerable)instantiator.Invoke(null, new[] { new FakeTypeManager() });
            if (enumInstance == null)
                return items;
            
            
            foreach (var unknownTypeItem in enumInstance)
            {
                if (unknownTypeItem is EnumerationValue enumValue)
                {
                    items.Add(new EnumItemModel
                    {
                        Name = new BilingualString(enumValue.Name, enumValue.Alias)
                    });
                }
            }

            return items;
        }

        private IDocument MakeSimpleEnumNode(PrimaryBslDocument primaryDoc)
        {
            var typeMarkup = MarkupProvider.GetEnumMarkup(primaryDoc.Owner);
            
            return new EnumModel
            {
                Owner = primaryDoc.Owner,
                Name = new BilingualString(typeMarkup.Name, typeMarkup.Alias),
                Description = XmlSummary(primaryDoc.SelfDoc),
                Example = XmlTextBlock(primaryDoc.SelfDoc, "example"),
                Items = primaryDoc.Fields.Select(kv => ConvertEnumValue(kv.Key, kv.Value)).ToList(),
                SeeAlso = XmlSeeAlso(primaryDoc.SelfDoc)
            };
        }

        private IDocument MakeClassNode(PrimaryBslDocument primaryDoc)
        {
            var typeMarkup = MarkupProvider.GetClassMarkup(primaryDoc.Owner);
            return new ClassModel
            {
                Owner = primaryDoc.Owner,
                Name = new BilingualString(typeMarkup.Name, typeMarkup.Alias),
                Description = XmlSummary(primaryDoc.SelfDoc),
                Example = XmlTextBlock(primaryDoc.SelfDoc, "example"),
                Properties = primaryDoc.Properties.Select(kv => ConvertProperty(kv.Key, kv.Value)).ToList(),
                Methods = primaryDoc.Methods.Select(kv => ConvertMethod(kv.Key, kv.Value)).ToList(),
                Constructors = primaryDoc.Constructors.Select(kv => ConvertConstructor(kv.Key, kv.Value)).ToList(),
                SeeAlso = XmlSeeAlso(primaryDoc.SelfDoc)
            };
        }

        private IEnumerable<SyntaxGroup> BuildTableOfContents(List<TocItem> items, Dictionary<string, IDocument> documentsByName)
        {
            var tocCategories = new List<SyntaxGroup>();
            VisitTocItems(items, tocCategories, documentsByName);

            return tocCategories;
        }

        private void VisitTocItems(IEnumerable<TocItem> items, IList<SyntaxGroup> categories,
            Dictionary<string, IDocument> categoriesByName)
        {
            if (items == null)
                return;
            
            foreach (var tocItem in items)
            {
                var declaredCategory = new SyntaxGroup();
                
                if (categoriesByName.TryGetValue(tocItem.Text, out var selfDoc))
                {
                    declaredCategory.Document = selfDoc;
                    categoriesByName.Remove(tocItem.Text);
                }
                else
                {
                    declaredCategory.Title = tocItem.Text;
                    declaredCategory.Page = tocItem.GeneratePage;
                }
                
                categories.Add(declaredCategory);
                
                VisitTocItems(tocItem.Items, declaredCategory.Items, categoriesByName);
            }
        }
        
        private IDocument MakeGlobalContextNode(PrimaryBslDocument primaryDoc)
        {
            var contextModel = new GlobalContextModel();
            contextModel.Owner = primaryDoc.Owner;
            contextModel.Title = primaryDoc.Title;
            contextModel.Description = XmlSummary(primaryDoc.SelfDoc);
            contextModel.Properties = primaryDoc.Properties.Select(kv => ConvertProperty(kv.Key, kv.Value)).ToList();
            contextModel.Methods = primaryDoc.Methods.Select(kv => ConvertMethod(kv.Key, kv.Value)).ToList();
            contextModel.SeeAlso = XmlSeeAlso(primaryDoc.SelfDoc);

            return contextModel;
        }

        private PropertyModel ConvertProperty(PropertyInfo property, XElement documentation)
        {
            var model = new PropertyModel();

            try
            {
                var markup = MarkupProvider.GetPropertyMarkup(property);
                model.Name = new BilingualString(markup.Name, markup.Alias);
                (model.CanRead, model.CanWrite) = PropertyReadWriteFlags(property, markup);
            }
            catch (ArgumentException)
            {
                var markup = MarkupProvider.GetMemberDocumentationMarkup(property);
                model.Name = new BilingualString(markup.Name, markup.Alias);
                (model.CanRead, model.CanWrite) = PropertyReadWriteFlags(property);
            }
            
            model.Description = XmlSummary(documentation);
            model.Returns = XmlTextBlock(documentation, "value");
            
            model.ClrName = property.Name;
            model.Example = XmlTextBlock(documentation, "example", true);
            model.SeeAlso = XmlSeeAlso(documentation);

            return model;
        }

        private (bool canRead, bool canWrite) PropertyReadWriteFlags(PropertyInfo property, ContextPropertyAttribute markup)
        {
            var hasReader = property.GetGetMethod() != null;
            var hasWriter = property.GetSetMethod() != null;

            return (hasReader && markup.CanRead, hasWriter && markup.CanWrite);
        }
        
        private (bool canRead, bool canWrite) PropertyReadWriteFlags(PropertyInfo property)
        {
            var hasReader = property.GetGetMethod() != null;
            var hasWriter = property.GetSetMethod() != null;

            return (hasReader, hasWriter);
        }

        private MethodModel ConvertMethod(MethodInfo method, XElement documentation)
        {
            var model = new MethodModel();

            try
            {
                var markup = MarkupProvider.GetMethodMarkup(method);
                model.Name = new BilingualString(markup.Name, markup.Alias);
            }
            catch (ArgumentException)
            {
                var markup = MarkupProvider.GetMemberDocumentationMarkup(method);
                model.Name = new BilingualString(markup.Name, markup.Alias);
            }
            
            model.Description = XmlSummary(documentation);
            model.ReturnTypeDocumentation = XmlReturns(documentation);
            model.Example = XmlTextBlock(documentation, "example", true);
            model.Parameters = CreateParameters(method, documentation, p => p.ParameterType != typeof(IBslProcess));
            model.SeeAlso = XmlSeeAlso(documentation);

            return model;
        }

        private List<ParameterModel> CreateParameters(MethodInfo method, XElement documentation, Func<ParameterInfo, bool> filter)
        {
            var documentedParams = documentation?.Elements("param")
                .ToDictionary(elem => elem.Attribute("name")?.Value ?? 
                                      throw new ArgumentException($"No name attribute in param section for method {method.DeclaringType}.{method.Name}"))
                ?? new Dictionary<string, XElement>();

            var models = new List<ParameterModel>();
            
            foreach (var parameter in method.GetParameters().Where(filter))
            {
                documentedParams.TryGetValue(parameter.Name!, out var paramDoc);
                var model = new ParameterModel
                {
                    Name = parameter.Name,
                    Description = _docConverter.ConvertTextBlock(paramDoc),
                    DefaultValue = parameter.DefaultValue?.ToString(),
                    IsOptional = parameter.IsOptional
                };
                
                models.Add(model);
            }

            return models;
        }

        private MethodModel ConvertConstructor(MethodInfo method, XElement documentation)
        {
            var model = new MethodModel();

            try
            {
                var markup = MarkupProvider.GetConstructorMarkup(method);
                model.Name = new BilingualString(markup.Name ?? "Основной");
            }
            catch (ArgumentException)
            {
                var markup = MarkupProvider.GetMemberDocumentationMarkup(method);
                model.Name = new BilingualString(markup.Name, markup.Alias);
            }
            
            if (documentation != null)
            {
                model.Description = XmlSummary(documentation);
                model.ReturnTypeDocumentation = XmlReturns(documentation);
                model.Example = XmlTextBlock(documentation, "example", true);
                model.Parameters = CreateParameters(method, documentation, p => p.ParameterType != typeof(TypeActivationContext));
            }

            return model;
        }

        private EnumItemModel ConvertEnumValue(FieldInfo field, XElement documentation)
        {
            var markup = MarkupProvider.GetEnumValueMarkup(field);
            return new EnumItemModel
            {
                Name = new BilingualString(markup.Name, markup.Alias),
                Description = XmlSummary(documentation)
            };
        }

        private string XmlSummary(XElement docs)
        {
            return XmlTextBlock(docs, "summary");
        }
        
        private string XmlReturns(XElement docs)
        {
            return XmlTextBlock(docs, "returns");
        }
        
        private string XmlTextBlock(XElement docs, string nodeName, bool isCode = false)
        {
            var stringValue = _docConverter.ConvertTextBlock(docs?.Element(nodeName), !isCode);
            return stringValue == "" ? null : // Чтобы свойство не писал сериализатор JSON
                stringValue;
        }
        
        private IReadOnlyCollection<string> XmlSeeAlso(XElement doc)
        {
            var links = _docConverter.ConvertSeeAlsoList(doc);
            return links.Count == 0 ? null : links;
        }

        private class FakeTypeManager : ITypeManager
        {
            public TypeDescriptor GetTypeByName(string name)
            {
                throw new NotImplementedException();
            }

            public TypeDescriptor GetTypeByFrameworkType(Type type)
            {
                throw new NotImplementedException();
            }

            public bool TryGetType(string name, out TypeDescriptor type)
            {
                throw new NotImplementedException();
            }

            public bool TryGetType(Type frameworkType, out TypeDescriptor type)
            {
                throw new NotImplementedException();
            }

            public TypeDescriptor RegisterType(string name, string alias, Type implementingClass)
            {
                return default;
            }

            public void RegisterType(TypeDescriptor typeDescriptor)
            {
                
            }

            public ITypeFactory GetFactoryFor(TypeDescriptor type)
            {
                throw new NotImplementedException();
            }

            public bool IsKnownType(Type type)
            {
                throw new NotImplementedException();
            }

            public bool IsKnownType(string typeName)
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<TypeDescriptor> RegisteredTypes()
            {
                throw new NotImplementedException();
            }
        }
    }
}