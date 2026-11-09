<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/">
    <html>
      <head>
        <title>Кадри науковців</title>
        <style>
          body { font-family: "Segoe UI", sans-serif; margin: 24px; }
          table { border-collapse: collapse; }
          th, td { border: 1px solid #999; padding: 6px 10px; text-align: left; }
          th { background: #eeeeee; }
        </style>
      </head>
      <body>
        <h1>Кадри науковців (Аналіз штатного розпису)</h1>
        <table>
          <tr>
            <th>П.І.П.</th>
            <th>Факультет</th>
            <th>Кафедра</th>
            <th>Лабораторія</th>
            <th>Посада</th>
            <th>Початок</th>
            <th>Кінець</th>
          </tr>
          <xsl:for-each select="Staff/Scientist">
            <tr>
              <td><xsl:value-of select="@FullName" /></td>
              <td><xsl:value-of select="@Faculty" /></td>
              <td><xsl:value-of select="@Department" /></td>
              <td>
                <xsl:choose>
                  <xsl:when test="@Laboratory"><xsl:value-of select="@Laboratory" /></xsl:when>
                  <xsl:otherwise>—</xsl:otherwise>
                </xsl:choose>
              </td>
              <td><xsl:value-of select="@Position" /></td>
              <td><xsl:value-of select="Term/@Start" /></td>
              <td>
                <xsl:choose>
                  <xsl:when test="Term/@End"><xsl:value-of select="Term/@End" /></xsl:when>
                  <xsl:otherwise>по теперішній час</xsl:otherwise>
                </xsl:choose>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
