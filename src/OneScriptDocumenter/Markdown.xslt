<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:ext="http://exslt.org/common"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                exclude-result-prefixes="msxsl xml ext"
                xmlns:xml="http://www.w3.org/XML/1998/namespace"
>
  <xsl:output method="xml"/>

  <xsl:template match="text()"></xsl:template>

  <xsl:template match="oscript-docs">
    <root>
      <xsl:apply-templates/>
    </root>
  </xsl:template>
  
  <xsl:template match="context">
    <document href="{./name}.md">
# <xsl:value-of select="./name"/> / <xsl:value-of select="./alias"/>
      <xsl:text>
</xsl:text>
      <xsl:apply-templates/>
    </document>
  </xsl:template>
  
  <xsl:template match="global-context">
    <document href="{./category}.md">
# <xsl:value-of select="./category"/>
      <xsl:apply-templates/>
    </document>
  </xsl:template>

  <xsl:template match="properties">
## Свойства
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="property">
### <xsl:value-of select="./name"/> / <xsl:value-of select="./alias"/>
    <xsl:text>
Доступ: </xsl:text>
    <xsl:choose>
      <xsl:when test="./readable/text() = 'true' and ./writeable/text() = 'true'">Чтение/Запись</xsl:when>
      <xsl:when test="./readable/text() = 'true' and ./writeable/text() = 'false'">Чтение</xsl:when>
      <xsl:when test="./readable/text() = 'false' and ./writeable/text() = 'true'">Запись</xsl:when>
      <xsl:otherwise>Недоступно</xsl:otherwise>
    </xsl:choose>
    <xsl:if test="string-length(value)">
      <xsl:text>

Тип значения: </xsl:text>
      <xsl:value-of select="./value"/>
    </xsl:if>
    <xsl:apply-templates/>
  </xsl:template>
  
  
  <xsl:template match="methods">
## Методы
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="method">
### <xsl:value-of select="./name"/> / <xsl:value-of select="./alias"/>()
    <xsl:apply-templates/>
    <xsl:call-template name="parameters-writeout"/>
    <xsl:if test="count(./returns) > 0">
      <xsl:text>
#### Возвращаемое значение
</xsl:text>
      <xsl:apply-templates select="./returns" mode="textVal"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="description" xml:space="preserve">
    <xsl:apply-templates select="example"/>
    <xsl:apply-templates mode="textVal"/>
  </xsl:template>

  <xsl:template match="example">
#### Пример:
<xsl:apply-templates mode="split"/>
  </xsl:template>

  <xsl:template match="text()" name="split" mode="split">
    <xsl:param name="pText" select="."/>

    <xsl:if test="string-length($pText)">
      <xsl:variable name="vToken" select="substring-before(concat($pText, '&#10;'), '&#10;')"/>
      <xsl:text>    </xsl:text><xsl:value-of select="$vToken"/>
      <xsl:text>&#10;</xsl:text>
      <xsl:call-template name="split">
        <xsl:with-param name="pText" select="substring-after($pText, '&#10;')"/>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <xsl:template match="constructors">
## Конструкторы

  <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ctor">
### <xsl:value-of select="./name"/>
    <xsl:apply-templates/>
    <xsl:call-template name="parameters-writeout"/>
  </xsl:template>

  <xsl:template name="parameters-writeout">
    <xsl:if test="count(./param) > 0">
      <xsl:text>
#### Параметры
</xsl:text>
      <xsl:for-each select="./param">
* *<xsl:value-of select="@name"/>*: <xsl:value-of select ="./text()"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="text()" mode="textVal" xml:space="preserve">
<xsl:value-of select="."/>
  </xsl:template>




  <!--xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template-->

  <xsl:template match="name|alias"></xsl:template>
  
</xsl:stylesheet>
