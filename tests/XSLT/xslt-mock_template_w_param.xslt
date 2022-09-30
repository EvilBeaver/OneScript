<?xml version = "1.0" encoding="UTF-8"?>

<xsl:stylesheet version = "3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:param name="testParam" />
  <xsl:output method="xml" indent="yes" />
  <xsl:template match=" / ">
    <new>
      <xsl:variable name="item" select="/root/item[last()]"/>
      <xsl:value-of select="concat($testParam, '\', $item)"/>
    </new>
  </xsl:template>
</xsl:stylesheet>
