/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml;
using System.Xml.Schema;
using OneScript.Commons;
using OneScript.StandardLibrary.Xml;
using OneScript.StandardLibrary.XMLSchema.Interfaces;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.XMLSchema.Objects
{
    internal class XMLSchemaSerializer
    {
        internal static IXSComponent CreateInstance(XmlSchemaObject xmlSchemaObject)
        {
            if (xmlSchemaObject is XmlSchemaExternal xmlExternal)
                return CreateIXSDirective(xmlExternal);

            else if (xmlSchemaObject is XmlSchemaAnnotated xmlAnnotated)
                return CreateIXSAnnotated(xmlAnnotated);

            else if (xmlSchemaObject is XmlSchemaAnnotation xmlAnnotation)
                return CreateXSAnnotation(xmlAnnotation);

            else if (xmlSchemaObject is XmlSchemaDocumentation xmlDocumentation)
                return new XSDocumentation(xmlDocumentation);

            else if (xmlSchemaObject is XmlSchemaAppInfo xmlAppInfo)
                return new XSAppInfo(xmlAppInfo);

            throw RuntimeException.InvalidArgumentType();
        }

        private static IXSDirective CreateIXSDirective(XmlSchemaExternal xmlSchemaExternal)
        {
            if (xmlSchemaExternal is XmlSchemaImport import)
                return new XSImport(import);

            else if (xmlSchemaExternal is XmlSchemaInclude include)
                return new XSInclude(include);

            else if (xmlSchemaExternal is XmlSchemaRedefine redefine)
                return new XSRedefine(redefine);

            throw RuntimeException.InvalidArgumentType();
        }

        internal static IXSType CreateIXSType(XmlSchemaType xmlType)
        {
            if (xmlType is XmlSchemaComplexType complexType)
                return new XSComplexTypeDefinition(complexType);

            else if (xmlType is XmlSchemaSimpleType simpleType)
                return CreateXSSimpleTypeDefinition(simpleType);

            throw RuntimeException.InvalidArgumentType();
        }

        private static IXSComponent CreateIXSAnnotated(XmlSchemaAnnotated xmlAnnotated)
        {
            if (xmlAnnotated is XmlSchemaType xmlType)
                return CreateIXSType(xmlType);

            else if (xmlAnnotated is XmlSchemaParticle xmlParticle)
            {
                if (string.IsNullOrEmpty(xmlParticle.MinOccursString) && string.IsNullOrEmpty(xmlParticle.MaxOccursString))
                    return CreateIXSFragment(xmlParticle);
                else
                    return new XSParticle(xmlParticle);
            }
            else if (xmlAnnotated is XmlSchemaFacet xmlFacet)
                return CreateIXSFacet(xmlFacet);

            else if (xmlAnnotated is XmlSchemaAttribute xmlAttribute)
                return new XSAttributeDeclaration(xmlAttribute);

            else if (xmlAnnotated is XmlSchemaAnyAttribute xmlAnyAttribute)
                return CreateXSWildcard(xmlAnyAttribute);

            else if (xmlAnnotated is XmlSchemaAttributeGroup xmlAttributeGroup)
                return new XSAttributeGroupDefinition(xmlAttributeGroup);

            else if (xmlAnnotated is XmlSchemaAttributeGroupRef xmlAttributeGroupRef)
                return new XSAttributeGroupDefinition(xmlAttributeGroupRef);

            else if (xmlAnnotated is XmlSchemaNotation xmlNotation)
                return new XSNotationDeclaration(xmlNotation);

            else if (xmlAnnotated is XmlSchemaGroup xmlGroup)
                return new XSModelGroupDefinition(xmlGroup);

          

            else
                throw RuntimeException.InvalidArgumentType();
        }

        internal static IXSFragment CreateIXSFragment(XmlSchemaParticle xmlParticle)
        {
            if (xmlParticle is XmlSchemaElement element)
                return new XSElementDeclaration(element);

            else if (xmlParticle is XmlSchemaGroupBase groupBase)
                return new XSModelGroup(groupBase);

            else if (xmlParticle is XmlSchemaAny xmlAny)
                return new XSWildcard(xmlAny);

            else if (xmlParticle is XmlSchemaGroupRef xmlGroupRef)
                return new XSModelGroupDefinition(xmlGroupRef);

            else
                throw RuntimeException.InvalidArgumentType();
        }

        internal static IXSFacet CreateIXSFacet(XmlSchemaFacet xmlFacet)
        {
            if (xmlFacet is XmlSchemaNumericFacet numericFacet)
                return CreateIXSNumericFacet(numericFacet);

            else if (xmlFacet is XmlSchemaEnumerationFacet enumerationFacet)
                return new XSEnumerationFacet(enumerationFacet);

            else if (xmlFacet is XmlSchemaMaxExclusiveFacet maxExclusiveFacet)
                return new XSMaxExclusiveFacet(maxExclusiveFacet);

            else if (xmlFacet is XmlSchemaMaxInclusiveFacet maxInclusiveFacet)
                return new XSMaxInclusiveFacet(maxInclusiveFacet);

            else if (xmlFacet is XmlSchemaMinExclusiveFacet minExclusiveFacet)
                return new XSMinExclusiveFacet(minExclusiveFacet);

            else if (xmlFacet is XmlSchemaMinInclusiveFacet minInclusiveFacet)
                return new XSMinInclusiveFacet(minInclusiveFacet);

            else if (xmlFacet is XmlSchemaPatternFacet patternFacet)
                return new XSPatternFacet(patternFacet);

            else if (xmlFacet is XmlSchemaWhiteSpaceFacet whitespaceFacet)
                return new XSWhitespaceFacet(whitespaceFacet);

            else
                throw RuntimeException.InvalidArgumentType();
        }

        private static IXSFacet CreateIXSNumericFacet(XmlSchemaNumericFacet numericFacet)
        {
            if (numericFacet is XmlSchemaFractionDigitsFacet fractionDigitsFacet)
                return new XSFractionDigitsFacet(fractionDigitsFacet);

            else if (numericFacet is XmlSchemaLengthFacet lengthFacet)
                return new XSLengthFacet(lengthFacet);

            else if (numericFacet is XmlSchemaMaxLengthFacet maxLengthFacet)
                return new XSMaxLengthFacet(maxLengthFacet);

            else if (numericFacet is XmlSchemaMinLengthFacet minLengthFacet)
                return new XSMinLengthFacet(minLengthFacet);

            else if (numericFacet is XmlSchemaTotalDigitsFacet totalDigitsFacet)
                return new XSTotalDigitsFacet(totalDigitsFacet);

            else
                throw RuntimeException.InvalidArgumentType();
        }

        internal static XSAnnotation CreateXSAnnotation(XmlSchemaAnnotation annotation)
            => new XSAnnotation(annotation);

        internal static XMLExpandedName CreateXMLExpandedName(XmlQualifiedName qualifiedName)
            => new XMLExpandedName(qualifiedName);

        internal static XSWildcard CreateXSWildcard(XmlSchemaAnyAttribute xmlAnyAttribute)
            => new XSWildcard(xmlAnyAttribute);

        internal static XSSimpleTypeDefinition CreateXSSimpleTypeDefinition(XmlSchemaSimpleType schemaType)
            => new XSSimpleTypeDefinition(schemaType);
    }
}
